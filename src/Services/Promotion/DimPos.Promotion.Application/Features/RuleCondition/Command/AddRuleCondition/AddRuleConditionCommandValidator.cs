using FluentValidation;

namespace DimPos.Promotion.Application.Features.RuleCondition.Command.AddRuleCondition;

public class AddRuleConditionCommandValidator : AbstractValidator<AddRuleConditionCommand>
{
    public AddRuleConditionCommandValidator()
    {
        RuleFor(x => x.PromotionRuleId)
            .NotEmpty().WithMessage("Id của luật khuyến mãi không được để trống.")
            .NotNull().WithMessage("Id của luật khuyến mãi không được để null.")
            .NotEqual(Guid.Empty).WithMessage("Id của luật khuyến mãi không được là Guid.Empty.");
        
        RuleFor(x => x.ConditionType)
            .IsInEnum().WithMessage("Loại điều kiện không hợp lệ.");

        RuleFor(x => x.Operator)
            .IsInEnum().WithMessage("Toán tử không hợp lệ.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Giá trị của điều kiện không được để trống.")
            .NotNull().WithMessage("Giá trị của điều kiện không được để trống.");
    }
}