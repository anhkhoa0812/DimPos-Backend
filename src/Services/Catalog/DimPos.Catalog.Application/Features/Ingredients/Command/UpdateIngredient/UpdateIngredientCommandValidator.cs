using FluentValidation;

namespace DimPos.Catalog.Application.Features.Ingredients.Command.UpdateIngredient;

public class UpdateIngredientCommandValidator : AbstractValidator<UpdateIngredientCommand>
{
    public UpdateIngredientCommandValidator()
    {
        RuleFor(x => x.IngredientId)
            .NotEmpty().WithMessage("Id của thành phần không được để trống.")
            .NotNull().WithMessage("Id của thành phần không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id của thành phần không được để trống.");
        
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Tên thành phần không được vượt quá 200 ký tự.");
        
        RuleFor(x => x.MeasureUnit)
            .MaximumLength(50).WithMessage("Đơn vị đo lường không được vượt quá 50 ký tự.");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Mô tả không được vượt quá 1000 ký tự.");
    }
}