using FluentValidation;

namespace DimPos.Catalog.Application.Features.Ingredients.Command.CreateIngredient;

public class CreateIngredientCommandValidator : AbstractValidator<CreateIngredientCommand>
{
    public CreateIngredientCommandValidator()
    {
        RuleFor(x => x.Code)
            .MaximumLength(50)
            .WithMessage("Mã thành phần không được vượt quá 50 ký tự.");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên thành phần không được để trống.")
            .NotNull().WithMessage("Tên thành phần không được để trống.")
            .MaximumLength(200).WithMessage("Tên thành phần không được vượt quá 200 ký tự.");
        
        RuleFor(x => x.MeasureUnit)
            .NotEmpty().WithMessage("Đơn vị đo lường không được để trống.")
            .NotNull().WithMessage("Đơn vị đo lường không được để trống.")
            .MaximumLength(50).WithMessage("Đơn vị đo lường không được vượt quá 50 ký tự.");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Mô tả không được vượt quá 1000 ký tự.");
    }
}