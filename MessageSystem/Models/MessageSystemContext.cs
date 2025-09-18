using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MessageSystem.Repositories;



namespace MessageSystem.Models
{
    public class MessageSystemContext : DbContext
    {
        private readonly ValidationInterceptor _validationInterceptor;
        // private readonly CommandLoggingInterceptor _commandLoggingInterceptor;

        private readonly IConfiguration appConfig;
        public DbSet<MS_Message> Messages { get; set; }
        public DbSet<MS_User> Users { get; set; }
        public DbSet<MS_Thread> Threads { get; set; }

        public DbSet<ThreadToUser> ThreadToUsers { get; set; }
        public DbSet<ThreadToMessage> ThreadToMessages { get; set; }

        public MessageSystemContext(IConfiguration configuration)
        {
            _validationInterceptor = new ValidationInterceptor();
            appConfig = configuration;
        }

        public MessageSystemContext()
        {
            _validationInterceptor = new ValidationInterceptor();
            appConfig = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        public MessageSystemContext(DbContextOptions<MessageSystemContext> options)
            : base(options)
        {
            _validationInterceptor = new ValidationInterceptor();
            // _commandLoggingInterceptor = new CommandLoggingInterceptor();
        }

        public MessageSystemContext(IConfiguration configuration, System.Globalization.CultureInfo cultureInfo)
            : this(configuration)
        {
            System.Globalization.CultureInfo.CurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.CurrentUICulture = cultureInfo;
            _validationInterceptor = new ValidationInterceptor();
            // _commandLoggingInterceptor = new CommandLoggingInterceptor();
        }

        public MessageSystemContext(IConfiguration configuration, ValidationInterceptor validationInterceptor)
            : this(configuration)
        {
            _validationInterceptor = validationInterceptor;
            // _commandLoggingInterceptor = new CommandLoggingInterceptor();
        }




        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(appConfig.GetConnectionString("MessageSystemConnection"));
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MessageSystemDb;Trusted_Connection=True;");
            //optionsBuilder.AddInterceptors(_validationInterceptor, _commandLoggingInterceptor);
            optionsBuilder.AddInterceptors(new ValidationInterceptor());
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

            //**************** USER SETUP ****************
            // Setup ThreadToUser primaryKey
            modelBuilder.Entity<MS_User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<MS_User>()
                .Property(u => u.UserName)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS")   //CASE INSENSITIVE
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<MS_User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<MS_User>()
                .Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<MS_User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<MS_User>()
                .Property(u => u.IsDisabled)
                .HasDefaultValue(false);

            modelBuilder.Entity<MS_User>()
                .Property(u => u.IsDeleted)
                .HasDefaultValue(false);

        }

        /*
        //this is a simple example of overriding SaveChanges to add custom validation logic
        //more complex validation can be done using SaveChangesInterceptor as shown in ValidationInterceptor class below
        //this is not required if using ValidationInterceptor
        //but is included here to show another way to do validation
        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<MS_Message>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var message = entry.Entity;
                    if (string.IsNullOrWhiteSpace(message.Text))
                    {
                        throw new InvalidOperationException("Message content cannot be empty.");
                    }
                }
            }
            return base.SaveChanges();
        }
        */  

        /*
        //this is from EF6, not EF Core
        //kept here for reference
        //in EF Core, use SaveChangesInterceptor as shown in ValidationInterceptor class below
        public static void ValidateEntities(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, MessageSystemContext context)
        {
            if (entry.Entity is MS_Message message)
            {
                MS_Message.ValidateEntity(entry as Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<MS_Message>, context);
            }
            // Add more entity types and their validation methods as needed
        }
        */

    }

    public class ValidationInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context as MessageSystemContext;
            if (context != null)
            {
                foreach (var entry in context.ChangeTracker.Entries<IValidatableObject>())
                {
                    var entity = entry.Entity;
                    var validationContext = new ValidationContext(entity, items: null );
                    Validator.ValidateObject(entity, validationContext, true);
                }

                MessageRepository mr = new MessageRepository(context);
                foreach (var entry in context.ChangeTracker.Entries<MS_Message>())
                {
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        mr.isValid(entry.Entity);
                        MS_Message.ValidateEntity(entry, context);
                    }
                }

                ThreadRepository tr = new ThreadRepository(context);
                foreach (var entry in context.ChangeTracker.Entries<MS_Thread>())
                {
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        tr.isValid(entry.Entity);
                        MS_Thread.ValidateEntity(entry, context);
                    }
                }

            }
            return base.SavingChanges(eventData, result);
        }
    }
}