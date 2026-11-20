using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Infrastructure.Data;
using SportsTeamManagement.Infrastructure.Repositories;
using SportsTeamManagement.Infrastructure.Services;

namespace SportsTeamManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddHttpClient("AuthenticationProviders", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            return services;
        }
    }
}
