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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("basic", document)] = new List<string>()
    });
});

// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Password hashing
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// Basic authentication + authorization
builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

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


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<JackpotHub>("/jackpot-hub");

app.Run();
