using System.Collections.Generic;

namespace MessageSystem.Models.Services
{
    public class ThreadService
    {
        private readonly Repositories.ThreadRepository _repo;

        public ThreadService(Repositories.ThreadRepository repo)
        {
            _repo = repo;
        }

        public MS_Thread CreateThread(string title, MS_Message msg, List<int> recipientIds)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Thread title cannot be empty.", nameof(title));
            if (msg == null)
                throw new ArgumentException("Message cannot be null.", nameof(msg));
            if (recipientIds == null || recipientIds.Count == 0)
                throw new ArgumentException("At least one recipient is required.", nameof(recipientIds));

            var thread = new MS_Thread(title, msg.SentByUserId);
            thread.ThreadToUsers = recipientIds.Select(id => new ThreadToUser(thread.ThreadId, id, id == msg.SentByUserId)).ToList();

            _repo.CreateThread(thread.Title, msg.SentByUserId);
            _repo.AddUsersToThread(thread.ThreadId, recipientIds, false);
            _repo.InsertMessage(thread.ThreadId, msg);

            return thread;
        }

        public MS_Thread? GetThreadById(int threadId)
        {
            return _repo.GetThreadById(threadId);
        }

        public List<MS_Thread> GetThreadsForUser(int userId)
        {
            return _repo.GetThreadsForUser(userId);
        }

        public void AddUserToThread(int threadId, int userId, bool owner = false)
        {
            _repo.AddUserToThread(threadId, userId, owner);
        }

        public void RemoveUserFromThread(int threadId, int userId)
        {
            _repo.RemoveUserFromThread(threadId, userId);
        }
    }
}