using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrationPractice.Domain;

public class User
{
    public long Id { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public int Age { get; set; }
    public required string RefreshToken { get; set; }
    public DateTimeOffset RefreshTokenExpiration { get; set; }
    
    public long? RoleId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; }
}