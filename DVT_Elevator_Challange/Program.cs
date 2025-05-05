using DVT_Elevator_Challange.Models;
using System.Text;




#region Setup

Console.OutputEncoding = Encoding.UTF8; // Do not remove. Ensures symbols like ↑ and ↓ render properly


// Initialize elevators, floors, and start the elevator engine.
ElevatorInputSession inputSession = new ElevatorInputSession();

List<IElevator> elevators = [.. Enumerable.Range(0, 5).Select(_ => new PassengerElevator())];
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

#endregion

#region Main


//Please don't penalise me for not having a graceful way of terminating my application.:(
//Apart from it being a tough few weeks, It's a console app, please just close it :), 

// Set up background tasks for display and user interaction
Timer requestTimer = new Timer(_ =>
{
    CreateRandomPassangerElevatorRequestInBackground();
}, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

Task displayTask = Task.Run(DisplayPassangerElevatorsPostitions);
Task inputTask = Task.Run(ListenForUserInputAsync);

// Main loop: periodically assigns pending elevator pickup requests
while (true)
{
    building.AssignPendingPickUps();
    await Task.Delay(2000);
}
#endregion

#region Simulation Helpers

/// <summary>
/// Handles user input for elevator request via console.
/// Type 'E' to begin a request and 'Q' anytime to cancel the session.
/// </summary>
async Task ListenForUserInputAsync()
{
    //[Developer Note]:
    //The input session is lacking. My main focus was on the actaul design of core concepts.
    //With a litle more time I am confident I can improve this significanlty.


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

        if (inputSession.IsCancelled)
        {
            inputSession.Reset();
            continue;
        }

        try
        {
            if (!inputSession.CurrentFloor.HasValue)
            {
                string? input = Console.ReadLine();
                if (input?.ToUpper() == "Q") inputSession.IsCancelled = true;

                else if (!int.TryParse(input, out int floor))
                {
                    inputSession.LatestErrorMessage = "❌ Invalid input. Please enter a valid number for your current floor.";
                }
                else
                {
                    inputSession.CurrentFloor = floor;
                    inputSession.ClearError();
                }
            }
            else if (!inputSession.DestinationFloor.HasValue)
            {
                string? input = Console.ReadLine();
                if (input?.ToUpper() == "Q") inputSession.IsCancelled = true;
                else if (!int.TryParse(input, out int floor))
                {
                    inputSession.LatestErrorMessage = "❌ Invalid input. Please enter a valid number for your destination floor.";
                }
                else
                {
                    inputSession.DestinationFloor = floor;
                    inputSession.ClearError();
                }
            }
            else if (!inputSession.NumberOfPeople.HasValue)
            {
                string? input = Console.ReadLine();
                if (input?.ToUpper() == "Q") inputSession.IsCancelled = true;
                else if (!int.TryParse(input, out int people) || people <= 0)
                {
                    inputSession.LatestErrorMessage = "❌ Number of people must be at least 1.";
                }
                else
                {
                    inputSession.NumberOfPeople = people;
                    inputSession.ClearError();
                }

                if (inputSession.IsComplete)
                {
                    try
                    {
                        PassengerPickupRequest request = new PassengerPickupRequest(inputSession.NumberOfPeople ?? 0, inputSession.CurrentFloor ?? 0, inputSession.DestinationFloor ?? 0, true);

                        building.RequestElevator(request);
                        inputSession.Reset();
                    }
                    catch (Exception exception)
                    {
                        inputSession.LatestErrorMessage = $"❌ {exception.Message}";

                        await Task.Delay(5000);
                        inputSession.Reset();
                    }
                }
            }
        }
        catch (Exception exception)
        {
            // In production, consider logging the exception
            inputSession.IsCancelled = true;
            inputSession.IsRequestInProgress = false;
        }

        await Task.Delay(100);
    }
}

/// <summary>
/// Displays the status of all elevators in the console in real time.
/// Highlights elevators assigned to a manual request.
/// </summary>
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

        foreach (PassengerElevator elevator in building.Elevators)
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
            Console.WriteLine("------------------------------");
            Console.WriteLine("Manual Request In Progress...");
            Console.WriteLine($"   Current Floor     : {inputSession.CurrentFloor?.ToString() ?? "?"}");
            Console.WriteLine($"   Destination Floor : {inputSession.DestinationFloor?.ToString() ?? "?"}");
            Console.WriteLine($"   Number of People  : {inputSession.NumberOfPeople?.ToString() ?? "?"}");
            Console.WriteLine($"   [Type 'Q' at any prompt to cancel]");
            Console.WriteLine("------------------------------");
            if (!string.IsNullOrEmpty(inputSession.LatestErrorMessage))
            {
                Console.WriteLine(inputSession.LatestErrorMessage);
            }
        }
        else
        {
            Console.WriteLine("Press [E] to request elevator...");
        }

        Console.WriteLine();
        Console.SetCursorPosition(originalCursorLeft, originalCursorTop);

        Console.ResetColor();
        Thread.Sleep(TimeSpan.FromSeconds(1));
    }
}

/// <summary>
/// Periodically generates a random elevator request for simulation purposes.
/// </summary>
void CreateRandomPassangerElevatorRequestInBackground()
{
    try
    {
        Random random = new Random();

        int minFloor = building.Floors.Min(e => e.FloorNo);
        int maxFloor = building.Floors.Max(e => e.FloorNo);

        int requestFloor = random.Next(minFloor, maxFloor + 1);

        ElevatorTravelDirection[] possibleDirections = (ElevatorTravelDirection[])Enum.GetValues(typeof(ElevatorTravelDirection));
        int directionIndex = random.Next(1, 3); // 1 = Up, 2 = Down (Exclude Idle = 0)
        ElevatorTravelDirection direction = possibleDirections[directionIndex];

        int destinationFloor = direction == ElevatorTravelDirection.Up
            ? random.Next(requestFloor + 1, maxFloor + 1)
            : random.Next(minFloor, requestFloor);

        int noPeople = random.Next(1, PassengerElevator.Capacity);

        PassengerPickupRequest request = new PassengerPickupRequest(noPeople, requestFloor, destinationFloor);
        building.RequestElevator(request);
    }
    catch (Exception exception)
    {
    }
}

/// <summary>
/// Maps elevator direction enum to a readable symbol.
/// </summary>
/// <param name="direction">The direction of the elevator.</param>
/// <returns>↑, ↓, or blank space for Idle.</returns>
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
