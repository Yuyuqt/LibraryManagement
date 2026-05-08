using BlazorWebAssembly;
using BlazorWebAssembly.Providers;
using BlazorWebAssembly.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add HttpClient with Backend address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://yuruyuruu-librarymanagement.hf.space/") });

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add Authorization Core
builder.Services.AddAuthorizationCore();

// Register Custom AuthStateProvider
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

// Register AuthService
builder.Services.AddScoped<AuthService>();

// Register ThemeService
builder.Services.AddScoped<ThemeService>();

// Register LibraryApiClient
builder.Services.AddScoped<LibraryApiClient>();

// Register WishlistService
builder.Services.AddScoped<WishlistService>();

// Register NotificationStateService
builder.Services.AddScoped<NotificationStateService>();

// Register ChatService
builder.Services.AddScoped<ChatService>();

await builder.Build().RunAsync();
