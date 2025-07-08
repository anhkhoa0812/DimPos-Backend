using FluentValidation;

namespace DimPos.Catalog.Application.Features.Products.Commands.UpdateProducts;

public class UpdateProductsCommandValidator : AbstractValidator<UpdateProductsCommand>
{
    public UpdateProductsCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Id của sản phẩm không được để trống.");

        RuleFor(x => x.UpdateProducts)
            .NotNull().WithMessage("Thông tin cập nhật sản phẩm không được để trống.");

        RuleFor(x => x.UpdateProducts.Name)
            .MinimumLength(1).WithMessage("Tên của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của sản phẩm không được vượt quá 200 ký tự");

        RuleFor(x => x.UpdateProducts.Description)
            .MinimumLength(1).WithMessage("Mô tả của sản phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(1000).WithMessage("Mô tả của sản phẩm không được vượt quá 1000 ký tự");
    }
}