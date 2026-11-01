using System.Text.Json;
using System.Text.Json.Serialization;
using Organization.Application.Exceptions;
using Organization.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddOpenApi();
builder.Services.AddOrganizationInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (OrganizationNotFoundException exception)
    {
        await WriteErrorAsync(context, StatusCodes.Status404NotFound, exception.Message);
    }
    catch (OrganizationConflictException exception)
    {
        await WriteErrorAsync(context, StatusCodes.Status409Conflict, exception.Message);
    }
    catch (OrganizationValidationException exception)
    {
        await WriteErrorAsync(context, StatusCodes.Status400BadRequest, exception.Message);
    }
});

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
{
    if (context.Response.HasStarted) throw new InvalidOperationException(message);
    context.Response.Clear();
    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        type = "about:blank",
        title = statusCode switch
        {
            404 => "Resource not found",
            409 => "Conflict",
            _ => "Validation error"
        },
        status = statusCode,
        detail = message
    }));
}
