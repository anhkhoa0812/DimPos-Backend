using FluentValidation;

namespace DimPos.Promotion.Application.Features.RuleCondition.Command.UpdateRuleCondition;

public class UpdateRuleConditionCommandValidator : AbstractValidator<UpdateRuleConditionCommand>
{
    public UpdateRuleConditionCommandValidator()
    {
        RuleFor(x => x.PromotionRuleId)
            .NotNull().WithMessage("Id quy tắc khuyến mãi không được bỏ trống")
            .NotEmpty().WithMessage("Id quy tắc khuyến mãi không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Id quy tắc khuyến mãi không được là Guid.Empty");
        RuleFor(x => x.RuleConditionId)
            .NotNull().WithMessage("Id điều kiện quy tắc khuyến mãi không được bỏ trống")
            .NotEmpty().WithMessage("Id điều kiện quy tắc khuyến mãi không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Id điều kiện quy tắc khuyến mãi không được là Guid.Empty");
        RuleFor(x => x.Operator)
            .IsInEnum().WithMessage("Toán tử không hợp lệ");
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Giá trị không được bỏ trống")
            .NotNull().WithMessage("Giá trị không được bỏ trống");
    }
}