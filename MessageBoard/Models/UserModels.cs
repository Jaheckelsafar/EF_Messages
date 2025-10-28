namespace MessageBoard.Models.UserModels;

#region "Shared"
public class UserInformation
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public UserInformation()
    {
    }

    public void Clear()
    {
        UserId = 0;
        Username = string.Empty;
        Name = string.Empty;
    }
}
#endregion "Shared"