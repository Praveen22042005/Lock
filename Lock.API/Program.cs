using Lock.API.Services;
using Lock.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("NeonDatabase") ?? string.Empty;
var keyPath = builder.Configuration.GetSection("ApiSettings")["RsaPrivateKeyPath"] ?? string.Empty;

builder.Services.AddSingleton(new LicenseService(connectionString));
builder.Services.AddSingleton(new SigningService(keyPath));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/activate", ActivateEndpoint.Handle);
app.MapPost("/validate", ValidateEndpoint.Handle);
app.MapPost("/deactivate", DeactivateEndpoint.Handle);

app.Run();
