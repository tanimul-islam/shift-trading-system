using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Data;
using shiftTrade.api.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using shiftTrade.Api.Contracts.Auth;
using System.Text.RegularExpressions;
using shiftTrade.Api.Services.Auth;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Authentication.BearerToken;


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
builder.Services.AddScoped<JwtTokenService>();
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



app.MapPost("/api/auth/register", async (RegisterOrganizationRequest request, UserManager<ApplicationUser> userManager, ApplicationDbContext db) =>
{
   await using var transaction = await db.Database.BeginTransactionAsync();
   
   
   
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


    var organization = new Organization
    {
        Name =request.OrganizationName
    };

    var location = new Location
    {
        Name = request.LocationName,
        OrganizationId = organization.Id
    };


    var membership = new OrganizationMembership
    {
        userId = user.Id,
        OrganizationId = organization.Id,
        Role = "Owner"
    };

    db.Organizations.Add(organization);
    db.Locations.Add(location);
    db.OrganizationMemberships.Add(membership);

    return Results.Created($"api/organizations/{organization.Id}", new
     {
        organization.Id,
        organization.Name,
        location = new
        {
            location.Id,
            location.Name
        },
        owner = new
        {
            user.Id,
            user.displayName,
            user.Email
        }
    });

}).AllowAnonymous().WithTags("Authentication");

//LOGIN AUTH

app.MapPost ("api/auth/login",async (
    LoginRequest request,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    JwtTokenService jwtTokenService
) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);

    if (user is null ||
        !await userManager.CheckPasswordAsync(user, request.Password))
    {
        return Results.Unauthorized();
    }

    var membership = await db.OrganizationMemberships
    .AsNoTracking().FirstOrDefaultAsync(membership => membership.userId == user.Id );

    if (membership is null)
    {
        return Results.Unauthorized();
    }

    var accessToken = jwtTokenService.CreateToken(user, membership);

    return Results.Ok(
        new
        {
            accessToken,
            tokenType ="Bearer",
            expiresIn = 7200,
            user = new
            {
                user.Id,
                user.displayName,
                user.Email
            },
            organization = new
            {
                membership.OrganizationId,
                membership.Role
            }
        }
    );
                                    
}).AllowAnonymous().WithTags("Authentication");


app.Run();
