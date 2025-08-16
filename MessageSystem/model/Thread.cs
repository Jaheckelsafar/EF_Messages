using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.Swift;
using System.Text.Json;



namespace EF_Messages
{
    [PrimaryKey("ThreadId")]
    public class MS_Thread
    {
        public int ThreadId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public MS_User? CreatedByUser { get; set; }

        public List<ThreadToMessage> ThreadToMessages { get; set; } = new List<ThreadToMessage>();
        public List<ThreadToUser> ThreadToUsers { get; set; } = new List<ThreadToUser>();

        public delegate void MessageAddedEventHandler(object sender, MS_Message message);
        public event MessageAddedEventHandler? MessageAdded;


        public MS_Thread(string name, int createdByUserId)
        {
            this.ThreadId = 0; // This will be set by the database
            this.CreatedAt = DateTime.UtcNow;
            this.Name = name;
            this.CreatedByUserId = createdByUserId;

            ThreadToUser threadToUser = new ThreadToUser(this.ThreadId, this.CreatedByUserId, true);
            this.ThreadToUsers.Add(threadToUser);
            threadToUser.Thread = this;
        }

        public MS_Thread()
        {
            this.ThreadId = 0; // This will be set by the database
            this.CreatedAt = DateTime.UtcNow;
            this.Name = string.Empty;
            this.CreatedByUserId = 0;
        }


        public static string GetMessagesJsonByThreadId(int threadId, MessageSystemContext dbContext)
        {
            var messages = dbContext.Set<ThreadToMessage>()
                .Where(ttm => ttm.ThreadId == threadId)
                .OrderBy(ttm => ttm.Position)
                .Select(ttm => new
                {
                    ttm.MessageId,
                    ttm.Position,
                    ttm.CreatedAt,
                    Message = ttm.Message != null ? new
                    {
                        ttm.Message.MessageId,
                        ttm.Message.Text,
                        ttm.Message.CreatedAt,
                        ttm.Message.SentByUserId,
                        ttm.Message.SentByUser.Name
                    } : null
                })
                .ToList();

            return JsonSerializer.Serialize(messages);
        }

        public static MS_Thread CreateThread(string title, MS_Message msg, List<MS_User> recipients, MessageSystemContext context)
        {
            return CreateThread(title, msg, recipients.Select(r => r.UserId).ToList(), context);
        }

        public static MS_Thread CreateThread(string title, MS_Message msg, List<int> recipientsIDs, MessageSystemContext context)
        {
            MS_Thread thread;

            // Validate inputs
            if (msg.SentByUserId <= 0)
                throw new ArgumentException("Thread owner user ID must be greater than zero.");
            if (recipientsIDs == null || recipientsIDs.Count == 0)
                throw new ArgumentException("At least one recipient is required to create a thread.");
            if (msg == null)
                throw new ArgumentException("Message cannot be null.");
            if (title == null || title.Trim().Length == 0)
                throw new ArgumentException("Thread title cannot be null.");
            if (!MS_User.ValidateUserIds(context, recipientsIDs))
                throw new ArgumentException("Not all recipient Ids are valid.");

            //begin database interactions
            using (var trasnsaction = context.Database.BeginTransaction())
            {
                // Create thread                
                thread = new MS_Thread(title, msg.SentByUserId);
                context.Threads.Add(thread);
                context.SaveChanges();

                //Add users for thread
                foreach (var userId in recipientsIDs)
                    thread.AddUser(userId, context, userId == msg.SentByUserId);

                thread.InsertMessage(msg, context);

                trasnsaction.Commit();
            }

            return thread;
        }

        //create an entry in the UserToThread table
        public void AddUser(int userId, MessageSystemContext context, bool owner = false)
        {
            try
            {
                // Validate state of database before adding user to thread
                var user = context.Users.Find(userId);
                if (user == null)
                    throw new ArgumentException($"User with id {userId} does not exist.");

                if (owner && ThreadToUsers.Any(tu => tu.Owner && tu.ThreadId == ThreadId && tu.UserId != userId))
                    throw new InvalidOperationException("Only one owner is allowed in the thread.");

                bool alreadyExists = context.ThreadToUsers.Any(tu => tu.ThreadId == ThreadId && tu.UserId == userId);
                if (alreadyExists)
                    throw new InvalidOperationException("User is already in the thread.");


                var threadToUser = new ThreadToUser(ThreadId, userId, owner);
                context.ThreadToUsers.Add(threadToUser);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding user {userId} to thread {ThreadId}: {ex.Message}");
            }
            context.SaveChanges();
        }

