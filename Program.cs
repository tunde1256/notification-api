using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotificationApi.Data;
using NotificationApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application is starting...");


logger.LogInformation("Configuring MongoDB settings...");
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddScoped<MongoDbContext>();


logger.LogInformation("Registering services...");
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddScoped<JwtService>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<JwtService>>(); 
    
    var secretKey = config["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing.");
    var issuer = config["Jwt:Issuer"];
    var audience = config["Jwt:Audience"];
    var expiryHours = double.Parse(config["Jwt:ExpiryHours"]);

    return new JwtService(secretKey, issuer, audience, expiryHours, logger);
});

logger.LogInformation("Configuring authentication...");
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
        logger.LogInformation("JWT authentication configured with Issuer: {Issuer}, Audience: {Audience}", 
            builder.Configuration["Jwt:Issuer"], builder.Configuration["Jwt:Audience"]);
    });


logger.LogInformation("Adding CORS policy...");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://yourfrontend.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    logger.LogInformation("CORS policy added to allow origins: https://yourfrontend.com");
});


logger.LogInformation("Adding controllers...");
builder.Services.AddControllers();


logger.LogInformation("Adding Swagger...");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var configuredPort = builder.Configuration["AppSettings:Port"] ?? builder.Configuration["PORT"] ?? "5145";
var appUrl = $"http://0.0.0.0:{configuredPort}";
app.Urls.Add(appUrl);
logger.LogInformation("Application will run on: {Url}", appUrl);


if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Running in development environment. Enabling Swagger...");
    app.UseSwagger();
    app.UseSwaggerUI();
}

logger.LogInformation("Enabling middleware pipeline...");
app.UseCors("AllowSpecificOrigins");
logger.LogInformation("CORS policy enabled.");

app.UseRouting();
logger.LogInformation("Routing middleware enabled.");

app.UseAuthentication();
logger.LogInformation("Authentication middleware enabled.");

app.UseAuthorization();
logger.LogInformation("Authorization middleware enabled.");

logger.LogInformation("Mapping controllers...");
app.MapControllers();

logger.LogInformation("Starting application...");
app.Run();
