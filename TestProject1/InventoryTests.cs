using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure;

namespace TestProject1
{
    [TestClass]
    public class InventoryTests
    {
        [TestMethod]
        public void AddItem_AddsItemToInventory()
        {
            var inventory = new Inventory();
            var item = new Item("Zwaard", "Een scherp zwaard.");

            inventory.AddItem(item);

            Assert.IsTrue(inventory.HasItem("Zwaard"));
        }


        [TestMethod]
        public void AddItem_WithSameName_OverwritesExistingItem()
        {
            var inventory = new Inventory();
            var item1 = new Item("Schild", "Een houten schild.");
            var item2 = new Item("Schild", "Een ijzeren schild.");

            inventory.AddItem(item1);
            inventory.AddItem(item2);

            Assert.IsTrue(inventory.HasItem("Schild"));
        }

        [TestMethod]
        public void AddItem_MultipleDifferentItems_AllAreAdded()
        {
            var inventory = new Inventory();

            inventory.AddItem(new Item("Zwaard", "Een scherp zwaard."));
            inventory.AddItem(new Item("Boog", "Een houten boog."));
            inventory.AddItem(new Item("Staf", "Een magische staf."));

            Assert.IsTrue(inventory.HasItem("Zwaard"));
            Assert.IsTrue(inventory.HasItem("Boog"));
            Assert.IsTrue(inventory.HasItem("Staf"));
        }

        [TestMethod]
        public void HasItem_ReturnsTrue_WhenItemIsPresent()
        {
            var inventory = new Inventory();
            inventory.AddItem(new Item("Potion", "Een helende drank."));

            Assert.IsTrue(inventory.HasItem("Potion"));
        }


        [TestMethod]
        public void HasItem_ReturnsFalse_WhenItemIsNotPresent()
        {
            var inventory = new Inventory();

            Assert.IsFalse(inventory.HasItem("Potion"));
        }

        [TestMethod]
        public void HasItem_ReturnsFalse_OnEmptyInventory()
        {
            var inventory = new Inventory();

            Assert.IsFalse(inventory.HasItem("Zwaard"));
        }

        [TestMethod]
        public void GetDisplayList_OnEmptyInventory_ReturnsNiets()
        {
            var inventory = new Inventory();

            Assert.AreEqual("Niets", inventory.GetDisplayList());
        }

        [TestMethod]
        public void GetDisplayList_IsNotNiets_AfterAddItem()
        {
            var inventory = new Inventory();
            inventory.AddItem(new Item("Helm", "Een ijzeren helm."));

            Assert.AreNotEqual("Niets", inventory.GetDisplayList());
        }
    }
}
