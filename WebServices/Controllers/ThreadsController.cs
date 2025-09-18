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
    private readonly ThreadRepository _threadRepo;
    private readonly MessageRepository _messageRepo;
    private readonly UserRepository _userRepo;
    private readonly MessageSystemContext _context;
    

    public ThreadsController(ThreadRepository threadRepo, MessageRepository messageRepo, UserRepository userRepo, MessageSystemContext context)
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

        var ret  = messages.OrderBy(m => m.ThreadToMessages.First().Position)
            .Select(m => new { m.MessageId, m.SentByUserId, m.CreatedAt, m.Text } ).ToList();

        var json = System.Text.Json.JsonSerializer.Serialize(ret);

        return Task.FromResult<IActionResult>(Ok(json));

    }

    // POST: api/threads
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateThread([FromBody] String name, [FromHeader] int userId)
    {
        //MS_Thread.CreateThread(thread.Name,thread.CreatedByUserId, thread.ThreadToMessages.First().Message, )
        var thread = _threadRepo.CreateThread(name, userId);
        


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
}