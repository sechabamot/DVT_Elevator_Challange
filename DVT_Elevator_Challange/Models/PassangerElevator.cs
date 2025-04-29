using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange.Models
{
    public class PassangerElevator : BasicElevator
    {
        private readonly Queue<PassangerElevatorRequest> stops = new();

        public static int Capacity { get; } = 10;
        public int PeopleInside { get; private set; }

        public bool CanPickup(int requestedFloor, ElevatorTravelDirection desiredDirection, int noPeople)
        {
            if (PeopleInside + noPeople > Capacity) return false;
            if (Direction == ElevatorTravelDirection.Idle) return true;

            bool goingSameWay =
                (Direction == ElevatorTravelDirection.Up && requestedFloor > CurrentFloor) ||
                (Direction == ElevatorTravelDirection.Down && requestedFloor < CurrentFloor);

            return goingSameWay && Direction == desiredDirection;
        }

        public void AddRequest(PassangerElevatorRequest stop)
        {
            if (PeopleInside + stop.NoPeopleGettingOff > Capacity)
            {
                return;
            }

            stops.Enqueue(stop);
            PeopleInside += stop.NoPeopleGettingOff;
        }

        public void Move()
        {
            if (Direction == ElevatorTravelDirection.Idle)
            {

                while (stops.Count > 0)
                {
                    PassangerElevatorRequest stop = stops.Dequeue();

                    // Step 1: Move to the passenger pickup floor
                    Direction = stop.RequestFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down;

                    while (CurrentFloor != stop.RequestFloorNo)
                    {
                        MoveOneFloor();
                    }

                    // Picked up passengers
                    PeopleInside = Math.Min(PeopleInside + stop.NoPeopleGettingOff, Capacity);

                    // Step 2: Move to the passenger destination floor
                    Direction = stop.DestinationFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down;

                    while (CurrentFloor != stop.DestinationFloorNo)
                    {
                        MoveOneFloor();
                    }

                    // Dropped off passengers
                    PeopleInside = Math.Max(0, PeopleInside - stop.NoPeopleGettingOff);

                    // Update direction
                    Direction = stops.Any()
                        ? (stops.Peek().RequestFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down)
                        : ElevatorTravelDirection.Idle;

                }

            }

            Direction = ElevatorTravelDirection.Idle;

        }

        private void MoveOneFloor()
        {
            if (Direction == ElevatorTravelDirection.Up)
            {
                CurrentFloor++;
            }
            else if (Direction == ElevatorTravelDirection.Down)
            {
                CurrentFloor--;
            }

            Thread.Sleep(2000); // 1 second per floor move
        }

        public class PassangerElevatorRequest : BasicElevatorStop
        {
            public int NoPeopleGettingOff { get; init; }
        }
    }
}
