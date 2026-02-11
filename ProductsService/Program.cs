var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var products = new Dictionary<int, Product>
{
    [1] = new(1, "Laptop", "Powerful laptop for work", 75000m, 10),
    [2] = new(2, "Smartphone", "Flagship smartphone", 45000m, 25),
    [3] = new(3, "Headphones", "Wireless headphones", 5000m, 50),
};
var nextId = 4;

app.MapGet("/api/products", () => products.Values);

app.MapGet("/api/products/{id}", (int id) =>
    products.TryGetValue(id, out var product) ? Results.Ok(product) : Results.NotFound());

app.MapGet("/api/products/search", (string q) =>
    products.Values.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase)));

app.MapPost("/api/products", (ProductDto dto) =>
{
    var product = new Product(nextId++, dto.Name, dto.Description, dto.Price, dto.Stock);
    products[product.Id] = product;
    return Results.Created($"/api/products/{product.Id}", product);
});

app.MapPut("/api/products/{id}/stock", (int id, StockDto dto) =>
{
    if (!products.TryGetValue(id, out var product)) return Results.NotFound();
    products[id] = product with { Stock = product.Stock + dto.Delta };
    return Results.Ok(products[id]);
});

Console.WriteLine("ProductsService started on http://localhost:5002");
app.Run("http://localhost:5002");

record Product(int Id, string Name, string Description, decimal Price, int Stock);
record ProductDto(string Name, string Description, decimal Price, int Stock);
record StockDto(int Delta);
