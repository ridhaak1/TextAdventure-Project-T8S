using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAdventure;

namespace TestProject1
{
    [TestClass]
    public class ItemTests
    {
        [TestMethod]
        public void Item_SetsNameAndDescription()
        {
            var item = new Item("Sleutel", "Een gouden sleutel.");

            Assert.AreEqual("Sleutel", item.Name);
            Assert.AreEqual("Een gouden sleutel.", item.Description);
        }

        [TestMethod]
        public void Item_RecordEquality_SameValues_AreEqual()
        {
            var a = new Item("Zwaard", "Een scherp zwaard.");
            var b = new Item("Zwaard", "Een scherp zwaard.");

            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void Item_RecordEquality_DifferentValues_AreNotEqual()
        {
            var a = new Item("Zwaard", "Een scherp zwaard.");
            var b = new Item("Sleutel", "Een gouden sleutel.");

            Assert.AreNotEqual(a, b);
        }
    }

}
