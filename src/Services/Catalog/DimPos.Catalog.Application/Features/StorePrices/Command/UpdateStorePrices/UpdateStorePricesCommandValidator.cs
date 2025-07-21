using FluentValidation;

namespace DimPos.Catalog.Application.Features.StorePrices.Command.UpdateStorePrices;

public class UpdateStorePricesCommandValidator : AbstractValidator<UpdateStorePricesCommand>
{
    public UpdateStorePricesCommandValidator()
    {
        RuleFor(x => x.StorePriceId)
            .NotEmpty().WithMessage("Id của giá cửa hàng không được để trống")
            .NotNull().WithMessage("Id của giá cửa hàng không được để trống")
            .NotEqual(Guid.Empty).WithMessage("Id của giá cửa hàng không được để trống");

        RuleFor(x => x.OverridePrice)
            .GreaterThan(0).WithMessage("Giá cửa hàng phải lớn hơn 0");
        RuleFor(x => x.CurrencyCode)
            .MaximumLength(10).WithMessage("Mã tiền tệ không được vượt quá 10 ký tự");
    }
}