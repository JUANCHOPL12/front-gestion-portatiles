using System.Net.Http.Headers;
using System.Net.Http.Json;
using CryptoVault.Client.DTOs;

namespace CryptoVault.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string? _jwtToken;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Configurar el token JWT recuperado tras el Login/Registro
    public void SetAuthToken(string token)
    {
        _jwtToken = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _jwtToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // --- ENDPOINTS DE AUTHCONTROLLER ---
    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Auth/register", dto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        }
        return new AuthResponseDto { Message = "Error al registrar usuario." };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Auth/login", dto);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                SetAuthToken(result.Token);
            }
            return result;
        }

        // 1. Si el middleware de Rate Limit bloquea por superar intentos (HTTP 429)
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var rateLimitError = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return new AuthResponseDto
            {
                Message = rateLimitError?.Message ?? "Demasiados intentos. Por favor espere 1 minuto antes de reintentar."
            };
        }

        // 2. Si el WAF bloquea la petición por detectar contenido malicioso (HTTP 403)
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            var wafError = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return new AuthResponseDto
            {
                Message = wafError?.Message ?? "Petición bloqueada por el Firewall de Aplicación (WAF)."
            };
        }

        // 3. Si la API devolvió un mensaje de error específico en el JSON
        try
        {
            var apiError = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (apiError != null && !string.IsNullOrEmpty(apiError.Message))
            {
                return apiError;
            }
        }
        catch
        {
            // Fallback si no se pudo parsear el JSON
        }

        return new AuthResponseDto { Message = "Credenciales inválidas." };
    }

    public async Task<MfaSetupResponseDto?> SetupMfaAsync()
    {
        var response = await _httpClient.PostAsync("api/Auth/mfa/setup", null);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<MfaSetupResponseDto>();
        }
        return null;
    }

    public async Task<bool> VerifyMfaAsync(MfaVerifyDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Auth/mfa/verify", dto);
        return response.IsSuccessStatusCode;
    }

    // --- ENDPOINTS DE VAULTCONTROLLER ---
    public async Task<List<VaultItemResponseDto>> GetMyVaultItemsAsync()
    {
        try
        {
            var items = await _httpClient.GetFromJsonAsync<List<VaultItemResponseDto>>("api/Vault");
            return items ?? new List<VaultItemResponseDto>();
        }
        catch
        {
            return new List<VaultItemResponseDto>();
        }
    }

    public async Task<VaultItemResponseDto?> CreateVaultItemAsync(CreateVaultItemDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Vault", dto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<VaultItemResponseDto>();
        }
        return null;
    }

    public async Task<DecryptedVaultItemDto?> DecryptVaultItemAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DecryptedVaultItemDto>($"api/Vault/{id}/decrypt");
        }
        catch
        {
            return null;
        }
    }

    // --- ENDPOINTS DE ADMINCONTROLLER ---
    public async Task<List<UserManagementDto>> GetUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Admin/users");
            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<UserManagementDto>>();
                return users ?? new List<UserManagementDto>();
            }
            return new List<UserManagementDto>();
        }
        catch
        {
            return new List<UserManagementDto>();
        }
    }

    public async Task<bool> ChangeUserRoleAsync(ChangeRoleDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Admin/change-role", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Admin/audit-logs");
            if (response.IsSuccessStatusCode)
            {
                var logs = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();
                return logs ?? new List<AuditLogDto>();
            }
            return new List<AuditLogDto>();
        }
        catch
        {
            return new List<AuditLogDto>();
        }
    }
}