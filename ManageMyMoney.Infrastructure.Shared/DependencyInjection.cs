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
        // Settings
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        
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

        // Email
        services.AddTransient<IEmailService, EmailService>();

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
