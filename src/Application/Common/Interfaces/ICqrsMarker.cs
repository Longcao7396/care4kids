namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Marker interface for CQRS commands that don't return a value.
/// </summary>
public interface ICommand : MediatR.IRequest<MediatR.Unit>
{
}

/// <summary>
/// Marker interface for CQRS commands that return a value.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<TResponse> : MediatR.IRequest<TResponse>
{
}

/// <summary>
/// Marker interface for CQRS queries.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQuery<TResponse> : MediatR.IRequest<TResponse>
{
}
