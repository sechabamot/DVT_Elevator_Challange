using DVT_Elevator_Challange.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
Console.OutputEncoding = Encoding.UTF8; //Do not remove

#region Setup

ElevatorInputSession inputSession = new ElevatorInputSession();
List<PassangerElevator> elevators = Enumerable.Range(0, 3).Select(_ => new PassangerElevator()).ToList();
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
building.StartElevators();

Timer requestTimer = new Timer(_ =>
{
    CreateRandomPassangerElevatorRequestInBackground();
}, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

#endregion

#region Main 

Task displayTask = Task.Run(DisplayPassangerElevatorsPostitions);
Task userInputTask = Task.Run(ListenForUserInputAsync);

while (true)
{
    building.AssignPendingPickUps();
    await Task.Delay(5000);
}

#endregion

#region Simulation Helpers

async Task ListenForUserInputAsync()
{
    while (true)
    {
        if (!inputSession.IsRequestInProgress)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.E)
            {
                inputSession.IsRequestInProgress = true;
            }

            await Task.Delay(100);
            continue;
        }

        // If input session is in progress
        if (inputSession.IsCancelled)
        {
            inputSession.Reset();
            continue;
        }

        try
        {
            if (!inputSession.CurrentFloor.HasValue)
            {
                //Console.Write("Enter your current floor (Q to cancel): ");
                string? input = Console.ReadLine();
                if (input?.ToUpper() == "Q") inputSession.IsCancelled = true;
                else inputSession.CurrentFloor = int.Parse(input ?? "0");
            }
            else if (!inputSession.DestinationFloor.HasValue)
            {
                //Console.Write("Enter destination floor (Q to cancel): ");
                string? input = Console.ReadLine();
                if (input?.ToUpper() == "Q") inputSession.IsCancelled = true;
                else inputSession.DestinationFloor = int.Parse(input ?? "0");
            }
            else if (!inputSession.NumberOfPeople.HasValue)
            {
                //Console.Write("Enter number of people (Q to cancel): ");
                string? input = Console.ReadLine();
                if (input?.ToUpper() == "Q") inputSession.IsCancelled = true;
                else inputSession.NumberOfPeople = int.Parse(input ?? "1");

                // All inputs gathered
                if (inputSession.IsComplete)
                {
                    var dir = inputSession.DestinationFloor > inputSession.CurrentFloor
                        ? ElevatorTravelDirection.Up
                        : ElevatorTravelDirection.Down;

                    building.RequestElevator(
                        inputSession.CurrentFloor.Value,
                        inputSession.DestinationFloor.Value,
                        dir,
                        inputSession.NumberOfPeople.Value,
                        ElevatorType.Passanger,
                        true
                    );

                    inputSession.Reset();
                }
            }
        }
        catch(Exception exception)
        {
            inputSession.IsCancelled = true;
            inputSession.IsRequestInProgress = false;
        }

        await Task.Delay(100);
    }
}
void DisplayPassangerElevatorsPostitions()
{

    while (true)
    {
        Console.Clear();

        int elevatorIndex = 1;
        int originalCursorLeft = Console.CursorLeft;
        int originalCursorTop = Console.CursorTop;

        Console.SetCursorPosition(0, 0);
        Console.WriteLine(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, 0);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("════════════ Elevators ════════════");
        Console.ResetColor();

        foreach (PassangerElevator elevator in building.Elevators)
        {
            string directionSymbol = GetDirectionSymbol(elevator.Direction);
            string status = elevator.Status.ToString();

            if (elevator.HighlightElevator)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Elevator {elevatorIndex,2}:  Floor {elevator.CurrentFloor,-2} | People {elevator.PeopleInside,-2} | {directionSymbol} | {status}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"Elevator {elevatorIndex,2}:  Floor {elevator.CurrentFloor,-2} | People {elevator.PeopleInside,-2} | {directionSymbol} | {status}");
            }
            elevatorIndex++;

        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;

        if (inputSession.IsRequestInProgress)
        {
            Console.WriteLine("🚦 Manual Request In Progress...");
            Console.WriteLine($"   Current Floor     : {inputSession.CurrentFloor?.ToString() ?? "?"}");
            Console.WriteLine($"   Destination Floor : {inputSession.DestinationFloor?.ToString() ?? "?"}");
            Console.WriteLine($"   Number of People  : {inputSession.NumberOfPeople?.ToString() ?? "?"}");
            Console.WriteLine($"   [Type 'Q' at any prompt to cancel]");
        }
        else
        {
            Console.WriteLine("💡 Press [E] to request elevator...");
        }

        // Padding
        Console.WriteLine();
        Console.SetCursorPosition(originalCursorLeft, originalCursorTop);

        Console.ResetColor();
        Thread.Sleep(TimeSpan.FromSeconds(1));
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
        ElevatorTravelDirection.Idle => " ",
        _ => "?"
    };
}

#endregion
