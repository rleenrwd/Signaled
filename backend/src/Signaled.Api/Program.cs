using Microsoft.EntityFrameworkCore;
using Signaled.Infrastructure.Persistence;
using Signaled.Application.Events;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Registers services that generate the API Documentation.

builder.Services.AddDbContext<SignaledDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("Signaled") ?? throw new InvalidOperationException(
        "The Signaled connection string is missing."
    )
));

builder.Services.AddScoped<IEventDraftStore, EventDraftStore>();
builder.Services.AddScoped<IEventNumberGenerator, EventNumberGenerator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Exposes the generated OpenAPI document at /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();