using System.Dynamic;
using System.Net.Http.Headers;
public class AuthService
{
    private string? _token;
    private int? _loggedInUserId;
    public int? LoiggedInUserId
    {
        get { return _loggedInUserId; }
    }

    public bool isLoggedIn
    {
        get { return _token != null; }
    }

    public AuthService()
    {
        _token = null;
    }

    public void SetBearerToken(string token)
    {
        _token = token;
    }

    public void ClearBearerToken()
    {
        _token = null;
    }

    public void ApplyAuthorization(HttpClient client)
    {
        if (!string.IsNullOrEmpty(_token))
        {
            if (client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Remove("Authorization");
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }
}