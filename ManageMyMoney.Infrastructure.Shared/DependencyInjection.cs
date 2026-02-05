using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ManageMyMoney.Core.Application.Common.Interfaces;
using ManageMyMoney.Infrastructure.Shared.Services.Email;
using ManageMyMoney.Infrastructure.Shared.Services.Export;
using ManageMyMoney.Infrastructure.Shared.Services.FileStorage;
using ManageMyMoney.Infrastructure.Shared.Services.Security;
using ManageMyMoney.Infrastructure.Shared.Settings;

namespace ManageMyMoney.Infrastructure.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Email Settings with environment variable fallback
        services.Configure<EmailSettings>(options =>
        {
            var emailSection = configuration.GetSection("EmailSettings");
            
            // For SendGrid API, we only need API Key and sender email
            options.Password = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
                ?? Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                ?? emailSection["Password"]
                ?? string.Empty;
            
            options.SenderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL")
                ?? emailSection["SenderEmail"]
                ?? string.Empty;
            
            options.SenderName = Environment.GetEnvironmentVariable("SENDER_NAME")
                ?? emailSection["SenderName"]
                ?? "ManageMyMoney";
            
            // These are not used by SendGrid API but kept for compatibility
            options.SmtpServer = "smtp.sendgrid.net";
            options.SmtpPort = 587;
            options.Username = "apikey";
            options.EnableSsl = true;
            options.TemplatesPath = emailSection["TemplatesPath"] ?? "Email/Templates";
        });
        
        // JWT Settings with environment variable fallback
        services.Configure<JwtSettings>(options =>
        {
            var jwtSection = configuration.GetSection("JwtSettings");
            options.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? jwtSection["SecretKey"]
                ?? "DefaultDevSecretKeyThatIsAtLeast32CharactersLong!";
            options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? jwtSection["Issuer"]
                ?? "ManageMyMoney";
            options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? jwtSection["Audience"]
                ?? "ManageMyMoneyUsers";
            options.AccessTokenExpirationMinutes = jwtSection.GetValue<int>("AccessTokenExpirationMinutes", 60);
            options.RefreshTokenExpirationDays = jwtSection.GetValue<int>("RefreshTokenExpirationDays", 7);
        });
        
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorageSettings"));

        // Email - Using SendGrid API instead of SMTP
        services.AddTransient<IEmailService, EmailServiceSendGrid>();

        // Export
        services.AddTransient<IExportService, ExcelExportService>();

        // File Storage
        services.AddTransient<IFileStorageService, LocalFileStorageService>();

        // Security
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ITokenService, JwtTokenService>();
        services.AddTransient<IDateTimeService, DateTimeService>();

        return services;
    }
}
