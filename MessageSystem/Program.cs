using EF_Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


partial class Program
{
    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
            .Build();


            var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();


        using (var context = new MessageSystemContext(configuration))
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            AddUsers(context);
            AddThreads(context);

            // Add a new message
            var message = new MS_Message { Text = "Hello, World!", SentByUserId = context.Users.Where(s => s.Name == "John Doe").First().UserId };
            AddMessageToThread(configuration, message, 1); // Assuming threadId 1 is "General Chat"
            message = new MS_Message { Text = "Goodbye, World!", SentByUserId = context.Users.Where(s => s.Name == "Jane Doe").First().UserId };
            AddMessageToThread(configuration, message, 1); // Assuming threadId 1 is "General Chat"
            message = new MS_Message { Text = "Meow!", SentByUserId = context.Users.Where(s => s.Name == "June Doe").First().UserId };
            AddMessageToThread(configuration, message, 2); // Assuming threadId 2 is "CATS!!!!!"
            message = new MS_Message { Text = "Stop being so dramatic Jane.", SentByUserId = context.Users.Where(s => s.Name == "Jack Doe").First().UserId };
            AddMessageToThread(configuration, message, 1); // Assuming threadId 1 is "General Chat"
            message = new MS_Message { Text = "What?", SentByUserId = context.Users.Where(s => s.Name == "Jack Doe").First().UserId };
            AddMessageToThread(configuration, message, 2); // Assuming threadId 2 is "CATS!!!!!"
            message = new MS_Message { Text = "Meow!", SentByUserId = context.Users.Where(s => s.Name == "June Doe").First().UserId };
            AddMessageToThread(configuration, message, 2); // Assuming threadId 2 is "CATS!!!!!"


            // display all users and messages
            Console.WriteLine("Users and Messages in the Database:");
            Console.WriteLine("-----------------------------------");
            foreach (var usr in context.Users)
            {
                Console.WriteLine($"User: {usr.Name}");
            }


            foreach (var msg in context.Messages)
            {
                Console.WriteLine($"Message: {msg.Text} by {msg.SentByUser?.Name}");
            }

            // display General Chat thread and messages
            var thrd = context.Threads
                .Where(t => t.Name == "General Chat")
                .Include(t => t.ThreadToMessages)
                    .ThenInclude(tm => tm.Message)
                        .ThenInclude(m => m.SentByUser)
                .Include(t => t.ThreadToUsers)
                    .ThenInclude(tu => tu.User)
                .FirstOrDefault();



            Console.WriteLine("\nGeneral Chat Thread and Messages:");
            Console.WriteLine("-----------------------------------");
            foreach (var msg in thrd.ThreadToMessages)
            {
                Console.WriteLine($"Message: {msg.Message.Text} by {msg.Message.SentByUser?.Name}");
            }


            //context.Database.EnsureDeleted();

            //Console.ReadLine();
        }
    }

    static void AddUsers(MessageSystemContext context)
    {
        // Add a new user if it does not exist
        var user = context.Users.Where(s => s.Name == "John Doe").FirstOrDefault();
        if (user == null)
        {
            user = new MS_User { Name = "John Doe", UserName = "john", Password = "password123" };
            context.Users.Add(user);
            context.SaveChanges();
        }

        // Add a collection of users if they do not exist
        if ((context.Users.Where(s => s.Name == "Jane Doe").FirstOrDefault() == null) &&
            (context.Users.Where(s => s.Name == "Jack Doe").FirstOrDefault() == null)
        )
        {
            List<MS_User> users = new List<MS_User>
            {
                new MS_User { Name = "Jane Doe", UserName = "jane", Password = "password123" },
                new MS_User { Name = "Jack Doe", UserName = "jack", Password = "password123" },
            };
            context.Users.AddRange(users);
        }

        // add user to the context rather than the set within the context
        user = context.Users.Where(s => s.Name == "June Doe").FirstOrDefault();
        if (user == null)
        {
            user = new MS_User { Name = "June Doe", UserName = "june", Password = "password123" };
            context.Add<MS_User>(user);
            context.SaveChanges();
        }
    }

    static void AddThreads(MessageSystemContext context)
    {
        // Add a new thread
        var thread = context.Threads.Where(s => s.Name == "General Chat").FirstOrDefault();
        if (thread == null)
        {
            thread = new MS_Thread("General Chat", context.Users.Where(s => s.Name == "John Doe").First().UserId);
            context.Threads.Add(thread);
            context.SaveChanges();
        }
        thread = context.Threads.Where(s => s.Name == "CATS!!!!!").FirstOrDefault();
        if (thread == null)
        {
            thread = new MS_Thread("CATS!!!!!", context.Users.Where(s => s.Name == "June Doe").First().UserId);
            context.Threads.Add(thread);
            context.SaveChanges();
        }
    }

    static void AddMessageToThread(IConfiguration configuration, MS_Message message, int threadId)
    {
        using (var context = new MessageSystemContext(configuration))
        {
            if (message == null || threadId <= 0)
            {
                throw new ArgumentException("Message or threadId cannot be null or zero.");
            }

            // insert message into the database
            context.Messages.Add(message);
            context.SaveChanges();

            // insert threadToMessage into the database
            if (context.Threads.Where(t => t.ThreadId == threadId).Count() == 0)
            {
                throw new ArgumentException("Thread does not exist.");
            }
            var threadToMessage = new ThreadToMessage
            {
                MessageId = message.MessageId,
                ThreadId = threadId,
                Position = context.ThreadToMessages.Count(t => t.ThreadId == threadId) + 1
            };
            context.ThreadToMessages.Add(threadToMessage);
            context.SaveChanges();

            // insert threadToUser into the database if it does not exist
            // Check if the user is already in the thread
            if (context.ThreadToUsers.Where(
                th => th.UserId == message.SentByUserId
                && th.ThreadId == context.Threads.Where(
                    th => th.ThreadId == threadId
                ).First().ThreadId
            ).Count() == 0)
            {
                var threadToUser = new ThreadToUser
                {
                    UserId = message.SentByUserId,
                    ThreadId = threadId
                };
                context.ThreadToUsers.Add(threadToUser);
                context.SaveChanges();
            }
        }
    }
}



