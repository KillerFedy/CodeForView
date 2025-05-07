using AutoMapper;
using BusinessLogicCore.Interfaces;
using Database.Models;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelsProject.ApiModels.BankTransaction;
using ModelsProject.Common;
using PaymentProviderDataAccess.Interfaces;
using PaymentProviderDataAccess.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Web;
using Utilities;

namespace BusinessLogicCore.Services
{
    public class TransactionProviderService(TransactionInfoRepository transactionInfoRepository, 
        PaymentProviderInfoRepository paymentProviderInfoRepository, IHttpClientFactory httpClientFactory, IPaymentProviderGateway paymentProviderGateway, 
        ILogger<TransactionProviderService> logger) : ITransactionProviderService
    {
        private readonly string _defaultProviderName = "DEFAULT_PROVIDER";
        private readonly string _forceModeName = "FORCE:";
        public async Task<string> TransactionProvide(BankTransactionDto bankTransactionDto)
        {
            var errorMsg = "";
            var missingProprties = new List<string>();
            FindMissingMainPropertyInBankTransaction(missingProprties, bankTransactionDto);
            if(missingProprties.Count > 0)
            {
                errorMsg = $"Отсутствуют следующие параметры запроса: {string.Join(", ", missingProprties)}";
                logger.LogError(errorMsg);
                return ErrorInitializer.InitializeErrorToString(bankTransactionDto.Stan, errorMsg);
            }
            ValidateBankTransactionAmount(bankTransactionDto);
            ValidateBankTransactionDateTime(bankTransactionDto);
            var providerTransactionRequest = new ProviderTransactionRequest(bankTransactionDto.Function, bankTransactionDto.Stan, bankTransactionDto.Rrn,
                bankTransactionDto.Sid, bankTransactionDto.Phone, bankTransactionDto.Pcode, bankTransactionDto.Amount, bankTransactionDto.Currency, bankTransactionDto.Terminal,
                bankTransactionDto.Date, bankTransactionDto.Time);
            switch(bankTransactionDto.Function)
            {
                case "bank_account":
                    return await TransactionAuth(providerTransactionRequest);
                case "bank_payment":
                    return await TransactionPay(providerTransactionRequest);
                case "getPaymentStatus":
                    return await TransactionPayStatus(providerTransactionRequest);
                default:
                    errorMsg = "Тип транзакции некорректен";
                    logger.LogError(errorMsg);
                    return ErrorInitializer.InitializeErrorToString(bankTransactionDto.Stan, errorMsg);
            }
        }

        private async Task<string> TransactionPayStatus(ProviderTransactionRequest providerTransactionRequest)
        {
            var errorMsg = "";
            var transactionPay = await transactionInfoRepository.GetItemByQueryAsync(x => x.Rrn == providerTransactionRequest.Rrn &&
            !x.IsTransactionAuth && x.PaymentName == providerTransactionRequest.Pcode && x.Phone == providerTransactionRequest.Phone
            && x.TerminalId == providerTransactionRequest.Terminal, include: x => x.Include(x => x.PaymentProviderInfo));
            if(transactionPay == null)
            {
                errorMsg = $"Транзакция платежа не найдена";
                logger.LogError(errorMsg);
                return ErrorInitializer.InitializeErrorToString(providerTransactionRequest.Stan, errorMsg);
            }
            ProviderTransactionResponse? transactionResponse;
            switch(transactionPay.PaymentProviderInfo!.ProviderName)
            {
                case "ELECSNET":
                   transactionResponse = await paymentProviderGateway.ProvideTransactionRequest(transactionPay.PaymentProviderInfo.ProviderUrl,
                        providerTransactionRequest);
                    break;
                default:
                    // Жёсткий костыль на проверку провайдера ФСГ
                    transactionResponse = new ProviderTransactionResponse(providerTransactionRequest.Stan, providerTransactionRequest.Rrn!,
                        providerTransactionRequest.Date!, providerTransactionRequest.Time!, providerTransactionRequest.Phone!,
                        providerTransactionRequest.Amount!, providerTransactionRequest.Currency!, 
                        transactionPay.IsErrorResponse ? "Не удалось совершить платёж" : "Платёж успешный"); 
                    break;
            }
            if (transactionResponse == null)
            {

                errorMsg = $"Банковский сервер {transactionPay.PaymentProviderInfo.ProviderName} " +
                $"не вернул ответа";
                logger.LogError(errorMsg);
                return ErrorInitializer.InitializeErrorToString(providerTransactionRequest.Stan, errorMsg);
            }
            var transactionResponseXml = ObjectSerializer.SerializeToXml(transactionResponse);
            return transactionResponseXml;
        }

        private async Task<string> TransactionAuth(ProviderTransactionRequest providerTransactionRequest)
        {
            var errorMsg = "";
            var transactionAuthList = await transactionInfoRepository.GetListByQueryAsync(x => x.Rrn == providerTransactionRequest.Rrn &&
            x.IsTransactionAuth);
            if (transactionAuthList != null && transactionAuthList.Count != 0)
            {
                errorMsg = $"Транзакция авторизации с RRN={providerTransactionRequest.Rrn} уже существует";
                logger.LogError(errorMsg);
                return ErrorInitializer.InitializeErrorToString(providerTransactionRequest.Stan, errorMsg);
            }
            var transactionInfo = new TransactionInfo(providerTransactionRequest.Rrn, providerTransactionRequest.Phone, providerTransactionRequest.Amount,
                providerTransactionRequest.Terminal, providerTransactionRequest.Currency);
            transactionInfo.InitAuthTransactionBeforeProviderRequest();
            await transactionInfoRepository.InsertAsync(transactionInfo);
            var transactionInfoResponse = "";
            var indexForceModeName = providerTransactionRequest.Pcode.IndexOf(_forceModeName);
            var isForceModeBankRequest = indexForceModeName != -1;
            providerTransactionRequest.SetPcode(isForceModeBankRequest ? providerTransactionRequest.Pcode.Remove(indexForceModeName, _forceModeName.Length)
                : providerTransactionRequest.Pcode);
            var providerTransactionResponse = await AutomaticallyProvideAuthRequest(transactionInfo, providerTransactionRequest, isForceModeBankRequest);
            if (providerTransactionResponse == null)
            {
                errorMsg = "Ни один банковский платёжный сервер не вернул ответа";
                logger.LogError(errorMsg);
                return ErrorInitializer.InitializeErrorToString(providerTransactionRequest.Stan, errorMsg);
            }
            var isErrorResponse = providerTransactionResponse.MBilling.Error != null;
            if (!isErrorResponse) InitProviderTransactionResponseInfoByPcode(providerTransactionRequest.Pcode,
                providerTransactionResponse.MBilling.AccountInfo);
            transactionInfoResponse = ObjectSerializer.SerializeToXml(providerTransactionResponse);
            transactionInfo.InitTransactionAfterProviderRequest(isErrorResponse, providerTransactionRequest.Pcode, transactionInfoResponse);
            if (isErrorResponse) transactionInfo.ProcessPaymentEnd();
            await transactionInfoRepository.UpdateAsync(transactionInfo);
            return transactionInfoResponse;
        }

        private async Task<string> TransactionPay(ProviderTransactionRequest providerTransactionRequest)
        {
            var errorMsg = "";
            var transactionInfo = await transactionInfoRepository.GetItemByQueryAsync(x => 
            !x.IsErrorResponse && 
            !x.IsPaymentEnd && 
            x.IsTransactionAuth
            && x.Rrn == providerTransactionRequest.Rrn && x.TerminalId == providerTransactionRequest.Terminal, 
            include: x => x.Include(x => x.PaymentProviderInfo));
            if (transactionInfo == null)
            {
                errorMsg = "Не удалось получить объект транзакции авторизации платежа";
                logger.LogError(errorMsg);
                return ErrorInitializer.InitializeErrorToString(providerTransactionRequest.Stan, errorMsg);
            }
            providerTransactionRequest.SetPcode(transactionInfo.PaymentProviderInfo.ProviderCodeName != null ? 
                transactionInfo.PaymentProviderInfo.ProviderCodeName : providerTransactionRequest.Pcode);
            var providerTransactionResponse = await paymentProviderGateway.ProvideTransactionRequest(transactionInfo.PaymentProviderInfo.ProviderUrl,
                providerTransactionRequest);
            if (providerTransactionResponse == null)
            {

                errorMsg = $"Банковский сервер {transactionInfo.PaymentProviderInfo.ProviderName} " +
                $"не вернул ответа";
                logger.LogError(errorMsg);
                return ErrorInitializer.InitializeErrorToString(providerTransactionRequest.Stan, errorMsg);
            }
            var isErrorResponse = providerTransactionResponse.MBilling.Error != null;
            if(!isErrorResponse) InitProviderTransactionResponseInfoByPcode(providerTransactionRequest.Pcode,
                providerTransactionResponse.MBilling.Payment);
            var response = ObjectSerializer.SerializeToXml(providerTransactionResponse);
            using (var transaction = await transactionInfoRepository.BeginTransactionAsync())
            {
                try
                {
                    var transactionPayInfo = new TransactionInfo(providerTransactionRequest.Rrn, providerTransactionRequest.Phone, 
                        providerTransactionRequest.Amount, providerTransactionRequest.Terminal, providerTransactionRequest.Currency);
                    transactionPayInfo.InitTransactionAfterProviderRequest(isErrorResponse, providerTransactionRequest.Pcode, response);
                    transactionPayInfo.InitPaymentProviderInfoId(transactionInfo.PaymentProviderInfoId);
                    transactionInfo.ProcessPaymentEnd();
                    transactionPayInfo.ProcessPaymentEnd();
                    await transactionInfoRepository.UpdateAsync(transactionInfo);
                    await transactionInfoRepository.InsertAsync(transactionPayInfo);
                    await transaction.CommitAsync();
                }
                catch(Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Ошибка при подготовке данных к обновлению/обновлении БД во время выполнения bank_payment");
                }
            }
            return response;
        }

        private async Task<ProviderTransactionResponse?> AutomaticallyProvideAuthRequest(TransactionInfo transactionInfo, 
            ProviderTransactionRequest providerTransactionRequest, bool forceMode = false)
        {
            ProviderTransactionResponse? providerTransactionResponse = null;
            var paymentProviderInfos = await paymentProviderInfoRepository.GetListByQueryAsync(x => 
            (forceMode ? x.ProviderCodeName == providerTransactionRequest.Pcode : x.PaymentName == providerTransactionRequest.Pcode),
                orderBy: x => x.OrderBy(x => x.Priority));
            if (paymentProviderInfos == null || paymentProviderInfos.Count == 0)
            {
                var defaultPaymentProviderInfo = await TryFindDefaultProvider();
                if (defaultPaymentProviderInfo == null)
                {
                    var errorMsg = "Не удалось получить информацию о банковских провайдерах";
                    logger.LogError(errorMsg);
                    return null;
                }
                if(paymentProviderInfos == null) paymentProviderInfos = [];
                paymentProviderInfos.Add(defaultPaymentProviderInfo);
            }
            foreach (var paymentProviderInfo in paymentProviderInfos)
            {
                providerTransactionRequest.SetPcode(paymentProviderInfo.ProviderCodeName ?? providerTransactionRequest.Pcode);
                transactionInfo.InitPaymentProviderInfoId(paymentProviderInfo.Id);
                providerTransactionResponse = await paymentProviderGateway.ProvideTransactionRequest(paymentProviderInfo.ProviderUrl, 
                    providerTransactionRequest);
                if (providerTransactionResponse == null) continue;
                if (providerTransactionResponse.MBilling.Error == null) break;
            }
            return providerTransactionResponse;
        }

        private async Task<PaymentProviderInfo?> TryFindDefaultProvider()
        {
            var selectedProviderInfo = await paymentProviderInfoRepository.GetItemByQueryAsync(x => x.ProviderName == _defaultProviderName,
                    asNoTracking: true);
            return selectedProviderInfo;
        }

        private async Task<string> BaseRequestToBankService(string serviceUrl, object bankTransactionDto)
        {
            try
            {
                var client = httpClientFactory.CreateClient("BankTransactionProviderClient");
                var uriBuilder = new UriBuilder(serviceUrl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                var properties = bankTransactionDto.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var jsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
                    if (jsonPropertyName != null)
                    {
                        var value = property.GetValue(bankTransactionDto)?.ToString();
                        if (value != null)
                        {
                            query[jsonPropertyName] = value;
                        }
                    }
                }
                uriBuilder.Query = query.ToString();
                var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
                logger.LogInformation($"Обращение к банковскому сервису:\n {request}");
                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation($"Сервис вернул: {responseContent}");
                    return responseContent;
                }
                logger.LogInformation($"По техническим причинам, сервис {serviceUrl} недоступен. Код ответа: {response.StatusCode}\n" +
                    $"Тело ответа:{responseContent}");
                return string.Empty;
            }
            catch(Exception ex)
            {
                logger.LogError($"Функционал запроса к сервису {serviceUrl} завершился с исключением: \n {ex.Message}");
                return string.Empty;
            }
        }

