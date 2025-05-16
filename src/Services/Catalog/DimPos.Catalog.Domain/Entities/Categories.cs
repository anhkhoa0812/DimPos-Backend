using System.ComponentModel.DataAnnotations;
using DimPos.Catalog.Domain.Entities.Common;
using DimPos.Catalog.Domain.Enums;

namespace DimPos.Catalog.Domain.Entities;
/// <summary>
/// 
/// </summary>
public class Categories : EntityAuditBase<Guid>
{

    public string? Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; }
    
    public int? DisplayOrder { get; set; }
    public string? PictureUrl { get; set; }
    public bool HasChildCategory { get; set; }
     
    public ECategoryStatus Status { get; set; }
    
    public Guid? ParentId { get; set; }
    
    public Guid BrandId { get; set; }
    public virtual IEnumerable<Products> Products { get; set; } = new List<Products>();
}