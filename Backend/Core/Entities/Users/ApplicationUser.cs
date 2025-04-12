namespace Backend.Core.Entities;

public class ApplicationUser
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Email { get; set; } = null!;

    public List<Survey> Surveys { get; set; } = new();
}