using System.ComponentModel.DataAnnotations;

namespace SharedProject.Events.Account;

public class CreateBrandAccountResponseModel
{
    [Required]
    public Guid BrandId { get; set; }
    public Guid? AccountId { get; set; }
}