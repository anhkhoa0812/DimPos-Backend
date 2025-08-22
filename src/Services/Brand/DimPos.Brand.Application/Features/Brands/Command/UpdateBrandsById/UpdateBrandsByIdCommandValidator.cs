using FluentValidation;

namespace DimPos.Brand.Application.Features.Brands.Command.UpdateBrandsById;

public class UpdateBrandsByIdCommandValidator : AbstractValidator<UpdateBrandsByIdCommand>
{
    public UpdateBrandsByIdCommandValidator()
    {
        RuleFor(x => x.BrandId)
            .NotNull().WithMessage("Id của thương hiệu không được bỏ trống")
            .NotEmpty().WithMessage("Id của thương hiệu không được bỏ trống")
            .NotEqual(Guid.Empty).WithMessage("Id của thương hiệu không được là Guid.Empty");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên của thương hiệu không được bỏ trống")
            .MaximumLength(200).WithMessage("Tên của thương hiệu không được vượt quá 200 ký tự");
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ của thương hiệu không được bỏ trống")
            .MaximumLength(1000).WithMessage("Địa chỉ của thương hiệu không được vượt quá 1000 ký tự");
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Số điện thoại của thương hiệu không được bỏ trống")
            .MaximumLength(20).WithMessage("Số điện thoại của thương hiệu không được vượt quá 20 ký tự");
    }
}