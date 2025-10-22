using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EF_Messages;
using Microsoft.AspNetCore.Authorization;
using MessageSystem.Models;
using MessageSystem.Repositories;
using System.Text.Json;
using Microsoft.VisualBasic;


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

    // GET: api/messages/getMessage/{id}
    [HttpGet("getMessage/{id}")]
    [Authorize]
    public async Task<IActionResult> GetMessage(int id)
    {
        var message = _messageRepo.GetMessageById(id);
        if (message == null)
            return NotFound();

        var json = JsonSerializer.Serialize(new { message.MessageId, message.SentByUserId, message.SentByUser?.UserName, message.CreatedAt, message.Text });
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return Ok(content);
    }

    // GET: api/messages/getMessages/{ids}
    [HttpGet("getMessages{ids}")]
    [Authorize]
    public async Task<IActionResult> GetMessages(List<int> ids)
    {
        var messages = _messageRepo.GetMessagesByIds(ids);
        if (messages.Count == 0)
            return NotFound();

        var msgs = messages.Select(m => new { m.MessageId, m.SentByUserId, m.SentByUser?.UserName, m.CreatedAt, m.Text }).ToList();


        var json = JsonSerializer.Serialize(msgs);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        return Ok(content);
    }

    // GET: api/messages/getMessagesInThread/{threadId}
    [HttpGet("getMessagesInThread/{threadId}")]
    [Authorize]
    public async Task<IActionResult> GetMessagesInThread(int threadId)
    {
        var thread = _threadRepo.GetThreadById(threadId);
        if (thread == null)
            return NotFound("Thread not found.");

        var messages = _messageRepo.GetMessagesInThread(threadId);
        if (messages.Count == 0)
            return NotFound("No messages found in the specified thread.");

        return Ok(messages);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMessage([FromBody] MS_Message message)
    {
        var msg = _messageRepo.InsertMessage(message);
        return CreatedAtAction(nameof(GetMessage), new { id = msg.MessageId }, msg);
    }
}
