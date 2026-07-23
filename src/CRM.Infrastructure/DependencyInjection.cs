using CRM.Application.Interfaces.Activities;
using CRM.Application.Interfaces.Addresses;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Auth;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Contracts;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.DanhMuc;
using CRM.Application.Interfaces.Email;
using CRM.Application.Interfaces.Invoices;
using CRM.Application.Interfaces.Leads;
using CRM.Application.Interfaces.Loyalty;
using CRM.Application.Interfaces.Opportunities;
using CRM.Application.Interfaces.PhieuThuChi;
using CRM.Application.Interfaces.Products;
using CRM.Application.Interfaces.Quotes;
using CRM.Application.Interfaces.Tickets;
using CRM.Application.Interfaces.Users;
using CRM.Domain.Interfaces.Messaging;
using CRM.Domain.Interfaces.Repositories;
using CRM.Domain.Interfaces.Services;
using CRM.Infrastructure.Identity;
using CRM.Infrastructure.Messaging;
using CRM.Infrastructure.Messaging.Consumers;
using CRM.Infrastructure.OpenAI;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Mappings;
using CRM.Infrastructure.Persistence.Repositories;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.Configure<OpenAiSettings>(configuration.GetSection(OpenAiSettings.SectionName));
        services.Configure<CRM.Application.Common.Models.LoyaltyOptions>(
            configuration.GetSection(CRM.Application.Common.Models.LoyaltyOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<CrmDbContext>(options =>
            options
                .UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

        services.AddAutoMapper(typeof(PersistenceMappingProfile).Assembly);
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddSingleton<TokenVersionCache>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserManagementRepository, UserManagementRepository>();
        services.AddScoped<IAuditLogReader, AuditLogReader>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IContractMilestoneRepository, ContractMilestoneRepository>();
        services.AddScoped<ICsatRepository, CsatRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IDanhMucRepository, DanhMucRepository>();
        services.AddScoped<ILoyaltyRepository, LoyaltyRepository>();
        services.AddScoped<CRM.Application.Interfaces.Analytics.IAnalyticsRepository, CRM.Infrastructure.Persistence.Repositories.AnalyticsRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPhieuThuChiRepository, PhieuThuChiRepository>();
        services.AddScoped<CRM.Application.Interfaces.Alerts.IAlertRepository, CRM.Infrastructure.Persistence.Repositories.AlertRepository>();

        services.AddScoped<IAuditLogPublisher, AuditLogPublisher>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IQuotePublicTokenService, QuotePublicTokenService>();
        services.AddScoped<IOpenAiService, OpenAiService>();

        services.AddHostedService<AuditLogConsumerHostedService>();
        services.AddHostedService<LoyaltyDailyJobHostedService>();
        services.AddHostedService<ContractExpirationJobHostedService>();
        services.AddHostedService<PaymentReminderJobHostedService>();
        services.AddHostedService<ContractRenewalReminderJobHostedService>();
        services.AddHostedService<TicketSlaEscalationJobHostedService>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };
            options.Events = JwtBearerEventHandlers.Create();
        });

        return services;
    }
}