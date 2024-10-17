namespace RegistrationPractice.WebApi.Contracts.Models;

public class RefreshTokenModel
{
    public string? UserName { get; set; }
    public string? JwtToken { get; set; }
    public string? RefreshToken { get; set; }
}