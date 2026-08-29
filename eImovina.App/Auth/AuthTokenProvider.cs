using eImovina.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;

namespace eImovina.App.Auth;

public class AuthTokenProvider
{
    private const string StorageKey = "eimovina_auth";
    private readonly ProtectedSessionStorage _storage;

    public AuthTokenProvider(ProtectedSessionStorage storage)
    {
        _storage = storage;
    }

    public async Task SaveAsync(LoginResponseDto loginResponse)
    {
        var json = JsonSerializer.Serialize(loginResponse);
        await _storage.SetAsync(StorageKey, json);
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
    }
}
