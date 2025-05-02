using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange.Models
{
    public class PassangerElevator : BasicElevator
    {
        public Queue<PassangerElevatorPickUpRequest> PendingPickups { get; protected set; } = new();
        public List<PassangerElevatorDropOffRequest> ActiveDropOffs { get; protected set; } = new();

        protected readonly List<PassangerElevatorPickUpRequest> requests = new();
        protected readonly List<PassangerElevatorPickUpRequest> handledRequests = new();

        public static int Capacity { get; } = 10;
        public int PeopleInside { get; protected set; }

        public PassangerElevator(int currentFloor = 0, ElevatorTravelDirection direction = ElevatorTravelDirection.Idle, int peopleInside = 0)
        {
            CurrentFloor = currentFloor;
            Direction = direction;
            PeopleInside = peopleInside;
            Status = ElevatorStatus.Idle;
        }

        public void AssignPickup(PassangerElevatorPickUpRequest request)
        {
            if (PeopleInside + request.NoPeopleRequestingElevator > Capacity)
            {
                return;
            }

            PendingPickups.Enqueue(request);

            //PassangerElevatorPickUpRequest? pickUpRequest 
            //    = PendingPickups.FirstOrDefault(r => r.RequestFloorNo == request.RequestFloorNo);

            //if (pickUpRequest != null)
            //{
            //    //What if there is already a pending pickup to that specific floor?
                
            //    pickUpRequest.NoPeopleRequestingElevator += request.NoPeopleRequestingElevator;
            //}
            //else
            //{
            //    PendingPickups.Add(request);
            //}

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

        public async Task RunAsync(Action<PassangerElevatorPickUpRequest> onPickupComplete)
        {
            while (true)
            {
                //Elevator should remain idle if no pickups or dropp-off
                if (!PendingPickups.Any() && !ActiveDropOffs.Any())
                {
                    HighlightElevator = false;

                    Direction = ElevatorTravelDirection.Idle;
                    Status = ElevatorStatus.Idle;

                    await Task.Delay(500); // Let system breathe
                    continue;
                }


                #region Handle Pickups

                if (PendingPickups.Any())
                {
                    var nextPickup = PendingPickups.Dequeue();

                    Direction = nextPickup.RequestFloorNo > CurrentFloor
                        ? ElevatorTravelDirection.Up
                        : ElevatorTravelDirection.Down;

                    Status = ElevatorStatus.Moving;

                    while (CurrentFloor != nextPickup.RequestFloorNo)
                    {
                        HighlightElevator = nextPickup.HighlightElevator;

                        await Task.Delay(2000);
                        CurrentFloor += Direction == ElevatorTravelDirection.Up ? 1 : -1;
                    }

                    Status = ElevatorStatus.LoadingPassengers;
                    await Task.Delay(1000);

                    PeopleInside += nextPickup.NoPeopleRequestingElevator;

                    ActiveDropOffs.Add(new PassangerElevatorDropOffRequest
                    {
                        Id = nextPickup.Id,
                        DestinationFloorNo = nextPickup.DestinationFloorNo,
                        NoPeopleGettingOff = nextPickup.NoPeopleRequestingElevator,
                        HighlightElevator = nextPickup.HighlightElevator
                    });

                    onPickupComplete(nextPickup);
                }

                #endregion

                #region Handle Dropoff

                if (ActiveDropOffs.Any())
                {
                    PassangerElevatorDropOffRequest nextDrop = ActiveDropOffs
                        .OrderBy(r => Math.Abs(CurrentFloor - r.DestinationFloorNo))
                        .First();

                    Direction = nextDrop.DestinationFloorNo > CurrentFloor
                        ? ElevatorTravelDirection.Up
                        : ElevatorTravelDirection.Down;

                    Status = ElevatorStatus.Moving;

                    while (CurrentFloor != nextDrop.DestinationFloorNo)
                    {
                        HighlightElevator = nextDrop.HighlightElevator;

                        await Task.Delay(3000); // Simulate movement
                        CurrentFloor += Direction == ElevatorTravelDirection.Up ? 1 : -1;
                    }

                    // Simulate people getting off the elevator.
                    Status = ElevatorStatus.UnloadingPassengers;
                    await Task.Delay(5000);

                    PeopleInside -= nextDrop.NoPeopleGettingOff;

                    ActiveDropOffs.Remove(nextDrop);
                }

                #endregion
            }
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

    }

    /// <summary>
    /// This represents unassigned people waiting on a floor.
    /// </summary>
    public class PassangerElevatorPickUpRequest
    {
        public string? Id { get; init; }
        public int RequestFloorNo { get; init; }
        public int DestinationFloorNo { get; init; }
        public int NoPeopleRequestingElevator { get; set; }
        public bool HighlightElevator { get; set; }

    }

    /// <summary>
    /// This represents people already inside elevators with known destinations
    /// </summary>
    public class PassangerElevatorDropOffRequest
    {
        public string? Id { get; init; }
        public int DestinationFloorNo { get; init; }
        public int NoPeopleGettingOff { get; init; }
        public bool HighlightElevator { get; set; }

    }
}
