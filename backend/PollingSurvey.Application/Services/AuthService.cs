using FluentValidation;
using PollingSurvey.Application.Common;
using PollingSurvey.Application.DTOs;
using PollingSurvey.Application.Interfaces;
using PollingSurvey.Application.Repositories;
using PollingSurvey.Domain.Entities;

namespace PollingSurvey.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterRequest> _validator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IValidator<RegisterRequest> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        // Validate request bằng FluentValidation
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return ServiceResult<RegisterResponse>.ValidationError(
                string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage))
            );
        }

        // Kiểm tra email đã tồn tại
        var existingByEmail = await _userRepository.GetUserByEmailAsync(request.Email);

        if (existingByEmail != null)
        {
            return ServiceResult<RegisterResponse>.Conflict(
                "Email is already registered.");
        }

        // Kiểm tra username đã tồn tại
        var existingByUsername = await _userRepository.GetUserByUsernameAsync(request.Username);

        if (existingByUsername != null)
        {
            return ServiceResult<RegisterResponse>.Conflict(
                "Username is already taken.");
        }

        // Tạo User mới
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password)
        };

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

        // Trả về thông tin user
        return ServiceResult<RegisterResponse>.Success(
            new RegisterResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            });
    }
}