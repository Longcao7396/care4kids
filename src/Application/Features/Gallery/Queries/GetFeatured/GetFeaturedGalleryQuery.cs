using GiveAID.Application.Features.Gallery.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Gallery.Queries.GetFeatured;

/// <summary>
/// Query to get featured gallery items.
/// </summary>
public class GetFeaturedGalleryQuery : IRequest<IEnumerable<GalleryDto>>
{
    public int Limit { get; set; } = 12;
}
