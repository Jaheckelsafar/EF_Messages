using Microsoft.AspNetCore.Mvc;
using EF_Messages;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly MessageSystemContext _context;

    public UsersController(MessageSystemContext context)
    {
        _context = context;
    }

    // POST: api/users/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var token = SecurityService.Login(request.UserName, request.Password, _context);
        if (token == null)
            return Unauthorized();

        return Ok(new { Token = token });
    }
}

// DTO for login request
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}