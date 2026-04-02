using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Application.Interfaces;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.Auth;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<LoginCommand, Result<SessionDto>>
{
    private static readonly TimeSpan SessionDuration = TimeSpan.FromDays(30);

    public async Task<Result<SessionDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByEmailAsync(
            request.Email.Trim().ToLowerInvariant(), cancellationToken);

        // Same error for wrong email and wrong password — avoid user enumeration
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<SessionDto>(Error.BusinessRule("Invalid email or password."));

        if (!user.IsActive)
            return Result.Failure<SessionDto>(Error.Forbidden("Account is disabled."));

        Session session = new()
        {
            UserId       = user.Id,
            Token        = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(SessionDuration),
        };

        await sessionRepository.CreateAsync(session, cancellationToken);

        return Result.Success(new SessionDto(
            user.Id,
            user.DisplayName,
            user.Email,
            session.Token,
            session.ExpiresAtUtc));
    }
}
