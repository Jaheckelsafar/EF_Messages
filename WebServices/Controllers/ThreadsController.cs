using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EF_Messages;
using MessageSystem.Models;
using MessageSystem.Repositories;

[ApiController]
[Route("api/[controller]")]
public class ThreadsController : ControllerBase
{
    private readonly IThreadRepository _threadRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly IUserRepository _userRepo;
    private readonly MessageSystemContext _context;


    public ThreadsController(IThreadRepository threadRepo, IMessageRepository messageRepo, IUserRepository userRepo, MessageSystemContext context)
    {
        _threadRepo = threadRepo;
        _messageRepo = messageRepo;
        _userRepo = userRepo;
        _context = context;
    }

    // GET: api/threads/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetThread(int id)
    {
        var thread = _threadRepo.GetThreadById(id);

        if (thread == null)
            return NotFound();

        return Ok(thread);
    }

    // GET: api/threads/{id}/messages
    [HttpGet("{id}/messages")]
    [Authorize]
    public Task<IActionResult> GetMessages(int id)
    {
        var messages = _messageRepo.GetMessagesInThread(id);
        if (messages == null || messages.Count == 0)
            return Task.FromResult<IActionResult>(NotFound());

        var ret = messages.OrderBy(m => m.ThreadToMessages.First().Position)
            .Select(m => new { m.MessageId, m.SentByUserId, m.CreatedAt, m.Text }).ToList();

        var json = System.Text.Json.JsonSerializer.Serialize(ret);

        return Task.FromResult<IActionResult>(Ok(json));

    }

    // POST: api/threads/Create
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateThread([FromBody] CreateThreadRequest request)
    {
        //MS_Thread.CreateThread(thread.Name,thread.CreatedByUserId, thread.ThreadToMessages.First().Message, )
        var thread = _threadRepo.CreateThread(request.Name, request.UserId);

        return CreatedAtAction(nameof(GetThread), new { id = thread.ThreadId }, thread);
    }

    // DELETE: api/threads/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteThread(int id)
    {
        var thread = await _context.Threads.FindAsync(id);
        if (thread == null)
            return NotFound();

        _context.Threads.Remove(thread);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST: api/threads/{threadId}/addUser/{userId}
    [HttpPost("{threadId}/addUser/{userId}")]
    [Authorize]
    public IActionResult AddUserToThread(int threadId, int userId, [FromQuery] bool owner = false)
    {
        var thread = _threadRepo.GetThreadById(threadId);
        if (thread == null)
            return NotFound("Thread not found");

        var user = _userRepo.GetUserById(userId);
        if (user == null)
            return NotFound("User not found");

        _threadRepo.AddUsersToThread(threadId, new List<int> { userId }, owner);
        return Ok();
    }
    
    public class CreateThreadRequest
    {
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}