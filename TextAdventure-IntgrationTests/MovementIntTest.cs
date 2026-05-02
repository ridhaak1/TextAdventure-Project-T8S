using TextAdventure;

namespace TextAdventure_IntgrationTests
{
    [TestClass]
    public sealed class MovementIntTest
    {
        private Building _world;

        [TestInitialize]
        public void Setup()
        {
            _world = GameSetup.CreateWorld();
        }

        [TestMethod]
        public void GoWest_FromStart_SchouldCauseGameOver()
        {
            //Arrange auto

            //Act
            _world.Move(Direction.w);

            //Assert
            Assert.IsTrue(_world.IsGameOver);
        }

        [TestMethod]
        public void GoNorth_FromStart_ShouldStayAlive()
        {
            //Arrange
            var roomBefore = _world.CurrentRoom;

            //Act
            _world.Move(Direction.n);

            //Assert
            Assert.AreEqual(roomBefore, _world.CurrentRoom);
            Assert.IsFalse(_world.IsGameOver);
        }

        [TestMethod]
        public void TakeSluetel_FromE_ShouldTakeSlueutel()
        {
            //Arrange
            _world.Move(Direction.e);

            //Act
            var item = _world.CurrentRoom.TakeItem("sleutel");
            _world.Inventory.AddItem(item);

            //Assert
            Assert.IsTrue(_world.Inventory.HasItem("sleutel"));
        }

        [TestMethod]
        public void Fight_WithoutSword_ShouldCauseGameOver()
        {
            //Arrange
            _world.Move(Direction.s);
            _world.Move(Direction.s);


            //Act
            _world.Fight();

            //Assert
            Assert.IsTrue(_world.IsGameOver);
        }

        [TestMethod]
        public void Fight_WithSword_ShouldKillMonster()
        {
            //Arrange
            _world.Move(Direction.s);
            var zwaard = _world.CurrentRoom.TakeItem("zwaard");
            _world.Inventory.AddItem(zwaard);

            _world.Move(Direction.s);

            //Act
            _world.Fight();

            //Assert
            Assert.IsFalse(_world.CurrentRoom.MonsterAlive);
            Assert.IsFalse(_world.IsGameOver);
        }

        [TestMethod]
        public void Escape_FromLivingMonster_ShouldCauseGameOver()
        {
            // Arrange
            _world.Move(Direction.s); 
            _world.Move(Direction.s); 

            // Act
            _world.Move(Direction.n);

            // Assert
            Assert.IsTrue(_world.IsGameOver);
        }

        [TestMethod]
        public void FullWinningPath_ShouldWinGame()
        {
            // Arrange 
            _world.Move(Direction.e);
            var sleutel = _world.CurrentRoom.TakeItem("sleutel");
            _world.Inventory.AddItem(sleutel!);

           
            _world.Move(Direction.w);          
            _world.Move(Direction.s);          
            var zwaard = _world.CurrentRoom.TakeItem("zwaard");
            _world.Inventory.AddItem(zwaard!);

            _world.Move(Direction.s);           
            _world.Fight();                    

            _world.Move(Direction.n);           
            _world.Move(Direction.n);           

            //Act
            _world.Move(Direction.n);          

            // Assert
            Assert.IsTrue(_world.IsWon);
            Assert.IsFalse(_world.IsGameOver);
        }
    }
}
