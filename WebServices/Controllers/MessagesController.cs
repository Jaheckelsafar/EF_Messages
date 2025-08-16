using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EF_Messages;
using Microsoft.AspNetCore.Authorization;


[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly MessageSystemContext _context;

    public MessagesController(MessageSystemContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetMessage(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
            return NotFound();
        return Ok(message);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMessage([FromBody] MS_Message message)
    {
        var msg = MS_Message.InsertMessage(message, _context);
        return CreatedAtAction(nameof(GetMessage), new { id = msg.MessageId }, msg);
    }



}
