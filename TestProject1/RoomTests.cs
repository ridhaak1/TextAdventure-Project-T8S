using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure;

namespace TestProject1
{
    [TestClass]
    public class RoomTests
    {
        [TestMethod]
        public void Room_Constructor_SetsNameAndDescription()
        {
            var room = new Room("Kelder", "Het is hier koud.");

            Assert.AreEqual("Kelder", room.Name);
            Assert.AreEqual("Het is hier koud.", room.Description);
        }

        [TestMethod]
        public void Room_DefaultFlags_AreFalse()
        {
            var room = new Room("Start", "Begin kamer.");

            Assert.IsFalse(room.IsDeadly);
            Assert.IsFalse(room.IsWin);
            Assert.IsFalse(room.MonsterAlive);
            Assert.IsNull(room.RequiredItem);
        }

        [TestMethod]
        public void Room_InitProperties_AreSetCorrectly()
        {
            var room = new Room("Uitgang", "Gefeliciteerd!")
            {
                IsWin = true,
                IsDeadly = false,
                RequiredItem = "Sleutel"
            };

            Assert.IsTrue(room.IsWin);
            Assert.IsFalse(room.IsDeadly);
            Assert.AreEqual("Sleutel", room.RequiredItem);
        }

        [TestMethod]
        public void Room_AddExit_AppearsInExits()
        {
            var start = new Room("Start", "Begin.");
            var kelder = new Room("Kelder", "Donker.");

            start.AddExit(Direction.s, kelder);

            Assert.IsTrue(start.Exits.ContainsKey(Direction.s));
            Assert.AreSame(kelder, start.Exits[Direction.s]);
        }

        [TestMethod]
        public void Room_AddExit_MultipleDirections()
        {
            var start = new Room("Start", "Begin.");
            start.AddExit(Direction.n, new Room("Noord", "Noord."));
            start.AddExit(Direction.e, new Room("Oost", "Oost."));

            Assert.AreEqual(2, start.Exits.Count);
        }

        [TestMethod]
        public void Room_TakeItem_ReturnsItemAndRemovesIt()
        {
            var room = new Room("Schatkamer", "Vol goud.");
            room.AddItem(new Item("Sleutel", "Een gouden sleutel."));

            var result = room.TakeItem("Sleutel");

            Assert.IsNotNull(result);
            Assert.AreEqual("Sleutel", result!.Name);
            Assert.IsNull(room.TakeItem("Sleutel")); // weg na take
        }

        [TestMethod]
        public void Room_TakeItem_CaseInsensitive()
        {
            var room = new Room("Kamer", "Test.");
            room.AddItem(new Item("Zwaard", "Scherp."));

            var result = room.TakeItem("ZWAARD");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Room_TakeItem_NonExistentItem_ReturnsNull()
        {
            var room = new Room("Lege kamer", "Leeg.");

            Assert.IsNull(room.TakeItem("Bom"));
        }

        [TestMethod]
        public void Room_MonsterAlive_CanBeChanged()
        {
            var room = new Room("Monsterkamer", "Eng.") { MonsterAlive = true };

            Assert.IsTrue(room.MonsterAlive);
            room.MonsterAlive = false;
            Assert.IsFalse(room.MonsterAlive);
        }
    }

}
