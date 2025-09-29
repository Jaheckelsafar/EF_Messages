using MessageSystem.Repositories; 
using MessageSystem.Models;


public class MessageService
{
    private readonly IMessageRepository _messageRepository;

    public MessageService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public MS_Message InsertMessage(MS_Message message)
    {
        return _messageRepository.InsertMessage(message);
    }

    public MS_Message CreateMessage(int sentByUserId, string text)
    {
        return _messageRepository.CreateMessage(sentByUserId, text);
    }

    // Add more service methods as needed
}   