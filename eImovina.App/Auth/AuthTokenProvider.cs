using eImovina.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Text.Json;

namespace eImovina.App.Auth;

public class AuthTokenProvider
{
    private const string StorageKey = "eimovina_auth";
    private readonly ProtectedSessionStorage _storage;
    private readonly HttpClient _httpClient;

    public AuthTokenProvider(ProtectedSessionStorage storage, HttpClient httpClient)
    {
        _storage = storage;
        _httpClient = httpClient;
    }

    public async Task SaveAsync(LoginResponseDto loginResponse)
    {
        var json = JsonSerializer.Serialize(loginResponse);
        await _storage.SetAsync(StorageKey, json);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
    }

    public async Task<LoginResponseDto?> GetAsync()
    {
        try
        {
            var result = await _storage.GetAsync<string>(StorageKey);
            if (!result.Success || string.IsNullOrWhiteSpace(result.Value))
                return null;

            return JsonSerializer.Deserialize<LoginResponseDto>(result.Value);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task ClearAsync()
    {
        await _storage.DeleteAsync(StorageKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
    public async Task RestoreSessionHeaderAsync()
    {
        var stored = await GetAsync();
        if (stored is not null)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", stored.AccessToken);
        }
    }
}
