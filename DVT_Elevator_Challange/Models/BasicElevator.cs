using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange.Models
{
    public interface IElevator
    {
        int CurrentFloor { get; }
        Direction Direction { get; }
        Status Status { get; }
        Task RunAsync(Action<PickupRequest> onPickupComplete);
        bool CanPickup(PickupRequest request);
        void AssignPickup(PickupRequest request);
        int? GetSuitabilityScore(PickupRequest request);
    }

    public abstract class BasicElevator : IElevator
    {
        public bool HighlightElevator { get; protected set; }
        public int CurrentFloor { get; protected set; }
        public Direction Direction { get; protected set; } = Direction.Idle;
        public Status Status { get; protected set; } = Status.Idle;
        public abstract Task RunAsync(Action<PickupRequest> onPickupComplete);
        public abstract bool CanPickup(PickupRequest request);
        public abstract void AssignPickup(PickupRequest request);
        public abstract int? GetSuitabilityScore(PickupRequest request);
    }

    public abstract class ElevatorRequest 
    {
        public int DestinationFloorNo { get; init; }
        public bool Highlight { get; set; }
    }

    public abstract class PickupRequest : ElevatorRequest
    {
        public int RequestFloorNo { get; init; }
    }

    public abstract class DropOffRequest : ElevatorRequest
    {
     
    }


    public enum ElevatorType
    {
        Passanger,
    }

    public enum Direction
    {
        Idle,
        Up,
        Down
    }

    public enum Status
    {
        Idle,
        Moving,
        LoadingPassengers,
        UnloadingPassengers
    }

}
