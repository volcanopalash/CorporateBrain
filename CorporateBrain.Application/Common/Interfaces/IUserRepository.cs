using CorporateBrain.Domain.Entities;

namespace CorporateBrain.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string id);
    Task AddAsync(User user);
    Task UpdateAsync(User user); // For later
}
