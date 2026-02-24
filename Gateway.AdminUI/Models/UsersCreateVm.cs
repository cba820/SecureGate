namespace Gateway.AdminUI.Models {
    public class UsersCreateVm {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(8)]
        public string Password { get; set; } = "";
    }
}
