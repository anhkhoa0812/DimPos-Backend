using FluentValidation;

namespace DimPos.Catalog.Application.Features.RecipeItems.Command.CreateRecipeItem;

public class CreateRecipeItemCommandValidator : AbstractValidator<CreateRecipeItemCommand>
{
    public CreateRecipeItemCommandValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEqual(Guid.Empty).WithMessage("Id của biến thể sản phẩm không được để trống.")
            .NotEmpty().WithMessage("Id của biến thể sản phẩm không được để trống.")
            .NotNull().WithMessage("Id của biến thể sản phẩm không được để trống.");
        RuleFor(x => x.IngredientId)
            .NotEqual(Guid.Empty).WithMessage("Id của thành phần không được để trống.")
            .NotEmpty().WithMessage("Id của thành phần không được để trống.")
            .NotNull().WithMessage("Id của thành phần không được để trống.");
        RuleFor(x => x.Quantity)
            .NotEmpty().WithMessage("Số lượng không được để trống.")
            .NotNull().WithMessage("Số lượng không được để trống.")
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
    }
}