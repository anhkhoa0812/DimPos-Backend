using DimPos.Payment.Domain.Models.Common;
using DimPos.Payment.Domain.Models.SystemPaymentMethod;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Payment.Application.Features.SystemPaymentMethod.Query.GetSystemPaymentMethodById;

public class GetSystemPaymentMethodByIdQueryHandler : IRequestHandler<GetSystemPaymentMethodByIdQuery, ApiResponse>
{
    private readonly IUnitOfWork<PaymentContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public GetSystemPaymentMethodByIdQueryHandler(IUnitOfWork<PaymentContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async ValueTask<ApiResponse> Handle(GetSystemPaymentMethodByIdQuery request, CancellationToken cancellationToken)
    {
        var systemPaymentMethod = await _unitOfWork.GetRepository<Domain.Entities.SystemPaymentMethods>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.SystemPaymentMethodId
        );
        if (systemPaymentMethod == null)
        {
            throw new BadHttpRequestException("Không tìm thấy phương thức thanh toán hệ thống");
        }
        
        var response = new GetSystemPaymentMethodByIdResponse()
        {
            Id = systemPaymentMethod.Id,
            Code = systemPaymentMethod.Code,
            Name = systemPaymentMethod.Name,
            Type = systemPaymentMethod.Type,
            LogoUrl = systemPaymentMethod.LogoUrl,
            Description = systemPaymentMethod.Description,
            IsGloballyActive = systemPaymentMethod.IsGloballyActive,
            ConfigurationSchema = systemPaymentMethod.ConfigurationSchema,
            CreatedDate = systemPaymentMethod.CreatedDate,
            LastModifiedDate = systemPaymentMethod.LastModifiedDate
        };
        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Lấy thông tin phương thức thanh toán hệ thống thành công",
            Data = response
        };
        
    }
}