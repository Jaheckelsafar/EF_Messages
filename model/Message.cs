using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace EF_Messages
{
    [PrimaryKey("MessageId")]
    public class MS_Message
    {
        public int MessageId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int SentByUserId { get; set; }
        [ForeignKey("SentByUserId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public MS_User? SentByUser { get; set; }
        public List<ThreadToMessage> ThreadToMessages { get; set; } = new List<ThreadToMessage>();

        public MS_Message(string text, int sentByUserId)
        {
            this.MessageId = 0; // This will be set by the database
            this.CreatedAt = DateTime.UtcNow;
            this.Text = text;
            this.SentByUserId = sentByUserId;
        }

        public MS_Message()
        {
            this.MessageId = 0;
            this.Text = string.Empty;
            this.CreatedAt = DateTime.UtcNow;
            this.SentByUserId = 0;
            this.ThreadToMessages = new List<ThreadToMessage>();
        }
    }
}