namespace Gateway.Api.Admin.Dtos {
    public sealed record CreateProxyConfigRequest(
    long Version = 1,
    bool IsActive = true
);

    public sealed record AddRouteRequest(
        string RouteId,
        string ClusterId,
        string PathMatch,
        string? AuthorizationPolicy,
        List<TransformDto>? Transforms
    );

    public sealed record TransformDto(string Key, string Value);

    public sealed record AddClusterRequest(string ClusterId);

    public sealed record AddDestinationRequest(string DestinationId, string Address);
}
