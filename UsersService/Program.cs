var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var users = new Dictionary<int, User>
{
    [1] = new(1, "ivan@mail.ru", "Ivan Petrov", "hashed_pass"),
    [2] = new(2, "anna@mail.ru", "Anna Sidorova", "hashed_pass"),
};
var nextId = 3;

app.MapGet("/api/users", () => users.Values);

app.MapGet("/api/users/{id}", (int id) =>
    users.TryGetValue(id, out var user) ? Results.Ok(user) : Results.NotFound());

app.MapPost("/api/users", (UserDto dto) =>
{
    var user = new User(nextId++, dto.Email, dto.Name, "hashed_" + dto.Password);
    users[user.Id] = user;
    return Results.Created($"/api/users/{user.Id}", user);
});

app.MapPost("/api/users/auth", (AuthDto dto) =>
{
    var user = users.Values.FirstOrDefault(u => u.Email == dto.Email);
    if (user == null) return Results.NotFound("User not found");
    return Results.Ok(new { user.Id, user.Name, Token = $"token_{user.Id}" });
});

Console.WriteLine("UsersService started on http://localhost:5001");
app.Run("http://localhost:5001");

record User(int Id, string Email, string Name, string PasswordHash);
record UserDto(string Email, string Name, string Password);
record AuthDto(string Email, string Password);
