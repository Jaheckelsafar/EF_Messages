using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
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
                            var principal = SecurityService.ValidateJwtToken(token, SecurityService.Issuer, SecurityService.Audience, SecurityService.SecretKey);
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
        builder.Services.AddControllers();




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


        using (var context = new EF_Messages.MessageSystemContext(configuration))
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            app.Run();
        }
    }   
 }




