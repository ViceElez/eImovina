using eImovina.Api.Data;
using eImovina.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<eImovinaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
//var jwtOptions = jwtSection.Get<JwtOptions>()
//    ?? throw new InvalidOperationException("Nedostaje Jwt konfiguracija.");

//if (jwtOptions.SigningKey.Length < 32)
//    throw new InvalidOperationException(
//        "Jwt:SigningKey mora imati najmanje 32 znaka.");

//builder.Services.Configure<JwtOptions>(jwtSection);
//builder.Services.AddScoped<JwtTokenService>();

//builder.Services
//    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = jwtOptions.Issuer,
//            ValidateAudience = true,
//            ValidAudience = jwtOptions.Audience,
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
//            ValidateLifetime = true,
//            ClockSkew = TimeSpan.FromSeconds(30)
//        };
//    });

//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build();

//    options.AddPolicy(
//        AuthorizationPolicies.Staff,
//        policy => policy.RequireRole("Admin", "Employee"));

//    options.AddPolicy(
//        AuthorizationPolicies.AdminOnly,
//        policy => policy.RequireRole("Admin"));
//});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();
    
app.Run();
