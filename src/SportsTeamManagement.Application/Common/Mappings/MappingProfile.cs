using AutoMapper;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Club, ClubDto>();
            CreateMap<CreateClubDto, Club>();
            CreateMap<UpdateClubDto, Club>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TrainingGroup, TrainingGroupDto>();
            CreateMap<CreateTrainingGroupDto, TrainingGroup>();

            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Student mappings
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => 
                    DateTime.UtcNow.Year - src.DateOfBirth.Year - 
                    (DateTime.UtcNow.DayOfYear < src.DateOfBirth.DayOfYear ? 1 : 0)))
                .ForMember(dest => dest.TotalAttendances, opt => opt.MapFrom(src => src.Attendances.Count(a => a.IsPresent)));
            CreateMap<CreateStudentDto, Student>();
            CreateMap<UpdateStudentDto, Student>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<StudentRegistrationDto, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Domain.Enums.UserRole.Student));

            // Trainer mappings
            CreateMap<Trainer, TrainerDto>();
            CreateMap<CreateTrainerDto, Trainer>();
            CreateMap<UpdateTrainerDto, Trainer>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Training Session mappings
            CreateMap<TrainingSession, TrainingSessionDto>()
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.EndTime - src.StartTime))
                .ForMember(dest => dest.AttendeeCount, opt => opt.MapFrom(src => src.Attendances.Count));
            CreateMap<CreateTrainingSessionDto, TrainingSession>();
            CreateMap<UpdateTrainingSessionDto, TrainingSession>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Attendance mappings
            CreateMap<Attendance, AttendanceDto>();
            CreateMap<CreateAttendanceDto, Attendance>();
            CreateMap<UpdateAttendanceDto, Attendance>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Event mappings
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.EndDate - src.StartDate))
                .ForMember(dest => dest.RegisteredCount, opt => opt.MapFrom(src => src.Registrations.Count(r => r.IsConfirmed)))
                .ForMember(dest => dest.IsRegistrationOpen, opt => opt.MapFrom(src => 
                    DateTime.UtcNow <= src.RegistrationDeadline && src.IsActive));
            CreateMap<CreateEventDto, Event>();
            CreateMap<UpdateEventDto, Event>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Event Registration mappings
            CreateMap<EventRegistration, EventRegistrationDto>();
        }
    }
}
