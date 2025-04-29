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

        public Building(List<PassangerElevator> elevators, List<BuildingFloor> floors)
        {
            if (floors == null || floors.Count < 3)
                throw new ArgumentException("Building must have at least 3 floors.");

            Elevators = elevators ?? throw new ArgumentNullException(nameof(elevators));
            Floors = floors;
        }

        public void RequestElevator(int currentFloor, int destinationFloor, ElevatorTravelDirection desiredDirection, int numberOfPeople, ElevatorType type = ElevatorType.Passanger)
        {
            switch (type)
            {
                case ElevatorType.Passanger:
                    {
                        HandlePassangerElevatorRequest(currentFloor, destinationFloor, desiredDirection, numberOfPeople);
                    }
                    break;
                default:

                    break;
            }
        }

        private void HandlePassangerElevatorRequest(int currentFloor, int destinationFloor, ElevatorTravelDirection desiredDirection, int numberOfPeople)
        {

            PassangerElevator? bestElevator = Elevators
                .Where(e => e.CanPickup(currentFloor, desiredDirection, numberOfPeople))
                .OrderBy(e => Math.Abs(e.CurrentFloor - currentFloor))
                .FirstOrDefault();

            if (bestElevator == null)
            {
                return;
            }

            bestElevator.AddRequest(new PassangerElevatorRequest
            {
                RequestFloorNo = currentFloor,
                DestinationFloorNo = destinationFloor,
                NoPeopleGettingOff = numberOfPeople
            });
            bestElevator.Move();
        }


    }

    public class BuildingFloor
    {
        public string? Name { get; init; }
        public int FloorNo { get; init; }

        //TODO: Accomidate for what kind of elevator can reach this building floor.
    }




}


