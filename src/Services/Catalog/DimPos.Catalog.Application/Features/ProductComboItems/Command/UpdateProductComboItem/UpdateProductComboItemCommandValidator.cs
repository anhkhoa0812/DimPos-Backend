using FluentValidation;

namespace DimPos.Catalog.Application.Features.ProductComboItems.Command.UpdateProductComboItem;

public class UpdateProductComboItemCommandValidator : AbstractValidator<UpdateProductComboItemCommand>
{
    public UpdateProductComboItemCommandValidator()
    {
        RuleFor(x => x.ProductComboItemId)
            .NotEmpty().WithMessage("Id của sản phẩm combo không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id của sản phẩm combo không được để trống.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng sản phẩm trong combo phải lớn hơn 0.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0.");
    }
}