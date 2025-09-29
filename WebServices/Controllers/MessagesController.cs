using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EF_Messages;
using Microsoft.AspNetCore.Authorization;
using MessageSystem.Models;
using MessageSystem.Repositories;


[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private MessageRepository _messageRepo;
    private ThreadRepository _threadRepo;
    private UserRepository _userRepo;
    private readonly MessageSystemContext _context;


    public MessagesController(MessageRepository messageRepo, ThreadRepository threadRepo, UserRepository userRepo, MessageSystemContext context)
    {
        _messageRepo = messageRepo;
        _threadRepo = threadRepo;
        _userRepo = userRepo;
        _context = context;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetMessage(int id)
    {
        var message = _messageRepo.GetMessageById(id);
        if (message == null)
            return NotFound();
        return Ok(message);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMessage([FromBody] MS_Message message)
    {
        var msg = _messageRepo.InsertMessage(message);
        return CreatedAtAction(nameof(GetMessage), new { id = msg.MessageId }, msg);
    }

    [HttpGet("thread/{threadId}")]
    [Authorize]
    public async Task<IActionResult> GetMessagesInThread(int threadId)
    {
        var messages = _messageRepo.GetMessagesInThread(threadId);
        return Ok(messages);
    }



}
