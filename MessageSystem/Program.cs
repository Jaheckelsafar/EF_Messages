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

            MS_User? usrJohn = MS_User.GetUserByName(context, "john");
            MS_User? usrJane = MS_User.GetUserByName(context, "jane");
            MS_User? usrJune = MS_User.GetUserByName(context, "june");
            MS_User? usrJack = MS_User.GetUserByName(context, "jack");


            var message = new MS_Message { Text = "Hello, World!", SentByUserId = usrJohn.UserId };
            MS_Thread thrdGen = MS_Thread.CreateThread("General Chat", message, new List<MS_User> { usrJane, usrJack, usrJune }, context);
            message = new MS_Message { Text = "Goodbye, World!", SentByUserId = usrJane.UserId };
            thrdGen.InsertMessage(
                new MS_Message { Text = "Stop being so dramatic Jane.", SentByUserId = usrJack.UserId },
                context);
            MS_Thread thrdCats = MS_Thread.CreateThread("CATS!!!!!", message, new List<MS_User> { usrJohn, usrJane, usrJack }, context);
            message = new MS_Message { Text = "Meow!", SentByUserId = usrJune.UserId };
            thrdCats.InsertMessage(message, context);
            message = new MS_Message { Text = "What?", SentByUserId = usrJack.UserId };
            thrdCats.InsertMessage(message, context);
            message = new MS_Message { Text = "Meow!", SentByUserId = usrJune.UserId };
            thrdCats.InsertMessage(message, context);

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
        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {
                MS_User.CreateUser("john", "password123", "John Doe", context);

                List<MS_User> users = new List<MS_User> {
                    new MS_User { Name = "Jane Doe", UserName = "jane", Password = "password123" },
                    new MS_User { Name = "Jack Doe", UserName = "jack", Password = "password123" },
                };
                MS_User.ImportUsers(users, context);

                MS_User.CreateUser("june", "password123", "June Doe", context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding users: {ex.Message}");
                transaction.Rollback();
                return;
            }
            transaction.Commit();
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
}



