using CorporateBrain.Domain.Common;

namespace CorporateBrain.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    // we will add PasswordHash and PasswordSalt later for security

    public User(string firstName, string lastName, string email)
    {
        
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

}
