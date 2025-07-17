using FluentValidation;

namespace DimPos.Promotion.Application.Features.RuleAction.Command.UpdateRuleAction;

public class UpdateRuleActionCommandValidator : AbstractValidator<UpdateRuleActionCommand>
{
    public UpdateRuleActionCommandValidator()
    {
        RuleFor(x => x.PromotionRuleId)
            .NotEmpty().WithMessage("Id quy tắc khuyến mãi không được bỏ trống")
            .NotNull().WithMessage("Id quy tắc khuyến mãi không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Id quy tắc khuyến mãi không được là Guid.Empty");
        RuleFor(x => x.RuleActionId)
            .NotEmpty().WithMessage("Id hành động quy tắc khuyến mãi không được bỏ trống")
            .NotNull().WithMessage("Id hành động quy tắc khuyến mãi không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Id hành động quy tắc khuyến mãi không được là Guid.Empty");
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Giá trị không được bỏ trống")
            .NotNull().WithMessage("Giá trị không được bỏ trống");
        RuleFor(x => x.MaxDiscountAmountForPercentage)
            .GreaterThan(0).WithMessage("Giá trị của MaxDiscountAmountForPercentage phải lớn hơn hoặc bằng 0.");
    }
}