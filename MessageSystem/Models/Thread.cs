using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.Swift;
using System.Text.Json;



namespace MessageSystem.Models
{
    [PrimaryKey("ThreadId")]
    public class MS_Thread
    {
        public int ThreadId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public MS_User? CreatedByUser { get; set; }
        public List<ThreadToMessage> ThreadToMessages { get; set; } = new();
        public List<ThreadToUser> ThreadToUsers { get; set; } = new();

        public MS_Thread() { }
        public MS_Thread(string name, int createdByUserId)
        {
            Title = name;
            CreatedByUserId = createdByUserId;
            CreatedAt = DateTime.UtcNow;
        }

        
        public static void ValidateEntity(EntityEntry<MS_Thread> entry, MessageSystemContext context)
        {
            var ttm = entry.Entity;
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

        /*
        //This shouldn't be necessary. We aren't doing anything too complex here
        public static void ValidateEntity(EntityEntry<ThreadToMessage> entry, MessageSystemContext context)
        {
            var ttm = entry.Entity;
        }
        */

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



        /*
        //This shouldn't be necessary. We aren't doing anything too complex here
        public static void ValidateEntity(EntityEntry<ThreadToMessage> entry, MessageSystemContext context)
        {
            var ttu = entry.Entity;
        }
        */
    }
    #endregion
}