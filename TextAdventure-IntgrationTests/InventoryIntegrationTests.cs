using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure;

namespace TextAdventure_IntgrationTests
{
    [TestClass]
    public class InventoryIntegrationTests
    {

        [TestMethod]
        public void TakeItem_ItemLeavesRoom_AndEntersInventory()
        {
            var item = new Item("Sleutel", "Een gouden sleutel.");
            var room = new Room("Gang", "Een donkere gang.");
            var inventory = new Inventory();

            room.AddItem(item);
            var takenItem = room.TakeItem("Sleutel");
            inventory.AddItem(takenItem);

            Assert.IsTrue(inventory.HasItem("Sleutel"));
        }

        [TestMethod]
        public void TakeItem_ItemIsNoLongerInRoom_AfterPickup()
        {
            var item = new Item("Sleutel", "Een gouden sleutel.");
            var room = new Room("Gang", "Een donkere gang.");

            room.AddItem(item);
            room.TakeItem("Sleutel");

            Assert.IsNull(room.TakeItem("Sleutel"));
        }

        [TestMethod]
        public void TakeItem_ReturnsNull_WhenItemNotInRoom()
        {
            var room = new Room("Gang", "Een donkere gang.");

            var result = room.TakeItem("Zwaard");

            Assert.IsNull(result);
        }


        [TestMethod]
        public void RequiredItem_InventoryHasKey_AllowsEntry()
        {
            var room = new Room("Geheime kamer", "Een verborgen kamer.") { RequiredItem = "Sleutel" };
            var inventory = new Inventory();
            inventory.AddItem(new Item("Sleutel", "Een gouden sleutel."));

            Assert.IsTrue(inventory.HasItem(room.RequiredItem));
        }

        [TestMethod]
        public void RequiredItem_InventoryMissingKey_BlocksEntry()
        {
            var room = new Room("Geheime kamer", "Een verborgen kamer.") { RequiredItem = "Sleutel" };
            var inventory = new Inventory();

            Assert.IsFalse(inventory.HasItem(room.RequiredItem));
        }


        [TestMethod]
        public void Fight_WithSwordInInventory_KillsMonster()
        {
            var room = new Room("Monstertoren", "Een donkere toren.") { MonsterAlive = true };
            var inventory = new Inventory();
            inventory.AddItem(new Item("Zwaard", "Een scherp zwaard."));

            if (inventory.HasItem("Zwaard"))
                room.MonsterAlive = false;

            Assert.IsFalse(room.MonsterAlive);
        }

        [TestMethod]
        public void Fight_WithoutSwordInInventory_MonsterSurvives()
        {
            var room = new Room("Monstertoren", "Een donkere toren.") { MonsterAlive = true };
            var inventory = new Inventory();

            if (inventory.HasItem("Zwaard"))
                room.MonsterAlive = false;

            Assert.IsTrue(room.MonsterAlive);
        }


        [TestMethod]
        public void Exit_IsAccessible_WhenRequiredItemIsInInventory()
        {
            var hallway = new Room("Gang", "Een donkere gang.");
            var lockedRoom = new Room("Kluis", "Een zware deur.") { RequiredItem = "Sleutel" };
            hallway.AddExit(Direction.n, lockedRoom);
            var inventory = new Inventory();
            inventory.AddItem(new Item("Sleutel", "Een gouden sleutel."));

            var destination = hallway.Exits[Direction.n];
            var canEnter = destination.RequiredItem == null || inventory.HasItem(destination.RequiredItem);

            Assert.IsTrue(canEnter);
        }

        [TestMethod]
        public void Exit_IsBlocked_WhenRequiredItemIsMissing()
        {
            var hallway = new Room("Gang", "Een donkere gang.");
            var lockedRoom = new Room("Kluis", "Een zware deur.") { RequiredItem = "Sleutel" };
            hallway.AddExit(Direction.n, lockedRoom);
            var inventory = new Inventory();

            var destination = hallway.Exits[Direction.n];
            var canEnter = destination.RequiredItem == null || inventory.HasItem(destination.RequiredItem);

            Assert.IsFalse(canEnter);
        }


        [TestMethod]
        public void FullFlow_PickUpKey_ThenUnlockDoor()
        {
            var startRoom = new Room("Hal", "Een grote hal.");
            var lockedRoom = new Room("Kluis", "Een zware deur.") { RequiredItem = "Sleutel" };
            startRoom.AddExit(Direction.n, lockedRoom);
            startRoom.AddItem(new Item("Sleutel", "Een gouden sleutel."));
            var inventory = new Inventory();

            var key = startRoom.TakeItem("Sleutel");
            inventory.AddItem(key);

            var destination = startRoom.Exits[Direction.n];
            var canEnter = destination.RequiredItem == null || inventory.HasItem(destination.RequiredItem);

            Assert.IsTrue(inventory.HasItem("Sleutel"));
            Assert.IsTrue(canEnter);
        }

        [TestMethod]
        public void FullFlow_PickUpSword_ThenFightMonster()
        {
            var monsterRoom = new Room("Monstertoren", "Een enge toren.") { MonsterAlive = true };
            monsterRoom.AddItem(new Item("Zwaard", "Een scherp zwaard."));
            var inventory = new Inventory();

            var sword = monsterRoom.TakeItem("Zwaard");
            inventory.AddItem(sword);

            if (inventory.HasItem("Zwaard"))
                monsterRoom.MonsterAlive = false;

            Assert.IsTrue(inventory.HasItem("Zwaard"));
            Assert.IsFalse(monsterRoom.MonsterAlive);
        }
    }
}
