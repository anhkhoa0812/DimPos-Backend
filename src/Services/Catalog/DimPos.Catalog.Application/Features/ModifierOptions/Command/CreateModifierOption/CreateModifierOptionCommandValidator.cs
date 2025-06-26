using FluentValidation;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Command.CreateModifierOption;

public class CreateModifierOptionCommandValidator : AbstractValidator<CreateModifierOptionCommand>
{
    public CreateModifierOptionCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Tên của tùy chọn không được nhiều hơn 200 ký tự.");
        RuleFor(x => x.IsActive)
            .NotEmpty().WithMessage("Trạng thái của tùy chọn không được để trống.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của tùy chọn không được nhiều hơn 1000 ký tự.");
    }
}