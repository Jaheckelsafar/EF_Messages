using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EF_Messages
{
    [PrimaryKey("MessageId")]
    public class MS_Message : IValidatableObject
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
        }



        public static MS_Message InsertMessage(MS_Message message, MessageSystemContext context)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message), "Message cannot be null");
            if (string.IsNullOrWhiteSpace(message.Text))
                throw new ArgumentException("Message text cannot be empty", nameof(message.Text));
            if (!MS_User.IsUserIdValid(message.SentByUserId, false))
                throw new ArgumentException("SentByUserId must be a valid user ID", nameof(message.SentByUserId));

            context.Messages.Add(message);
            context.SaveChanges();

            return message;
        }

        public static MS_Message CreateMessage(int SentByUserId, string Text, MessageSystemContext context)
        {
            if (!MS_User.IsUserIdValid(SentByUserId, false))
                throw new ArgumentException("SentByUserId must be a valid user ID", nameof(SentByUserId));
            if (string.IsNullOrWhiteSpace(Text))
                throw new ArgumentException("Message text cannot be empty", nameof(Text));
            MS_Message msg = new MS_Message(Text, SentByUserId);
            context.Messages.Add(msg);
            context.SaveChanges();

            return msg;
        }

        public bool IsValid(MessageSystemContext context)
        {
            var results = new List<ValidationResult>();
            //return this.Validate(new ValidationContext(context)).Count() == 0;
            return Validator.TryValidateObject(this, new ValidationContext(context), validationResults: results, validateAllProperties: true);

        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Text))
                yield return new ValidationResult("Message text cannot be empty", new[] { nameof(Text) });
                
            if (!MS_User.IsUserIdValid(SentByUserId, false))
                yield return new ValidationResult("SentByUserId must be a valid user ID", new[] { nameof(SentByUserId) });
        }

        // Entity-level validation for EF Core
        public static void ValidateEntity(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<MS_Message> entry, MessageSystemContext context)
        {
            var message = entry.Entity;
            if (!MS_User.IsUserIdValid(message.SentByUserId, false))
            {
                throw new ValidationException($"SentByUserId {message.SentByUserId} is not a valid user ID.");
            }
        }
    }
}