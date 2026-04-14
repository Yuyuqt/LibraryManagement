using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Backend.Features.Users
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

        [HttpPost]
        public async Task<ActionResult<UserCreateResponse>> CreateUser([FromBody] UserCreateRequest request)
        {
            var response = await _userService.CreateUser(request);
            return Ok(response);
        }
    }
}
