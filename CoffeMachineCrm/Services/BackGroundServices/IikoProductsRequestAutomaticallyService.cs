using System.Reflection;
using BusinessLogicCore.BusinessLogicModels;
using BusinessLogicCore.Infrastructure;
using BusinessLogicCore.Infrastructure.Iiko;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using ModelsProject.Enums;
using ModelsProject.Iiko.Nomenclature;
using ModelsProject.ProductRequestIikoXMLData;

namespace BusinessLogicCore.BackgroundService;

/// <summary>
///     автоматический сервис подачи заявок в iiko chain
/// </summary>
public class IikoProductsRequestAutomaticallyService(ILogger<IikoProductsRequestAutomaticallyService> logger, 
    IBaseIikoRequestService baseIikoRequestService,
    IIikoCommonService iikoCommonService,
    ITradepointProductRequestParserService tradepointProductRequestParserService,
    IEmailService emailService,
    IEmailTradepointRequestHandlerService emailTradepointRequestHandlerService,
    ITradepointRequestFeedbackService tradepointRequestFeedbackService,
    IOptions<AppSettings> appsettings) : TaskManager(logger)
{
    private IEnumerable<CustomerResponseDto>? _customers;
    private IEnumerable<NomenclatureItem>? _nomenclatureItems;

    private const string _itDepartmentEmail = "geek@cmachine.ru";

    public Uri? IikoChainUrl => appsettings?.Value?.IikoConnection?.ChainApiUrl;

    protected override async Task RunTaskAsync()
    {
        try
        {
            if (appsettings?.Value?.EmailTradePointRequestsConnection?.Enabled??false)
            {
                var licenseHashResponse = await baseIikoRequestService.GetAuthorizeLicenseHash(IikoChainUrl);
                if (licenseHashResponse != null && licenseHashResponse.LicenseInfo != null)
                {
                    var hashes = await baseIikoRequestService.GetAuthorizeHashes(
                        licenseHashResponse.LicenseInfo.LicenseHash,
                        licenseHashResponse.LicenseInfo.StateHash, IikoChainUrl);
                    var entityVersionResponse = await baseIikoRequestService.GetEntityVersion(IikoChainUrl);
                    if (_customers == null) _customers = await iikoCommonService.GetCustomers();
                    if (_nomenclatureItems == null)
                        _nomenclatureItems = await iikoCommonService.GetEntities(new NomenclatureItemsQueryObject
                            { InlcludeDeleted = false });
                    var mails = await emailService.GetExcelUnreadEmails(appsettings.Value
                        .EmailTradePointRequestsConnection);
                    if (mails.Any())
                    {
                        var messages = await emailService.GetMessagesFromEmails(mails,
                            appsettings.Value.EmailTradePointRequestsConnection);
                        messages = messages.Where(message => message.Attachments.ToArray().Length > 0).ToList();
                        foreach (var message in messages)
                        {
                            var senderName = message.From.Mailboxes.FirstOrDefault()?.Name ?? "Заказчик";
                            var senderEmail = message.From.Mailboxes.FirstOrDefault()?.Address;
                            if (senderEmail != null)
                                foreach (var attachment in message.Attachments)
                                    if (attachment is MimePart part && part.FileName.EndsWith(".xlsx"))
                                        try
                                        {
                                            var request =
                                                await tradepointProductRequestParserService
                                                    .ParseTradePointRequestsExcelFile(part, _nomenclatureItems,
                                                        _customers);
                                            request.LicenseHash = hashes.LicenseInfo.LicenseHash;
                                            request.StateHash = hashes.LicenseInfo.StateHash;
                                            request.EntityVersion = entityVersionResponse.EntitiesUpdate.Revision;
                                            var feedback = tradepointRequestFeedbackService
                                                .GetFeedbackTradepointProductRequest(request);
                                            if (!request.IsCorrect) throw new Exception(feedback);
                                            var xmlData = new ProductsRequestData(request);
                                            var result = await iikoCommonService
                                                .SaveOrUpdateProductRequestDocumentWithValidation(xmlData);
                                            var isSuccess =
                                                await iikoCommonService.SaveOrUpdateProductRequestDocument(xmlData,
                                                    result);
                                            await emailTradepointRequestHandlerService
                                                .SendAnswerEmailTradePointRequest(senderEmail, senderName,
                                                    $"Заявка успешно обработана. {feedback}",
                                                    EmailTradepointRequestStatusEnum.Success);
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                var currentPathProject =
                                                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                                                if (currentPathProject != null)
                                                {
                                                    var filesPath = Path.Combine(currentPathProject, "Files");
                                                    var exampleXlsx = Path.Combine(filesPath,
                                                        "Шаблон заказа кофейни на продукты.xlsx");
                                                    var mimeEntityExample =
                                                        new MimePart("application", "vnd.ms - excel")
                                                        {
                                                            FileName = "Тестовый заказ.xlsx",
                                                            Content = new MimeContent(File.OpenRead(exampleXlsx)),
                                                            ContentTransferEncoding = ContentEncoding.Base64
                                                        };
                                                    await emailTradepointRequestHandlerService
                                                        .SendAnswerEmailTradePointRequest(senderEmail,
                                                            senderName, ex.Message,
                                                            EmailTradepointRequestStatusEnum.ProcessingError,
                                                            mimeEntityExample);
                                                }
                                            }
                                            catch (Exception exInner)
                                            {
                                                logger.LogError(ex, "Ошибка отправки ошибки при отправки письма.");
                                                await emailService.SendSystemError(_itDepartmentEmail, 
                                                    $"Ошибка отправки ошибки при отправки письма. {exInner.Message}", 
                                                    appsettings.Value.EmailTradePointRequestsConnection);
                                                logger.LogError(exInner,
                                                    "Ошибка отправки ошибки при отправки письма.");
                                            }
                                        }
                        }

                        await emailService.MarkMessagesAsRead(mails,
                            appsettings.Value.EmailTradePointRequestsConnection);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await emailService.SendSystemError(_itDepartmentEmail, $"Не удалось обработать письма. {ex.Message}",
                appsettings?.Value?.EmailTradePointRequestsConnection);
            logger.LogError(ex, "не удалось обработать письма");
        }
    }
}