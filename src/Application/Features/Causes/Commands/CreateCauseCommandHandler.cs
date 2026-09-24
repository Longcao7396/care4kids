using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Features.Causes.Commands;

namespace GiveAID.Application.Features.Causes.Commands;

/// <summary>
/// Handler for CreateCauseCommand.
/// </summary>
public class CreateCauseCommandHandler : IRequestHandler<CreateCauseCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCauseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCauseCommand request, CancellationToken cancellationToken)
    {
        var cause = new Domain.Entities.Cause
        {
            CauseCode = request.CauseCode,
            CauseName = request.CauseName,
            Description = request.Description,
            Icon = request.Icon,
            TargetAmount = request.TargetAmount,
            ParentCauseId = request.ParentCauseId,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };

        _context.Causes.Add(cause);
        await _context.SaveChangesAsync(cancellationToken);

        return cause.CauseId;
    }
}
