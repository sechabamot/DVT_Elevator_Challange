using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange.Models
{
    public class ElevatorInputSession
    {
        public bool IsRequestInProgress { get; set; } = false;
        public bool IsCancelled { get; set; } = false;
        public int? CurrentFloor { get; set; }
        public int? DestinationFloor { get; set; }
        public int? NumberOfPeople { get; set; }

        public bool IsComplete =>
            CurrentFloor.HasValue &&
            DestinationFloor.HasValue &&
            NumberOfPeople.HasValue;

        public void Reset()
        {
            IsRequestInProgress = false;
            IsCancelled = false;
            CurrentFloor = null;
            DestinationFloor = null;
            NumberOfPeople = null;
        }
    }

}
