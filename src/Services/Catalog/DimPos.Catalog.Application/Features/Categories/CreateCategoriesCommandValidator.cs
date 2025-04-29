using FluentValidation;

namespace DimPos.Catalog.Application.Features.Categories;

public class CreateCategoriesCommandValidator : AbstractValidator<CreateCategoriesCommand>
{
    public CreateCategoriesCommandValidator()
    {
        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Mã danh mục không được quá 50 ký tự");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên của danh mục không được để trống")
            .MinimumLength(1).WithMessage("Tên của danh mục không được ít hơn 1 ký tự")
            .MaximumLength(200).WithMessage("Tên của danh mục không được quá 200 ký tự");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả của danh mục không được quá 1000 ký tự");
        
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Loại danh mục không được để trống");
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái không được để trống");
        
    }
}