using System.Text.Json.Serialization;

namespace Delivergate.Desktop.Models;

public class UserDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

public class LoginResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public LoginData? Data { get; set; }
}

public class LoginData
{
    [JsonPropertyName("user")]
    public UserDto? User { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";
}

public class CategoryDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public override string ToString() => Name;
}

public class ItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("category_id")]
    public long? CategoryId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("flavor")]
    public string? Flavor { get; set; }

    [JsonPropertyName("price")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Price { get; set; }

    [JsonPropertyName("stock_quantity")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int StockQuantity { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; } = "scoop";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("category")]
    public CategoryDto? Category { get; set; }

    public string CategoryName => Category?.Name ?? "Unassigned";

    public string PriceFormatted => $"LKR {Price:N2}";
}

public class CustomerDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

public class ApiResponse<T>
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class ApiCollectionResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];
}
