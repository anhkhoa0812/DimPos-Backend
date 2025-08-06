using FluentValidation;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;

public class CreateModifierGroupsCommandValidator : AbstractValidator<CreateModifierGroupsCommand>
{
    public CreateModifierGroupsCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên của nhóm tùy chọn không được để trống.")
            .MinimumLength(1).WithMessage("Tên của nhóm tùy chọn không được ít hơn 1 ký tự.")
            .MaximumLength(200).WithMessage("Tên của nhóm tùy chọn không được nhiều hơn 200 ký tự.");
        RuleFor(x => x.SelectedType)
            .NotEmpty().WithMessage("Kiểu tùy chọn không được để trống.");
        RuleFor(x => x.IsActive)
            .NotEmpty().WithMessage("Trạng thái của nhóm tùy chọn không được để trống.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của nhóm tùy chọn không được nhiều hơn 1000 ký tự.");
        RuleFor(x => x.SelectedType)
            .IsInEnum();
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0.");
    }
}

public class CreateModifierOptionsValidator : AbstractValidator<CreateModifierOptions>
{
    public CreateModifierOptionsValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Tên của tùy chọn không được để trống.")
            .NotEmpty().WithMessage("Tên của tùy chọn không được để trống.")
            .MaximumLength(200).WithMessage("Tên của tùy chọn không được nhiều hơn 200 ký tự.");
        RuleFor(x => x.IsActive)
            .NotEmpty().WithMessage("Trạng thái của tùy chọn không được để trống.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của tùy chọn không được nhiều hơn 1000 ký tự.");
    }
}