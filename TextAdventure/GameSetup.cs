namespace TextAdventure;
public static class GameSetup
{
    public static Building CreateWorld()
    {
        var start = new Room("Start", "Je staat in het midden van een stoffige kerker.");
        var links = new Room("Dodelijke Gang", "Zodra je de kamer binnenstapt, valt het plafond naar beneden!") { IsDeadly = true };
        var rechts = new Room("Schatkamer", "Een kleine kamer met een kist op de grond.");
        var boven = new Room("De Uitgang", "Gefeliciteerd! Je hebt de weg naar buiten gevonden.") { IsWin = true, RequiredItem = "Sleutel" };
        var beneden = new Room("Kelder", "Het is hier koud en vochtig.");
        var monsterKamer = new Room("Monsterkamer", "Een donker hol dat naar rottend vlees stinkt.") { MonsterAlive = true };

        var secretVault = new Room(
            "Secret Vault",
            "Een zware stalen deur staat voor je. Een digitaal slot knippert rood.")
        {
            IsEncrypted = true,
            EncryptedRoomId = "room_vault"
        };

        var adminSanctum = new Room(
            "Admin Sanctum",
            "Een mysterieuze deur met een Admin-symbool. Alleen beheerders mogen passeren.")
        {
            IsEncrypted = true,
            EncryptedRoomId = "room_godmode"
        };

        LoadEncryptedContent(secretVault, "room_vault.enc");
        LoadEncryptedContent(adminSanctum, "room_godmode.enc");

        rechts.AddItem(new Item("Sleutel", "Een gouden sleutel."));
        beneden.AddItem(new Item("Zwaard", "Een vlijmscherp zwaard."));

        start.AddExit(Direction.w, links);
        start.AddExit(Direction.e, rechts);
        start.AddExit(Direction.n, boven);
        start.AddExit(Direction.s, beneden);
        beneden.AddExit(Direction.n, start);
        beneden.AddExit(Direction.s, monsterKamer);
        monsterKamer.AddExit(Direction.n, beneden);
        rechts.AddExit(Direction.w, start);


        rechts.AddExit(Direction.e, secretVault);
        links.AddExit(Direction.w, adminSanctum);


        return new Building(start);
    }

    private static void LoadEncryptedContent(Room room, string filename)
    {
        // Look next to the executable, or in the current directory
        var paths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, filename),
            Path.Combine(Directory.GetCurrentDirectory(), filename),
        };

        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                room.EncryptedContent = File.ReadAllText(path).Trim();
                return;
            }
        }

        Console.WriteLine($"[!] Waarschuwing: {filename} niet gevonden. Voer de EncryptionTool eerst uit.");
    }
}
