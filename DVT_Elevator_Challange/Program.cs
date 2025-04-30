using DVT_Elevator_Challange.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

#region Simulation paramaters

CancellationTokenSource cancelSrc = new CancellationTokenSource();
const int updateIntervalInMilliseconds = 1000;

#endregion

#region Core Setup

List<PassangerElevator> elevators = Enumerable.Range(0, 10).Select(_ => new PassangerElevator()).ToList();
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
Timer requestTimer = new Timer(_ =>
{
    CreateRandomPassangerElevatorRequestInBackground();
}, null, TimeSpan.Zero, TimeSpan.FromSeconds(2.5));

// Set up tasks
Task userInputTask = Task.Run(ListenForUserInputAsync);
Task displayTask = Task.Run(DisplayPassangerElevatorsPostitions);

// Wait for both to run together
await Task.WhenAll(userInputTask, displayTask);

async Task ListenForUserInputAsync()
{
    while (!cancelSrc.Token.IsCancellationRequested)
    {
        Console.WriteLine();
        Console.WriteLine("Manual Request: Enter 'R' to request an elevator, or Enter to continue...");
        string? input = Console.ReadLine();

        if (string.Equals(input, "R", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                Console.WriteLine("Enter your current floor:");
                int requestFloor = int.Parse(Console.ReadLine() ?? "0");

                Console.WriteLine("Enter your destination floor:");
                int destinationFloor = int.Parse(Console.ReadLine() ?? "0");

                Console.WriteLine("How many people?");
                int numberOfPeople = int.Parse(Console.ReadLine() ?? "1");

                ElevatorTravelDirection direction = destinationFloor > requestFloor
                    ? ElevatorTravelDirection.Up
                    : ElevatorTravelDirection.Down;

                building.RequestElevator(requestFloor, destinationFloor, direction, numberOfPeople, ElevatorType.Passanger, true);

                Console.WriteLine("✅ Request submitted!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        await Task.Delay(500);
    }
}
void DisplayPassangerElevatorsPostitions()
{
    while (!cancelSrc.Token.IsCancellationRequested)
    {
        int originalCursorLeft = Console.CursorLeft;
        int originalCursorTop = Console.CursorTop;

        Console.SetCursorPosition(0, 0);
        Console.WriteLine(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, 0);

        Console.Write("Elevator Status: ");

        foreach (PassangerElevator elevator in building.Elevators)
        {
            string directionSymbol = GetDirectionSymbol(elevator.Direction);

            if (elevator.HighlightElevator)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[ *{elevator.CurrentFloor} {directionSymbol}* ] ");
                Console.ResetColor();
            }
            else
            {
                Console.Write($"[ {elevator.CurrentFloor} {directionSymbol} ] ");
            }
        }

        // Padding
        Console.WriteLine();
        Console.SetCursorPosition(originalCursorLeft, originalCursorTop);

        Thread.Sleep(updateIntervalInMilliseconds);
    }
}

void CreateRandomPassangerElevatorRequestInBackground()
{
    Random random = new Random();

    int minFloor = building.Floors.Min(e => e.FloorNo);
    int maxFloor = building.Floors.Max(e => e.FloorNo);

    int requestFloor = random.Next(minFloor, maxFloor + 1);

    ElevatorTravelDirection[] possibleDirections = (ElevatorTravelDirection[])Enum.GetValues(typeof(ElevatorTravelDirection));
    int directionIndex = random.Next(1, 3); // 1 = Up, 2 = Down (Exclude Idle = 0)
    ElevatorTravelDirection direction = possibleDirections[directionIndex];

    int destinationFloor;

    if (direction == ElevatorTravelDirection.Up)
    {
        destinationFloor = random.Next(requestFloor + 1, maxFloor + 1); // must be above requestFloor
    }
    else // ElevatorTravelDirection.Down
    {
        destinationFloor = random.Next(minFloor, requestFloor); // must be below requestFloor
    }

    int noPeople = random.Next(1, PassangerElevator.Capacity); // at least 1 person

    building.RequestElevator(requestFloor, destinationFloor, direction, noPeople, ElevatorType.Passanger);
}



string GetDirectionSymbol(ElevatorTravelDirection direction)
{
    return direction switch
    {
        ElevatorTravelDirection.Up => "↑",
        ElevatorTravelDirection.Down => "↓",
        ElevatorTravelDirection.Idle => "",
        _ => "?"
    };
}

#endregion

