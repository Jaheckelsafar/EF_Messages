using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using EF_Messages;


public partial class Program
{
    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("./appSettings.json", optional: true, reloadOnChange: true)
            .Build();


        var builder = WebApplication.CreateBuilder(args);

        // Register MessageSystemContext with DI
        builder.Services.AddDbContext<EF_Messages.MessageSystemContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MessageSystemConnection")));

        // Add authentication services (JWT Bearer)
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                //options.Authority = "https://your-auth-server.com"; // <-- Replace with your authority
                //options.Audience = "your-api-audience";             // <-- Replace with your audience
                                                                    // For development, you can set options.RequireHttpsMetadata = false;
                options.RequireHttpsMetadata = false;
            });

        builder.Services.AddAuthorization();

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

        app.UseAuthentication();
        app.UseAuthorization();



        using (var context = new EF_Messages.MessageSystemContext(configuration))
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            app.MapGet("getthread/{tid}", (int tid) =>
            {
                var thread = MS_Thread.GetMessagesJsonByThreadId(tid, context);
                if (string.IsNullOrEmpty(thread))
                {
                    return Results.NotFound("Thread not found.");
                }
                return Results.Ok(thread);
            }
            ).WithName("GetThread")
            ;//.RequireAuthorization();

            app.Run();
        }
    }   
 }




