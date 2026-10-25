using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Delivergate.Desktop.Models;
using Delivergate.Desktop.Services;

namespace Delivergate.Desktop.Dialogs;

public partial class ItemEditDialog : Window
{
    private readonly ItemDto? _existingItem;
    private readonly List<CategoryDto> _categories;

    public ItemDto? ResultItem { get; private set; }

    public ItemEditDialog(ItemDto? itemToEdit, List<CategoryDto> categories)
    {
        InitializeComponent();
        _existingItem = itemToEdit;
        _categories = categories;

        CmbCategory.ItemsSource = _categories;

        if (_existingItem != null)
        {
            DialogTitleText.Text = "Edit Inventory Item";
            BtnSave.Content = "Update Item";

            TxtName.Text = _existingItem.Name;
            TxtFlavor.Text = _existingItem.Flavor;
            TxtPrice.Text = _existingItem.Price.ToString("F2", CultureInfo.InvariantCulture);
            TxtStock.Text = _existingItem.StockQuantity.ToString();
            TxtDescription.Text = _existingItem.Description;

            if (_existingItem.CategoryId.HasValue)
            {
                CmbCategory.SelectedValue = _existingItem.CategoryId.Value;
            }

            foreach (ComboBoxItem item in CmbUnit.Items)
            {
                if (string.Equals(item.Content?.ToString(), _existingItem.Unit, StringComparison.OrdinalIgnoreCase))
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorBanner.Visibility = Visibility.Collapsed;

        var name = TxtName.Text.Trim();
        var flavor = TxtFlavor.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("Item name is required.");
            TxtName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(flavor))
        {
            ShowError("Flavor is required.");
            TxtFlavor.Focus();
            return;
        }

        if (!decimal.TryParse(TxtPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price < 0)
        {
            ShowError("Please enter a valid price (>= 0).");
            TxtPrice.Focus();
            return;
        }

        if (!int.TryParse(TxtStock.Text.Trim(), out var stock) || stock < 0)
        {
            ShowError("Please enter a valid stock quantity (integer >= 0).");
            TxtStock.Focus();
            return;
        }

        var selectedCategory = CmbCategory.SelectedItem as CategoryDto;
        var selectedUnitItem = CmbUnit.SelectedItem as ComboBoxItem;
        var unit = selectedUnitItem?.Content?.ToString() ?? "scoop";

        ResultItem = new ItemDto
        {
            Id = _existingItem?.Id ?? 0,
            Name = name,
            Flavor = flavor,
            Price = price,
            StockQuantity = stock,
            Unit = unit,
            Description = TxtDescription.Text.Trim(),
            CategoryId = selectedCategory?.Id,
            Category = selectedCategory,
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
