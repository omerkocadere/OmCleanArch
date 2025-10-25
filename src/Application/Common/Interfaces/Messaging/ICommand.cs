namespace CleanArch.Application.Common.Interfaces.Messaging;

/// <summary>
/// Marker interface for all command types.
/// Enables polymorphic handling without generic constraints.
/// </summary>
public interface ICommandMarker { }

public interface ICommand : ICommandMarker, IRequest<Result> { }

public interface ICommand<TResponse> : ICommandMarker, IRequest<Result<TResponse>> { }
