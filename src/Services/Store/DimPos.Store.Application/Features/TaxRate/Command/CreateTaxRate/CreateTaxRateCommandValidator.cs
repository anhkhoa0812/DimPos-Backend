using FluentValidation;

namespace DimPos.Store.Application.Features.TaxRate.Command.CreateTaxRate;

public class CreateTaxRateCommandValidator : AbstractValidator<CreateTaxRateCommand>
{
    public CreateTaxRateCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Id của cửa hàng không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id của cửa hàng không đúng định dạng.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên thuế không được để trống.")
            .MaximumLength(100).WithMessage("Tên thuế không được vượt quá 100 ký tự.");
        RuleFor(x => x.Rate)
            .NotEmpty().WithMessage("Tỷ lệ thuế không được để trống.")
            .GreaterThanOrEqualTo(0).WithMessage("Tỷ lệ thuế phải lớn hơn hoặc bằng 0.")
            .LessThanOrEqualTo(100).WithMessage("Tỷ lệ thuế phải nhỏ hơn hoặc bằng 100.");
    }
}