using System.Net.Http.Json;

namespace TextAdventure
{
    // Verbinding met de MinimalApi-Auth via HTTPS (https://localhost:7298)
    public static class ApiClient
    {
        private const string ApiUrl = "https://localhost:7298";

        // Dev-certificaat accepteren (zelfondertekend tijdens ontwikkeling)
        private static readonly HttpClient _http = new(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        // Login: stuurt gebruikersnaam + wachtwoord, geeft JWT token terug (of null bij fout)
        public static async Task<string?> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(
                    $"{ApiUrl}/api/auth/login", new { username, password });

                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.token;
            }
            catch
            {
                // API niet bereikbaar
                return null;
            }
        }

        // Haal keyshare op voor een kamer via HTTPS (vereist geldig JWT token)
        public static async Task<string?> GetKeyshareAsync(string roomId, string jwt)
        {
            try
            {
                // JWT meesturen als Bearer token in de header
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{ApiUrl}/api/keys/keyshare/{roomId}");
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<KeyshareResponse>();
                return result?.keyshare;
            }
            catch
            {
                return null;
            }
        }

        // Hulpklassen voor JSON-deserialisatie van API-antwoorden
        private record LoginResponse(string token, string username, string role);
        private record KeyshareResponse(string roomId, string keyshare);
    }
}
