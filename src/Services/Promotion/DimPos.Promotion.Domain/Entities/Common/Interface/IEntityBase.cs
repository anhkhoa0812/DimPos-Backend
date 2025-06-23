namespace DimPos.Promotion.Domain.Entities.Common.Interface;

public interface IEntityBase<T>
{
    T Id { get; set; }
}