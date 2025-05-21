using EF_Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;


var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
    .Build();


using (var context = new MessageSystemContext(configuration))
{
    // Ensure the database is created
    context.Database.EnsureCreated();

    // Add a new user
    var user = context.Users.Where(s => s.Name == "John Doe").FirstOrDefault();
    if (user == null)
    {
        user = new MS_User { Name = "John Doe" };
        context.Users.Add(user);
        context.SaveChanges();
    }
    user = context.Users.Where(s => s.Name == "Jane Doe").FirstOrDefault();
    if (user == null)
    {
        user = new MS_User { Name = "Jane Doe" };
        context.Users.Add(user);
        context.SaveChanges();
    }
    user = context.Users.Where(s => s.Name == "Jack Doe").FirstOrDefault();
    if (user == null)
    {
        user = new MS_User { Name = "Jack Doe" };
        context.Users.Add(user);
        context.SaveChanges();
    }
    user = context.Users.Where(s => s.Name == "June Doe").FirstOrDefault();
    if (user == null)
    {
        user = new MS_User { Name = "June Doe" };
        context.Users.Add(user);
        context.SaveChanges();
    }

    // Add a new thread
    var thread = new EF_Messages.MS_Thread { Name = "General Chat", CreatedByUserId = context.Users.Where(s => s.Name == "John Doe").First().Id };
    context.Threads.Add(thread);
    context.SaveChanges();
    var threadToUser = new ThreadToUser { UserId = thread.CreatedByUserId, ThreadId = thread.Id, Owner = true };
    context.ThreadToUsers.Add(threadToUser);
    context.SaveChanges();
    thread = new EF_Messages.MS_Thread { Name = "CATS!!!!!", CreatedByUserId = context.Users.Where(s => s.Name == "June Doe").First().Id };
    context.Threads.Add(thread);
    context.SaveChanges();
    threadToUser = new ThreadToUser { UserId = thread.CreatedByUserId, ThreadId = thread.Id, Owner = true };
    context.ThreadToUsers.Add(threadToUser);
    context.SaveChanges();


    // Add a new message
    var message = new MS_Message { Text = "Hello, World!", SentByUserId = context.Users.Where(s => s.Name == "John Doe").First().Id };
    context.Messages.Add(message);
    context.SaveChanges();
    if (context.ThreadToUsers.Where(th => th.UserId == message.SentByUserId && th.ThreadId == context.Threads.Where(th => th.Name == "General Chat").First().Id).Count() == 0)
    {
        threadToUser = new ThreadToUser { UserId = message.SentByUserId, ThreadId = context.Threads.Where(th => th.Name == "General Chat").First().Id };
        context.ThreadToUsers.Add(threadToUser);
        context.SaveChanges();
    }


    message = new MS_Message { Text = "Goodbye, World!", SentByUserId = context.Users.Where(s => s.Name == "Jane Doe").First().Id };
    context.Messages.Add(message);
    context.SaveChanges();
    if (context.ThreadToUsers.Where(th => th.UserId == message.SentByUserId && th.ThreadId == context.Threads.Where(th => th.Name == "General Chat").First().Id).Count() == 0)
    {
        threadToUser = new ThreadToUser { UserId = message.SentByUserId, ThreadId = context.Threads.Where(th => th.Name == "General Chat").First().Id };
        context.ThreadToUsers.Add(threadToUser);
        context.SaveChanges();
    }

    message = new MS_Message { Text = "Meow!", SentByUserId = context.Users.Where(s => s.Name == "June Doe").First().Id };
    context.Messages.Add(message);
    context.SaveChanges();
    if (context.ThreadToUsers.Where(th => th.UserId == message.SentByUserId && th.ThreadId == context.Threads.Where(th => th.Name == "General Chat").First().Id).Count() == 0)
    {
        threadToUser = new ThreadToUser { UserId = message.SentByUserId, ThreadId = context.Threads.Where(th => th.Name == "General Chat").First().Id };
        context.ThreadToUsers.Add(threadToUser);
        context.SaveChanges();
    }

    message = new MS_Message { Text = "Stop being so dramatic Jane.", SentByUserId = context.Users.Where(s => s.Name == "Jack Doe").First().Id };
    context.Messages.Add(message);
    context.SaveChanges();
    if (context.ThreadToUsers.Where(th => th.UserId == message.SentByUserId && th.ThreadId == context.Threads.Where(th => th.Name == "General Chat").First().Id).Count() == 0)
    {
        threadToUser = new ThreadToUser { UserId = message.SentByUserId, ThreadId = context.Threads.Where(th => th.Name == "General Chat").First().Id };
        context.ThreadToUsers.Add(threadToUser);
        context.SaveChanges();
    }

    message = new MS_Message { Text = "What?", SentByUserId = context.Users.Where(s => s.Name == "Jack Doe").First().Id };
    context.Messages.Add(message);
    context.SaveChanges();
    if (context.ThreadToUsers.Where(th => th.UserId == message.SentByUserId && th.ThreadId == context.Threads.Where(th => th.Name == "CATS!!!!!").First().Id).Count() == 0)
    {
        threadToUser = new ThreadToUser { UserId = message.SentByUserId, ThreadId = context.Threads.Where(th => th.Name == "CATS!!!!!").First().Id };
        context.ThreadToUsers.Add(threadToUser);
        context.SaveChanges();
    }


    message = new MS_Message { Text = "Meow!", SentByUserId = context.Users.Where(s => s.Name == "June Doe").First().Id };
    context.Messages.Add(message);
    context.SaveChanges();
    if (context.ThreadToUsers.Where(th => th.UserId == message.SentByUserId && th.ThreadId == context.Threads.Where(th => th.Name == "CATS!!!!!").First().Id).Count() == 0)
    {
        threadToUser = new ThreadToUser { UserId = message.SentByUserId, ThreadId = context.Threads.Where(th => th.Name == "CATS!!!!!").First().Id };
        context.ThreadToUsers.Add(threadToUser);
        context.SaveChanges();
    }


    foreach (var usr in context.Users)
    {
        Console.WriteLine($"User: {usr.Name}");
    }


    foreach (var msg in context.Messages)
    {
        Console.WriteLine($"Message: {msg.Text} by {msg.SentByUser?.Name}");
    }

    //context.Database.EnsureDeleted();
}



