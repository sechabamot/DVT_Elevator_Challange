using DVT_Elevator_Challange.Models;
using DVT_Elevator_Challange_Tests.Models;

namespace DVT_Elevator_Challange_Tests
{
    public class BuildingTests
    {
        [Fact]
        public void Building_ShouldThrowArgumentException_WhenLessThanThreeFloors()
        {
            List<IElevator> elevators = new List<IElevator> { new PassengerElevator() };
            List<BuildingFloor> floors = new List<BuildingFloor>
            {
                new BuildingFloor { FloorNo = 0 },
                new BuildingFloor { FloorNo = 1 }
            };

            Assert.Throws<ArgumentException>(() => new Building(elevators, floors));
        }

        [Fact]
        public void Building_ShouldThrowArgumentException_WhenDuplicateFloorNumbersExist()
        {
            // Arrange
            List<IElevator> elevators = new List<IElevator> { new PassengerElevator() };

            List<BuildingFloor> floorsWithDuplicates = new List<BuildingFloor>
            {
                new BuildingFloor { FloorNo = 0, Name = "Ground" },
                new BuildingFloor { FloorNo = 1, Name = "Floor 1" },
                new BuildingFloor { FloorNo = 1, Name = "Another Floor 1" }, // duplicate
                new BuildingFloor { FloorNo = 2, Name = "Floor 2" }
            };

            // Act & Assert
            ArgumentException ex = Assert.Throws<ArgumentException>(() =>
                new Building(elevators, floorsWithDuplicates)
            );

            Assert.Contains("unique floor numbers", ex.Message);
        }

        [Fact]
        public void FindBestElevator_ShouldReturnNearestIdleElevator()
        {
            PassengerElevator elevator1 = new PassengerElevator(2, Direction.Idle);
            PassengerElevator elevator2 = new PassengerElevator(5, Direction.Idle);
            TestableBuilding building = new TestableBuilding(new List<IElevator> { elevator1, elevator2 }, GetSampleFloors());

            PassengerPickupRequest request = new PassengerPickupRequest { RequestFloorNo = 3, DestinationFloorNo = 6, NoPeople = 1 };
            IElevator? bestElevator = building.ExposeFindBestElevator(request);

            Assert.Equal(elevator1, bestElevator);
        }

        [Fact]
        public void FindBestElevator_ShouldPrioritizeElevatorAlreadyMovingTowardsRequest()
        {
            PassengerElevator movingElevator = new PassengerElevator(2, Direction.Up);
            PassengerElevator idleElevator = new PassengerElevator(0, Direction.Idle);

            TestableBuilding building = new TestableBuilding(new List<IElevator> { movingElevator, idleElevator }, GetSampleFloors());

            PassengerPickupRequest request = new PassengerPickupRequest { RequestFloorNo = 3, DestinationFloorNo = 5, NoPeople = 1 };
            IElevator? bestElevator = building.ExposeFindBestElevator(request);

            Assert.Equal(movingElevator, bestElevator);
        }

        [Fact]
        public void FindBestElevator_ShouldReturnNull_WhenNoElevatorCanPickup()
        {
            TestablePassangerElevator fullElevator1 = new TestablePassangerElevator();
            TestablePassangerElevator fullElevator2 = new TestablePassangerElevator();

            fullElevator1.ForceFullCapacity();
            fullElevator2.ForceFullCapacity();

            TestableBuilding building = new TestableBuilding(new List<IElevator> { fullElevator1, fullElevator2 }, GetSampleFloors());
            PassengerPickupRequest request = new PassengerPickupRequest { RequestFloorNo = 1, DestinationFloorNo = 2, NoPeople = 2 };

            IElevator? bestElevator = building.ExposeFindBestElevator(request);

            Assert.Null(bestElevator);
        }

        [Fact]
        public void RequestElevator_ShouldAssignRequestToElevator()
        {
            TestablePassangerElevator elevator = new TestablePassangerElevator();
            TestableBuilding building = new TestableBuilding(new List<IElevator> { elevator }, GetSampleFloors());

            PassengerPickupRequest request = new PassengerPickupRequest
            {
                RequestFloorNo = 0,
                DestinationFloorNo = 2,
                NoPeople = 5,
                Highlight = true
            };

            building.RequestElevator(request);
            building.AssignPendingPickUps();

            Assert.True(elevator.PendingPickups.Count > 0);
        }

        [Fact]
        public void RequestElevator_ShouldNotAssign_WhenAllElevatorsFull()
        {
            TestablePassangerElevator elevator = new TestablePassangerElevator();
            elevator.ForceFullCapacity();

            TestableBuilding building = new TestableBuilding(new List<IElevator> { elevator }, GetSampleFloors());

            PassengerPickupRequest request = new PassengerPickupRequest
            {
                RequestFloorNo = 2,
                DestinationFloorNo = 4,
                NoPeople = 3
            };

            building.RequestElevator(request);
            building.AssignPendingPickUps();

            Assert.Empty(elevator.PendingPickups);
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
