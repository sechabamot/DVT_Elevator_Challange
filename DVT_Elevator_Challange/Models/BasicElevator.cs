using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange.Models
{
    public abstract class BasicElevator
    {
        public bool HighlightElevator { get; protected set; }
        public int CurrentFloor { get; protected set; }
        public ElevatorTravelDirection Direction { get; protected set; }
        public ElevatorStatus Status { get; protected set; } = ElevatorStatus.Idle;

    }

    public abstract class BasicElevatorStop
    {
        protected BasicElevatorStop()
        {
            
        }

        public string Id { get; init; }
        public bool HighlightRequest { get; set; }
        public int RequestFloorNo { get; init; }
        public int DestinationFloorNo { get; init; }
    }


    public enum ElevatorType
    {
        Passanger,
    }

    public enum ElevatorTravelDirection
    {
        Idle,
        Up,
        Down
    }

    public enum ElevatorStatus
    {
        Idle,
        Moving,
        LoadingPassengers,
        UnloadingPassengers
    }

}
