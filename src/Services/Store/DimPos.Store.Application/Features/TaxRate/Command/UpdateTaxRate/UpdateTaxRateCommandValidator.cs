using FluentValidation;

namespace DimPos.Store.Application.Features.TaxRate.Command.UpdateTaxRate;

public class UpdateTaxRateCommandValidator : AbstractValidator<UpdateTaxRateCommand>
{
    public UpdateTaxRateCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Id của cửa hàng không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id của cửa hàng không đúng định dạng.");
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Tên thuế không được vượt quá 100 ký tự.");
        RuleFor(x => x.Rate)
            .GreaterThanOrEqualTo(0).WithMessage("Tỷ lệ thuế phải lớn hơn hoặc bằng 0.")
            .LessThanOrEqualTo(100).WithMessage("Tỷ lệ thuế phải nhỏ hơn hoặc bằng 100.");
    }
}