using Microsoft.AspNetCore.Mvc;
using MessageSystem.Repositories;
using MessageSystem.Models;
using EF_Messages;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly ISecurityService _securityService;
    private readonly MessageSystemContext _context;

    public UsersController(IUserRepository userRepo, ISecurityService securityService, MessageSystemContext context)
    {
        _userRepo = userRepo;
        _securityService = securityService;
        _context = context;
    }

    // POST: api/users/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var token = _securityService.Login(request.UserName, request.Password);
        if (token == null)
            return Unauthorized();
        
        var user = _userRepo.GetUserByName(request.UserName);

        return Ok(new { Token = token, UserId = user?.UserId });
    }
}

// DTO for login request
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}