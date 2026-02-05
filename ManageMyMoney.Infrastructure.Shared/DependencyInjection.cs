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
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
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
