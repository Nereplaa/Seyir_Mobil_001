namespace SeyirMobil.Api.Models;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, string Username, string Role);

public record CreateUserRequest(string Username, string Password, string Role);

public record UserSummary(int Id, string Username, string Role, DateTime OlusturmaTarihi);

public record FrontendLogRequest(string Eylem, string? Detay, string Sayfa);
