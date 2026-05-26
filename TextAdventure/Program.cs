namespace TextAdventure;

public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== C# Text Adventure ===\n");

        // Genereer versleutelde kamers eenmalig bij opstarten
        GameSetup.InitialiseerEncryptie();

        // Verplicht inloggen via de MinimalApi-Auth
        var jwt = await InloggenAsync();
        if (jwt == null)
        {
            Console.WriteLine("Login mislukt. Start de MinimalApi-Auth en probeer opnieuw.");
            return;
        }

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
                    Console.WriteLine("Commando's: help, look, inventory, go [n|e|s|w], take [item], fight, unlock [roomId], quit");
                    break;
                case "look":
                    world.CurrentRoom.ShowDescription(world.Inventory);
                    break;
                case "inventory":
                    Console.WriteLine($"Je draagt: {world.Inventory.GetDisplayList()}");
                    break;
                case "go" when input.Length > 1:
                    if (Enum.TryParse<Direction>(input[1], out var dir)) world.Move(dir);
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
                // Ontgrendel een versleutelde kamer met keyshare (API) + passphrase (spel)
                case "unlock" when input.Length > 1:
                    await OntgrendelKamerAsync(input[1], jwt);
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

    // Vraag login-gegevens, stuur ze naar de API en geef het JWT token terug
    private static async Task<string?> InloggenAsync()
    {
        Console.WriteLine("Zorg dat MinimalApi-Auth draait op http://localhost:5056");
        Console.WriteLine("Registreer eerst via Swagger als je nog geen account hebt.\n");

        Console.Write("Gebruikersnaam: ");
        var username = Console.ReadLine() ?? "";

        Console.Write("Wachtwoord: ");
        var password = Console.ReadLine() ?? "";

        var jwt = await ApiClient.LoginAsync(username, password);

        if (jwt != null)
            Console.WriteLine($"\nIngelogd als {username}! Veel succes.\n");
        else
            Console.WriteLine("\nLogin mislukt. Controleer gebruikersnaam/wachtwoord.");

        return jwt;
    }

    // Haal keyshare op via API, vraag passphrase aan speler, ontsleutel de kamer
    private static async Task OntgrendelKamerAsync(string roomId, string jwt)
    {
        Console.Write("Geef de passphrase (hint: zoek in het spel): ");
        var passphrase = Console.ReadLine() ?? "";

        // Keyshare ophalen bij de API met het JWT token
        var keyshare = await ApiClient.GetKeyshareAsync(roomId, jwt);
        if (keyshare == null)
        {
            Console.WriteLine("Keyshare niet gevonden. Geen toegang (controleer roomId of rechten).");
            return;
        }

        // Kamer ontsleutelen: sleutel = keyshare + passphrase
        var inhoud = EncryptionHelper.OpenKamer(roomId, keyshare, passphrase);
        if (inhoud != null)
            Console.WriteLine($"\n[ONTGRENDELD] {inhoud}");
        else
            Console.WriteLine("Verkeerde passphrase. De kamer blijft gesloten.");
    }
}
