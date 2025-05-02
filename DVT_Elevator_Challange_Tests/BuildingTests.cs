using DVT_Elevator_Challange.Models;
using DVT_Elevator_Challange_Tests.Models;

namespace DVT_Elevator_Challange_Tests
{
    public class BuildingTests
    {
        [Fact]
        public void Building_ShouldThrowArgumentException_WhenLessThanThreeFloors()
        {
            // Arrange
            List<PassangerElevator> elevators = new List<PassangerElevator> { new PassangerElevator() };
            List<BuildingFloor> floors = new List<BuildingFloor>
            {
                new BuildingFloor { FloorNo = 0 },
                new BuildingFloor { FloorNo = 1 }
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Building(elevators, floors));
        }

        [Fact]
        public void FindBestElevator_ShouldReturnNearestIdleElevator()
        {
            // Arrange
            PassangerElevator elevator1 = new PassangerElevator(2, ElevatorTravelDirection.Idle);
            PassangerElevator elevator2 = new PassangerElevator(5, ElevatorTravelDirection.Idle);

            TestableBuilding building = new TestableBuilding(new List<PassangerElevator> { elevator1, elevator2 }, GetSampleFloors());

            // Act
            PassangerElevator? bestElevator = building.ExposeFindBestElevator(3, ElevatorTravelDirection.Up, 1);

            // Assert
            Assert.Equal(elevator1, bestElevator); // elevator1 (floor 2) is closer to floor 3
        }

        [Fact]
        public void FindBestElevator_ShouldPrioritizeElevatorAlreadyMovingTowardsRequest()
        {
            // Arrange
            PassangerElevator movingElevator = new PassangerElevator(2, ElevatorTravelDirection.Up);
            PassangerElevator idleElevator = new PassangerElevator(0, ElevatorTravelDirection.Idle);

            TestableBuilding building = new TestableBuilding(new List<PassangerElevator> { movingElevator, idleElevator }, GetSampleFloors());

            // Act
            PassangerElevator? bestElevator = building.ExposeFindBestElevator(3, ElevatorTravelDirection.Up, 1);

            // Assert
            Assert.Equal(movingElevator, bestElevator);
        }

        [Fact]
        public void FindBestElevator_ShouldReturnNull_WhenNoElevatorCanPickup()
        {
            // Arrange
            TestablePassangerElevator fullElevator = new TestablePassangerElevator();
            TestablePassangerElevator fullElevatorTwo = new TestablePassangerElevator();

            fullElevator.ForceFullCapacity();
            fullElevatorTwo.ForceFullCapacity();

            TestableBuilding building = new TestableBuilding(new List<PassangerElevator> { fullElevator, fullElevatorTwo }, GetSampleFloors());

            // Act
            PassangerElevator? bestElevator = building.ExposeFindBestElevator(1, ElevatorTravelDirection.Up, 2);

            // Assert
            Assert.Null(bestElevator);
        }

        [Fact]
        public void RequestElevator_ShouldAssignRequestToElevator()
        {
            // Arrange
            TestablePassangerElevator elevator = new TestablePassangerElevator();
            TestableBuilding building = new TestableBuilding(new List<PassangerElevator> { elevator }, GetSampleFloors());

            // Act
            building.RequestElevator(0, 5, ElevatorTravelDirection.Up, 2);
            building.AssignPendingPickUps();

            // Assert
            Assert.True(elevator.PendingPickups.Count > 0);
        }

     


        private List<BuildingFloor> GetSampleFloors()
        {
            return new List<BuildingFloor>
            {
                new BuildingFloor { FloorNo = -2 },
                new BuildingFloor { FloorNo = -1 },
                new BuildingFloor { FloorNo = 0 },
                new BuildingFloor { FloorNo = 1 },
                new BuildingFloor { FloorNo = 2 },
                new BuildingFloor { FloorNo = 3 },
                new BuildingFloor { FloorNo = 4 },
                new BuildingFloor { FloorNo = 5 },
            };
        }

    }
}
