using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Users
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserCreateRequest request)
        {
            try
            {
                var response = await _userService.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetUser), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UserUpdateRequest request)
        {
            var updatedUser = await _userService.UpdateUserAsync(id, request);
            if (updatedUser == null) return NotFound();
            return Ok(updatedUser);
        }

        [HttpPatch("{id}/role")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UserRoleUpdateRequest request)
        {
            var success = await _userService.UpdateUserRoleAsync(id, request.Role);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}

