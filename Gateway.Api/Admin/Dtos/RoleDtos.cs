namespace Gateway.Api.Admin.Dtos {
    public record CreateRoleRequest(string Name);
    public record RoleResponse(string Id, string Name);
    public record SetUserRoleRequest(string UserId, string RoleName);
}
