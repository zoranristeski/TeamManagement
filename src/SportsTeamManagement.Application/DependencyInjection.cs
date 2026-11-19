using Microsoft.Extensions.DependencyInjection;
using SportsTeamManagement.Application.Common.Mappings;
using SportsTeamManagement.Application.Services;
using SportsTeamManagement.Application.Services.Interfaces;

namespace SportsTeamManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<ITrainingSessionService, TrainingSessionService>();
            services.AddScoped<IClubService, ClubService>();
            services.AddScoped<ITrainingGroupService, TrainingGroupService>();

            return services;
        }
    }
}
