using Microsoft.AspNetCore.Mvc;
using MessageSystem.Repositories;
using MessageSystem.Models;
using EF_Messages;
using Microsoft.AspNetCore.Authorization;

namespace MessageSystem.Controllers;

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
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("UserName and Password are required.");
            
        var token = _securityService.Login(request.UserName, request.Password);
        if (token == null)
            return Unauthorized();

        var user = _userRepo.GetUserByName(request.UserName);

        return Ok(new { Token = token, UserId = user?.UserId });
    }

    // POST: api/users/register
    [HttpPost("register")]
    public IActionResult Register([FromBody] MinimalUserData userData)
    {
        var userName = userData.UserName;
        var password = userData.Password;
        var name = userData.Name;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name))
            return BadRequest("UserName, Password, and Name are required.");

        if (_userRepo.UserNameExists(userName))
            return Conflict("UserName already exists.");

        var newUser = new MS_User
        {
            UserName = userName,
            Password = password,
            Name = name
        };

        if (string.IsNullOrWhiteSpace(newUser.UserName) || string.IsNullOrWhiteSpace(newUser.Password) || string.IsNullOrWhiteSpace(newUser.Name))
            return BadRequest("UserName, Password, and Name are required.");

        try
        {
            var user = _userRepo.CreateUser(newUser.UserName, newUser.Password, newUser.Name);
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetUser(int id)
    {
        var user = _userRepo.GetUserById(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }
}

// DTO for login request
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// DTO for login request
public class MinimalUserData
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}