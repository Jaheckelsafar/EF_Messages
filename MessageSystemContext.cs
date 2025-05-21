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
    }
}