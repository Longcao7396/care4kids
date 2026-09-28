using MediatR;

namespace GiveAID.Application.Features.Faqs.Commands.Delete;

/// <summary>
/// Handler for DeleteFaqCommand.
/// </summary>
public class DeleteFaqCommandHandler : IRequestHandler<DeleteFaqCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteFaqCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = await _context.Faqs.FindAsync(new object[] { request.FaqId }, cancellationToken);

        if (faq == null)
        {
            throw new InvalidOperationException($"FAQ with ID {request.FaqId} not found.");
        }

        faq.IsDeleted = true;
        faq.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
