using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;
using Backend.Features.Subscriptions;
using Backend.Features.Loyalty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Features.Users
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto> CreateUserAsync(UserCreateRequest request);
        Task<UserDto?> UpdateUserAsync(Guid id, UserUpdateRequest request);
        Task<bool> UpdateUserRoleAsync(Guid id, string role);
        Task<bool> DeleteUserAsync(Guid id);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILoyaltyService _loyaltyService;

        public UserService(AppDbContext context, ISubscriptionService subscriptionService, ILoyaltyService loyaltyService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _loyaltyService = loyaltyService;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateUserAsync(UserCreateRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new Exception("Email already registered.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                StudentId = request.StudentId,
                Role = "Member", // Default role
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(user.StudentId))
            {
                await _subscriptionService.SubscribeUserAsync(user.Id, 3); // 3 is "Basic Yearly"
            }

            // Loyalty Integration: Register the new user and process SIGNUP event
            string externalUserId = user.Id.ToString();
            string userMobile = user.PhoneNumber ?? "0000000000";
            await _loyaltyService.RegisterUserAsync(externalUserId, user.Email, userMobile);
            await _loyaltyService.ProcessEventAsync(
                externalUserId: externalUserId,
                eventKey: "SIGNUP",
                eventValue: 20,
                referenceId: $"USR-{user.Id}",
                description: "New User Registration",
                email: user.Email,
                mobile: userMobile
            );

            return MapToDto(user);
        }

        public async Task<UserDto?> UpdateUserAsync(Guid id, UserUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            bool isNewlyStudent = string.IsNullOrEmpty(user.StudentId) && !string.IsNullOrEmpty(request.StudentId);

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;
            user.StudentId = request.StudentId;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (isNewlyStudent)
            {
                await _subscriptionService.SubscribeUserAsync(user.Id, 3);
            }

            return MapToDto(user);
        }

        public async Task<bool> UpdateUserRoleAsync(Guid id, string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Soft delete
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                StudentId = user.StudentId,
                Address = user.Address,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                BanStatus = user.BanStatus,
                SuspensionEndDate = user.SuspensionEndDate
            };
        }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name => FullName;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? StudentId { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? BanStatus { get; set; }
        public DateTime? SuspensionEndDate { get; set; }
    }

    public class UserCreateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? StudentId { get; set; }
        public string? Address { get; set; }
    }

    public class UserUpdateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? StudentId { get; set; }
        public string? Address { get; set; }
    }

    public class UserRoleUpdateRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
