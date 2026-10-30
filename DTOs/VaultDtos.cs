namespace CryptoVault.Client.DTOs;

public class CreateVaultItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class VaultItemResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DecryptedVaultItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DecryptedPassword { get; set; } = string.Empty;
}

public class ChangeRoleDto
{
    public Guid UserId { get; set; }
    public string NewRole { get; set; } = string.Empty;
}