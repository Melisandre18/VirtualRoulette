using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using VirtualRouletteApi.Data;
using VirtualRouletteApi.Domain;
using VirtualRouletteApi.Extensions;
using VirtualRouletteApi.Hubs;
using VirtualRouletteApi.Services.Auth;
using VirtualRouletteApi.Services.Balance;
using VirtualRouletteApi.Services.Bets;
using VirtualRouletteApi.Services.Jackpot;
using VirtualRouletteApi.Services.Roulette;
using VirtualRouletteApi.Infrastructure.Storage;
using VirtualRouletteApi.Infrastructure.Errors;
using VirtualRouletteApi.Auth.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VirtualRouletteApi", Version = "v1" });

    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authorization header."
    });
    
    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header."
    });


    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("basic", document)] = new List<string>(),
        [new OpenApiSecuritySchemeReference("bearer", document)] = new List<string>()
    });
});

// EF Core + PostgreSQL
var provider = (builder.Configuration["Storage:Provider"] ?? "Postgres").Trim();

if (provider == "Postgres")
{
    builder.Services.AddDbContext<AppDbContext>(opts =>
        opts.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
}

builder.Services.AddStorage(builder.Configuration);

// Password hashing
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// Basic authentication + authorization
builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

//Jwt
builder.Services.AddSingleton<ITokenService, TokenService>();


//Balance, Deposit, Withdraw
builder.Services.AddScoped<IBalanceService, BalanceService>();

//Betting
builder.Services.AddSingleton<IRouletteService, RouletteService>();
builder.Services.AddScoped<IBetService, BetService>();

//Jackpot
builder.Services.AddScoped<IJackpotService, JackpotService>();

//SignalR
builder.Services.AddSignalR();
builder.Services.AddHostedService<SignOutWorker>();


var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VirtualRouletteApi v1");
    });
}

if (provider == "Postgres")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<JackpotHub>("/jackpot-hub");

app.Run();

public partial class Program { }