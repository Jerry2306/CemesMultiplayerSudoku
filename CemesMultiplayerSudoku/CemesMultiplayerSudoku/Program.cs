using CemesMultiplayerSudoku.Client;
using CemesMultiplayerSudoku.Client.Core.Services;
using CemesMultiplayerSudoku.Components;
using CemesMultiplayerSudoku.GameSession.Hubs;
using CemesMultiplayerSudoku.GameSession.Services;
using CemesMultiplayerSudoku.GameSession.Services.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();
builder.Services.AddCors();
builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IdentificationGenerator>();
builder.Services.AddSingleton<GamesManagerService>();
builder.Services.AddSingleton<PlayerConnectionsService>();

builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<GameSessionManager>();

var app = builder.Build();

var corsOrigins = builder.Configuration["Cors:Origins"]?.Split(';') ?? Array.Empty<string>();
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(corsOrigins));

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapHub<GameSessionHub>("hub/game-session");
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

using var context = new SudokuContext();
context.Database.Migrate();

app.Run();