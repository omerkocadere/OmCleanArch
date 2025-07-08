using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Common.Interfaces.Messaging;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
