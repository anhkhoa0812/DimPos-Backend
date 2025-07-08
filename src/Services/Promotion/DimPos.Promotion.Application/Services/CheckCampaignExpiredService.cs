// using DimPos.Promotion.Domain.Entities;
// using DimPos.Promotion.Domain.Enums;
// using DimPos.Promotion.Infrastructure.Persistence;
// using DimPos.Promotion.Infrastructure.Repositories.Interface;
//
// namespace DimPos.Promotion.Application.Services;
//
// public class CheckCampaignExpiredService(IUnitOfWork<PromotionContext> unitOfWork, ILogger logger) : BackgroundService
// {
//     
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         logger.Information("CheckCampaignExpiredService running at: {Time}", TimeUtil.GetCurrentSEATime());
//         var now = DateTime.Now;
//         var hours = 23 - now.Hour;
//         var minutes = 59 - now.Minute;
//         var seconds = 59 - now.Second;
//         var secondsTillMidnight = hours * 3600 + minutes * 60 + seconds;
//         
//         await Task.Delay(TimeSpan.FromSeconds(secondsTillMidnight), stoppingToken);
//         
//         try
//         {
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 var expiredCampaigns = await unitOfWork.GetRepository<Campaigns>().GetListAsync(
//                     predicate: x => TimeUtil.GetCurrentSEATime() > x.EndDate
//                 );
//                 foreach (var expiredCampaign in expiredCampaigns)
//                 {
//                     expiredCampaign.Status = ECampaignsStatus.Inactive;
//                 }
//
//                 unitOfWork.GetRepository<Campaigns>().UpdateRange(expiredCampaigns);
//                 
//                 await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
//             }
//         }
//         catch (Exception ex)
//         {
//             logger.Error("Error: " + ex.Message);
//             await ExecuteAsync(stoppingToken);
//         }
//     }
//     
//     public override Task StartAsync(CancellationToken cancellationToken)
//     {
//         logger.Warning("Worker STARTING: {Time}", TimeUtil.GetCurrentSEATime());
//         return base.StartAsync(cancellationToken);
//     }
//
//     public override Task StopAsync(CancellationToken cancellationToken)
//     {
//         logger.Warning("Worker STOPPING: {Time}", TimeUtil.GetCurrentSEATime());
//         return base.StopAsync(cancellationToken);
//     }
// }