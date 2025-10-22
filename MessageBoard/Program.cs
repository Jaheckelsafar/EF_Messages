using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MessageBoard;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<AuthService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp =>
                            {
                                var AuthService = sp.GetRequiredService<AuthService>();

                                var client = new HttpClient { BaseAddress = new Uri("http://localhost:5258/") };
                                AuthService.ApplyAuthorization(client);
                                return client;
                            })
;





await builder.Build().RunAsync();
