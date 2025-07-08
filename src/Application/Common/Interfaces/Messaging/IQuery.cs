using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Common.Interfaces.Messaging;

public interface IQuery : IRequest<Result> { }

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
