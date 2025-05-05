# 🛗 Elevator Simulation Console App

This is a C# console-based elevator simulation designed to mimic how a building with multiple elevators operates under different passenger loads and requests.

## 🚀 Features

- Simulates up to 10 passenger elevators moving independently.
- Supports random and manual elevator requests.
- Visual representation of elevator positions, directions, and status in real-time.
- Request highlighting for visibility.
- Input session handler with step-by-step guided request entry.
- Input validation with error feedback through a shared error field.

## 🧩 Tech Stack

- .NET 6+
- Console Application
- Object-Oriented Design
- Clean Architecture Principles
- Interfaces and Abstraction for elevator behavior

## 🧠 Concepts Demonstrated

- Polymorphism via `IElevator` interface.
- Elevator-specific pickup suitability scoring.
- Real-time async elevator operations using `Task` and `Timer`.
- Queue and List-based request management.
- Unit testing with xUnit.

## ▶️ How to Run

1. Clone the repository.
2. Open the solution in **Visual Studio** or **Rider**.
3. Build and run the console application.

### Manual Requests

- Press `E` to initiate an elevator request.
- Enter:
  - Current floor
  - Destination floor
  - Number of people (must be greater than 0)
- Press `Q` during any step to cancel.

### Auto Simulation

- Every 10 (Adjustable) seconds, a random elevator request is generated in the background.

## ✅ Testing

Run the unit tests using your preferred test runner (e.g., Test Explorer in Visual Studio). Key units tested:

- `PassengerElevator` behavior
- Suitability scoring
- Building elevator assignment
- Input validation

(Still requires a few more tests)

