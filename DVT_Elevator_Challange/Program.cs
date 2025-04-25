using System;
using System.Collections.Generic;
using System.Linq;

#region Core Setup

List<Elevator> elevators = Enumerable.Range(0, 2)
    .Select(_ => new Elevator())
    .ToList();

List<BuildingFloor> floors = new List<BuildingFloor>
{
    new BuildingFloor { Name = "Lower 2", FloorNo = -2 },
    new BuildingFloor { Name = "Basement", FloorNo = -1 },
    new BuildingFloor { Name = "Ground", FloorNo = 0 },
    new BuildingFloor { Name = "Floor 1", FloorNo = 1 },
    new BuildingFloor { Name = "Floor 2", FloorNo = 2 },
    new BuildingFloor { Name = "Floor 3", FloorNo = 3 },
    new BuildingFloor { Name = "Floor 4", FloorNo = 4 },
    new BuildingFloor { Name = "Floor 5", FloorNo = 5 }
};

Building building = new Building(elevators, floors);

#endregion

#region Models

public class Building
{
    public List<Elevator> Elevators { get; init; }
    public List<BuildingFloor> Floors { get; init; }

    public Building(List<Elevator> elevators, List<BuildingFloor> floors)
    {
        if (floors == null || floors.Count < 3)
            throw new ArgumentException("Building must have at least 3 floors.");

        Elevators = elevators ?? throw new ArgumentNullException(nameof(elevators));
        Floors = floors;
    }

    public void RequestElevator(int currentFloor, Direction desiredDirection, int numberOfPeople)
    {
        Console.WriteLine($"User at floor {currentFloor} requests to go {desiredDirection} with {numberOfPeople} people.");

        Elevator? bestElevator = Elevators
            .Where(e => e.CanPickup(currentFloor, desiredDirection, numberOfPeople))
            .OrderBy(e => Math.Abs(e.CurrentFloor - currentFloor))
            .FirstOrDefault();

        if (bestElevator == null)
        {
            Console.WriteLine("No available elevator can currently take the request.");
            return;
        }

        bestElevator.AddRequest(new ElevatorStop
        {
            FloorNo = currentFloor,
            NoPeopleGettingOff = numberOfPeople
        });

        bestElevator.Move();
    }

}

public class BuildingFloor
{
    public string Name { get; init; } = string.Empty;
    public int FloorNo { get; init; }
}

public class Elevator
{
    private readonly Queue<ElevatorStop> stops = new();

    public int CurrentFloor { get; private set; }
    public int Capacity { get; }
    public int PeopleInside { get; private set; }
    public Direction Direction { get; private set; }

    public Elevator(int initialFloor = 0, int capacity = 10)
    {
        CurrentFloor = initialFloor;
        Capacity = capacity;
        PeopleInside = 0;
        Direction = Direction.Idle;
    }

    public bool CanPickup(int requestedFloor, Direction desiredDirection, int noPeople)
    {
        if (PeopleInside + noPeople > Capacity) return false;
        if (Direction == Direction.Idle) return true;

        bool goingSameWay =
            (Direction == Direction.Up && requestedFloor > CurrentFloor) ||
            (Direction == Direction.Down && requestedFloor < CurrentFloor);

        return goingSameWay && Direction == desiredDirection;
    }

    public void AddRequest(ElevatorStop stop)
    {
        if (PeopleInside + stop.NoPeopleGettingOff > Capacity)
        {
            Console.WriteLine("Cannot add request: elevator over capacity.");
            return;
        }

        stops.Enqueue(stop);
        PeopleInside += stop.NoPeopleGettingOff;
        Console.WriteLine($"Stop added: Floor {stop.FloorNo}, +{stop.NoPeopleGettingOff} people.");
    }

    public void Move()
    {
        while (stops.Count > 0)
        {
            ElevatorStop stop = stops.Dequeue();

            Direction = stop.FloorNo > CurrentFloor ? Direction.Up : Direction.Down;
            Console.WriteLine($"Elevator moving {Direction} from floor {CurrentFloor} to {stop.FloorNo}...");

            CurrentFloor = stop.FloorNo;

            Console.WriteLine($"Arrived at floor {CurrentFloor}. {stop.NoPeopleGettingOff} passenger(s) exit.");
            PeopleInside = Math.Max(0, PeopleInside - stop.NoPeopleGettingOff);

            Direction = stops.Any()
                ? (stops.Peek().FloorNo > CurrentFloor ? Direction.Up : Direction.Down)
                : Direction.Idle;
        }

        Console.WriteLine("Elevator is now idle.");
        Direction = Direction.Idle;
    }
}

public class ElevatorStop
{
    public int FloorNo { get; init; }
    public int NoPeopleGettingOff { get; init; }
}

#endregion

#region Enums

public enum Direction
{
    Up,
    Down,
    Idle
}

#endregion
