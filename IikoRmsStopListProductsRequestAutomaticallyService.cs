using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeForView
{
    public class IikoRmsStopListProductsRequestAutomaticallyService(ILogger<IikoRmsStopListProductsRequestAutomaticallyService> logger,
            IOptions<AppSettings> appSettings, IServiceProvider serviceProvider) : TaskManager(logger)
    {
        protected override async Task RunTaskAsync()
        {
            try
            {
                if (appSettings.Value.IikoRmsStopListProductsRequestsConfiguration != null &&
                    appSettings.Value.IikoRmsStopListProductsRequestsConfiguration.Enabled)
                {
                    using var scope = serviceProvider.CreateScope();
                    var iikoRmsService = scope.ServiceProvider.GetRequiredService<IIikoRmsService>();
                    var baseIikoRequestService = scope.ServiceProvider.GetRequiredService<IBaseIikoRequestService>();
                    var iikoRmsStopListProductsParserService =
                        scope.ServiceProvider.GetRequiredService<IIikoRmsStopListProductsParserService>();
                    var iikoRmsItemRepository = scope.ServiceProvider.GetRequiredService<IikoRmsItemRepository>();
                    var iikoRmsItems = await iikoRmsItemRepository.GetAll();
                    var stopListsProductsRequest = new StopListsProductsRequest();
                    var nowDate = DateTime.Now;
                    var previosDate = nowDate.AddDays(-6);
                    previosDate = previosDate.Date;
                    stopListsProductsRequest.FromTime = previosDate;
                    stopListsProductsRequest.ToTime = nowDate;
                    foreach (var iikoRmsItem in iikoRmsItems)
                    {
                        try
                        {
                            var url = new Uri(iikoRmsItem.Url);
                            stopListsProductsRequest.UpdateClientCallId();
                            var licenseHashResponse = await baseIikoRequestService.GetAuthorizeLicenseHash(url, true);
                            if (licenseHashResponse.LicenseInfo != null)
                            {
                                var hashes = await baseIikoRequestService.GetAuthorizeHashes(
                                    licenseHashResponse.LicenseInfo.LicenseHash,
                                    licenseHashResponse.LicenseInfo.StateHash, url, true);
                                var entityVersionResponse = await baseIikoRequestService.GetEntityVersion(url, true);
                                if (hashes.LicenseInfo != null)
                                {
                                    stopListsProductsRequest.LicenseHash = hashes.LicenseInfo.LicenseHash;
                                    stopListsProductsRequest.StateHash = hashes.LicenseInfo.StateHash;
                                }

                                stopListsProductsRequest.EntitiesVersion = entityVersionResponse.EntitiesUpdate.Revision;
                            }
                            var res = await iikoRmsService.GetIikoRmsInfoStopListsProductsForPeriod(stopListsProductsRequest,
                                url);
                            await iikoRmsStopListProductsParserService.ParseStopListProduct(res, iikoRmsItem.Id);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Не удалось обработать rms {iikoRmsItem.Url}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "не удалось обработать информацию по продуктам в стоп-листах");
            }
        }
    }
}
