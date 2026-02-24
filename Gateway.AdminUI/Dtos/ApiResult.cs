namespace Gateway.AdminUI.Dtos {
    public record ApiResult(bool Ok, string? Error) {
        public static ApiResult Success() => new(true, null);
        public static ApiResult Fail(string? error) => new(false, string.IsNullOrWhiteSpace(error) ? "Error desconocido" : error);
    }

    public record ApiResult<T>(bool Ok, T? Data, string? Error) {
        public static ApiResult<T> Success(T data) => new(true, data, null);
        public static ApiResult<T> Fail(string? error) => new(false, default, string.IsNullOrWhiteSpace(error) ? "Error desconocido" : error);
    }
}
