using System.Windows;
using Delivergate.Desktop.Services;

namespace Delivergate.Desktop;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorNotification.Visibility = Visibility.Collapsed;

        var email = TxtEmail.Text.Trim();
        var password = TxtPassword.Password;
        var apiUrl = TxtApiUrl.Text.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            ShowError("Please enter your email address.");
            TxtEmail.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter your password.");
            TxtPassword.Focus();
            return;
        }

        if (!string.IsNullOrWhiteSpace(apiUrl))
        {
            ApiService.Instance.BaseUrl = apiUrl;
        }

        BtnLogin.IsEnabled = false;
        BtnLogin.Content = "Authenticating...";

        try
        {
            var (success, message, user) = await ApiService.Instance.LoginAsync(email, password);

            if (success && user != null)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            else
            {
                ShowError(message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to communicate with API server: {ex.Message}");
        }
        finally
        {
            BtnLogin.IsEnabled = true;
            BtnLogin.Content = "Log In to Shop";
        }
    }

    private void FillOwner_Click(object sender, RoutedEventArgs e)
    {
        TxtEmail.Text = "owner@delivergate.lk";
        TxtPassword.Password = "password123";
        ErrorNotification.Visibility = Visibility.Collapsed;
    }

    private void FillManager_Click(object sender, RoutedEventArgs e)
    {
        TxtEmail.Text = "manager@delivergate.lk";
        TxtPassword.Password = "password123";
        ErrorNotification.Visibility = Visibility.Collapsed;
    }

    private void FillCashier_Click(object sender, RoutedEventArgs e)
    {
        TxtEmail.Text = "cashier@delivergate.lk";
        TxtPassword.Password = "password123";
        ErrorNotification.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string message)
    {
        ErrorMessage.Text = message;
        ErrorNotification.Visibility = Visibility.Visible;
    }
}
