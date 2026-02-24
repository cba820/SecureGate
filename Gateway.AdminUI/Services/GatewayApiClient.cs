using Gateway.AdminUI.Dtos;
using System.Net;
using System.Net.Http.Headers;

namespace Gateway.AdminUI.Services {
    public class GatewayApiClient {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GatewayApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CreateClientWithAuth() {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var token = _httpContextAccessor.HttpContext?.Session.GetString("ADMIN_JWT");
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // Auth
        public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest req) {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var resp = await client.PostAsJsonAsync("_admin/auth/login", req);

            if (resp.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
                return ApiResult<LoginResponse>.Fail("Credenciales inválidas o usuario no es Admin.");

            if (!resp.IsSuccessStatusCode)
                return ApiResult<LoginResponse>.Fail(await resp.Content.ReadAsStringAsync());

            var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            return data is null
                ? ApiResult<LoginResponse>.Fail("Respuesta inválida del servidor.")
                : ApiResult<LoginResponse>.Success(data);
        }

        // Roles
        public async Task<ApiResult<List<RoleResponse>>> GetRolesAsync() {
            var client = CreateClientWithAuth();
            var resp = await client.GetAsync("_admin/roles");

            if (!resp.IsSuccessStatusCode)
                return ApiResult<List<RoleResponse>>.Fail(await resp.Content.ReadAsStringAsync());

            var data = await resp.Content.ReadFromJsonAsync<List<RoleResponse>>() ?? new();
            return ApiResult<List<RoleResponse>>.Success(data);
        }

        public async Task<ApiResult> CreateRoleAsync(string name) {
            var client = CreateClientWithAuth();
            var resp = await client.PostAsJsonAsync("_admin/roles", new CreateRoleRequest(name));

            if (!resp.IsSuccessStatusCode)
                return ApiResult.Fail(await resp.Content.ReadAsStringAsync());

            return ApiResult.Success();
        }

        public async Task<ApiResult> DeleteRoleAsync(string roleName) {
            var client = CreateClientWithAuth();
            var resp = await client.DeleteAsync($"_admin/roles/{Uri.EscapeDataString(roleName)}");

            if (!resp.IsSuccessStatusCode)
                return ApiResult.Fail(await resp.Content.ReadAsStringAsync());

            return ApiResult.Success();
        }

        // Users
        public async Task<ApiResult<List<UserResponse>>> GetUsersAsync() {
            var client = CreateClientWithAuth();
            var resp = await client.GetAsync("_admin/users");

            if (!resp.IsSuccessStatusCode)
                return ApiResult<List<UserResponse>>.Fail(await resp.Content.ReadAsStringAsync());

            var data = await resp.Content.ReadFromJsonAsync<List<UserResponse>>() ?? new();
            return ApiResult<List<UserResponse>>.Success(data);
        }

        public async Task<ApiResult> CreateUserAsync(CreateUserRequest req) {
            var client = CreateClientWithAuth();
            var resp = await client.PostAsJsonAsync("_admin/users", req);

            if (!resp.IsSuccessStatusCode)
                return ApiResult.Fail(await resp.Content.ReadAsStringAsync());

            return ApiResult.Success();
        }

        public async Task<ApiResult> DeleteUserAsync(string id) {
            var client = CreateClientWithAuth();
            var resp = await client.DeleteAsync($"_admin/users/{id}");

            if (!resp.IsSuccessStatusCode)
                return ApiResult.Fail(await resp.Content.ReadAsStringAsync());

            return ApiResult.Success();
        }

        public async Task<ApiResult<List<string>>> GetUserRolesAsync(string userId) {
            var client = CreateClientWithAuth();
            var resp = await client.GetAsync($"_admin/users/{userId}/roles");

            if (!resp.IsSuccessStatusCode)
                return ApiResult<List<string>>.Fail(await resp.Content.ReadAsStringAsync());

            var data = await resp.Content.ReadFromJsonAsync<List<string>>() ?? new();
            return ApiResult<List<string>>.Success(data);
        }

        public async Task<ApiResult> AddUserRoleAsync(string userId, string roleName) {
            var client = CreateClientWithAuth();
            var resp = await client.PostAsync($"_admin/users/{userId}/roles/{Uri.EscapeDataString(roleName)}", null);

            if (!resp.IsSuccessStatusCode)
                return ApiResult.Fail(await resp.Content.ReadAsStringAsync());

            return ApiResult.Success();
        }

        public async Task<ApiResult> RemoveUserRoleAsync(string userId, string roleName) {
            var client = CreateClientWithAuth();
            var resp = await client.DeleteAsync($"_admin/users/{userId}/roles/{Uri.EscapeDataString(roleName)}");

            if (!resp.IsSuccessStatusCode)
                return ApiResult.Fail(await resp.Content.ReadAsStringAsync());

            return ApiResult.Success();
        }
    }
}
