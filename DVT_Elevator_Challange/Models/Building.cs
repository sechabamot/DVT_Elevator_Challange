using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static DVT_Elevator_Challange.Models.PassangerElevator;

namespace DVT_Elevator_Challange.Models
{

    public class Building
    {
        public List<PassangerElevator> Elevators { get; init; } = new List<PassangerElevator>();
        public List<BuildingFloor> Floors { get; init; } = new List<BuildingFloor>();
        public List<PassangerElevatorPickUpRequest> PickUpRequests { get; protected set; } = new List<PassangerElevatorPickUpRequest>();
        public List<PassangerElevatorDropOffRequest> DropOffRequests { get; protected set; } = new List<PassangerElevatorDropOffRequest>();


        public Building(List<PassangerElevator> elevators, List<BuildingFloor> floors)
        {
            if (floors == null || floors.Count < 3)
                throw new ArgumentException("Building must have at least 3 floors.");

            Elevators = elevators ?? throw new ArgumentNullException(nameof(elevators));
            Floors = floors;
        }

        public void RequestElevator(int currentFloor, int destinationFloor, ElevatorTravelDirection desiredDirection, int numberOfPeople, ElevatorType type = ElevatorType.Passanger, bool highlightRequest = false)
        {
            switch (type)
            {
                case ElevatorType.Passanger:
                    {
                        HandlePassangerElevatorRequest(currentFloor, destinationFloor, desiredDirection, numberOfPeople, highlightRequest);
                    }
                    break;
                default:

                    break;
            }
        }

        private void HandlePassangerElevatorRequest(int requestFloorNo, int destinationFloor, ElevatorTravelDirection desiredDirection, int numberOfPeople, bool highlightRequest = false)
        {
            //Step 1: Let's add the request to memeory unless a similar request already exists

            PassangerElevatorPickUpRequest? existingRequest = PickUpRequests
                .FirstOrDefault(r => r.RequestFloorNo == requestFloorNo);
            
            if (existingRequest != null)
            {
                
            }
            else
            {

                Guid guid = Guid.NewGuid();
                PickUpRequests.Add(new PassangerElevatorPickUpRequest
                {
                    Id = guid.ToString(),
                    RequestFloorNo = requestFloorNo,
                    NoPeopleRequestingElevator = numberOfPeople,
                });

            }

            //PassangerElevator? bestElevator = FindBestElevator(requestFloorNo, desiredDirection, numberOfPeople);

            //if (bestElevator == null)
            //{
            //    return;
            //}

            //bestElevator.AddRequest(new PassangerElevatorPickUpRequest
            //{
            //    Id = guid.ToString(),
            //    RequestFloorNo = requestFloorNo,
            //    DestinationFloorNo = destinationFloor,
            //    NoPeopleRequestingElevator = numberOfPeople,
            //    HighlightRequest = highlightRequest
            //});
            //bestElevator.Move();
        }

        public void AssignPendingPickUps()
        {
            List<PassangerElevatorPickUpRequest> assigned = new();

            foreach (var request in PickUpRequests)
            {
                PassangerElevator? bestElevator = FindBestElevator(
                    request.RequestFloorNo,
                    request.RequestFloorNo >= Elevators.Min(e => e.CurrentFloor) ? ElevatorTravelDirection.Up : ElevatorTravelDirection.Down,
                    request.NoPeopleRequestingElevator
                );

                if (bestElevator != null)
                {
                    bestElevator.AssignPickup(request); // We'll build this method in the elevator later
                    assigned.Add(request); // Mark this request as assigned
                }
            }

            // Remove successfully assigned pickups
            foreach (var r in assigned)
            {
                PickUpRequests.Remove(r);
            }
        }


        protected PassangerElevator? FindBestElevator(int requestFloor, ElevatorTravelDirection desiredDirection, int numberOfPeople)
        {

            List<PassangerElevator> candidateElevators = Elevators
            .Where(elevator =>
                elevator.CanPickup(requestFloor, desiredDirection, numberOfPeople) &&
                (
                    elevator.Direction == ElevatorTravelDirection.Idle ||
                    (elevator.Direction == desiredDirection &&
                    (
                        (desiredDirection == ElevatorTravelDirection.Up && elevator.CurrentFloor <= requestFloor) ||
                        (desiredDirection == ElevatorTravelDirection.Down && elevator.CurrentFloor >= requestFloor)
                    ))
                ))
            .OrderBy(elevator => Math.Abs(elevator.CurrentFloor - requestFloor))
            .ToList();

            return candidateElevators.FirstOrDefault();
        }

    }

    public class BuildingFloor
    {
        public string? Name { get; init; }
        public int FloorNo { get; init; }

        //TODO: Accomidate for what kind of elevator can reach this building floor.
    }




}


