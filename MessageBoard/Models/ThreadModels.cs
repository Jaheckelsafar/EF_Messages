
namespace MessageBoard.Models.ThreadModels;



#region responses
public class ThreadResponse
{
    public string ThreadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public List<int> MessageIds { get; set; } = new();
    public List<int> UserIds { get; set; } = new();

    public ThreadResponse()
    {
        ThreadId = string.Empty;
    }
}

public class ThreadCardResponse
{
    public int ThreadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;

    public ThreadCardResponse()
    {
        ThreadId = 0;
    }
}
#endregion responses