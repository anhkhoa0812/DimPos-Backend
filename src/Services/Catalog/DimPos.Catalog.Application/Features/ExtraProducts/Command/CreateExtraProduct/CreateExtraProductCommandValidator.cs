using FluentValidation;

namespace DimPos.Catalog.Application.Features.ExtraProducts.Command.CreateExtraProduct;

public class CreateExtraProductCommandValidator : AbstractValidator<CreateExtraProductCommand>
{
    public CreateExtraProductCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotNull().WithMessage("Mã của sản phẩm extra không được bỏ trống")
            .NotEmpty().WithMessage("Mã của sản phẩm extra không được bỏ trống")
            .MinimumLength(1).WithMessage("Mã của sản extra phẩm phải có ít nhất 1 ký tự")
            .MaximumLength(50).WithMessage("Mã của sản extra phẩm không được vượt quá 50 ký tự");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên của sản phẩm extra không được bỏ trống")
            .MinimumLength(1).WithMessage("Tên của sản phẩm extra phải có ít nhất 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của sản phẩm extra không được vượt quá 200 ký tự");
        
        RuleFor(p => p.Description)
            .MaximumLength(1000).WithMessage("Mô tả của sản phẩm extra không được vượt quá 1000 ký tự");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá của sản phẩm extra phải lớn hơn 0")
            .NotNull().WithMessage("Giá của sản phẩm không extra được bỏ trống")
            .NotEmpty().WithMessage("Giá của sản phẩm không extra được bỏ trống");
        RuleFor(p => p.Sku)
            .MaximumLength(255).WithMessage("Mã SKU của sản phẩm extra không được vượt quá 255 ký tự");
        RuleForEach(x => x.ProductImages).SetValidator(new CreateExtraProductImagesValidator());

    }
}
public class CreateExtraProductImagesValidator : AbstractValidator<CreateExtraProductImages>
{
    private static readonly string[] _allowedExtensions = new[]
    {
        ".jpeg", ".png", ".jpg", ".gif", ".bmp", ".webp"
    };
    public CreateExtraProductImagesValidator()
    {
        RuleFor(x => x.Image)
            .Cascade(CascadeMode.Stop)
            .Must(file =>
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                return _allowedExtensions.Contains(extension);
            }).WithMessage("Chỉ các định dạng tệp .jpeg, .png, .jpg, .gif, .bmp, .webp  được phép tải lên.");
    }
}