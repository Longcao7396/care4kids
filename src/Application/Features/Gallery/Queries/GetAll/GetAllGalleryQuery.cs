using GiveAID.Application.Features.Gallery.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Gallery.Queries.GetAll;

/// <summary>
/// Query to get all gallery items.
/// </summary>
public class GetAllGalleryQuery : IRequest<IEnumerable<GalleryDto>>
{
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
