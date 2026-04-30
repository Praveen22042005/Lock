using Lock.API.Services;
using Lock.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("NeonDatabase") ?? string.Empty;

var envKeyContents = Environment.GetEnvironmentVariable("RSA_PRIVATE_KEY_CONTENTS");
var keySource = !string.IsNullOrEmpty(envKeyContents) 
    ? envKeyContents 
    : builder.Configuration.GetSection("ApiSettings")["RsaPrivateKeyPath"] ?? string.Empty;

builder.Services.AddSingleton(new LicenseService(connectionString));
builder.Services.AddSingleton(new SigningService(keySource));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/activate", ActivateEndpoint.Handle);
app.MapPost("/validate", ValidateEndpoint.Handle);
app.MapPost("/deactivate", DeactivateEndpoint.Handle);

app.Run();
