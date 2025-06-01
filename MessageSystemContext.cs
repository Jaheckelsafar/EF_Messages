using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;



namespace EF_Messages
{
    class MessageSystemContext : DbContext
    {
        private readonly IConfiguration appConfig;
        public DbSet<MS_Message> Messages { get; set; }
        public DbSet<MS_User> Users { get; set; }
        public DbSet<MS_Thread> Threads { get; set; }

        public DbSet<ThreadToUser> ThreadToUsers { get; set; }
        public DbSet<ThreadToMessage> ThreadToMessages { get; set; }

        public MessageSystemContext(IConfiguration configuration)
        {
            appConfig = configuration;
        }

        public MessageSystemContext()
        {
            appConfig = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(appConfig.GetConnectionString("MessageSystemConnection"));
            //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MessageSystemDb;Trusted_Connection=True;");
        }

        public MessageSystemContext(IConfiguration configuration, System.Globalization.CultureInfo cultureInfo)
            : this(configuration)
        {
            System.Globalization.CultureInfo.CurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.CurrentUICulture = cultureInfo;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One-to-many: MS_User -> MS_Message
            modelBuilder.Entity<MS_Message>()
                .HasOne(m => m.SentByUser)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SentByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // One-to-many: MS_User -> MS_Thread
            modelBuilder.Entity<MS_Thread>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.Threads)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Many-to-many: MS_Thread <-> MS_Message
            modelBuilder.Entity<ThreadToMessage>()
                .HasKey(tm => new { tm.ThreadId, tm.MessageId });

            // Many-to-many: MS_Thread <-> MS_User
            modelBuilder.Entity<ThreadToUser>()
                .HasKey(tu => new { tu.ThreadId, tu.UserId });
        }
    }
}