using CorporateBrain.Domain.Entities;

namespace CorporateBrain.Application.Common.Interfaces;

public interface IJwtProvider
{
    string Generate(User user); 
}
