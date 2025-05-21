using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace EF_Messages
{
    [PrimaryKey("Id")]
    public class MS_User
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public List<MS_Message>? Messages { get; set; }
        public List<MS_Thread>? Threads { get; set; }
    }
}