using DVT_Elevator_Challange.Models;
using DVT_Elevator_Challange_Tests.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DVT_Elevator_Challange.Models.PassangerElevator;

namespace DVT_Elevator_Challange_Tests
{
    public class PassangerElevatorTests
    {
        [Fact]
        public void AddRequest_ShouldRejectRequest_WhenOverCapacity()
        {
            TestablePassangerElevator elevator = new TestablePassangerElevator(0, ElevatorTravelDirection.Idle, peopleInside: 10);
            elevator.AssignPickup(new PassangerElevatorPickUpRequest
            {
                RequestFloorNo = 0,
                DestinationFloorNo = 5,
                NoPeopleRequestingElevator = 1
            });

            Assert.False(elevator.HandledRequests.Count > 0);
        }

        [Fact]
        public void CanPickup_ShouldReturnFalse_WhenOverCapacity()
        {
            TestablePassangerElevator elevator = new TestablePassangerElevator(2, ElevatorTravelDirection.Idle, 10);
            bool result = elevator.CanPickup(3, ElevatorTravelDirection.Up, 1);

            Assert.False(result);
        }

        [Fact]
        public void CanPickup_ShouldReturnTrue_WhenIdleAndUnderCapacity()
        {
            TestablePassangerElevator elevator = new TestablePassangerElevator(1, ElevatorTravelDirection.Idle, peopleInside: 3);
            bool result = elevator.CanPickup(2, ElevatorTravelDirection.Up, 1);

            Assert.True(result);
        }

        [Fact]
        public async Task Elevator_ShouldIdle_WhenNoPickupOrDropsOffs()
        {
            //Arrange
            PassangerElevator elevator = new PassangerElevator();

            //Act
            _ = Task.Run(() => elevator.RunAsync(TestableOnPickupComplete));

            //Assert
            bool isIdle = await WaitForConditionAsync(
                () => elevator.Status == ElevatorStatus.Idle && elevator.Direction == ElevatorTravelDirection.Idle,
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
            PassangerElevator elevator = new PassangerElevator(0, ElevatorTravelDirection.Idle, peopleInside: 0);

            //Act
            _ = Task.Run(() => elevator.RunAsync(TestableOnPickupComplete));
            elevator.AssignPickup(new PassangerElevatorPickUpRequest
            {
                RequestFloorNo = 1,
                DestinationFloorNo = 5,
                NoPeopleRequestingElevator = 3
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
            TestablePassangerElevator elevator = new TestablePassangerElevator(0, ElevatorTravelDirection.Idle, peopleInside: 0);
            PassangerElevatorPickUpRequest pickUpRequest = new PassangerElevatorPickUpRequest
            {
                RequestFloorNo = 1,
                DestinationFloorNo = 4,
                NoPeopleRequestingElevator = 2
            };

            //Act            
            _ = Task.Run(() => elevator.RunAsync(TestableOnPickupComplete));
            elevator.AssignPickup(pickUpRequest);
            
            bool reached = await WaitForConditionAsync(
                condition: () => elevator.CurrentFloor == 4 && elevator.Status == ElevatorStatus.Idle,
                timeoutSeconds: 30,
                pollIntervalMs: 250
            );

            // Assert
            Assert.True(reached, "Elevator did not reach expected floor in time.");
            Assert.Equal(4, elevator.CurrentFloor);
            Assert.Equal(ElevatorTravelDirection.Idle, elevator.Direction);
            Assert.Empty(elevator.PendingPickups);
        }

        private void TestableOnPickupComplete(PassangerElevatorPickUpRequest request)
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
