using GiveAID.Application.Features.Causes.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Causes.Queries.GetTree;

/// <summary>
/// Query to get causes in a hierarchical tree structure.
/// </summary>
public class GetCauseTreeQuery : IRequest<IEnumerable<CauseDto>>
{
}
