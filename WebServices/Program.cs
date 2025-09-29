using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MessageSystem.Models;
using MessageSystem.Services;
using MessageSystem.Repositories;

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
        builder.Services.AddDbContext<MessageSystemContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("MessageSystemConnection")));

        // Register repositories and services with DI
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IMessageRepository, MessageRepository>();
        builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
        builder.Services.AddScoped<ISecurityService, SecurityService>();



        // Add authentication services (JWT Bearer)
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "http://localhost:5063/"; // <-- Replace with your authority
                options.Audience = SecurityService.Audience;        // <-- Replace with your audience
                                                                    // For development, you can set options.RequireHttpsMetadata = false;
                options.RequireHttpsMetadata = false;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
                        {
                            token = token.Substring("Bearer ".Length);
                            // Resolve SecurityService from DI
                            var securityService = context.HttpContext.RequestServices.GetRequiredService<ISecurityService>();
                            var principal = securityService.ValidateJwtToken(token, SecurityService.Issuer, SecurityService.Audience, SecurityService.SecretKey);
                            if (principal != null)
                            {
                                context.Principal = principal;
                                context.Success();
                            }
                            else
                            {
                                context.Fail("Invalid JWT token.");
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();
        // Add controllers with JSON options to handle reference loops
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });




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
        
        app.MapControllers();


        using (var context = new MessageSystemContext(configuration))
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            app.Run();
        }
    }   
 }




