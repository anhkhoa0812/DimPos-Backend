using FluentValidation;

namespace DimPos.Promotion.Application.Features.Campaign.Command.UpdateCampaign;

public class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255).WithMessage("Tên chiến dịch không được vượt quá 255 ký tự.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả chiến dịch không được vượt quá 1000 ký tự.");
    }
}