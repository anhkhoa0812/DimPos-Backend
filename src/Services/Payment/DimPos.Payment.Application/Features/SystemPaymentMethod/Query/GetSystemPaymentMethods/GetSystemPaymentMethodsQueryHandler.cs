using DimPos.Payment.Application.Services.Interface;
using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Models.Common;
using DimPos.Payment.Domain.Models.SystemPaymentMethod;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Payment.Application.Features.SystemPaymentMethod.Query.GetSystemPaymentMethods;

public class GetSystemPaymentMethodsQueryHandler : IRequestHandler<GetSystemPaymentMethodsQuery, ApiResponse>
{
    private readonly IUnitOfWork<PaymentContext> _unitOfWork;
    private readonly ILogger _logger;
    public GetSystemPaymentMethodsQueryHandler(IUnitOfWork<PaymentContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async ValueTask<ApiResponse> Handle(GetSystemPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var systemPaymentMethods = await _unitOfWork.GetRepository<SystemPaymentMethods>().GetPagingListAsync(
            selector: x => new GetSystemPaymentMethodsResponse()
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Type = x.Type,
                LogoUrl = x.LogoUrl,
                Description = x.Description,
                IsGloballyActive = x.IsGloballyActive,
                ConfigurationSchema = x.ConfigurationSchema,
                CreatedDate = x.CreatedDate,
                LastModifiedDate = x.LastModifiedDate
            },
            predicate: x => string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name),
            page: request.Page,
            size: request.Size,
            sortBy: request.SortBy ?? "CreatedDate",
            isAsc: request.IsAsc
        );
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy danh sách phương thức thanh toán hệ thống thành công",
            Data = systemPaymentMethods
        };
    }
}