        private void InitProviderTransactionResponseInfoByPcode(string pcode, PaymentProviderDataAccess.Models.MainResponseInfo mainResponseInfo)
        {
            var providerInfo = pcode + ";";
            providerInfo = providerInfo.Insert(0, "PCODE=");
            mainResponseInfo.SetInfo(mainResponseInfo.Info.Insert(0, providerInfo));
        }

        private void FindMissingMainPropertyInBankTransaction(List<string> missingProperties, BankTransactionDto bankTransactionDto)
        {
            if(string.IsNullOrEmpty(bankTransactionDto.Amount))
                missingProperties.Add(nameof(BankTransactionDto.Amount));
            if (string.IsNullOrEmpty(bankTransactionDto.Rrn))
                missingProperties.Add(nameof(BankTransactionDto.Rrn));
            if (string.IsNullOrEmpty(bankTransactionDto.Phone))
                missingProperties.Add(nameof(BankTransactionDto.Phone));
            if (string.IsNullOrEmpty(bankTransactionDto.Pcode))
                missingProperties.Add(nameof(BankTransactionDto.Pcode));
            if (string.IsNullOrEmpty(bankTransactionDto.Terminal))
                missingProperties.Add(nameof(BankTransactionDto.Terminal));
            if (string.IsNullOrEmpty(bankTransactionDto.Currency))
                missingProperties.Add(nameof(BankTransactionDto.Currency));
        }

        private void ValidateBankTransactionAmount(BankTransactionDto bankTransactionDto)
        {
            bankTransactionDto.Amount = bankTransactionDto.Amount.Replace(",", ".");
        }

        private void ValidateBankTransactionDateTime(BankTransactionDto bankTransactionDto)
        {
            var dateTimeNow = DateTime.Now;
            var dateString = dateTimeNow.ToString("yyyyMMdd");
            var timeString = dateTimeNow.ToString("HHmmss");
            bankTransactionDto.Date = dateString;
            bankTransactionDto.Time = timeString;
        }
    }
}
