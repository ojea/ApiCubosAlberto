using ApiCoreOAuthEmpleados.Helpers;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using NSwag.Generation.Processors.Security;
using NSwag;
using ApiCoreCubos.Repositories;
using ApiCubosAlberto.Data;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddAzureClients(factory =>
//{
//    factory.AddSecretClient
//    (builder.Configuration.GetSection("KeyVault"));
//});

//SecretClient secretClient =
//builder.Services.BuildServiceProvider().GetService<SecretClient>();

//KeyVaultSecret secret =
//    await secretClient.GetSecretAsync("SqlAzure");
//string connectionString = secret.Value;

builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient
    (builder.Configuration.GetSection("KeyVault"));

});

SecretClient secretClient = builder.Services.BuildServiceProvider().GetService<SecretClient>();

KeyVaultSecret secret = await secretClient.GetSecretAsync("SecretKey"); //Aqui ponemos el nombre del secret
KeyVaultSecret audienceKey = await secretClient.GetSecretAsync("Audience");
KeyVaultSecret issuerKey = await secretClient.GetSecretAsync("Issuer");

string secretKey = secret.Value;
string audience = audienceKey.Value;
string issuer = issuerKey.Value;

HelperActionServicesOAuth helper = new HelperActionServicesOAuth(secretKey, audience, issuer);

builder.Services.AddSingleton<HelperActionServicesOAuth>(helper);
//esta instancia del helper debemos incluirla dentro
//de nuestra app solamente una vez para que todo lo que
//hemos creado dentro no se genere de nuevo
//habilitamos los servicios de auth que hemos creado 
//en el helper con action<>
builder.Services.AddAuthentication
    (helper.GetAuthenticateSchema())
    .AddJwtBearer(helper.GetJwtBearerOptions());

// Add services to the container.
string connectionString =
    builder.Configuration.GetConnectionString("SqlCubos");

builder.Services.AddTransient<CubosRepository>();
builder.Services.AddDbContext<CubosContext>
    (options => options.UseSqlServer(connectionString));


builder.Services.AddControllers();

builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "API Doctores";
    document.Description = "API de la app de Dotores";
    document.AddSecurity("JWT", Enumerable.Empty<string>(),
        new NSwag.OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "Authorization",
            In = OpenApiSecurityApiKeyLocation.Header,
            Description = "Copia y pega el Token en el campo 'Value:' así: Bearer {Token JWT}."
        }
    );
    document.OperationProcessors.Add(
    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Api OAuth Empleados :D",
        Description = "Api con token de seguridad",
    });
});

var app = builder.Build();
app.UseOpenApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(url: "/swagger/v1/swagger.json"
        , name: "Api OAuth Empleados");
    options.RoutePrefix = "";
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();