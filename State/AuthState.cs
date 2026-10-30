namespace CryptoVault.Client.State;

public class AuthState
{
    public bool IsLoggedIn { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;
    public string UserRole { get; private set; } = string.Empty;

    public event Action? OnStateChanged;

    public void SetUser(string email, string role)
    {
        IsLoggedIn = true;
        UserEmail = email;
        UserRole = role;
        NotifyStateChanged();
    }

    public void Logout()
    {
        IsLoggedIn = false;
        UserEmail = string.Empty;
        UserRole = string.Empty;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}