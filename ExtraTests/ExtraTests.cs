using TextAdventure;

namespace ExtraTests
{
    [TestClass]
    public sealed class ExtraTests
    {
        private Building _world;

       [TestInitialize]
        public void Setup()
        {
            _world = GameSetup.CreateWorld();
        }

        [TestMethod]
        public void MovingEast_ShouldChangeCurrentRoom()
        {
            // Act
            _world.Move(Direction.e);

            // Assert
            Assert.AreNotEqual("Start", _world.CurrentRoom.Name);
        }

        [TestMethod]
        public void TakingSword_ShouldAddItToInventory()
        {
            // Arrange
            _world.Move(Direction.s);

            // Act
            var sword = _world.CurrentRoom.TakeItem("zwaard");
            _world.Inventory.AddItem(sword!);

            // Assert
            Assert.IsTrue(_world.Inventory.HasItem("zwaard"));
        }

        [TestMethod]
        public void TakingItem_ShouldRemoveItFromRoom()
        {
            // Arrange
            _world.Move(Direction.e);

            // Act
            var key = _world.CurrentRoom.TakeItem("sleutel");

            // Assert
            Assert.IsNull(_world.CurrentRoom.TakeItem("sleutel"));
        }

        [TestMethod]
        public void ReturningFromSwordRoom_ShouldGoBackToStart()
        {
            // Arrange
            _world.Move(Direction.s);

            // Act
            _world.Move(Direction.n);

            // Assert
            Assert.AreEqual("Start", _world.CurrentRoom.Name);
        }

        [TestMethod]
        public void FightingMonster_WithSword_ShouldKeepGameRunning()
        {
            // Arrange
            _world.Move(Direction.s);

            var sword = _world.CurrentRoom.TakeItem("zwaard");
            _world.Inventory.AddItem(sword!);

            _world.Move(Direction.s);

            // Act
            _world.Fight();

            // Assert
            Assert.IsFalse(_world.IsGameOver);
            Assert.IsFalse(_world.CurrentRoom.MonsterAlive);
        }

        [TestMethod]
        public void EnteringNorthRoom_WithKey_ShouldWinGame()
        {
            // Arrange
            _world.Move(Direction.e);

            var key = _world.CurrentRoom.TakeItem("sleutel");
            _world.Inventory.AddItem(key!);

            _world.Move(Direction.w);

            // Act
            _world.Move(Direction.n);

            // Assert
            Assert.IsTrue(_world.IsWon);
        }
    }
}
