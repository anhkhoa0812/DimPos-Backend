using FluentValidation;

namespace DimPos.Inventory.Application.Features.InventoryStock.Command.UpdateQuantiyOfInventoryStock;

public class UpdateQuantityOfInventoryStockCommandValidator : AbstractValidator<UpdateQuantityOfInventoryStockCommand>
{
    public UpdateQuantityOfInventoryStockCommandValidator()
    {
        RuleFor(x => x.InventoryStockId)
            .NotEmpty().WithMessage("Id cuả kho không được để trống.")
            .NotNull().WithMessage("Id cuả kho không được để trống.")
            .NotEqual(Guid.Empty).WithMessage("Id cuả kho không được để trống.");
        RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Số lượng không được để trống.")
            .GreaterThanOrEqualTo(0).WithMessage("Số lượng phải lớn hơn hoặc bằng 0.");
        RuleFor(x => x.ReasonManualAdjustment)
            .NotEmpty().WithMessage("Lý do điều chỉnh kho không được để trống.")
            .NotNull().WithMessage("Lý do điều chỉnh kho không được để trống.")
            .MaximumLength(1000).WithMessage("Lý do điều chỉnh kho không được vượt quá 1000 ký tự.");
        RuleFor(x => x.Note)
            .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự.");
    }
}