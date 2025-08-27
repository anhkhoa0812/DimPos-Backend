using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Models.Common;
using DimPos.Payment.Infrastructure.Persistence;
using DimPos.Payment.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Payment.Application.Features.SystemPaymentMethod.Command.UpdateSystemPaymentMethod;

public class UpdateSystemPaymentMethodCommandHandler : IRequestHandler<UpdateSystemPaymentMethodCommand, ApiResponse>
{
    private readonly IUnitOfWork<PaymentContext> _unitOfWork;
    private readonly ILogger _logger;
    
    public UpdateSystemPaymentMethodCommandHandler(IUnitOfWork<PaymentContext> unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async ValueTask<ApiResponse> Handle(UpdateSystemPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var systemPaymentMethod = await _unitOfWork.GetRepository<SystemPaymentMethods>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.SystemPaymentMethodId
        );

        if (systemPaymentMethod == null)
        {
            throw new BadHttpRequestException("Không tìm thấy phương thức thanh toán hệ thống");
        }
        
        systemPaymentMethod.Name = request.Name ?? systemPaymentMethod.Name;
        systemPaymentMethod.Description = request.Description ?? systemPaymentMethod.Description;
        systemPaymentMethod.IsGloballyActive = request.IsGloballyActive ?? systemPaymentMethod.IsGloballyActive;
        
        _unitOfWork.GetRepository<SystemPaymentMethods>().UpdateAsync(systemPaymentMethod);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (!isSuccess)
        {
            throw new BadHttpRequestException("Cập nhật phương thức thanh toán hệ thống không thành công");
        }

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Cập nhật phương thức thanh toán hệ thống thành công",
            Data = systemPaymentMethod.Id
        };
    }
}