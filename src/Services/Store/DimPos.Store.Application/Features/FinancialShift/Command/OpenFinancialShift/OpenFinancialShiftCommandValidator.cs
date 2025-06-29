using FluentValidation;

namespace DimPos.Store.Application.Features.FinancialShift.Command.OpenFinancialShift;

public class OpenFinancialShiftCommandValidator : AbstractValidator<OpenFinancialShiftCommand>
{
    public OpenFinancialShiftCommandValidator()
    {
        RuleFor(x => x.OpeningCashActual)
            .GreaterThan(0).WithMessage("Số tiền mở ca phải lớn hơn 0");
        
        RuleFor(x => x.OpeningDifferenceReason)
            .MaximumLength(1000).WithMessage("Lý do chênh lệch mở ca không được vượt quá 1000 ký tự");
        
    }
}