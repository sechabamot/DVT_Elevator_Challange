using DVT_Elevator_Challange.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange_Tests.Models
{
    internal class TestablePassangerElevator : PassengerElevator
    {
        //public IReadOnlyList<PassengerPickupRequest> HandledRequests => handledRequests.AsReadOnly();

        public TestablePassangerElevator()
        {

        }

        public TestablePassangerElevator(
          int currentFloor = 0,
          ElevatorTravelDirection direction = ElevatorTravelDirection.Idle,
          int peopleInside = 0
          )
        {
            CurrentFloor = currentFloor;
            Direction = direction;
            PeopleInside = peopleInside;
            Status = ElevatorStatus.Idle;
        }

        public void ForceFullCapacity () 
        {
            PeopleInside = Capacity;
        }
    }
}
