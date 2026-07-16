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
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IValidator<RegisterRequest> validator,
        IValidator<LoginRequest> loginValidator,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
        _loginValidator = loginValidator;
        _jwtService = jwtService;
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

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // Validate request bằng FluentValidation
        var validationResult = await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return ServiceResult<LoginResponse>.ValidationError(
                string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage))
            );
        }

        // Tìm user theo email hoặc username
        var user = await _userRepository.GetUserByEmailAsync(request.UsernameOrEmail)
                   ?? await _userRepository.GetUserByUsernameAsync(request.UsernameOrEmail);

        if (user == null)
        {
            return ServiceResult<LoginResponse>.Unauthorized("Invalid username/email or password.");
        }

        // Kiểm tra mật khẩu bằng BCrypt
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return ServiceResult<LoginResponse>.Unauthorized("Invalid username/email or password.");
        }

        // Sinh JWT token
        var (token, expiresAt) = _jwtService.GenerateToken(user);

        return ServiceResult<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = expiresAt
        });
    }
}
