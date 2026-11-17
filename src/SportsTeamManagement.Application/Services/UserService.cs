using AutoMapper;
using SportsTeamManagement.Application.Common.Interfaces;
using SportsTeamManagement.Application.Common.Models;
using SportsTeamManagement.Application.DTOs;
using SportsTeamManagement.Application.Services.Interfaces;
using SportsTeamManagement.Domain.Entities;

namespace SportsTeamManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result.Failure<UserDto>("User not found");

            var userDto = _mapper.Map<UserDto>(user);
            return Result.Success(userDto);
        }

        public async Task<Result<UserDto>> GetUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByAuth0IdAsync(auth0Id, cancellationToken);
            if (user == null)
                return Result.Failure<UserDto>("User not found");

            var userDto = _mapper.Map<UserDto>(user);
            return Result.Success(userDto);
        }

        public async Task<Result<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
            if (user == null)
                return Result.Failure<UserDto>("User not found");

            var userDto = _mapper.Map<UserDto>(user);
            return Result.Success(userDto);
        }

        public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
        {
            // Check if user already exists
            var existingUser = await _unitOfWork.Users.GetByAuth0IdAsync(createUserDto.Auth0Id, cancellationToken);
            if (existingUser != null)
                return Result.Failure<UserDto>("User with this Auth0 ID already exists");

            var existingEmailUser = await _unitOfWork.Users.GetByEmailAsync(createUserDto.Email, cancellationToken);
            if (existingEmailUser != null)
                return Result.Failure<UserDto>("User with this email already exists");

            var user = _mapper.Map<User>(createUserDto);
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);
            return Result.Success(userDto);
        }

        public async Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result.Failure<UserDto>("User not found");

            _mapper.Map(updateUserDto, user);
            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);
            return Result.Success(userDto);
        }

        public async Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result.Failure("User not found");

            await _unitOfWork.Users.DeleteAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Result.Success(userDtos);
        }
    }
}
