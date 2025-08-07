using FluentValidation;

namespace DimPos.Catalog.Application.Features.ModifierOptions.Command.UpdateModifierOptions;

public class UpdateModifierOptionsCommandValidator : AbstractValidator<UpdateModifierOptionsCommand>
{
    public UpdateModifierOptionsCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id của tùy chọn không được để trống.");
        RuleFor(x => x.UpdateModifierOptions).SetValidator(new UpdateModifierOptionsRequestValidator());
    }
}
public class UpdateModifierOptionsRequestValidator : AbstractValidator<UpdateModifierOptionsRequest>
{
    public UpdateModifierOptionsRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên của tùy chọn không được để trống.")
            .MaximumLength(200).WithMessage("Tên của tùy chọn không được nhiều hơn 200 ký tự.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của tùy chọn không được nhiều hơn 1000 ký tự.");
        RuleFor(x => x.PriceDelta)
            .GreaterThanOrEqualTo(0).WithMessage("Giá trị thay đổi phải lớn hơn hoặc bằng 0.");
    }
}