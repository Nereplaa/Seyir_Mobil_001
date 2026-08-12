namespace SeyirMobil.Api.Models;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, string Username, string Role);

public record CreateUserRequest(string Username, string Password, string Role, string Email);

public record UserSummary(int Id, string Username, string Role, string? Email, DateTime OlusturmaTarihi);

public record FrontendLogRequest(string Eylem, string? Detay, string Sayfa);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Token, string NewPassword);
