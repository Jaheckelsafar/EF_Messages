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
using MessageSystem.Models;
using MessageSystem.Repositories;

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
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IMessageRepository, MessageRepository>();
        builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
        builder.Services.AddScoped<ISecurityService, SecurityService>();
        builder.Services.AddDbContext<MessageSystemContext>();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        
        //var provider = builder.Services.BuildServiceProvider();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MessageSystemContext>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var msgRepo = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
            var threadRepo = scope.ServiceProvider.GetRequiredService<IThreadRepository>();
            //SeedData(context);
            //      }

            //using (var context = new MessageSystemContext(configuration))
            //{
            // Ensure the database is created
            context.Database.EnsureCreated();

            /*            UserRepository userRepo = new UserRepository(context);
                        MessageRepository msgRepo = new MessageRepository(context);
                        ThreadRepository threadRepo = new ThreadRepository(context);
            */
            AddUsers(userRepo);

            MS_User? usrJohn = MS_User.GetUserByName(context, "john");
            MS_User? usrJane = MS_User.GetUserByName(context, "jane");
            MS_User? usrJune = MS_User.GetUserByName(context, "june");
            MS_User? usrJack = MS_User.GetUserByName(context, "jack");

            MS_Message msg = msgRepo.CreateMessage(usrJohn.UserId, "Hello, World!");
            MS_Thread thrdGen = threadRepo.CreateThread("General Chat", usrJohn.UserId);
            threadRepo.AddUsersToThread(thrdGen.ThreadId, new List<int> { usrJane.UserId, usrJack.UserId, usrJune.UserId });
            threadRepo.InsertMessage(thrdGen.ThreadId, msg);
            msg = msgRepo.CreateMessage(usrJane.UserId, "Goodbye, World!");
            threadRepo.InsertMessage(thrdGen.ThreadId, msg);
            MS_Thread thrdCats = threadRepo.CreateThread("CATS!!!!!", usrJune.UserId);
            threadRepo.AddUsersToThread(thrdCats.ThreadId, new List<int> { usrJohn.UserId, usrJane.UserId, usrJack.UserId });
            msg = msgRepo.CreateMessage(usrJune.UserId, "Meow!");
            threadRepo.InsertMessage(thrdCats.ThreadId, msg);
            msg = msgRepo.CreateMessage(usrJack.UserId, "What?");
            threadRepo.InsertMessage(thrdCats.ThreadId, msg);
            threadRepo.InsertMessage(thrdCats.ThreadId, msgRepo.CreateMessage(usrJune.UserId, "Meow!"));
            threadRepo.InsertMessage(thrdGen.ThreadId, msgRepo.CreateMessage(usrJack.UserId, "Stop being so dramatic Jane."));


            // display all users and messages
            Console.WriteLine("Users and Messages in the Database:");
            Console.WriteLine("-----------------------------------");
            foreach (var usr in context.Users)
            {
                Console.WriteLine($"User: {usr.Name}");
            }


            foreach (var allMsg in context.Messages)
            {
                Console.WriteLine($"Message: {allMsg.Text} by {allMsg.SentByUser?.Name}");
            }

            // display General Chat thread and messages
            var thrd = threadRepo.GetThreadById(thrdGen.ThreadId);

            Console.WriteLine("\nGeneral Chat Thread and Messages:");
            Console.WriteLine("-----------------------------------");
            foreach (var thrdMsg in thrd.ThreadToMessages)
            {
                Console.WriteLine($"Message: {thrdMsg.Message.Text} by {thrdMsg.Message.SentByUser?.Name}");
            }

            //context.Database.EnsureDeleted();

            //Console.ReadLine();
        }
    }

    static void AddUsers(IUserRepository userRepo)
    {
        try
        {
            userRepo.CreateUser("john", "password123", "John Doe");

            List<MS_User> users = new List<MS_User> {
                new MS_User { Name = "Jane Doe", UserName = "jane", Password = "password123" },
                new MS_User { Name = "Jack Doe", UserName = "jack", Password = "password123" },
            };
            userRepo.ImportUsers(users);

        userRepo.CreateUser("june", "password123", "June Doe");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding users: {ex.Message}");
            return;
        }
    }
}




