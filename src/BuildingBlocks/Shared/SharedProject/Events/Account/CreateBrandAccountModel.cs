namespace SharedProject.Events.Account;

public class CreateBrandAccountModel
{
    public Guid BrandId { get; set; }
    public string Code { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}