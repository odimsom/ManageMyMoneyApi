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
        services.Configure<EmailSettings>(options =>
        {
            configuration.GetSection("EmailSettings").Bind(options);

            // Override with Environment Variables (Railway/SendGrid support)
            var smtpServer = configuration["SMTP_SERVER"];
            if (!string.IsNullOrEmpty(smtpServer)) options.SmtpServer = smtpServer;

            var smtpPort = configuration["SMTP_PORT"];
            if (!string.IsNullOrEmpty(smtpPort) && int.TryParse(smtpPort, out var port)) options.SmtpPort = port;

            var senderEmail = configuration["SENDER_EMAIL"];
            if (!string.IsNullOrEmpty(senderEmail)) options.SenderEmail = senderEmail;

            var senderName = configuration["SENDER_NAME"];
            if (!string.IsNullOrEmpty(senderName)) options.SenderName = senderName;

            var emailUsername = configuration["EMAIL_USERNAME"];
            if (!string.IsNullOrEmpty(emailUsername)) options.Username = emailUsername;

            var sendGridApiKey = configuration["SENDGRID_API_KEY"];
            if (!string.IsNullOrEmpty(sendGridApiKey)) options.Password = sendGridApiKey;
            
            // Fallback to standard SMTP_PASSWORD if SendGrid key isn't used/named differently
            var smtpPassword = configuration["SMTP_PASSWORD"];
            if (!string.IsNullOrEmpty(smtpPassword)) options.Password = smtpPassword;
            
            var enableSsl = configuration["SMTP_ENABLE_SSL"];
            if (!string.IsNullOrEmpty(enableSsl) && bool.TryParse(enableSsl, out var ssl)) options.EnableSsl = ssl;
        });

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
