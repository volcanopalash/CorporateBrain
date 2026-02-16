using CorporateBrain.Application;
using CorporateBrain.Application.Common.Interfaces;
using CorporateBrain.Application.DTOs;
using CorporateBrain.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CorporateBrain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateUserDto> _validator;

    public UsersController(IUserRepository userRepository, IUnitOfWork unitOfWork, IValidator<CreateUserDto> validator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto request)
    {
        // 1. Validate the Request
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // 1. Map DTO to Entity
        var user = new User(request.FirstName, request.LastName, request.Email);

        // 2. Add to Repository (Pending state)
        await _userRepository.AddAsync(user);

        // 3. Save Changes (Commit to DB)
        await _unitOfWork.SaveChangesAsync();
        
        // 4. Return Success
        return Ok(user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Map Entity to DTO (Hide internals)
        var userDto = new UserDto(user.Id, user.FirstName, user.LastName, user.Email);
        return Ok(userDto);
    }
}