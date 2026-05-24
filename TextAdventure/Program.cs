using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace TextAdventure;

public class Program
{
    private static string? _jwtToken = null;

    public static async Task Main()
    {
        Console.WriteLine("=== Secure Text Adventure ===");
        Console.WriteLine("Je moet inloggen voor je kan spelen.\n");

        bool loggedIn = await LoginFlow();
        if (!loggedIn)
        {
            Console.WriteLine("Aanmelden mislukt. Het spel wordt afgesloten.");
            return;
        }

        await TestSecureEndpoint();

        var world = GameSetup.CreateWorld();
        Console.WriteLine("\nWelkom bij de C# Text Adventure!");
        world.CurrentRoom.ShowDescription(world.Inventory);

        while (!world.IsGameOver && !world.IsWon)
        {
            Console.Write("\n> ");

            var rawInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(rawInput)) continue;

            var input = rawInput.Trim().ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (input.Length == 0) continue;

            string cmd = input[0];

            switch (cmd)
            {
                case "help":
                    Console.WriteLine("Commando's: help, look, inventory, go [n|e|s|w], take [item], fight, quit");
                    break;

                case "look":
                    world.CurrentRoom.ShowDescription(world.Inventory);
                    break;

                case "inventory":
                    Console.WriteLine($"Je draagt: {world.Inventory.GetDisplayList()}");
                    break;

                case "go" when input.Length > 1:
                    if (Enum.TryParse<Direction>(input[1], ignoreCase: true, out var dir))
                        await world.Move(dir, TryUnlockRoomAsync);
                    else
                        Console.WriteLine($"Ongeldige richting '{input[1]}'. Kies: n, e, s, w");
                    break;

                case "go":
                    Console.WriteLine("Waarheen? Gebruik: go [n/e/s/w]");
                    break;

                case "take" when input.Length > 1:
                    var itemName = string.Join(" ", input[1..]);
                    var item = world.CurrentRoom.TakeItem(itemName);
                    if (item != null) world.Inventory.AddItem(item);
                    Console.WriteLine(item != null ? $"Je pakt: {item.Name}" : "Dat ligt hier niet.");
                    break;

                case "take":
                    Console.WriteLine("Wat wil je oppakken? Gebruik: take [naam]");
                    break;

                case "fight":
                    world.Fight();
                    break;

                case "quit":
                    Console.WriteLine("Spel afgesloten. Tot ziens!");
                    return;

                default:
                    Console.WriteLine($"Onbekend commando '{cmd}'. Typ 'help' voor een overzicht.");
                    break;
            }
        }

        Console.WriteLine(world.IsWon ? "\n--- GEWONNEN ---" : "\n--- GAME OVER ---");
    }

    public static async Task<bool> TryUnlockRoomAsync(Room room)
    {
        Console.WriteLine($"\n🔐 '{room.Name}' is versleuteld. Start ontsleutelingsproces...");

        Console.WriteLine($"[*] Key share ophalen van API voor room '{room.EncryptedRoomId}'...");
        string keyshare;
        try
        {
            var resp = await Http.GetAsync($"/api/keys/keyshare/{room.EncryptedRoomId}");
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadFromJsonAsync<JsonElement>();
                Console.WriteLine($"[!] Toegang geweigerd: {err.GetProperty("error").GetString()}");
                return false;
            }
            var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
            keyshare = data.GetProperty("keyshare").GetString()!;
            Console.WriteLine($"[+] Key share ontvangen.");
        }
        catch { Console.WriteLine("[!] API niet bereikbaar."); return false; }

        Console.Write("[?] Voer je persoonlijke Passphrase in: ");
        var passphrase = ReadPassword();

        string certPassword = DerivePassword(keyshare, passphrase);
        Console.WriteLine($"[*] Certificaatwachtwoord afgeleid (SHA256).");

        Console.Write("[?] Pad naar CMS_Encryption_Key.pfx: ");
        var pfxPath = Console.ReadLine()?.Trim() ?? "";
        if (!System.IO.File.Exists(pfxPath))
        {
            Console.WriteLine("[!] PFX bestand niet gevonden.");
            return false;
        }

        try
        {
            var cert = new X509Certificate2(pfxPath, certPassword);
            using var rsa = cert.GetRSAPrivateKey()
                ?? throw new Exception("Geen privésleutel gevonden in certificaat.");

            byte[] encryptedBytes = Convert.FromBase64String(room.EncryptedContent!);
            byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
            string content = Encoding.UTF8.GetString(decryptedBytes);

            Console.WriteLine("\n[+] ✅ Ontsleuteling geslaagd!\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(content);
            Console.ResetColor();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Ontsleuteling mislukt: {ex.Message}");
            return false;
        }
    }

    private static string DerivePassword(string keyshare, string passphrase)
    {
        string combined = $"{keyshare}:{passphrase}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ReadPassword()
    {
        var sb = new StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace && sb.Length > 0) sb.Length--;
            else if (key.Key != ConsoleKey.Backspace) sb.Append(key.KeyChar);
        }
        Console.WriteLine();
        return sb.ToString();
    }

    private static async Task<bool> LoginFlow()
    {
        const string apiBase = "https://localhost:7298";
        const int maxAttempts = 3;

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        using var client = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            Console.Write("Gebruikersnaam: ");
            var username = Console.ReadLine()?.Trim();

            Console.Write("Wachtwoord: ");
            var password = ReadPasswordMasked();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Gebruikersnaam en wachtwoord mogen niet leeg zijn.\n");
                continue;
            }

            try
            {
                var payload = new { username, password };
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(body);
                    _jwtToken = doc.RootElement.GetProperty("token").GetString();
                    var user = doc.RootElement.GetProperty("username").GetString();
                    Console.WriteLine($"\nWelkom, {user}! Login geslaagd.");
                    return true;
                }
                else if ((int)response.StatusCode == 403)
                {
                    Console.WriteLine("Je account is vergrendeld na te veel foutieve pogingen. Neem contact op met de beheerder.");
                    return false;
                }
                else
                {
                    Console.WriteLine($"Ongeldige inloggegevens. Poging {attempt}/{maxAttempts}.\n");
                }
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Kan de server niet bereiken. Controleer of de API actief is (https://localhost:7298).");
                return false;
            }
            catch (Exception)
            {
                Console.WriteLine("Er is een onverwachte fout opgetreden. Probeer opnieuw.");
                return false;
            }
        }

        Console.WriteLine("Maximum aantal pogingen bereikt.");
        return false;
    }

    private static string ReadPasswordMasked()
    {
        var password = new System.Text.StringBuilder();
        try
        {
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
        }
        catch (InvalidOperationException)
        {
            return Console.ReadLine() ?? string.Empty;
        }
        Console.WriteLine();
        return password.ToString();
    }

    public static string? GetToken() => _jwtToken;

    private static async Task TestSecureEndpoint()
    {
        const string apiBase = "https://localhost:7298";

        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        using var client = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };

        if (!string.IsNullOrWhiteSpace(_jwtToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        try
        {
            var response = await client.GetAsync("/api/auth/me");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("\nSecure endpoint werkt:");
                Console.WriteLine(content);
            }
            else
            {
                Console.WriteLine("\nGeen toegang tot secure endpoint.");
            }
        }
        catch
        {
            Console.WriteLine("\nFout bij verbinden met secure endpoint.");
        }
    }
}