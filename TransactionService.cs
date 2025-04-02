using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeForView
{
    public class TransactionService
    {
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
            if (!isErrorResponse) InitProviderTransactionResponseInfoByPcode(providerTransactionRequest.Pcode,
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
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Ошибка при подготовке данных к обновлению/обновлении БД во время выполнения bank_payment");
                }
            }
            return response;
        }
    }
}
