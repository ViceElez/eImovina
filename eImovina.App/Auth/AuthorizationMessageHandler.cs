using System.Net.Http.Headers;

namespace eImovina.App.Auth;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly AuthTokenProvider _tokenProvider;

    public AuthorizationMessageHandler(AuthTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stored = await _tokenProvider.GetAsync();

        Console.WriteLine($"[AuthHandler] Token present: {stored is not null}, " +
                           $"Token value (first 20 chars): {stored?.AccessToken?[..Math.Min(20, stored.AccessToken.Length)]}");

        if (stored is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", stored.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
