using MassTransit;

namespace CleanArch.Infrastructure.Messaging;

/// <summary>
/// Dummy implementation of IPublishEndpoint used when MassTransit is disabled.
/// This allows the application to start successfully without MassTransit dependencies.
/// </summary>
public class DummyPublishEndpoint : IPublishEndpoint
{
    public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
    {
        // Return a dummy handle since no actual connection is made
        return new DummyConnectHandle();
    }

    public Task Publish<T>(T message, CancellationToken cancellationToken = default)
        where T : class
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish<T>(
        T message,
        IPipe<PublishContext<T>> publishPipe,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        where T : class
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish(object message, CancellationToken cancellationToken = default)
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish(
        object message,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = default
    )
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish(
        object message,
        Type messageType,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = default
    )
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish<T>(object values, CancellationToken cancellationToken = default)
        where T : class
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish<T>(
        object values,
        IPipe<PublishContext<T>> publishPipe,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }

    public Task Publish<T>(
        object values,
        IPipe<PublishContext> publishPipe,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        // No-op: Message publishing is disabled
        return Task.CompletedTask;
    }
}

/// <summary>
/// Dummy implementation of ConnectHandle for DummyPublishEndpoint
/// </summary>
public class DummyConnectHandle : ConnectHandle
{
    public void Disconnect()
    {
        // No-op: No actual connection to disconnect
    }

    public void Dispose()
    {
        // No-op: No resources to dispose
    }
}
