using MediatR;

namespace CleanArch.Domain.Common;

public abstract record BaseEvent(Guid Id) : INotification { }