        public static void RemoveUserFromThread(int threadId, int userId, MessageSystemContext context)
        {
            var threadToUser = context.ThreadToUsers.FirstOrDefault(tu => tu.ThreadId == threadId && tu.UserId == userId);
            if (threadToUser == null)
                throw new InvalidOperationException($"User with id {userId} is not in thread {threadId}.");
            if (threadToUser.Owner)
            {
                throw new InvalidOperationException($"User with id {userId} is the owner of thread {threadId}. Please reassing ownership before removing User.");
            }

            context.ThreadToUsers.Remove(threadToUser);
            context.SaveChanges();
        }

        public void InsertMessage(MS_Message message, MessageSystemContext context)
        {
            bool inTransaction = context.Database.CurrentTransaction != null;
            IDbContextTransaction transaction = null;

            var thread = context.Threads.Find(ThreadId);
            if (thread is null)
                throw new ArgumentException($"Thread with id {ThreadId} does not exist.");

            if (message is null)
                throw new ArgumentException("Message cannot be null.");

            //begin transaction if not already in one
            if (inTransaction) context.Database.UseTransaction(context.Database.CurrentTransaction?.GetDbTransaction());
            else transaction = context.Database.BeginTransaction();

            try
            {
                // Save the message if not already saved
                if (message.MessageId <= 0)
                    MS_Message.InsertMessage(message, context);

                // Determine the position of message in thread
                int nextPosition = context.ThreadToMessages
                    .Where(ttm => ttm.ThreadId == ThreadId)
                    .Select(ttm => ttm.Position)
                    .OrderByDescending(p => p)
                    .FirstOrDefault() + 1;

                // Place message in thread
                var threadToMessage = new ThreadToMessage(ThreadId, message.MessageId, nextPosition);
                context.ThreadToMessages.Add(threadToMessage);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Rollback transaction if we are responsible for it
                if (!inTransaction && transaction != null)
                    transaction.Rollback();
                throw new Exception($"Error inserting message into thread: {ex.Message}");
            }
            if (!inTransaction && transaction != null)
                transaction.Commit();

            MessageAdded?.Invoke(this, message);
        }

    }



    #region Thread Associated Helper classes
    // thread structure for the message system
    public class ThreadToMessage
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        [ForeignKey("ThreadId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public MS_Thread? Thread { get; set; }

        public int MessageId { get; set; }
        [ForeignKey("MessageId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public MS_Message? Message { get; set; }

        public int Position { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ThreadToMessage(int threadId, int messageId, int position = 0)
        {
            this.Id = 0; // This will be set by the database
            this.CreatedAt = DateTime.UtcNow;
            this.Position = position;
            this.ThreadId = threadId;
            this.MessageId = messageId;
        }

        public ThreadToMessage()
        {
            this.Id = 0; // This will be set by the database
            this.ThreadId = 0;
            this.MessageId = 0;
            this.Position = 0;
            this.CreatedAt = DateTime.UtcNow;
        }
    }

    [PrimaryKey("Id")]
    [Table("ThreadToUser")]
    public class ThreadToUser
    {
        public int Id { get; set; }
        public bool Owner { get; set; }
        public int ThreadId { get; set; }
        [ForeignKey("ThreadId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public MS_Thread? Thread { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public MS_User? User { get; set; }
        public int LastReadPosition { get; set; } = 0;

        public ThreadToUser(int threadId, int userId, bool owner = false)
        {
            this.Owner = owner;
            this.ThreadId = threadId;
            this.UserId = userId;
        }
        public ThreadToUser()
        {
            this.Owner = false;
            this.ThreadId = 0;
            this.UserId = 0;
        }

    }
#endregion
}