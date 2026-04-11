namespace TextAdventure;

public class Inventory
{
    private readonly Dictionary<string, Item> _items = new();

    public void AddItem(Item item) => _items[item.Name.ToLower()] = item;

    public bool HasItem(string itemName) => _items.ContainsKey(itemName.ToLower());

    public string GetDisplayList() =>
        _items.Count > 0 ? string.Join(", ", _items.Values.Select(i => i.Name)) : "Niets";
}