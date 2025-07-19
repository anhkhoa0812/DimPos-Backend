using FluentValidation;

namespace DimPos.MenuCombo.Application.Features.BrandMenu.Command.UpdateBrandMenu;

public class UpdateBrandMenuCommandValidator : AbstractValidator<UpdateBrandMenuCommand>
{
    public UpdateBrandMenuCommandValidator()
    {
        RuleFor(x => x.BrandMenuId)
            .NotEqual(Guid.Empty).WithMessage("Id của Brand Menu không được để trống.")
            .NotNull().WithMessage("Id của Brand Menu không được để trống.")
            .NotEmpty().WithMessage("Id của Brand Menu không được để trống.");
        
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Tên menu không được vượt quá 200 ký tự");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả menu không được vượt quá 1000 ký tự");
    }
}