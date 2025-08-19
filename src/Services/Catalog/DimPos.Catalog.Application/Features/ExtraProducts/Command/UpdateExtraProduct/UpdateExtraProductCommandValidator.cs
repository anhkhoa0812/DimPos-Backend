using FluentValidation;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Command.UpdateExtraProduct;

public class UpdateExtraProductCommandValidator : AbstractValidator<UpdateExtraProductCommand>
{
    public UpdateExtraProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(1).WithMessage("Tên của sản phẩm extra phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của sản phẩm extra không được vượt quá 200 ký tự");
        
        RuleFor(p => p.Description)
            .MaximumLength(1000).WithMessage("Mô tả của sản phẩm extra không được vượt quá 1000 ký tự");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá của sản phẩm extra phải lớn hơn 0");
    }
}