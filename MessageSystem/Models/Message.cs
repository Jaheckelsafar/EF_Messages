using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MessageSystem.Models
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



        public MS_Message() { }

        public MS_Message(string text, int sentByUserId)
        {
            this.CreatedAt = DateTime.UtcNow;
            this.Text = text;
            this.SentByUserId = sentByUserId;
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
        }

        // custom auto Entity-level validation for EF Core, called from the DbContext SaveChanges interceptor
        // this is in addition to the IValidatableObject.Validate method above
        public static void ValidateEntity(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<MS_Message> entry, MessageSystemContext context)
        {
            var message = entry.Entity;
        }
    }
}