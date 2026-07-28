using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Data;
using shiftTrade.api.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using shiftTrade.Api.Services.Auth;
using Microsoft.OpenApi;
using shiftTrade.Api.Endpoints;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter only the JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        });
});

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
app.MapDebtEndpoints();

app.MapAuthEndpoints();
app.MapEmployeeEndpoints();
app.MapShiftEndpoints();
app.MapDashboardEndpoints();
app.MapAdminEndpoints();
app.Run();
