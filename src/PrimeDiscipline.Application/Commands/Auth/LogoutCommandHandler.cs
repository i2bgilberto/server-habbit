using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.Auth;

public sealed class LogoutCommandHandler(ISessionRepository sessionRepository)
    : IRequestHandler<LogoutCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await sessionRepository.RevokeAsync(request.Token, cancellationToken);
        return Result.Success(true);
    }
}
