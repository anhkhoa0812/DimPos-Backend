using FluentValidation;

namespace DimPos.Promotion.Application.Features.PromotionRule.Command.UpdatePromotionRule;

public class UpdatePromotionRuleCommandValidator : AbstractValidator<UpdatePromotionRuleCommand>
{
    public UpdatePromotionRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Tên của luật khuyến mãi không được vượt quá 100 ký tự.");
        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Mô tả ngắn của luật khuyến mãi không được vượt quá 200 ký tự.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của luật khuyến mãi không được vượt quá 1000 ký tự.");
        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Ưu tiên của luật khuyến mãi không được để trống.");
    }
}