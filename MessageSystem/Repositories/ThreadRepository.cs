using MessageSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MessageSystem.Repositories
{
    public class ThreadRepository
    {
        private readonly MessageSystemContext _context;

        public ThreadRepository(MessageSystemContext context)
        {
            _context = context;
        }

        public MS_Thread? GetThreadById(int threadId)
        {
            return _context.Threads
                .Include(t => t.ThreadToMessages)
                    .ThenInclude(ttm => ttm.Message)
                .Include(t => t.ThreadToUsers)
                    .ThenInclude(ttu => ttu.User)
                .FirstOrDefault(t => t.ThreadId == threadId);
        }

        public List<MS_Thread> GetThreadsForUser(int userId)
        {
            return _context.Threads
                .Include(t => t.ThreadToUsers)
                .Where(t => t.ThreadToUsers.Any(ttu => ttu.UserId == userId))
                .ToList();
        }

        public MS_Thread InsertThread(MS_Thread thread)
        {
            if (!isValid(thread))
                throw new ArgumentException("Thread is not valid", nameof(thread));
            _context.Threads.Add(thread);
            _context.SaveChanges();
            return thread;
        }

        public MS_Thread CreateThread(string title, int createdByUserId)
        {
            IDbContextTransaction? currentTransaction = _context.Database.CurrentTransaction;
            bool inTransaction = _context.Database.CurrentTransaction != null;
            if (inTransaction)
                //currentTransaction = _context.Database.CurrentTransaction;
                currentTransaction = _context.Database.UseTransaction(_context.Database.CurrentTransaction.GetDbTransaction());
            else
                currentTransaction = _context.Database.BeginTransaction();


            MS_Thread newThread = new MS_Thread
            {
                Title = title,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = createdByUserId,
            };

            if (!isValid(newThread))
                throw new ArgumentException("Thread is not valid", nameof(newThread));
            _context.Threads.Add(newThread);
            _context.SaveChanges();
            _context.ThreadToUsers.Add(new ThreadToUser(newThread.ThreadId, createdByUserId, true));
            _context.SaveChanges();
            return newThread;
        }

        #region User operations
        public void AddUserToThread(int threadId, int userId, bool owner = false)
        {
            AddUsersToThread(threadId, new List<int> { userId }, owner);
        }

        public void AddUsersToThread(int threadId, List<int> userIds, bool owner = false)
        {
            List<int> ValidUserIds = new UserRepository(_context).GetValiduserIds(userIds);
            foreach (var id in ValidUserIds)
            {
                var threadToUser = new ThreadToUser(threadId, id, owner);
                _context.ThreadToUsers.Add(threadToUser);
            }
            _context.SaveChanges();
        }


        public void RemoveUserFromThread(int threadId, int userId)
        {
            var threadToUser = _context.ThreadToUsers.FirstOrDefault(tu => tu.ThreadId == threadId && tu.UserId == userId);
            if (threadToUser != null)
            {
                _context.ThreadToUsers.Remove(threadToUser);
                _context.SaveChanges();
            }
        }
        #endregion


        public void InsertMessage(int threadId, MS_Message message)
        {
            MessageRepository msgRepo = new MessageRepository(_context);
            if (message == null)
                throw new ArgumentNullException(nameof(message), "Message cannot be null");
            if (_context.Threads.Find(threadId) == null)
                throw new ArgumentException("ThreadId must be a valid thread ID", nameof(threadId));
            if (message.MessageId == 0)
                message = msgRepo.InsertMessage(message);
            else if (msgRepo.GetMessageById(message.MessageId) == null)
                throw new ArgumentException("MessageId must be a valid message ID", nameof(message.MessageId));
            var threadToMessage = new ThreadToMessage(threadId, message.MessageId);
            _context.ThreadToMessages.Add(threadToMessage);
            _context.SaveChanges();
        }

        public bool isValid(MS_Thread thread)
        {
            var userRepo = new UserRepository(_context);
            if (thread == null)
                return false;
            if (string.IsNullOrWhiteSpace(thread.Title))
                return false;
            if (!userRepo.IsUserIdValid(thread.CreatedByUserId))
                return false;
            foreach (var ttu in thread.ThreadToUsers)
            {
                if (!userRepo.IsUserIdValid(ttu.UserId))
                    return false;
            }
            return true;
        }

        //should these be here? or in their own repos?
        //I'm putting them here as TTU and TTM are not likely to be used outside of threads
        //and this avoids the need for multiple repos in the service layer
        public bool isTTUValid(ThreadToUser ttu)
        {
            if (ttu == null)
                return false;
            if (!new UserRepository(_context).IsUserIdValid(ttu.UserId))
                return false;
            if (_context.Threads.Find(ttu.ThreadId) == null)
                return false;
            return true;
        }

        public bool isTTMValid(ThreadToMessage ttm)
        {
            if (ttm == null)
                return false;
            if (_context.Threads.Find(ttm.ThreadId) == null)
                return false;
            if (_context.Messages.Find(ttm.MessageId) == null)
                return false;
            return true;
        }
        


    }
}