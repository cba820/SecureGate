using Gateway.AdminUI.Dtos;

namespace Gateway.AdminUI.Models {
    public class UsersIndexVm {
        public List<UserResponse> Users { get; set; } = new();
        public List<string> AllRoles { get; set; } = new();
        public Dictionary<string, List<string>> UserRoles { get; set; } = new();
    }
}
