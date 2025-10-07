using System.Text;
using FluentValidation;
using Infra.Mongo;
using App.Auth;
using Infra.Users;
using Infra.EvOwners;
using Infra.Stations;
using Infra.Schedules;
using Infra.Bookings;
using App.EvOwners;
using App.Stations;
using App.Bookings;
using App.Qr;
using Infra.Qr;
using Microsoft.AspNetCore.Authentication.JwtBearer;  
using Microsoft.IdentityModel.Tokens;                
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);


// Bind settings
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IMongoContext, MongoContext>();

// Controllers + FluentValidation
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly); 

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IJwtIssuer, JwtIssuer>();
builder.Services.AddHostedService<Api.Startup.SeedUsersHostedService>();
builder.Services.AddSingleton<IEvOwnerRepository, EvOwnerRepository>();
builder.Services.AddSingleton<IStationRepository, StationRepository>();
builder.Services.AddSingleton<IScheduleRepository, ScheduleRepository>();
builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
builder.Services.AddSingleton<IEvOwnerService, EvOwnerService>();
builder.Services.AddSingleton<IStationService, StationService>();
builder.Services.AddSingleton<IBookingService, BookingService>();
builder.Services.AddSingleton<IQrRepository, QrRepository>();
builder.Services.AddSingleton<IQrService, QrService>();
builder.Services.AddSingleton<INearbyStations, NearbyStations>();

var frontendUrl = "http://localhost:5173";
// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Swagger (with JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Bearer token"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend"); 
// Ensure indexes on startup
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<IMongoContext>();
    await IndexBuilder.EnsureAsync(ctx.Database);
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health
app.MapGet("/healthz", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));

app.Run();