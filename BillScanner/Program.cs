using BillScanner.Extension;
using Business.Extension;
using Infrastructure.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddPresentation(builder.Configuration);
builder.Services.AddSettings(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplications(builder.Configuration);

// Add global exception handler
builder.Services.AddExceptionHandler<BillScanner.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // Enable global exception handling

app.UseOpenApiWithSwagger();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Minimal API endpoints
app.MapGet("/cats", () =>
{
    var catBreeds = new[]
    {
        new { Id = 1, Name = "Persian", Origin = "Iran", Description = "Long-haired, flat-faced, gentle temperament" },
        new { Id = 2, Name = "Maine Coon", Origin = "United States", Description = "Large, fluffy, friendly giant" },
        new { Id = 3, Name = "Siamese", Origin = "Thailand", Description = "Vocal, social, distinctive blue eyes" },
        new { Id = 4, Name = "Bengal", Origin = "United States", Description = "Wild appearance, leopard-like spots" },
        new { Id = 5, Name = "Ragdoll", Origin = "United States", Description = "Blue eyes, docile, goes limp when picked up" },
        new { Id = 6, Name = "British Shorthair", Origin = "United Kingdom", Description = "Round face, dense coat, calm" },
        new { Id = 7, Name = "Sphynx", Origin = "Canada", Description = "Hairless, warm to touch, energetic" },
        new { Id = 8, Name = "Scottish Fold", Origin = "Scotland", Description = "Folded ears, sweet personality" },
        new { Id = 9, Name = "Abyssinian", Origin = "Ethiopia", Description = "Active, playful, ticked coat" },
        new { Id = 10, Name = "Russian Blue", Origin = "Russia", Description = "Silver-blue coat, green eyes, reserved" }
    };

    return Results.Ok(catBreeds);
})
.WithName("GetCatBreeds")
.WithOpenApi(operation =>
{
    operation.Summary = "Get a list of cat breeds";
    operation.Description = "Returns a collection of popular cat breeds with their origins and descriptions";
    return operation;
})
.AllowAnonymous();

app.Run();