using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Delivergate.Desktop.Dialogs;
using Delivergate.Desktop.Models;
using Delivergate.Desktop.Services;

namespace Delivergate.Desktop;

public partial class MainWindow : Window
{
    private List<CategoryDto> _categories = [];
    private List<ItemDto> _allItems = [];
    private List<CustomerDto> _allCustomers = [];

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (!AuthSession.Current.IsAuthenticated)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
            return;
        }

        SetupUserRoleUi();
        await LoadCategoriesAsync();
        await LoadItemsAsync();
        await LoadCustomersAsync();
    }

    private void SetupUserRoleUi()
    {
        var session = AuthSession.Current;
        var user = session.User!;

        TxtUserName.Text = user.Name;
        TxtUserEmail.Text = user.Email;
        TxtRoleBadge.Text = session.RoleBadgeText;

        if (session.IsOwner)
        {
            RoleBadgeBorder.Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Emerald Green
            TxtRbacSummary.Text = "👑 Owner: Full CRUD access. Allowed to add, edit, view, and delete items and customers.";
            BtnAddItem.IsEnabled = true;
        }
        else if (session.IsManager)
        {
            RoleBadgeBorder.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235)); // Royal Blue
            TxtRbacSummary.Text = "📋 Manager: Can add, view, and edit items & customers. Delete operations are restricted to Owner only.";
            BtnAddItem.IsEnabled = true;
        }
        else // Cashier
        {
            RoleBadgeBorder.Background = new SolidColorBrush(Color.FromRgb(217, 119, 6)); // Amber
            TxtRbacSummary.Text = "💳 Cashier: Can view & edit existing items (No add/delete). Can add, view, & edit customers (No delete).";
            BtnAddItem.IsEnabled = false;
            BtnAddItem.ToolTip = "Cashiers are not permitted to add items (Owner & Manager only).";
            BtnAddItem.Opacity = 0.5;
        }
    }

    #region Inventory Items Operations

    private async Task LoadCategoriesAsync()
    {
        try
        {
            _categories = await ApiService.Instance.GetCategoriesAsync();

            var comboList = new List<CategoryDto>
            {
                new() { Id = 0, Name = "All Flavor Categories" }
            };
            comboList.AddRange(_categories);

            CmbItemCategory.ItemsSource = comboList;
            CmbItemCategory.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ShowNotification($"Failed to load categories: {ex.Message}", isError: true);
        }
    }

    private async Task LoadItemsAsync(string? search = null, long? categoryId = null)
    {
        try
        {
            _allItems = await ApiService.Instance.GetItemsAsync(search, categoryId);
            GridItems.ItemsSource = _allItems;
            TxtItemStats.Text = $"Items: {_allItems.Count}";
        }
        catch (Exception ex)
        {
            ShowNotification($"Failed to load inventory items: {ex.Message}", isError: true);
        }
    }

    private async void SearchItems_Click(object sender, RoutedEventArgs e)
    {
        var searchTerm = TxtItemSearch.Text.Trim();
        var selectedCatId = (CmbItemCategory.SelectedValue as long?) ?? 0;
        await LoadItemsAsync(string.IsNullOrEmpty(searchTerm) ? null : searchTerm, selectedCatId > 0 ? selectedCatId : null);
    }

    private void ItemSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchItems_Click(sender, e);
        }
    }

    private async void ItemCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            SearchItems_Click(sender, e);
        }
    }

    private async void ResetItems_Click(object sender, RoutedEventArgs e)
    {
        TxtItemSearch.Text = string.Empty;
        CmbItemCategory.SelectedIndex = 0;
        await LoadItemsAsync();
    }

    private async void RefreshItems_Click(object sender, RoutedEventArgs e)
    {
        await LoadItemsAsync();
        ShowNotification("Inventory items refreshed from API server.", isError: false);
    }

    private async void AddItem_Click(object sender, RoutedEventArgs e)
    {
        if (!AuthSession.Current.CanAddItem)
        {
            ShowNotification("403 Forbidden: Cashier role is not permitted to add items. Only Owner and Manager can add items.", isError: true);
            return;
        }

        var dialog = new ItemEditDialog(null, _categories)
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && dialog.ResultItem != null)
        {
            var (success, message, createdItem) = await ApiService.Instance.CreateItemAsync(dialog.ResultItem);
            if (success)
            {
                ShowNotification($"Item '{createdItem?.Name ?? dialog.ResultItem.Name}' added successfully!", isError: false);
                await LoadItemsAsync();
            }
            else
            {
                ShowNotification(message, isError: true);
            }
        }
    }

    private async void EditItemRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ItemDto item })
        {
            if (!AuthSession.Current.CanEditItem)
            {
                ShowNotification("403 Forbidden: You do not have permission to edit items.", isError: true);
                return;
            }

            var dialog = new ItemEditDialog(item, _categories)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.ResultItem != null)
            {
                var (success, message, updatedItem) = await ApiService.Instance.UpdateItemAsync(dialog.ResultItem);
                if (success)
                {
                    ShowNotification($"Item '{updatedItem?.Name ?? dialog.ResultItem.Name}' updated successfully!", isError: false);
                    await LoadItemsAsync();
                }
                else
                {
                    ShowNotification(message, isError: true);
                }
            }
        }
    }

    private async void DeleteItemRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ItemDto item })
        {
            if (!AuthSession.Current.CanDeleteItem)
            {
                var roleName = AuthSession.Current.RoleBadgeText;
                ShowNotification($"403 Forbidden: Only the Owner can delete inventory items. '{roleName}' role is not authorized.", isError: true);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete '{item.Name}'?\nThis action cannot be undone.",
                "Confirm Delete Item",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                var (success, message) = await ApiService.Instance.DeleteItemAsync(item.Id);
                if (success)
                {
                    ShowNotification($"Item '{item.Name}' deleted successfully.", isError: false);
                    await LoadItemsAsync();
                }
                else
                {
                    ShowNotification(message, isError: true);
                }
            }
        }
    }

    #endregion

    #region Customer Management Operations

    private async Task LoadCustomersAsync(string? search = null)
    {
        try
        {
            _allCustomers = await ApiService.Instance.GetCustomersAsync(search);
            GridCustomers.ItemsSource = _allCustomers;
            TxtCustomerStats.Text = $"Customers: {_allCustomers.Count}";
        }
        catch (Exception ex)
        {
            ShowNotification($"Failed to load customer records: {ex.Message}", isError: true);
        }
    }

    private async void SearchCustomers_Click(object sender, RoutedEventArgs e)
    {
        var search = TxtCustomerSearch.Text.Trim();
        await LoadCustomersAsync(string.IsNullOrEmpty(search) ? null : search);
    }

    private void CustomerSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchCustomers_Click(sender, e);
        }
    }

    private async void ResetCustomers_Click(object sender, RoutedEventArgs e)
    {
        TxtCustomerSearch.Text = string.Empty;
        await LoadCustomersAsync();
    }

    private async void RefreshCustomers_Click(object sender, RoutedEventArgs e)
    {
        await LoadCustomersAsync();
        ShowNotification("Customer records refreshed from API server.", isError: false);
    }

    private async void AddCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (!AuthSession.Current.CanAddCustomer)
        {
            ShowNotification("403 Forbidden: You do not have permission to add customers.", isError: true);
            return;
        }

        var dialog = new CustomerEditDialog(null)
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && dialog.ResultCustomer != null)
        {
            var (success, message, created) = await ApiService.Instance.CreateCustomerAsync(dialog.ResultCustomer);
            if (success)
            {
                ShowNotification($"Customer '{created?.Name ?? dialog.ResultCustomer.Name}' added successfully!", isError: false);
                await LoadCustomersAsync();
            }
            else
            {
                ShowNotification(message, isError: true);
            }
        }
    }

    private async void EditCustomerRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: CustomerDto customer })
        {
            if (!AuthSession.Current.CanEditCustomer)
            {
                ShowNotification("403 Forbidden: You do not have permission to edit customer records.", isError: true);
                return;
            }

            var dialog = new CustomerEditDialog(customer)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.ResultCustomer != null)
            {
                var (success, message, updated) = await ApiService.Instance.UpdateCustomerAsync(dialog.ResultCustomer);
                if (success)
                {
                    ShowNotification($"Customer '{updated?.Name ?? dialog.ResultCustomer.Name}' updated successfully!", isError: false);
                    await LoadCustomersAsync();
                }
                else
                {
                    ShowNotification(message, isError: true);
                }
            }
        }
    }

    private async void DeleteCustomerRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: CustomerDto customer })
        {
            if (!AuthSession.Current.CanDeleteCustomer)
            {
                var roleName = AuthSession.Current.RoleBadgeText;
                ShowNotification($"403 Forbidden: Only the Owner can delete customer records. '{roleName}' role is not authorized.", isError: true);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete customer '{customer.Name}'?\nThis action cannot be undone.",
                "Confirm Delete Customer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                var (success, message) = await ApiService.Instance.DeleteCustomerAsync(customer.Id);
                if (success)
                {
                    ShowNotification($"Customer '{customer.Name}' deleted successfully.", isError: false);
                    await LoadCustomersAsync();
                }
                else
                {
                    ShowNotification(message, isError: true);
                }
            }
        }
    }

    #endregion

    #region Notifications & Auth

    private void ShowNotification(string message, bool isError)
    {
        NotificationMessage.Text = message;
        if (isError)
        {
            NotificationBanner.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242));
            NotificationBanner.BorderBrush = new SolidColorBrush(Color.FromRgb(252, 165, 165));
            NotificationBanner.BorderThickness = new Thickness(1);
            NotificationMessage.Foreground = new SolidColorBrush(Color.FromRgb(153, 27, 27));
        }
        else
        {
            NotificationBanner.Background = new SolidColorBrush(Color.FromRgb(236, 253, 245));
            NotificationBanner.BorderBrush = new SolidColorBrush(Color.FromRgb(110, 231, 183));
            NotificationBanner.BorderThickness = new Thickness(1);
            NotificationMessage.Foreground = new SolidColorBrush(Color.FromRgb(6, 95, 70));
        }

        NotificationBanner.Visibility = Visibility.Visible;
    }

    private void DismissNotification_Click(object sender, RoutedEventArgs e)
    {
        NotificationBanner.Visibility = Visibility.Collapsed;
    }

    private async void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        await ApiService.Instance.LogoutAsync();

        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    #endregion
}