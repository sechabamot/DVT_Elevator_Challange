using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace DVT_Elevator_Challange.Models
{
    public class PassangerElevator : BasicElevator
    {
        protected readonly List<PassangerElevatorPickUpRequest> requests = new();
        protected readonly List<PassangerElevatorPickUpRequest> handledRequests = new();

        public static int Capacity { get; } = 10;
        public int PeopleInside { get; protected set; }

        // Special constructor for Testing only
        public PassangerElevator(
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

        public bool CanPickup(int requestedFloor, ElevatorTravelDirection desiredDirection, int noPeople)
        {
            if (PeopleInside + noPeople > Capacity) return false;
            if (Direction == ElevatorTravelDirection.Idle) return true;

            bool goingSameWay =
                (Direction == ElevatorTravelDirection.Up && requestedFloor > CurrentFloor) ||
                (Direction == ElevatorTravelDirection.Down && requestedFloor < CurrentFloor);

            return goingSameWay && Direction == desiredDirection;
        }

        public void AddRequest(PassangerElevatorPickUpRequest request)
        {
            if (PeopleInside + request.NoPeopleRequestingElevator > Capacity)
            {
                return;
            }


            requests.Add(request);
            PeopleInside += request.NoPeopleRequestingElevator;
        }

        public void Start()
        {
            CancellationToken cancellationToken = CancellationToken.None;
            while (!cancellationToken.IsCancellationRequested)
            {
                PassangerElevatorPickUpRequest stop = GetNextBestRequest();
                //List<PassangerElevatorRequest> duplicateStops = requests
                //    .Where(r => 
                //    r.DestinationFloorNo == stop.DestinationFloorNo 
                //    &&
                //    r.Id != stop.Id)
                //    .ToList();

                requests.Remove(stop);
                handledRequests.Add(stop);

                // Step 1: Move to the passenger pickup floor
                Status = ElevatorStatus.Moving;
                Direction = stop.RequestFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down;

                //Move elevator up so long as it is not at the destination floor.
                while (CurrentFloor != stop.RequestFloorNo)
                {
                    MoveOneFloor();
                }

                // Step 2: Pick up passengers (This means we are now on the request floor no.
                Status = ElevatorStatus.LoadingPassengers;
                Thread.Sleep(2000);

                PeopleInside += stop.NoPeopleRequestingElevator; //People have gotten onto elevator

                // Step 3: Move to destination floor
                Status = ElevatorStatus.Moving;
                Direction = stop.DestinationFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down;

                while (CurrentFloor != stop.DestinationFloorNo)
                {
                    MoveOneFloor();
                }

                // Step 4: Drop off passengers
                Status = ElevatorStatus.UnloadingPassengers;
                Thread.Sleep(2000);

                //What if there are passangers from another request 
                PeopleInside -= stop.NoPeopleRequestingElevator; //People who requested elevator drop off

                PassangerElevatorPickUpRequest nextStop = GetNextBestRequest();

                Direction = nextStop != null
                    ? (nextStop.RequestFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down)
                    : ElevatorTravelDirection.Idle;

                Status = nextStop != null
                    ? ElevatorStatus.Moving
                    : ElevatorStatus.Idle;
            }

            Direction = ElevatorTravelDirection.Idle;
            Status = ElevatorStatus.Idle;
        }

        public void Move()
        {
            while (requests.Count > 0)
            {
                PassangerElevatorPickUpRequest stop = GetNextBestRequest();
                //List<PassangerElevatorRequest> duplicateStops = requests
                //    .Where(r => 
                //    r.DestinationFloorNo == stop.DestinationFloorNo 
                //    &&
                //    r.Id != stop.Id)
                //    .ToList();

                requests.Remove(stop);
                handledRequests.Add(stop);

                // Step 1: Move to the passenger pickup floor
                Status = ElevatorStatus.Moving;
                Direction = stop.RequestFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down;

                //Move elevator up so long as it is not at the destination floor.
                while (CurrentFloor != stop.RequestFloorNo)
                {
                    MoveOneFloor();
                }

                // Step 2: Pick up passengers (This means we are now on the request floor no.
                Status = ElevatorStatus.LoadingPassengers;
                Thread.Sleep(2000);

                PeopleInside += stop.NoPeopleRequestingElevator; //People have gotten onto elevator

                // Step 3: Move to destination floor
                Status = ElevatorStatus.Moving;
                Direction = stop.DestinationFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down;

                while (CurrentFloor != stop.DestinationFloorNo)
                {
                    MoveOneFloor();
                }

                // Step 4: Drop off passengers
                Status = ElevatorStatus.UnloadingPassengers;
                Thread.Sleep(2000);

                //What if there are passangers from another request 
                PeopleInside -= stop.NoPeopleRequestingElevator; //People who requested elevator drop off

                PassangerElevatorPickUpRequest nextStop = GetNextBestRequest();

                Direction = nextStop != null
                    ? (nextStop.RequestFloorNo > CurrentFloor ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down)
                    : ElevatorTravelDirection.Idle;

                Status = nextStop != null
                    ? ElevatorStatus.Moving
                    : ElevatorStatus.Idle;
            }

            Direction = ElevatorTravelDirection.Idle;
            Status = ElevatorStatus.Idle;
        }

        protected PassangerElevatorPickUpRequest GetNextBestRequest()
        {
            if (Direction == ElevatorTravelDirection.Idle)
            {
                //Prioratise rquest with shortest distance going either direction.
                return requests.OrderBy(r => Math.Abs(CurrentFloor - r.RequestFloorNo)).First();
            }

            //Prioratise request on direction and distance.
            List<PassangerElevatorPickUpRequest> directional = requests
                .Where(r => Direction == ElevatorTravelDirection.Up
                                ? r.RequestFloorNo >= CurrentFloor
                                : r.RequestFloorNo <= CurrentFloor)
                .OrderBy(r => Math.Abs(CurrentFloor - r.RequestFloorNo))
                .ToList();

            return directional.FirstOrDefault()
                ?? requests.OrderBy(r => Math.Abs(CurrentFloor - r.RequestFloorNo)).First();
        }


        public bool ShoulHighlightMovement()
        {
            return requests.Any(s => s.HighlightRequest);
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

        public bool HasRequests()
        {
            return requests.Count > 0;
        }

        internal void AssignPickup(PassangerElevatorPickUpRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This represents unassigned people waiting on a floor.
        /// </summary>
        public class PassangerElevatorPickUpRequest
        {
            public string Id { get; init; }
            public int RequestFloorNo { get; init; }
            public int NoPeopleRequestingElevator { get; init; }
        }

        /// <summary>
        /// This represents people already inside elevators with known destinations
        /// </summary>
        public class PassangerElevatorDropOffRequest
        {
            public string Id { get; init; }
            public int DestinationFloorNo { get; init; }
            public int NoPeopleGettingOff{ get; init; }

        }
    }
}
