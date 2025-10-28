namespace MessageBoard.Models.Security;

public class LoginResponse
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string? Token { get; set; }
}

public class LoginModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
} 



public class RegisterModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Name { get; set; }
}
public class RegisterResponse
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
}
