using System.ComponentModel.DataAnnotations;
using DimPos.Catalog.Domain.Entities.Common;

namespace DimPos.Catalog.Domain.Entities;
/// <summary>
/// 
/// </summary>
public class Categories : EntityAuditBase<Guid>
{

    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    
    public int? DisplayOrder { get; set; }
    public string? PictureUrl { get; set; }
    public bool? HasChildCategory { get; set; } = false;
     
    public int? Status { get; set; } = 1;
    
    public Guid? ParentId { get; set; }
    
    public Guid? BrandId { get; set; }
    
    public virtual Categories? Parent { get; set; }
    
    public virtual IEnumerable<Categories>? Childrens { get; set; } = new List<Categories>();
    public virtual IEnumerable<Products> Products { get; set; } = new List<Products>();
}