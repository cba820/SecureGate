using Gateway.AdminUI.Dtos;

namespace Gateway.AdminUI.Models {
    public class RolesIndexVm {
        public List<RoleResponse> Roles { get; set; } = new();
        public string NewRoleName { get; set; } = "";
    }
}
