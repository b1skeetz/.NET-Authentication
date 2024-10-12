namespace RegistrationPractice.WebApi.Contracts.Models;

public class AuthenticationResponse
{
    public required string UserName { get; set; }
    public required string Token { get; set; }
}