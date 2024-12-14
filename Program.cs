using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotificationApi.Data;
using NotificationApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB settings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddScoped<MongoDbContext>();  // Scoped for MongoDbContext to avoid singleton issues

// Register application services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();  // Ensure UserService interacts with MongoDB

// Register JwtService for handling JWT token creation and validation
builder.Services.AddScoped<JwtService>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var secretKey = config["Jwt:SecretKey"];
    var issuer = config["Jwt:Issuer"];
    var audience = config["Jwt:Audience"];
    var expiryHours = double.Parse(config["Jwt:ExpiryHours"]);

    return new JwtService(secretKey, issuer, audience, expiryHours);
});

// Add logging service
builder.Services.AddLogging();

// Add authentication using JWT Bearer tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

// Add controllers to the services container
builder.Services.AddControllers();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Set application URL, fallback if not configured
var appUrl = builder.Configuration["AppUrl"] ?? "http://127.0.0.1:5145";
app.Urls.Add(appUrl);

// Configure middleware pipeline
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Swagger UI for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map controllers to endpoints
app.MapControllers();

// Start the application
app.Run();
