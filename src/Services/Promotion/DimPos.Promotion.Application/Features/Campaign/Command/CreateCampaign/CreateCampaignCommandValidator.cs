using DimPos.Promotion.Infrastructure.Utils;
using FluentValidation;

namespace DimPos.Promotion.Application.Features.Campaign.Command.CreateCampaign;

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên chiến dịch không được để trống.")
            .NotNull().WithMessage("Tên chiến dịch không được để trống.")
            .MaximumLength(255).WithMessage("Tên chiến dịch không được vượt quá 255 ký tự.");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả chiến dịch không được vượt quá 1000 ký tự.");
        
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Ngày bắt đầu không được để trống.")
            .NotNull().WithMessage("Ngày bắt đầu không được để trống.")
            .GreaterThan(TimeUtil.GetCurrentSEATime()).WithMessage("Ngày bắt đầu phải lớn hơn ngày hiện tại.");
        
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Ngày kết thúc không được để trống.")
            .NotNull().WithMessage("Ngày kết thúc không được để trống.")
            .GreaterThan(x => x.StartDate).WithMessage("Ngày kết thúc phải lớn hơn ngày bắt đầu.");
        
        RuleFor(x => x.Channel)
            .IsInEnum().WithMessage("Kênh chiến dịch không hợp lệ.");
        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Ưu tiên chiến dịch không được để trống.")
            .NotNull().WithMessage("Ưu tiên chiến dịch không được để trống.");
        
        RuleFor(x => x.MaxTotalUsageLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Giới hạn sử dụng tối đa phải lớn hơn hoặc bằng 0.")
            .When(x => x.MaxTotalUsageLimit.HasValue);
        RuleFor(x => x.MaxUsagePerCustomerLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Giới hạn sử dụng tối đa cho mỗi khách hàng phải lớn hơn hoặc bằng 0.")
            .When(x => x.MaxUsagePerCustomerLimit.HasValue);
    }
}