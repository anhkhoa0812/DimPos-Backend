using FluentValidation;

namespace DimPos.Promotion.Application.Features.PromotionRule.Command.CreatePromotionRule;

public class CreatePromotionRuleCommandValidator : AbstractValidator<CreatePromotionRuleCommand>
{
    public CreatePromotionRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().NotNull().WithMessage("Tên của luật khuyến mãi không được để trống.")
            .MaximumLength(100).WithMessage("Tên của luật khuyến mãi không được vượt quá 100 ký tự.");
        RuleFor(x => x.ShortDescription)
            .NotEmpty().NotNull().WithMessage("Mô tả ngắn của luật khuyến mãi không được để trống.")
            .MaximumLength(500).WithMessage("Mô tả ngắn của luật khuyến mãi không được vượt quá 200 ký tự.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của luật khuyến mãi không được vượt quá 1000 ký tự.");

        RuleFor(x => x.Priority)
            .NotNull().NotEmpty().WithMessage("Ưu tiên của luật khuyến mãi không được để trống.")
            .NotEmpty().WithMessage("Ưu tiên của luật khuyến mãi không được để trống.");

        RuleForEach(x => x.RuleConditions).SetValidator(new CreateRuleConditionRequestValidator());
        RuleFor(x => x.RuleActions).SetValidator(new CreateRuleActionRequestValidator());
    }
}
public class CreateRuleConditionRequestValidator : AbstractValidator<CreateRuleConditionRequest>
{
    public CreateRuleConditionRequestValidator()
    {
        RuleFor(x => x.ConditionType)
            .IsInEnum().WithMessage("Loại điều kiện không hợp lệ.");

        RuleFor(x => x.Operator)
            .IsInEnum().WithMessage("Toán tử không hợp lệ.");

        RuleFor(x => x.Value)
            .NotEmpty().NotNull().WithMessage("Giá trị của điều kiện không được để trống.");
    }
}
public class CreateRuleActionRequestValidator : AbstractValidator<CreateRuleActionRequest>
{
    public CreateRuleActionRequestValidator()
    {
        RuleFor(x => x.ActionType)
            .IsInEnum().WithMessage("Loại hành động không hợp lệ.");

        RuleFor(x => x.Value)
            .NotEmpty().NotNull().WithMessage("Giá trị của hành động không được để trống.");
        
        RuleFor(x => x.MaxDiscountAmountForPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Số tiền giảm giá tối đa cho phần trăm phải lớn hơn hoặc bằng 0.");
    }
}