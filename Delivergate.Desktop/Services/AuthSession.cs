using Delivergate.Desktop.Models;

namespace Delivergate.Desktop.Services;

public class AuthSession
{
    public static AuthSession Current { get; } = new();

    public string? Token { get; set; }
    public UserDto? User { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && User != null;

    public bool IsOwner => User?.Role?.Equals("owner", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsManager => User?.Role?.Equals("manager", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsCashier => User?.Role?.Equals("cashier", StringComparison.OrdinalIgnoreCase) == true;

    // RBAC Rules based on technical assessment matrix:
    // Owner: Add/Edit/View/Delete Items & Customers
    // Manager: Add/Edit/View Items & Customers (No delete)
    // Cashier: Edit/View Items (No add/delete), Add/Edit/View Customers (No delete)
    public bool CanAddItem => IsOwner || IsManager;
    public bool CanEditItem => IsOwner || IsManager || IsCashier;
    public bool CanDeleteItem => IsOwner;

    public bool CanAddCustomer => IsOwner || IsManager || IsCashier;
    public bool CanEditCustomer => IsOwner || IsManager || IsCashier;
    public bool CanDeleteCustomer => IsOwner;

    public string RoleBadgeText => User?.Role?.ToUpperInvariant() ?? "GUEST";

    public void Clear()
    {
        Token = null;
        User = null;
    }
}
