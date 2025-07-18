using FluentValidation;

namespace DimPos.Catalog.Application.Features.RecipeItems.Command.UpdateRecipeItem;

public class UpdateRecipeItemCommandValidator : AbstractValidator<UpdateRecipeItemCommand>
{
    public UpdateRecipeItemCommandValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotNull().WithMessage("Id của biến thể sản phẩm không được để trống.")
            .NotEmpty().WithMessage("Id của biến thể sản phẩm không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id của biến thể sản phẩm không được để trống.");
        
        RuleFor(x => x.RecipeItemId)
            .NotNull().WithMessage("Id của thành phần công thức không được để trống.")
            .NotEmpty().WithMessage("Id của thành phần công thức không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id của thành phần công thức không được để trống.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng thành phần công thức phải lớn hơn 0.");
    }
}