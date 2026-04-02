using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Application.Interfaces;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.Auth;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        bool exists = await userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (exists)
            return Result.Failure<UserDto>(Error.Conflict($"Email '{request.Email}' is already registered."));

        User user = new()
        {
            Email        = request.Email.Trim().ToLowerInvariant(),
            DisplayName  = request.DisplayName.Trim(),
            Timezone     = request.Timezone,
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAtUtc = DateTime.UtcNow,
        };

        User created = await userRepository.CreateAsync(user, cancellationToken);

        return Result.Success(new UserDto(
            created.Id,
            created.Email,
            created.DisplayName,
            created.Timezone,
            created.CreatedAtUtc));
    }
}
