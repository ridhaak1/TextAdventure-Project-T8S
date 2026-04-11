
namespace TextAdventureAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        var authGroup = app.MapGroup("/api/auth").RequireAuthorization();
        authGroup.MapPost("/register", (HttpContext httpContext, User user) =>
        {
        })
        .WithName("RegisterUser")
        .WithOpenApi();

        app.Run();
    }
}
