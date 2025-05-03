using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange.Models
{
    /// <summary>
    /// Represents a passenger elevator that can handle pickup and drop-off requests within a building.
    /// It maintains a queue of pending pickups and a list of active drop-offs, simulating real-world elevator behavior.
    /// </summary>
    public class PassengerElevator : BasicElevator
    {
        /// <summary>
        /// A queue of pending pickup requests, each representing people waiting on specific floors.
        /// </summary>
        public Queue<PassengerPickupRequest> PendingPickups { get; protected set; } = new();

        /// <summary>
        /// A list of passengers currently inside the elevator and their respective destinations.
        /// </summary>
        public List<PassangerDropOffRequest> ActiveDropOffs { get; protected set; } = new();

        /// <summary>
        /// Defines the maximum number of people the elevator can carry at once.
        /// Shared across all instances, and typically constant in real-world scenarios.
        /// </summary>
        public static int Capacity { get; } = 10;

        /// <summary>
        /// The current number of people inside the elevator.
        /// </summary>
        public int PeopleInside { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PassengerElevator"/> class.
        /// Allows optional parameters to set the initial state for testing or custom simulations.
        /// </summary>
        /// <param name="currentFloor">The floor where the elevator starts. Default is 0.</param>
        /// <param name="direction">The starting direction of travel. Default is Idle.</param>
        /// <param name="peopleInside">Initial number of people inside the elevator. Default is 0.</param>
        public PassengerElevator(int currentFloor = 0, Direction direction = Direction.Idle, int peopleInside = 0)
        {
            CurrentFloor = currentFloor;
            Direction = direction;
            PeopleInside = peopleInside;
            Status = Status.Idle;
        }

        /// <summary>
        /// Assigns a pickup request to this elevator if it has capacity and the request is valid.
        /// </summary>
        /// <param name="request">The pickup request to be assigned.</param>
        public override void AssignPickup(PickupRequest request)
        {
            if (request is PassengerPickupRequest passengerRequest)
            {
                if (PeopleInside + passengerRequest.NoPeople > Capacity)
                {
                    return;
                }
                PendingPickups.Enqueue(passengerRequest);
            }
        }

        /// <summary>
        /// Determines whether the elevator is able to accept the given pickup request.
        /// Evaluates current direction, idle state, and capacity constraints.
        /// </summary>
        /// <param name="request">The pickup request being evaluated.</param>
        /// <returns>True if the elevator can accept the request; otherwise false.</returns>
        public override bool CanPickup(PickupRequest request)
        {
            if (request is not PassengerPickupRequest passenger) return false;
            if (PeopleInside + passenger.NoPeople > Capacity) return false;

            bool goingSameWay =
                Direction == Direction.Up && passenger.RequestFloorNo > CurrentFloor ||
                Direction == Direction.Down && passenger.RequestFloorNo < CurrentFloor;

            return Direction == Direction.Idle || goingSameWay;
        }

        /// <summary>
        /// Executes the elevator's main run loop asynchronously.
        /// Continuously processes pickups and drop-offs while updating status and direction.
        /// </summary>
        /// <param name="onPickupComplete">Callback to notify the building when a pickup is completed.</param>
        public override async Task RunAsync(Action<PickupRequest> onPickupComplete)
        {
            while (true)
            {
                if (!PendingPickups.Any() && !ActiveDropOffs.Any())
                {
                    HighlightElevator = false;
                    Direction = Direction.Idle;
                    Status = Status.Idle;
                    await Task.Delay(500);
                    continue;
                }

                #region Handle Pickups

                if (PendingPickups.Any())
                {
                    PassengerPickupRequest nextPickup = PendingPickups.Dequeue();

                    Direction = nextPickup.RequestFloorNo > CurrentFloor
                        ? Direction.Up
                        : Direction.Down;

                    Status = Status.Moving;

                    while (CurrentFloor != nextPickup.RequestFloorNo)
                    {
                        HighlightElevator = nextPickup.Highlight;
                        await Task.Delay(2000);
                        CurrentFloor += Direction == Direction.Up ? 1 : -1;
                    }

                    Status = Status.LoadingPassengers;
                    await Task.Delay(1000);

                    PeopleInside += nextPickup.NoPeople;

                    ActiveDropOffs.Add(new PassangerDropOffRequest
                    {
                        DestinationFloorNo = nextPickup.DestinationFloorNo,
                        NoPeopleGettingOff = nextPickup.NoPeople,
                        Highlight = nextPickup.Highlight
                    });

                    onPickupComplete(nextPickup);
                }

                #endregion

                #region Handle Dropoff

                if (ActiveDropOffs.Any())
                {
                    PassangerDropOffRequest nextDrop = ActiveDropOffs
                        .OrderBy(r => Math.Abs(CurrentFloor - r.DestinationFloorNo))
                        .First();

                    Direction = nextDrop.DestinationFloorNo > CurrentFloor
                        ? Direction.Up
                        : Direction.Down;

                    Status = Status.Moving;

                    while (CurrentFloor != nextDrop.DestinationFloorNo)
                    {
                        HighlightElevator = nextDrop.Highlight;
                        await Task.Delay(3000);
                        CurrentFloor += Direction == Direction.Up ? 1 : -1;
                    }

                    Status = Status.UnloadingPassengers;
                    await Task.Delay(5000);

                    PeopleInside -= nextDrop.NoPeopleGettingOff;
                    ActiveDropOffs.Remove(nextDrop);
                }

                #endregion
            }
        }

        /// <summary>
        /// Calculates how suitable this elevator is for the given request.
        /// The lower the score, the more suitable the elevator is.
        /// </summary>
        /// <param name="request">The pickup request to evaluate.</param>
        /// <returns>
        /// A non-null integer score if the elevator can handle the request; otherwise null.
        /// A lower score means a closer elevator.
        /// </returns>
        public override int? GetSuitabilityScore(PickupRequest request)
        {
            if (request is not PassengerPickupRequest passenger) return null;
            if (!CanPickup(request)) return null;

            return Math.Abs(CurrentFloor - passenger.RequestFloorNo);
        }
    }

    /// <summary>
    /// Represents a request made by passengers waiting on a specific floor for a passenger elevator.
    /// Includes the number of people making the request and the floor they want to go to.
    /// </summary>
    public class PassengerPickupRequest : PickupRequest
    {
        /// <summary>
        /// Gets or sets the number of people requesting the elevator at the given floor.
        /// </summary>
        public int NoPeople { get; set; }
    }


    /// <summary>
    /// Represents a drop-off instruction for passengers who are already inside a passenger elevator.
    /// Each drop-off request contains the floor to stop at and the number of people to disembark there.
    /// </summary>
    public class PassangerDropOffRequest : DropOffRequest
    {
        /// <summary>
        /// Gets the number of people who will get off at the specified destination floor.
        /// </summary>
        public int NoPeopleGettingOff { get; init; }
    }

}
