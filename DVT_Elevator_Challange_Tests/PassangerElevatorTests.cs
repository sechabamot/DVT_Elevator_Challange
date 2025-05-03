using DVT_Elevator_Challange.Models;
using DVT_Elevator_Challange_Tests.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DVT_Elevator_Challange.Models.PassengerElevator;

namespace DVT_Elevator_Challange_Tests
{
    public class PassangerElevatorTests
    {
        [Fact]
        public void AssignPickup_ShouldNotQeuePickup_WhenOverCapacity()
        {
            TestablePassangerElevator elevator = new TestablePassangerElevator(0, Direction.Idle, peopleInside: 10);
            elevator.AssignPickup(new PassengerPickupRequest
            {
                RequestFloorNo = 0,
                DestinationFloorNo = 5,
                NoPeople = 1
            });

            Assert.False(elevator.PendingPickups.Count > 0);
        }

        [Fact]
        public void CanPickup_ShouldReturnFalse_WhenOverCapacity()
        {
            PassengerElevator elevator = new PassengerElevator(2, Direction.Idle, 10);

            PassengerPickupRequest request = new PassengerPickupRequest()
            {
                RequestFloorNo = 2,
                DestinationFloorNo = 5,
                NoPeople = 1
            };

            Assert.False(elevator.CanPickup(request));
        }

        [Fact]
        public void CanPickup_ShouldReturnTrue_WhenIdleAndUnderCapacity()
        {
            PassengerElevator elevator = new PassengerElevator(1, Direction.Idle, peopleInside: 3);
            
            PassengerPickupRequest request = new PassengerPickupRequest()
            {
                RequestFloorNo = 2,
                DestinationFloorNo = 5,
                NoPeople = 1
            };

            Assert.True(elevator.CanPickup(request));
        }

        [Fact]
        public async Task Elevator_ShouldIdle_WhenNoPickupOrDropsOffs()
        {
            PassengerElevator elevator = new PassengerElevator();
            
            _ = Task.Run(() => elevator.RunAsync(TestableOnPickupComplete));
            bool isIdle = await WaitForConditionAsync(
                () => elevator.Status == Status.Idle && elevator.Direction == Direction.Idle,
                10
            );

            Assert.Empty(elevator.PendingPickups);
            Assert.Empty(elevator.ActiveDropOffs);
            Assert.True(isIdle, "Elevator did not remain idle when no requests exist.");
        }

        [Fact]
        public async void RunAsync_ShouldLoadAndUnloadPassengersCorrectly()
        {
            //Arrange
            PassengerElevator elevator = new PassengerElevator(0, Direction.Idle, peopleInside: 0);

            //Act
            _ = Task.Run(() => elevator.RunAsync(TestableOnPickupComplete));
            elevator.AssignPickup(new PassengerPickupRequest
            {
                RequestFloorNo = 1,
                DestinationFloorNo = 5,
                NoPeople = 3
            });

            //Assert
            bool increased = await WaitForConditionAsync(
               condition: () => elevator.PeopleInside == 3 ,
               timeoutSeconds: 30,
               pollIntervalMs: 250
           );
            Assert.True(increased, "Number of people in the elevator did not increase.");

            bool decreased = await WaitForConditionAsync(
             condition: () => elevator.PeopleInside < 3,
             timeoutSeconds: 30,
             pollIntervalMs: 250
            );
            Assert.True(decreased, "Number of people in the elevator did not decreased.");
        }

        [Fact]
        public async void RunAsync_ShouldProcessRequest_AndReachCorrectFloor()
        {
            // Arrange
            TestablePassangerElevator elevator = new TestablePassangerElevator(0, Direction.Idle, peopleInside: 0);
            PassengerPickupRequest pickUpRequest = new PassengerPickupRequest
            {
                RequestFloorNo = 1,
                DestinationFloorNo = 4,
                NoPeople = 2
            };

            //Act            
            _ = Task.Run(() => elevator.RunAsync(TestableOnPickupComplete));
            elevator.AssignPickup(pickUpRequest);
            
            bool reached = await WaitForConditionAsync(
                condition: () => elevator.CurrentFloor == 4 && elevator.Status == Status.Idle,
                timeoutSeconds: 30,
                pollIntervalMs: 250
            );

            // Assert
            Assert.True(reached, "Elevator did not reach expected floor in time.");
            Assert.Equal(4, elevator.CurrentFloor);
            Assert.Equal(Direction.Idle, elevator.Direction);
            Assert.Empty(elevator.PendingPickups);
        }

        [Fact]
        public void GetSuitabilityScore_ShouldReturnDistance_WhenEligible()
        {
            PassengerElevator elevator = new PassengerElevator(currentFloor: 3, direction: Direction.Idle, peopleInside: 0);
            PassengerPickupRequest request = new PassengerPickupRequest
            {
                RequestFloorNo = 5,
                DestinationFloorNo = 7,
                NoPeople = 2
            };

            int? score = elevator.GetSuitabilityScore(request);

            Assert.NotNull(score);
            Assert.Equal(2, score);
        }

        [Fact]
        public void GetSuitabilityScore_ShouldReturnNull_WhenOverCapacity()
        {
            PassengerElevator elevator = new PassengerElevator(currentFloor: 1, direction: Direction.Idle, peopleInside: PassengerElevator.Capacity);
            PassengerPickupRequest request = new PassengerPickupRequest
            {
                RequestFloorNo = 2,
                DestinationFloorNo = 4,
                NoPeople = 1
            };

            int? score = elevator.GetSuitabilityScore(request);

            Assert.Null(score);
        }

    
        private void TestableOnPickupComplete(PickupRequest request)
        {
          
        }

        private async Task<bool> WaitForConditionAsync(Func<bool> condition, int timeoutSeconds = 10, int pollIntervalMs = 100)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                if (condition())
                    return true;

                await Task.Delay(pollIntervalMs);
            }

            return false;
        }

    }
}
