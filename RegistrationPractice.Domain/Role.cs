namespace RegistrationPractice.Domain;

public class Role
{
    public long Id { get; set; }
    public required string RoleName { get; set; }

    public List<User> Users { get; set; } = [];
}