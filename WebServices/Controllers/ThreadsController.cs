using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EF_Messages;

[ApiController]
[Route("api/[controller]")]
public class ThreadsController : ControllerBase
{
    private readonly MessageSystemContext _context;

    public ThreadsController(MessageSystemContext context)
    {
        _context = context;
    }

    // GET: api/threads/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetThread(int id)
    {
        var thread = await _context.Threads
            .Include(t => t.ThreadToMessages)
                .ThenInclude(ttm => ttm.Message)
            .Include(t => t.ThreadToUsers)
                .ThenInclude(ttu => ttu.User)
            .FirstOrDefaultAsync(t => t.ThreadId == id);

        if (thread == null)
            return NotFound();

        return Ok(thread);
    }

    // GET: api/threads/{id}/messages
    [HttpGet("{id}/messages")]
    [Authorize]
    public IActionResult GetThreadMessages(int id)
    {
        var json = MS_Thread.GetMessagesJsonByThreadId(id, _context);
        return Ok(json);
    }

    // POST: api/threads
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateThread([FromBody] MS_Thread thread)
    {
        //MS_Thread.CreateThread(thread.Name,thread.CreatedByUserId, thread.ThreadToMessages.First().Message, )
        _context.Threads.Add(thread);
        await _context.SaveChangesAsync();
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