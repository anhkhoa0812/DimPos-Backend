using FluentValidation;

namespace DimPos.Catalog.Application.Features.ModifierGroups.Command.UpdateModifierGroups;

public class UpdateModifierGroupsCommandValidator : AbstractValidator<UpdateModifierGroupsCommand>
{
    public UpdateModifierGroupsCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id của nhóm tùy chọn không được để trống.");
        RuleFor(x => x.UpdateModifierGroupsRequest).SetValidator(new UpdateModifierGroupsRequestValidator());
    }
}
public class UpdateModifierGroupsRequestValidator : AbstractValidator<UpdateModifierGroupsRequest>
{
    public UpdateModifierGroupsRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(1).WithMessage("Tên của nhóm tùy chọn không được ít hơn 1 ký tự.")
            .MaximumLength(200).WithMessage("Tên của nhóm tùy chọn không được nhiều hơn 200 ký tự.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của nhóm tùy chọn không được nhiều hơn 1000 ký tự.");
        RuleFor(x => x.SelectedType)
            .IsInEnum().WithMessage("Loại đã chọn không hợp lệ.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự hiển thị phải lớn hơn hoặc bằng 0.");
        RuleForEach(x => x.ModifierOptions).SetValidator(new UpdateModifierOptionsValidator());
    }
}

public class UpdateModifierOptionsValidator : AbstractValidator<UpdateModifierOptions>
{
    public UpdateModifierOptionsValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên của tùy chọn không được để trống.")
            .MaximumLength(200).WithMessage("Tên của tùy chọn không được nhiều hơn 200 ký tự.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của tùy chọn không được nhiều hơn 1000 ký tự.");
    }
}