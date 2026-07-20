using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Data;
using shiftTrade.api.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using shiftTrade.Api.Contracts.Auth;
using System.Text.RegularExpressions;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidation();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
?? throw new InvalidOperationException("Connection string 'Default Connection' not found." );

builder.Services.AddDbContext<ApplicationDbContext>(options=>
options.UseNpgsql(connectionString)
);

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail =true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();


var jwtKey = builder.Configuration["Jwt:Key"]
?? throw new InvalidOperationException("JWT sign in key was not found");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
?? throw new InvalidOperationException("JWT Issuer Not Found");

var jwtAudience = builder.Configuration["Jwt:Audience"]
?? throw new InvalidOperationException("JWT Audience Not Found");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters=new TokenValidationParameters
    {
        ValidateIssuer = true,
ValidIssuer = jwtIssuer,
ValidateAudience = true,
ValidAudience = jwtAudience,
ValidateLifetime = true,
ValidateIssuerSigningKey = true,
IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtKey)),
    ClockSkew = TimeSpan.Zero

    };
});

builder.Services.AddAuthorization();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<ApplicationUser> userManager) =>
{
    var user = new ApplicationUser
    {
        UserName = request.EmailAddress,
        Email = request.EmailAddress,
        displayName = request.DisplayName

    };

    var result = await userManager.CreateAsync(user,request.Password);
    if(!result.Succeeded)
    {
        var errors = result.Errors
        .GroupBy(error=>error.Code)
        .ToDictionary(
                group => group.Key,
            group => group.Select(error =>error.Description).ToArray()
        );
        return Results.ValidationProblem(errors);
    }

    return Results.Created($"api/employees/{user.Id}", new
    {
        user.Id,
        user.displayName,
        user.Email
    });

}).AllowAnonymous().WithTags("Authentication");

app.Run();
