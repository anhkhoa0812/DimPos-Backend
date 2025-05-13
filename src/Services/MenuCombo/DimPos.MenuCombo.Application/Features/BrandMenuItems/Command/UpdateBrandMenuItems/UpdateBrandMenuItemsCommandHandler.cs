// using DimPos.MenuCombo.Application.Services.Interface;
// using DimPos.MenuCombo.Domain.Models.Common;
// using DimPos.MenuCombo.Infrastructure.Persistence;
// using DimPos.MenuCombo.Infrastructure.Repositories.Interface;
// using Mediator;
//
// namespace DimPos.MenuCombo.Application.Features.BrandMenuItems.Command.UpdateBrandMenuItems;
//
// public class UpdateBrandMenuItemsCommandHandler : IRequestHandler<UpdateBrandMenuItemsCommand, ApiResponse>
// {
//     private readonly IUnitOfWork<MenuComboContext> _unitOfWork;
//     private readonly ILogger _logger;
//     private readonly IClaimService _claimService;
//     public UpdateBrandMenuItemsCommandHandler(IUnitOfWork<MenuComboContext> unitOfWork,
//         ILogger logger, IClaimService claimService)
//     {
//         _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
//     }
//     
//     public async ValueTask<ApiResponse> Handle(UpdateBrandMenuItemsCommand request, CancellationToken cancellationToken)
//     {
//         var brandId = _claimService.GetBrandId;
//         if (brandId == Guid.Empty)
//         {
//             throw new BadHttpRequestException("Không tìm thấy brandId");
//         }
//         var brandMenu = await _unitOfWork.GetRepository<Domain.Entities.BrandMenu>().SingleOrDefaultAsync(
//             predicate: x => x.BrandId == brandId && x.Id == request.BrandMenuId
//         );
//         if (brandMenu == null)
//         {
//             throw new BadHttpRequestException("Không tìm thấy BrandMenu");
//         }
//         
//     }
// }