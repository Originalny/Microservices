var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

var orders = new Dictionary<int, Order>();
var nextId = 1;

const string Gateway = "http://localhost:5000";

app.MapGet("/api/orders", () => orders.Values);

app.MapGet("/api/orders/{id}", async (int id, HttpClient http) =>
{
    if (!orders.TryGetValue(id, out var order)) return Results.NotFound();

    var userTask = http.GetFromJsonAsync<User>($"{Gateway}/users/{order.UserId}");
    var productTask = http.GetFromJsonAsync<Product>($"{Gateway}/products/{order.ProductId}");

    await Task.WhenAll(userTask, productTask);

    return Results.Ok(new
    {
        order.Id,
        order.Quantity,
        order.Status,
        order.CreatedAt,
        User = userTask.Result,
        Product = productTask.Result
    });
});

app.MapPost("/api/orders", async (OrderDto dto, HttpClient http) =>
{
    var userResp = await http.GetAsync($"{Gateway}/users/{dto.UserId}");
    if (!userResp.IsSuccessStatusCode)
        return Results.BadRequest("User not found");

    var product = await http.GetFromJsonAsync<Product>($"{Gateway}/products/{dto.ProductId}");
    if (product == null)
        return Results.BadRequest("Product not found");
    if (product.Stock < dto.Quantity)
        return Results.BadRequest("Insufficient product stock");

    await http.PutAsJsonAsync($"{Gateway}/products/{dto.ProductId}/stock", new { Delta = -dto.Quantity });

    var order = new Order(nextId++, dto.UserId, dto.ProductId, dto.Quantity, "Created", DateTime.UtcNow);
    orders[order.Id] = order;

    return Results.Created($"/api/orders/{order.Id}", order);
});

app.MapPut("/api/orders/{id}/status", (int id, StatusDto dto) =>
{
    if (!orders.TryGetValue(id, out var order)) return Results.NotFound();
    orders[id] = order with { Status = dto.Status };
    return Results.Ok(orders[id]);
});

Console.WriteLine("OrdersService started on http://localhost:5003");
app.Run("http://localhost:5003");

record Order(int Id, int UserId, int ProductId, int Quantity, string Status, DateTime CreatedAt);
record OrderDto(int UserId, int ProductId, int Quantity);
record StatusDto(string Status);
record User(int Id, string Email, string Name, string PasswordHash);
record Product(int Id, string Name, string Description, decimal Price, int Stock);
