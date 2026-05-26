using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TextAdventure;
public record LoginResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("role")] string Role
);


public class AuthenticationService
{

    private readonly HttpClient _http = new() { BaseAddress = new Uri("http://localhost:5056") };
    public bool IsLoggedIn { get; private set; } = false;
    public string? Token { get; private set; } = null;

    public async Task LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", new { username, password });

        if (response.IsSuccessStatusCode)
        {
            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Token = login?.Token;
            IsLoggedIn = true;
            Console.WriteLine("Login geslaagd.");
        }
        else
        {
            Token = null;
            IsLoggedIn = false;
            Console.WriteLine("Login mislukt. Controleer je gebruikersnaam en wachtwoord.");
        }
    }

    public void Dispose() => _http.Dispose();
}
