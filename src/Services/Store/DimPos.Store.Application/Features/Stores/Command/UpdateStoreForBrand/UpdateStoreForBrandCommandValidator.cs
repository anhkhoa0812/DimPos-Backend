using FluentValidation;

namespace DimPos.Store.Application.Features.Stores.Command.UpdateStoreForBrand;

public class UpdateStoreForBrandCommandValidator : AbstractValidator<UpdateStoreForBrandCommand>
{
    public UpdateStoreForBrandCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Id cuả cửa hàng không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id cuả cửa hàng không được để trống.");
        RuleFor(x => x.StartingStoreCashLending)
            .GreaterThan(0).WithMessage("Số tiền đầu quỹ không được nhỏ hơn hoặc bằng 0.");
    }
}