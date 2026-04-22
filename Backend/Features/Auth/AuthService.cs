using DbConnect.Data;
using DbConnect.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Backend.Features.Subscriptions;
using Backend.Features.Loyalty;

namespace Backend.Features.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILoyaltyService _loyaltyService;

        public AuthService(AppDbContext context, IConfiguration configuration, ISubscriptionService subscriptionService, ILoyaltyService loyaltyService)
        {
            _context = context;
            _configuration = configuration;
            _subscriptionService = subscriptionService;
            _loyaltyService = loyaltyService;
        }

        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new Exception("User already exists with this email.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                StudentId = request.StudentId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
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

            return await AuthenticateUser(user);
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                throw new Exception("User account is deactivated.");
            }

            return await AuthenticateUser(user);
        }

        private async Task<AuthResponse> AuthenticateUser(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "YourSuperSecretKeyForLibraryManagementSystem_AtLeast32CharsLong");
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return new AuthResponse
            {
                Token = tokenHandler.WriteToken(token),
                FullName = user.FullName,
                Role = user.Role,
                Expiry = tokenDescriptor.Expires.Value
            };
        }
    }
}
