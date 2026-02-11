var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

var routes = new Dictionary<string, string>
{
    ["users"] = "http://localhost:5001",
    ["products"] = "http://localhost:5002",
    ["orders"] = "http://localhost:5003"
};

app.MapGet("/{service}/{**path}", async (string service, string? path, HttpClient http) =>
{
    if (!routes.TryGetValue(service, out var url))
        return Results.NotFound($"Service '{service}' not found");

    var endpoint = string.IsNullOrEmpty(path) ? $"{url}/api/{service}" : $"{url}/api/{service}/{path}";

    try
    {
        var response = await http.GetAsync(endpoint);
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Service connection error: {ex.Message}");
    }
});

app.MapPost("/{service}/{**path}", async (string service, string? path, HttpRequest request, HttpClient http) =>
{
    if (!routes.TryGetValue(service, out var url))
        return Results.NotFound($"Service '{service}' not found");

    var endpoint = string.IsNullOrEmpty(path) ? $"{url}/api/{service}" : $"{url}/api/{service}/{path}";

    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

    var response = await http.PostAsync(endpoint, content);
    var result = await response.Content.ReadAsStringAsync();
    return Results.Content(result, "application/json", statusCode: (int)response.StatusCode);
});

app.MapPut("/{service}/{**path}", async (string service, string? path, HttpRequest request, HttpClient http) =>
{
    if (!routes.TryGetValue(service, out var url))
        return Results.NotFound($"Servuce '{service}' not found");

    var endpoint = $"{url}/api/{service}/{path}";

    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

    var response = await http.PutAsync(endpoint, content);
    var result = await response.Content.ReadAsStringAsync();
    return Results.Content(result, "application/json", statusCode: (int)response.StatusCode);
});

app.MapGet("/", () => new
{
    Service = "API Gateway",
    Routes = new
    {
        Users = "/users/* -> localhost:5001",
        Products = "/products/* -> localhost:5002",
        Orders = "/orders/* -> localhost:5003"
    }
});

Console.WriteLine("API Gateway started on http://localhost:5000");
Console.WriteLine("Routes: /users /products /orders");
app.Run("http://localhost:5000");
