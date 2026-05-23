using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace TextAdventure;

public class Program
{
    public static async Task Main()
    {
        
        var auth = new AuthenticationService();

        Console.WriteLine("Inloggen");
        Console.Write("Gebruikersnaam: ");
        var username = Console.ReadLine()!;
        Console.Write("Wachtwoord: ");
        var password = Console.ReadLine()!;

        await auth.LoginAsync(username, password);

        if (auth.IsLoggedIn)
        {

            var world = GameSetup.CreateWorld();
            Console.WriteLine("Welkom bij de C# Text Adventure!");
            world.CurrentRoom.ShowDescription(world.Inventory);

            while (!world.IsGameOver && !world.IsWon)
            {
                Console.Write("\n> ");
                var input = Console.ReadLine()?.ToLower().Split(' ');
                if (input == null || input.Length == 0) continue;

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
                        if (Enum.TryParse<Direction>(input[1], out var dir)) await world.Move(dir, TryUnlockRoomAsync);
                        else Console.WriteLine("Ongeldige richting.");
                        break;
                    case "take" when input.Length > 1:
                        var item = world.CurrentRoom.TakeItem(input[1]);
                        if (item != null) world.Inventory.AddItem(item);
                        Console.WriteLine(item != null ? $"Je pakt: {item.Name}" : "Dat ligt hier niet.");
                        break;
                    case "fight":
                        world.Fight();
                        break;
                    case "quit":
                        return;
                    default:
                        Console.WriteLine("Onbekend commando. Typ 'help'.");
                        break;
                }
            }

            Console.WriteLine(world.IsWon ? "\n--- GEWONNEN ---" : "\n--- GAME OVER ---");
        }
    }

    public static async Task<bool> TryUnlockRoomAsync(Room room)
    {
        Console.WriteLine($"\n🔐 '{room.Name}' is versleuteld. Start ontsleutelingsproces...");

        // 1. Get key share from API
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

        // 2. Ask for passphrase
        Console.Write("[?] Voer je persoonlijke Passphrase in: ");
        var passphrase = ReadPassword();

        // 3. Derive certificate password
        string certPassword = DerivePassword(keyshare, passphrase);
        Console.WriteLine($"[*] Certificaatwachtwoord afgeleid (SHA256).");

        // 4. Ask for PFX path
        Console.Write("[?] Pad naar CMS_Encryption_Key.pfx: ");
        var pfxPath = Console.ReadLine()?.Trim() ?? "";
        if (!System.IO.File.Exists(pfxPath))
        {
            Console.WriteLine("[!] PFX bestand niet gevonden.");
            return false;
        }

        // 5. Load certificate and decrypt
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
}
