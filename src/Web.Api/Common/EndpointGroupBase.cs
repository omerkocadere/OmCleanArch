﻿namespace CleanArch.Web.Api.Common;

public abstract class EndpointGroupBase
{
    public virtual string? GroupName { get; }
    public abstract void Map(RouteGroupBuilder groupBuilder);
}
