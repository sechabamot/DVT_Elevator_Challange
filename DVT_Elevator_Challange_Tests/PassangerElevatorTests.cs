using DVT_Elevator_Challange.Models;
using DVT_Elevator_Challange_Tests.Models;
using System;
using System.Collections.Generic;
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
            elevator.AddRequest(new PassangerElevator.PassangerElevatorPickUpRequest
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
        public void Move_ShouldProcessRequest_AndReachCorrectFloor()
        {
            var elevator = new TestablePassangerElevator(0, ElevatorTravelDirection.Idle, peopleInside: 0);

            elevator.AddRequest(new PassangerElevatorPickUpRequest
            {
                RequestFloorNo = 1,
                DestinationFloorNo = 4,
                NoPeopleRequestingElevator = 2
            });

            elevator.Move();

            Assert.Equal(4, elevator.CurrentFloor);
            Assert.Equal(ElevatorTravelDirection.Idle, elevator.Direction);
            Assert.False(elevator.HasRequests());
            Assert.Single(elevator.HandledRequests);
        }

        [Fact]
        public void Move_ShouldIncreaseAndDecreasePeopleInside()
        {
            PassangerElevator elevator = new PassangerElevator(0, ElevatorTravelDirection.Idle, peopleInside: 0);

            elevator.AddRequest(new PassangerElevatorPickUpRequest
            {
                RequestFloorNo = 1,
                DestinationFloorNo = 5,
                NoPeopleRequestingElevator = 3
            });
            elevator.Move();

            Assert.Equal(0, elevator.PeopleInside); // Picked up and dropped off
        }


    }
}
