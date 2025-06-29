using FluentValidation;

namespace DimPos.Store.Application.Features.FinancialShiftConfig.Command.UpdateFinancialShiftConfig;

public class UpdateFinancialShiftConfigCommandValidator : AbstractValidator<UpdateFinancialShiftConfigCommand>
{
    public UpdateFinancialShiftConfigCommandValidator()
    {
        RuleFor(x => x.OpeningTime)
            .NotEqual(default(TimeOnly))
            .WithMessage("Thời gian mở cửa không được để trống.")
            .LessThan(x => x.ClosingTime)
            .WithMessage("Thời gian mở cửa phải nhỏ hơn thời gian đóng cửa.");
        RuleFor(x => x.ClosingTime)
            .NotEqual(default(TimeOnly))
            .WithMessage("Thời gian đóng cửa không được để trống.")
            .GreaterThan(x => x.OpeningTime)
            .WithMessage("Thời gian đóng cửa phải lớn hơn thời gian mở cửa.");
    }
}