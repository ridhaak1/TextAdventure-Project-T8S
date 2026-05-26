namespace TextAdventure;

public static class GameSetup
{
    // Keyshares komen overeen met appsettings.json in MinimalApi-Auth
    private const string KeyshareVault    = "a3f9e2b1c4d7f0e5a8b2c6d9"; // room_vault
    private const string PassphraseVault  = "draken";                    // hint: inscriptie in Kelder

    private const string KeyshareGodmode  = "9f3c1b7e2a4d8c0f6b5e3a1d"; // room_godmode (Admin only)
    private const string PassphraseGodmode = "geheim";                   // hint: briefje in Monsterkamer

    public static Building CreateWorld()
    {
        // Alle kamers aanmaken
        var start        = new Room("Start", "Je staat in het midden van een stoffige kerker.");
        var links        = new Room("Dodelijke Gang", "Zodra je de kamer binnenstapt, valt het plafond naar beneden!") { IsDeadly = true };
        var rechts       = new Room("Schatkamer", "Een kleine kamer met een kist. Aan de oostzijde staat een zware kluisdeur.");
        var boven        = new Room("De Uitgang", "Gefeliciteerd! Je hebt de weg naar buiten gevonden.") { IsWin = true, RequiredItem = "Sleutel" };
        var beneden      = new Room("Kelder", "Het is hier koud en vochtig. Je ziet een inscriptie op de muur: 'draken'");
        var monsterKamer = new Room("Monsterkamer", "Een donker hol. Op de grond ligt een verkreukeld briefje: 'geheim'") { MonsterAlive = true };

        // Kamer 1: versleuteld met room_vault (toegankelijk voor alle spelers)
        var kluis        = new Room("Kluis", "Een versleutelde kamer. Typ 'unlock room_vault' om te openen.");

        // Kamer 2: versleuteld met room_godmode (alleen Admin kan keyshare ophalen)
        var geheimeKamer = new Room("Geheime Kamer", "Een zware deur met een rood waarschuwingslicht. Typ 'unlock room_godmode' (vereist Admin).");

        // Items plaatsen
        rechts.AddItem(new Item("Sleutel", "Een gouden sleutel."));
        beneden.AddItem(new Item("Zwaard", "Een vlijmscherp zwaard."));

        // Uitgangen koppelen
        start.AddExit(Direction.w, links);
        start.AddExit(Direction.e, rechts);
        start.AddExit(Direction.n, boven);
        start.AddExit(Direction.s, beneden);
        beneden.AddExit(Direction.n, start);
        beneden.AddExit(Direction.s, monsterKamer);
        beneden.AddExit(Direction.e, geheimeKamer); // Kamer 2: oost vanuit Kelder
        monsterKamer.AddExit(Direction.n, beneden);
        rechts.AddExit(Direction.w, start);
        rechts.AddExit(Direction.e, kluis);          // Kamer 1: oost vanuit Schatkamer
        kluis.AddExit(Direction.w, rechts);
        geheimeKamer.AddExit(Direction.w, beneden);

        return new Building(start);
    }

    // Genereer de versleutelde .enc bestanden eenmalig bij opstart van het spel
    public static void InitialiseerEncryptie()
    {
        // Kamer 1: toegankelijk voor alle ingelogde spelers
        EncryptionHelper.InitialiseerKamer(
            "room_vault",
            KeyshareVault,
            PassphraseVault,
            "Je betreedt de geheime kluis! Er liggen stapels goud en diamanten. Je hebt de verborgen schat gevonden!"
        );

        // Kamer 2: keyshare enkel op te halen met Admin-rol via de API
        EncryptionHelper.InitialiseerKamer(
            "room_godmode",
            KeyshareGodmode,
            PassphraseGodmode,
            "Je bent in de God Mode kamer! Hier bewaar je de ultieme macht. Alleen de Admin bereikt dit niveau."
        );
    }
}
