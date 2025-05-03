using DVT_Elevator_Challange.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT_Elevator_Challange_Tests.Models
{
    internal class TestableBuilding : Building
    {
        public TestableBuilding(List<IElevator> elevators, List<BuildingFloor> floors) : base(elevators, floors)
        {

        }

        public IElevator? ExposeFindBestElevator(PickupRequest request)
        {
            return FindBestElevator(request);
        }
    }
}
