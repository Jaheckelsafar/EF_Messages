using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using System.Net.Http;
using MessageBoard.Models.UserModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<MessageBoard.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<AuthService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp =>
                            {
                                var AuthService = sp.GetRequiredService<AuthService>();

                                var client = new HttpClient { BaseAddress = new Uri("http://localhost:5063/") };
                                //var client = new HttpClient { BaseAddress = new Uri("http://localhost:5258/") };
                                AuthService.ApplyAuthorization(client);
                                return client;
                            });
builder.Services.AddScoped<UserInformation>();






await builder.Build().RunAsync();
