namespace DimPos.Payment.Domain.Entities.Common.Interface;

public interface IDateTracking
{
    DateTime CreatedDate { get; set; }
    DateTime? LastModifiedDate { get; set; } 
}