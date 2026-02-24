namespace Gateway.AdminUI.Dtos {
    public record CreateRoleRequest(string Name);
    public record RoleResponse(string Id, string Name);
}
