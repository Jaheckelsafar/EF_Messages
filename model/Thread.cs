using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace EF_Messages
{
    [PrimaryKey("Id")]
    public class MS_Thread
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public MS_User? CreatedByUser { get; set; }

        public List<ThreadToMessage> ThreadToMessages { get; set; } = new List<ThreadToMessage>();
        public List<ThreadToUser> ThreadToUsers { get; set; } = new List<ThreadToUser>();

        public MS_Thread(string name, int createdByUserId)
        {
            this.Id = 0; // This will be set by the database
            this.CreatedAt = DateTime.UtcNow;
            this.Name = name;
            this.CreatedByUserId = createdByUserId;

            ThreadToUser threadToUser = new ThreadToUser(this.Id, this.CreatedByUserId, true);
            this.ThreadToUsers.Add(threadToUser);
            threadToUser.Thread = this;
        }

        public MS_Thread()
        {
            this.Id = 0; // This will be set by the database
            this.CreatedAt = DateTime.UtcNow;
            this.Name = string.Empty;
            this.CreatedByUserId = 0;
        }
    }

    [PrimaryKey("Id")]
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

        public int position { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ThreadToMessage(int threadId, int messageId, int position = 0)
        {
            this.Id = 0; // This will be set by the database
            this.CreatedAt = DateTime.UtcNow;
            this.position = position;
            this.ThreadId = threadId;
            this.MessageId = messageId;
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
}