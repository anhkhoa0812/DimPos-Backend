using FluentValidation;

namespace DimPos.Promotion.Application.Features.CampaignStore.Command;

public class CreateCampaignStoreCommandValidator : AbstractValidator<CreateCampaignStoreCommand>
{
    public CreateCampaignStoreCommandValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Id của chiến dịch không được để trống.");

        RuleFor(x => x.StoreIds)
            .NotEmpty().WithMessage("Danh sách Id cửa hàng không được để trống.")
            .Must(storeIds => storeIds.Count > 0).WithMessage("Danh sách Id cửa hàng phải có ít nhất một cửa hàng.");
    }
}