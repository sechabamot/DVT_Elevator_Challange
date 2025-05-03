using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static DVT_Elevator_Challange.Models.PassengerElevator;

namespace DVT_Elevator_Challange.Models
{
    /// <summary>
    /// Represents a multi-floor building capable of managing and dispatching multiple elevators.
    /// This class acts as the central controller (or brain) that tracks elevator locations,
    /// passenger pickup requests, and handles basic elevator assignment logic.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// A collection of elevators currently operating in the building.
        /// These can be of different types (passenger, freight, etc.) and must implement the <see cref="IElevator"/> interface.
        /// </summary>
        public List<IElevator> Elevators { get; init; } = new();

        /// <summary>
        /// A list of all the physical floors in the building.
        /// Each floor is represented by a <see cref="BuildingFloor"/> object containing floor number and an optional label.
        /// </summary>
        public List<BuildingFloor> Floors { get; init; } = new();

        /// <summary>
        /// A list of pending pickup requests made by passengers waiting on specific floors.
        /// Each request includes the origin floor, destination floor, number of people, and any flags for UI highlighting.
        /// </summary>
        public List<PickupRequest> PickUpRequests { get; protected set; } = new();

        /// <summary>
        /// A list of drop-off instructions currently queued across all elevators.
        /// Not yet used by the <c>Building</c> class directly but available for future global coordination logic.
        /// </summary>
        public List<DropOffRequest> DropOffRequests { get; protected set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class.
        /// Validates floor count, prevents duplicate floor numbers, and sets up elevator infrastructure.
        /// </summary>
        /// <param name="elevators">List of elevators available to the building.</param>
        /// <param name="floors">List of floors present in the building. Must contain at least three unique floors.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the building has fewer than three floors or when duplicate floor numbers are found.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when the elevator list is null.</exception>
        public Building(List<IElevator> elevators, List<BuildingFloor> floors)
        {
            if (floors == null || floors.Count < 3)
                throw new ArgumentException("Building must have at least 3 floors.");

            bool hasDuplicates = floors
                .GroupBy(f => f.FloorNo)
                .Any(g => g.Count() > 1);

            if (hasDuplicates)
                throw new ArgumentException("Building floors must have unique floor numbers.");

            Elevators = elevators ?? throw new ArgumentNullException(nameof(elevators));
            Floors = floors;
        }


        /// <summary>
        /// Adds a pickup request to the building's queue.
        /// This request will later be assigned to a suitable elevator via <see cref="AssignPendingPickUps"/>.
        /// </summary>
        /// <param name="request">The pickup request to queue.</param>
        public void RequestElevator(PickupRequest request)
        {
            PickUpRequests.Add(request);
        }

        /// <summary>
        /// Tries to assign all currently queued pickup requests to the most suitable elevators.
        /// Each elevator is evaluated for suitability based on distance, direction, and capacity.
        /// Successfully assigned requests are removed from the pending queue.
        /// </summary>
        public void AssignPendingPickUps()
        {
            List<PickupRequest> requests = PickUpRequests.ToList();

            foreach (PickupRequest request in requests)
            {
                IElevator? bestElevator = FindBestElevator(request);

                if (bestElevator != null)
                {
                    bestElevator.AssignPickup(request);
                    PickUpRequests.Remove(request);
                }
            }
        }

        /// <summary>
        /// Determines the best elevator to assign to a given pickup request.
        /// Scoring is based on proximity, direction, and ability to pick up.
        /// </summary>
        /// <param name="request">The pickup request to evaluate.</param>
        /// <returns>The best available elevator or <c>null</c> if none are suitable.</returns>
        protected IElevator? FindBestElevator(PickupRequest request)
        {
            Direction direction = request.DestinationFloorNo > request.RequestFloorNo
                ? Direction.Up
                : Direction.Down;

            return Elevators
                .Where(elevator =>
                    elevator.CanPickup(request) &&
                    (
                        elevator.Direction == Direction.Idle ||
                        (
                            elevator.Direction == direction &&
                            (
                                (direction == Direction.Up && elevator.CurrentFloor <= request.RequestFloorNo) ||
                                (direction == Direction.Down && elevator.CurrentFloor >= request.RequestFloorNo)
                            )
                        )
                    ))
                .OrderBy(elevator => Math.Abs(elevator.CurrentFloor - request.RequestFloorNo))
                .FirstOrDefault();
        }

        /// <summary>
        /// Starts elevator operation asynchronously.
        /// Each elevator is run independently and continuously processes pickups and drop-offs.
        /// </summary>
        public void StartElevators()
        {
            foreach (IElevator elevator in Elevators)
            {
                _ = Task.Run(() => elevator.RunAsync(OnPickupComplete));
            }
        }

        /// <summary>
        /// Callback used when a pickup request has been completed by an elevator.
        /// Currently a placeholder for future event-driven coordination.
        /// </summary>
        /// <param name="request">The completed pickup request.</param>
        private void OnPickupComplete(PickupRequest request)
        {
            // Reserved for analytics or system state updates
        }
    }

    /// <summary>
    /// Represents a single floor within a building.
    /// Each floor has a numeric identifier and an optional display name (e.g., \"Ground\", \"Basement\").
    /// </summary>
    public class BuildingFloor
    {
        /// <summary>
        /// An optional human-readable name for the floor.
        /// This could be labels like "Ground", "Lower 2", or "Penthouse".
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The numeric identifier for the floor.
        /// Positive for upper floors, 0 for ground level, negative for basements.
        /// </summary>
        public int FloorNo { get; init; }
    }


}


