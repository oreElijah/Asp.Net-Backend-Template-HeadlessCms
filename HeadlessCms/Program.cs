using HeadlessCms.Data;
using HeadlessCms.Exceptions;
using HeadlessCms.Interfaces;
using HeadlessCms.Models;
using HeadlessCms.Repositories;
using HeadlessCms.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAuthorization(

);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});
builder.Host.UseSerilog();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

builder.Services.AddHangfire(
    config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddHangfireServer();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();

Log.Logger = new LoggerConfiguration()
.ReadFrom.Configuration(builder.Configuration)
.CreateLogger();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultSignInScheme =
    options.DefaultScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"] ?? throw new InvalidOperationException("JWT:SigningKey is not configured."))
        )
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.HttpContext.Request.Method == "OPTIONS")
            {
                context.NoResult();
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrWhiteSpace(jti))
            {
                context.Fail("Token is missing jti.");
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
            var revoked = await cache.GetStringAsync(jti);
            if (!string.IsNullOrWhiteSpace(revoked))
            {
                context.Fail("Token has been revoked.");
            }
        }
    };
});
builder.Services.AddScoped<IContentTypeRepository, ContentTypeRepository>();
builder.Services.AddScoped<IContentEntryRepository, ContentEntryRepository>();
builder.Services.AddScoped<IContentValueRepository, ContentValueRepository>();
builder.Services.AddScoped<IContentValueService, ContentValueService>();
builder.Services.AddScoped<IContentTypeService, ContentTypeService>();
builder.Services.AddScoped<IFieldRepository, FieldRepository>();
builder.Services.AddScoped<IFieldService, FieldService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();
app.UseRouting();

app.UseCors("AllowFrontend");

app.UseMiddleware<GlobalExceptionHandler>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    //Seeding Admin user from onset 
    var AdminEmail = builder.Configuration["AdminDefaultEmail"];
    var adminSeedPassword = builder.Configuration["AdminDefaultPwd"];

    var user = await userManager.FindByEmailAsync(AdminEmail);

    if (user == null)
    {
        if (string.IsNullOrWhiteSpace(adminSeedPassword))
        {
            Console.WriteLine("Skipping default Admin seeding: AdminPassword is not configured.");
        }
        else
        {
            Console.WriteLine("Creating default Admin user");
            var adminUser = new AppUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminSeedPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                Console.WriteLine("Failed to create default Admin user: ");
            }
        }
    }
    else
    {
        Console.WriteLine("Default Admin user already exists");
    }

}


app.UseSerilogRequestLogging();
app.MapControllers().RequireCors("AllowFrontend");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
}
await app.RunAsync();