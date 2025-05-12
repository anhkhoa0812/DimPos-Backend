using FluentValidation;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Command.CreateBrandMenu;

public class CreateBrandMenuCommandValidator : AbstractValidator<CreateBrandMenuCommand>
{
    public CreateBrandMenuCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên menu không được để trống")
            .MaximumLength(200).WithMessage("Tên menu không được vượt quá 200 ký tự");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả menu không được vượt quá 1000 ký tự");
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Loại menu không hợp lệ");
    }
}