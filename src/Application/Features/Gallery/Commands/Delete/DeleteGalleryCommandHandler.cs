using MediatR;

namespace GiveAID.Application.Features.Gallery.Commands.Delete;

/// <summary>
/// Handler for DeleteGalleryCommand.
/// </summary>
public class DeleteGalleryCommandHandler : IRequestHandler<DeleteGalleryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteGalleryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteGalleryCommand request, CancellationToken cancellationToken)
    {
        var gallery = await _context.Gallery.FindAsync(new object[] { request.GalleryId }, cancellationToken);

        if (gallery == null)
        {
            throw new InvalidOperationException($"Gallery item with ID {request.GalleryId} not found.");
        }

        _context.Gallery.Remove(gallery);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
