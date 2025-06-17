using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;



namespace EF_Messages
{
    public class MessageSystemContext : DbContext
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

            //**************** MS_MESSAGE SETUP ****************
            // Setup MS_Message primaryKey
            modelBuilder.Entity<MS_Message>()
                .HasKey(m => m.MessageId);

            // Setup One-to-many: MS_User -> MS_Message
            modelBuilder.Entity<MS_Message>()
                .HasOne(m => m.SentByUser)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SentByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            //**************** MS_THREAD SETUP ****************
            // Setup MS_Thread primaryKey
            modelBuilder.Entity<MS_Thread>()
                .HasKey(t => t.ThreadId);

            // One-to-many: MS_User -> MS_Thread
            modelBuilder.Entity<MS_Thread>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.Threads)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            //**************** THREADTOMESSAGE SETUP ****************
            // Setup ThreadToMessage primaryKey
            modelBuilder.Entity<ThreadToMessage>()
                .HasKey(tm => new { tm.Id });

            //**************** THREADTOUSER SETUP ****************
            // Setup ThreadToUser primaryKey
            modelBuilder.Entity<ThreadToUser>()
                .HasKey(tu => new { tu.Id });
        }
    }
}