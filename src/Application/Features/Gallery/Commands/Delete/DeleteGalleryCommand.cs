using MediatR;

namespace GiveAID.Application.Features.Gallery.Commands.Delete;

/// <summary>
/// Command to delete a gallery item.
/// </summary>
public class DeleteGalleryCommand : IRequest<bool>
{
    public int GalleryId { get; set; }
}
