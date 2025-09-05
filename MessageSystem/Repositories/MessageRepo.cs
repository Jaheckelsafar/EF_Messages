using System.Transactions;
using MessageSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MessageSystem.Repositories
{
    public interface IMessageRepository
    {
        MS_Message InsertMessage(MS_Message message);
        MS_Message CreateMessage(int sentByUserId, string text);
  
    }

    public class MessageRepository
    {
        private readonly MessageSystemContext _context;

        public MessageRepository(MessageSystemContext context)
        {
            _context = context;
        }

        public MS_Message InsertMessage(MS_Message message)
        {
            UserRepository userRepo = new UserRepository(_context);

            IDbContextTransaction? currentTransaction = _context.Database.CurrentTransaction;
            bool inTransaction = _context.Database.CurrentTransaction != null;
            if (inTransaction)
                //currentTransaction = _context.Database.CurrentTransaction;
                currentTransaction = _context.Database.UseTransaction(_context.Database.CurrentTransaction.GetDbTransaction());
            else
                currentTransaction = _context.Database.BeginTransaction();

            try
            {
                if (isValid(message))
                {
                    _context.Messages.Add(message);
                    _context.SaveChanges();
                }
            }
            catch
            {
                if (!inTransaction)
                    currentTransaction?.Rollback();
                throw;
            }
            if (!inTransaction)
                currentTransaction?.Commit();
                
            return message;
        }

        public MS_Message CreateMessage(int sentByUserId, string text)
        {
            var msg = new MS_Message(text, sentByUserId);
            return InsertMessage(msg);
        }

        public MS_Message? GetMessageById(int messageId)
        {
            return _context.Messages.Find(messageId);
        }
        
        public bool isValid(MS_Message message)
        {
            var userRepo = new UserRepository(_context);
            if (message == null)
                return false;
            if (string.IsNullOrWhiteSpace(message.Text))
                return false;
            if (!userRepo.IsUserIdValid(message.SentByUserId, false))
                return false;
            return true;
        }

        // Add more repository methods as needed
    }
}        