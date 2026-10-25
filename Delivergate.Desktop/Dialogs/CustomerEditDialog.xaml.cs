using System.Windows;
using Delivergate.Desktop.Models;

namespace Delivergate.Desktop.Dialogs;

public partial class CustomerEditDialog : Window
{
    private readonly CustomerDto? _existingCustomer;

    public CustomerDto? ResultCustomer { get; private set; }

    public CustomerEditDialog(CustomerDto? customerToEdit)
    {
        InitializeComponent();
        _existingCustomer = customerToEdit;

        if (_existingCustomer != null)
        {
            DialogTitleText.Text = "Edit Customer Record";
            BtnSave.Content = "Update Customer";

            TxtName.Text = _existingCustomer.Name;
            TxtEmail.Text = _existingCustomer.Email;
            TxtPhone.Text = _existingCustomer.Phone;
            TxtAddress.Text = _existingCustomer.Address;
            TxtNotes.Text = _existingCustomer.Notes;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorBanner.Visibility = Visibility.Collapsed;

        var name = TxtName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("Customer name is required.");
            TxtName.Focus();
            return;
        }

        var email = TxtEmail.Text.Trim();
        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
        {
            ShowError("Please provide a valid email address.");
            TxtEmail.Focus();
            return;
        }

        ResultCustomer = new CustomerDto
        {
            Id = _existingCustomer?.Id ?? 0,
            Name = name,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            Phone = string.IsNullOrWhiteSpace(TxtPhone.Text.Trim()) ? null : TxtPhone.Text.Trim(),
            Address = string.IsNullOrWhiteSpace(TxtAddress.Text.Trim()) ? null : TxtAddress.Text.Trim(),
            Notes = string.IsNullOrWhiteSpace(TxtNotes.Text.Trim()) ? null : TxtNotes.Text.Trim(),
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBanner.Visibility = Visibility.Visible;
    }
}
