namespace TextAdventure;

public class Room
{
    public string Name { get; }
    public string Description { get; }
    public bool IsDeadly { get; init; }
    public string? RequiredItem { get; init; }
    public bool IsWin { get; init; }
    public bool MonsterAlive { get; set; }

    public Dictionary<Direction, Room> Exits { get; } = new();
    private readonly Dictionary<string, Item> _items = new();

    public Room(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void AddExit(Direction dir, Room room) => Exits[dir] = room;
    public void AddItem(Item item) => _items[item.Name.ToLower()] = item;

    public Item? TakeItem(string itemName)
    {
        if (_items.Remove(itemName.ToLower(), out var item)) return item;
        return null;
    }
    public void ShowDescription(Inventory inv)
    {
        Console.WriteLine($"\n--- {Name} ---");
        Console.WriteLine(Description);
        if (_items.Count > 0) Console.WriteLine($"Items hier: {string.Join(", ", _items.Values.Select(i => i.Name))}");
        Console.WriteLine($"Uitgangen: {string.Join(", ", Exits.Keys)}");
        if (MonsterAlive) Console.WriteLine("PAS OP: Er staat een monster voor je!");
    }
}


