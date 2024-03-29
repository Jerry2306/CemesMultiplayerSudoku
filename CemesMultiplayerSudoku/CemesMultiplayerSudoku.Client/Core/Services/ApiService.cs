using System.Net;
using Microsoft.AspNetCore.Components;

namespace CemesMultiplayerSudoku.Client.Core.Services;

public class ApiService
{
    public bool HasSessionToken { get; private set; }
    public bool IsUserSet { get; private set; }

    public string UserName { get; set; } = string.Empty;
    public string UserColor { get; set; } = string.Empty;

    private readonly HttpClient _client;

    public ApiService(
        IHttpClientFactory clientFactory,
        NavigationManager navigationManager)
    {
        _client = clientFactory.CreateClient();
        _client.BaseAddress = new Uri(navigationManager.BaseUri);
    }

    public void SetSessionToken(string sessionToken)
    {
        HasSessionToken = true;
        _client.DefaultRequestHeaders.Add("x-session-token", sessionToken);
    }

    public async Task<string?> SetUser(string name, string color)
    {
        var postUrl = $"game-session/set-user?name={WebUtility.UrlEncode(name)}";
        if (!string.IsNullOrEmpty(color))
            postUrl += $"&color={WebUtility.UrlEncode(color)}";

        var result = await _client.PostAsync(postUrl, null);
        if (result.IsSuccessStatusCode)
        {
            UserName = name;
            UserColor = color;
            IsUserSet = true;
            return null;
        }

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> CreateGame(string name)
    {
        var result = await _client.PostAsync($"game-session/create-game?name={WebUtility.UrlEncode(name)}", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> JoinGame(string code)
    {
        var result = await _client.PostAsync($"game-session/join-game?code={WebUtility.UrlEncode(code)}", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> LeaveGame()
    {
        var result = await _client.PostAsync("game-session/leave-game", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> SelectField(int x, int y)
    {
        var result = await _client.PostAsync($"game-session/select-field?x={x}&y={y}", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> DeselectField()
    {
        var result = await _client.PostAsync("game-session/deselect-field", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> SetNumber(int value, int x, int y)
    {
        var result = await _client.PostAsync($"game-session/set-number?value={value}&x={x}&y={y}", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> EraseNumber(int x, int y)
    {
        var result = await _client.PostAsync($"game-session/erase-number?x={x}&y={y}", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> SetNote(int value, int x, int y)
    {
        var result = await _client.PostAsync($"game-session/set-note?value={value}&x={x}&y={y}", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> EraseNote(int value, int x, int y)
    {
        var result = await _client.PostAsync($"game-session/erase-note?value={value}&x={x}&y={y}", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> GetHint(int x, int y)
    {
        var result = await _client.PostAsync($"game-session/get-hint?x={x}&y={y}", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string?> NewBoard()
    {
        var result = await _client.PostAsync("game-session/new-board", null);
        if (result.IsSuccessStatusCode)
            return null;

        return await result.Content.ReadAsStringAsync();
    }
}