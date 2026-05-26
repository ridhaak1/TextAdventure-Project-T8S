using TextAdventure;

namespace TestProject1
{
    [TestClass]
    public class InventoryTests
    {
        // Test: item toevoegen en daarna controleren of het aanwezig is
        [TestMethod]
        public void AddItem_ItemToegevoegd_IsAanwezig()
        {
            var inv = new Inventory();
            inv.AddItem(new Item("Sleutel", "Gouden sleutel."));

            Assert.IsTrue(inv.HasItem("Sleutel"));
        }

        // Test: item dat nooit werd toegevoegd mag niet gevonden worden
        [TestMethod]
        public void HasItem_ItemNietAanwezig_RetourneertFalse()
        {
            var inv = new Inventory();

            Assert.IsFalse(inv.HasItem("Zwaard"));
        }

        // Test: HasItem is hoofdlettersonge­voelig (case-insensitive)
        [TestMethod]
        public void HasItem_HoofdletterOnGevoelig_VindtItem()
        {
            var inv = new Inventory();
            inv.AddItem(new Item("Zwaard", "Scherp."));

            Assert.IsTrue(inv.HasItem("ZWAARD"));
        }

        // Test: lege inventory geeft de tekst "Niets" terug
        [TestMethod]
        public void GetDisplayList_LegeInventory_ToontNiets()
        {
            var inv = new Inventory();

            Assert.AreEqual("Niets", inv.GetDisplayList());
        }

        // Test: meerdere items worden allebei getoond in de lijst
        [TestMethod]
        public void GetDisplayList_MetItems_ToontAlleNamen()
        {
            var inv = new Inventory();
            inv.AddItem(new Item("Sleutel", "Gouden."));
            inv.AddItem(new Item("Zwaard", "Scherp."));
            var list = inv.GetDisplayList();

            Assert.IsTrue(list.Contains("Sleutel"));
            Assert.IsTrue(list.Contains("Zwaard"));
        }
    }
}
