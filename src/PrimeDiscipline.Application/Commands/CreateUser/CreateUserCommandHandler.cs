using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.CreateUser;

public sealed class CreateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        bool alreadyExists = await userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (alreadyExists)
            return Result.Failure<UserDto>(Error.Conflict($"A user with email '{request.Email}' already exists."));

        User newUser = new()
        {
            Email       = request.Email.Trim().ToLowerInvariant(),
            DisplayName = request.DisplayName.Trim(),
            Timezone    = request.Timezone,
            CreatedAtUtc = DateTime.UtcNow,
        };

        User created = await userRepository.CreateAsync(newUser, cancellationToken);

        UserDto dto = new(
            created.Id,
            created.Email,
            created.DisplayName,
            created.Timezone,
            created.CreatedAtUtc);

        return Result.Success(dto);
    }
}
