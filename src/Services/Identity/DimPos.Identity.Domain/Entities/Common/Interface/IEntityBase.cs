namespace DimPos.Identity.Domain.Entities.Common.Interface;

public interface IEntityBase<T>
{
    T Id { get; set; }
}