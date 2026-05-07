using System.IO;
using System.Text;
using DbConnect.Data;
using DbConnect.Entities;
using Backend.Features.Auth;
using Backend.Features.Users;
using Backend.Features.Books;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Backend.Features.Subscriptions;
using Backend.Features.Categories;
using Backend.Features.Borrowings;
using Backend.Features.Loyalty;
using Backend.Features.Notification;
using Backend.Features.Wishlist;
using Backend.Features.Wallet;
using FirebaseAdmin;

using Google.Apis.Auth.OAuth2;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Firebase Admin SDK Initialization
var firebaseConfig = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
if (!string.IsNullOrEmpty(firebaseConfig))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromJson(firebaseConfig)
    });
}
else if (File.Exists("LibraryFirebase.json"))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile("LibraryFirebase.json")
    });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWasm",
        policy => policy.WithOrigins(
                "https://localhost:7058",
                "http://localhost:5158",
                "https://librarymanagement-eosin-omega.vercel.app",
                "https://yuruyuruu-librarymanagement.hf.space"
            )
            .AllowAnyMethod()
            .AllowAnyHeader());
});


builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library Management API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBorrowingService, BorrowingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddHostedService<NotificationBackgroundService>();


// Register Loyalty API Client
builder.Services.AddHttpClient<ILoyaltyService, LoyaltyService>(client =>
{
    client.BaseAddress = new Uri("http://150.95.88.91:4100");
});

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"] ?? "YourSuperSecretKeyForLibraryManagementSystem_AtLeast32CharsLong");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowWasm");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

