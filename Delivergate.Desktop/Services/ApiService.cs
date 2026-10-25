using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Delivergate.Desktop.Models;

namespace Delivergate.Desktop.Services;

public class ApiService
{
    private static readonly Lazy<ApiService> _instance = new(() => new ApiService());
    public static ApiService Instance => _instance.Value;

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public string BaseUrl { get; set; } = "http://127.0.0.1:8000/api/";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    private void ApplyAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(AuthSession.Current.Token)
            ? new AuthenticationHeaderValue("Bearer", AuthSession.Current.Token)
            : null;
    }

    private string BuildUrl(string relativePath)
    {
        return $"{BaseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }

    public async Task<(bool Success, string Message, UserDto? User)> LoginAsync(string email, string password)
    {
        try
        {
            var payload = new { email, password };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BuildUrl("auth/login"), content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = ParseErrorMessage(responseBody, (int)response.StatusCode);
                return (false, errorMsg, null);
            }

            var loginResult = JsonSerializer.Deserialize<LoginResponse>(responseBody, _jsonOptions);
            if (loginResult?.Data?.Token != null && loginResult.Data.User != null)
            {
                AuthSession.Current.Token = loginResult.Data.Token;
                AuthSession.Current.User = loginResult.Data.User;
                return (true, loginResult.Message, loginResult.Data.User);
            }

            return (false, "Invalid response from server.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Connection error: {ex.Message}. Make sure the Laravel server is running.", null);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            ApplyAuthHeader();
            await _httpClient.PostAsync(BuildUrl("auth/logout"), null);
        }
        catch
        {
            // Ignored on local teardown
        }
        finally
        {
            AuthSession.Current.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<List<ItemDto>> GetItemsAsync(string? search = null, long? categoryId = null)
    {
        ApplyAuthHeader();
        var url = BuildUrl("items");
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            queryParams.Add($"category_id={categoryId.Value}");
        }

        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(ParseErrorMessage(body, (int)response.StatusCode));
        }

        var result = JsonSerializer.Deserialize<ApiCollectionResponse<ItemDto>>(body, _jsonOptions);
        return result?.Data ?? [];
    }

    public async Task<(bool Success, string Message, ItemDto? Item)> CreateItemAsync(ItemDto item)
    {
        ApplyAuthHeader();
        var payload = new
        {
            category_id = item.CategoryId > 0 ? item.CategoryId : null,
            name = item.Name,
            flavor = item.Flavor,
            price = item.Price,
            stock_quantity = item.StockQuantity,
            unit = item.Unit,
            description = item.Description,
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(BuildUrl("items"), content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return (false, ParseErrorMessage(body, (int)response.StatusCode), null);
        }

        var result = JsonSerializer.Deserialize<ApiResponse<ItemDto>>(body, _jsonOptions);
        return (true, "Item created successfully.", result?.Data);
    }

    public async Task<(bool Success, string Message, ItemDto? Item)> UpdateItemAsync(ItemDto item)
    {
        ApplyAuthHeader();
        var payload = new
        {
            category_id = item.CategoryId > 0 ? item.CategoryId : null,
            name = item.Name,
            flavor = item.Flavor,
            price = item.Price,
            stock_quantity = item.StockQuantity,
            unit = item.Unit,
            description = item.Description,
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(BuildUrl($"items/{item.Id}"), content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return (false, ParseErrorMessage(body, (int)response.StatusCode), null);
        }

        var result = JsonSerializer.Deserialize<ApiResponse<ItemDto>>(body, _jsonOptions);
        return (true, "Item updated successfully.", result?.Data);
    }

    public async Task<(bool Success, string Message)> DeleteItemAsync(long id)
    {
        ApplyAuthHeader();
        var response = await _httpClient.DeleteAsync(BuildUrl($"items/{id}"));
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return (false, ParseErrorMessage(body, (int)response.StatusCode));
        }

        return (true, "Item deleted successfully.");
    }

    public async Task<List<CustomerDto>> GetCustomersAsync(string? search = null)
    {
        ApplyAuthHeader();
        var url = BuildUrl("customers");
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search.Trim())}";
        }

        var response = await _httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(ParseErrorMessage(body, (int)response.StatusCode));
        }

        var result = JsonSerializer.Deserialize<ApiCollectionResponse<CustomerDto>>(body, _jsonOptions);
        return result?.Data ?? [];
    }

    public async Task<(bool Success, string Message, CustomerDto? Customer)> CreateCustomerAsync(CustomerDto customer)
    {
        ApplyAuthHeader();
        var payload = new
        {
            name = customer.Name,
            email = string.IsNullOrWhiteSpace(customer.Email) ? null : customer.Email,
            phone = string.IsNullOrWhiteSpace(customer.Phone) ? null : customer.Phone,
            address = string.IsNullOrWhiteSpace(customer.Address) ? null : customer.Address,
            notes = string.IsNullOrWhiteSpace(customer.Notes) ? null : customer.Notes,
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(BuildUrl("customers"), content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return (false, ParseErrorMessage(body, (int)response.StatusCode), null);
        }

        var result = JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(body, _jsonOptions);
        return (true, "Customer created successfully.", result?.Data);
    }

    public async Task<(bool Success, string Message, CustomerDto? Customer)> UpdateCustomerAsync(CustomerDto customer)
    {
        ApplyAuthHeader();
        var payload = new
        {
            name = customer.Name,
            email = string.IsNullOrWhiteSpace(customer.Email) ? null : customer.Email,
            phone = string.IsNullOrWhiteSpace(customer.Phone) ? null : customer.Phone,
            address = string.IsNullOrWhiteSpace(customer.Address) ? null : customer.Address,
            notes = string.IsNullOrWhiteSpace(customer.Notes) ? null : customer.Notes,
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(BuildUrl($"customers/{customer.Id}"), content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return (false, ParseErrorMessage(body, (int)response.StatusCode), null);
        }

        var result = JsonSerializer.Deserialize<ApiResponse<CustomerDto>>(body, _jsonOptions);
        return (true, "Customer updated successfully.", result?.Data);
    }

    public async Task<(bool Success, string Message)> DeleteCustomerAsync(long id)
    {
        ApplyAuthHeader();
        var response = await _httpClient.DeleteAsync(BuildUrl($"customers/{id}"));
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return (false, ParseErrorMessage(body, (int)response.StatusCode));
        }

        return (true, "Customer deleted successfully.");
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        ApplyAuthHeader();
        var response = await _httpClient.GetAsync(BuildUrl("categories"));
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var result = JsonSerializer.Deserialize<ApiCollectionResponse<CategoryDto>>(body, _jsonOptions);
        return result?.Data ?? [];
    }

    private string ParseErrorMessage(string responseBody, int statusCode)
    {
        if (statusCode == 403)
        {
            return "403 Forbidden: Your role is not authorized to perform this action.";
        }

        if (statusCode == 401)
        {
            return "401 Unauthorized: Invalid credentials or session expired.";
        }

        try
        {
            var node = JsonNode.Parse(responseBody);
            if (node?["errors"] is JsonObject errorsObj)
            {
                var sb = new StringBuilder();
                foreach (var prop in errorsObj)
                {
                    if (prop.Value is JsonArray arr)
                    {
                        foreach (var item in arr)
                        {
                            sb.AppendLine($"• {item?.GetValue<string>()}");
                        }
                    }
                }
                if (sb.Length > 0)
                {
                    return sb.ToString().TrimEnd();
                }
            }

            if (node?["message"] != null)
            {
                return node["message"]!.GetValue<string>();
            }
        }
        catch
        {
            // Fallback
        }

        return $"Request failed with HTTP {statusCode}.";
    }
}
