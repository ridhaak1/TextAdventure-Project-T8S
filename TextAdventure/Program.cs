namespace TextAdventure;

public class Program
{
    public static void Main()
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
