using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange.Models
{
    public abstract class BasicElevator
    {
        public int CurrentFloor { get; protected set; }
        public ElevatorTravelDirection Direction { get; protected set; }

    }

    public abstract class BasicElevatorStop
    {
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
}
