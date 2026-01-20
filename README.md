# Movie Show Seat Booking Backend System

## Overview

This project implements a **backend-only seat management system** for movie shows.  
It focuses exclusively on **seat availability, temporary holds, and confirmed bookings** while ensuring **data consistency under concurrent access**.

The system is designed to handle real-world edge cases such as:
- Multiple users booking the same seats simultaneously
- Seats being held but not confirmed
- Expired seat holds becoming available again
- Duplicate booking attempts
- System restarts during in-progress bookings

At no point can a seat be booked more than once.

---

## Why This Project

This project was built to demonstrate:

- Designing backend systems that behave correctly under **concurrent access**
- Handling **temporary resource reservation** with expiry
- Preventing **double booking** using database-level guarantees
- Building **self-healing systems** that recover from failures automatically

It focuses on correctness and reliability rather than UI or payment flows.

---

## Technology Stack

- **Framework**: ASP.NET Core Web API
- **ORM**: Entity Framework Core
- **Database**: SQL Server (LocalDB / SQL Server Express)
- **Concurrency Control**: Serializable Transactions
- **API Documentation**: Swagger (OpenAPI)
- **Background Processing**: Hosted Background Service

---

## Core Concepts & Design Decisions

### Seat Lifecycle
Each seat can be in one of the following states:

| Status     | Description |
|------------|-------------|
| Available  | Free to be selected |
| Held       | Temporarily reserved |
| Booked     | Permanently booked |

---

### Seat Hold Mechanism

- Seats are **held temporarily** for a configurable duration
- Each hold has:
  - `HoldId`
  - Expiry timestamp
- Holds prevent other users from selecting the same seats
- If not confirmed before expiry, seats become **Available** again

---

### Booking Confirmation

- Booking is only allowed for **valid, unexpired holds**
- Booking is **idempotent**
- Repeated confirmation requests return `"Already booked"`
- Booking confirmation is idempotent to safely handle client retries, network failures, or duplicate requests.

---

### Concurrency Handling

To prevent race conditions:
- All seat hold and booking operations run inside **serializable database transactions**

This guarantees:
- No double booking
- No overselling
- Correct behavior under heavy concurrency

### Example Concurrency Scenario

- Two users attempt to hold seat `S1` at the same time
- One transaction succeeds
- The other transaction fails and retries or returns a conflict
- Result: seat `S1` is held only once


---

### Background Cleanup Service

A background service periodically:
- Releases expired seat holds
- Marks expired holds as `Expired`
- Ensures data correctness even if:
- Users abandon booking
- System restarts
- API is not called after expiry

This improves **system reliability** and **self-healing behavior**.

> Note: Although holds have an expiry timestamp, the background cleanup service.
> ensures that expired holds are released even if no API request is made after expiry.
> This prevents stale data and guarantees consistency across restarts.


---

## High-Level Flow

1. User requests available seats
2. User holds seats → seats move to `Held`
3. Hold expires OR booking confirmed
4. If confirmed → seats move to `Booked`
5. If expired → seats return to `Available`
6. Background service cleans up expired holds

---

## What the System Can Answer

- Total seats for a show
- Available seats
- Held (temporarily reserved) seats
- Booked seats
- What happens when a booking is not completed

---

## Database Configuration

The application uses SQL Server and Entity Framework Core.

⚠️ **Important: Connection String Setup**

Before running `dotnet ef database update`, ensure that a valid SQL Server
connection string is configured in `appsettings.json`.

By default, the project is configured to run with **SQL Server LocalDB**:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=MovieSeatBookingDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```
---

## API Endpoints

### Create a Show

**POST** `/api/shows`
**Request**
```json
{
  "movieName": "Interstellar",
  "totalSeats": 50
}
```
**Response**
```json
{
  "id": 3
}
```

### Get All Shows

- Returns all available movie shows with minimal details.
- This endpoint is intended for clients to discover show IDs dynamically
- instead of relying on hardcoded values.

**GET** `/api/shows`

**Response**
```json
[
  {
    "id": 1,
    "movieName": "Interstellar"
  },
  {
    "id": 2,
    "movieName": "Inception"
  }
]
```

### Get Seat Summary

**GET** `/api/shows/{showId}/seats/summary`

**Response**
```json
{
  "total": 50,
  "available": 45,
  "held": 3,
  "booked": 2
}
```

### Get Available Seats

**GET** `/api/shows/{showId}/seats/available`

**Response**

```json
["S1", "S2", "S5", "S6"]
```

### Hold Seats

**POST** `/api/holds?showId=1`

**Request**
```json
{
  "seatNumbers": ["S1", "S2"],
  "holdDurationSeconds": 120
}
```

**Response**
```json
{
  "holdId": "e1c6a3d2-9f3c-4e1a-b4c8-8a1c2d3e4f5g",
  "expiry": "2026-01-23T10:30:00Z"
}
```
### Confirm Booking

**POST** `/api/holds/{holdId}/confirm`

**Response**

Booked

---

### Validation Rules

- Seat list cannot be empty
- Duplicate seat numbers are rejected
- Seat hold duration is limited to **5 minutes (300 seconds)**
- Non-existent show is rejected
- Seats must be Available to be held
- Expired or invalid holds cannot be booked

### ▶️ How to Run the Project

Prerequisites

- .NET SDK **10.0**
  Download: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
- SQL Server (LocalDB / SQL Server Express)

1. Clone the repository
```bash
git clone https://github.com/ADITHYANBASOK/MovieTicketBooking-ASPDotNET.git
cd MovieTicketBooking-ASPDotNET
```
2. Restore dependencies
- Run this command inside the project directory:
```bash
dotnet restore MovieTicketBooking/MovieTicketBooking.csproj
```
3. Create database and apply migrations
- Run EF Core commands from the project folder:
```bash
cd MovieTicketBooking
dotnet ef database update
```
4. Run the application
```bash
dotnet run
```
---

## API Documentation (Swagger)

Optional but good:

```md
Swagger UI:
https://localhost:{configured-port}/swagger
```

### Testing Instructions

- Use Swagger UI to:
- Create a show
- Fetch available seats
- Hold seats from multiple browser tabs
- Confirm booking
- Wait for expiry and verify seat release

### Assumptions & Limitations:

- No authentication (out of scope)
- No payment processing
- Single show per booking
- Seat numbering is sequential
- Time-based expiry uses UTC

### Conclusion

- This system prioritizes:
- Correctness under concurrency
- Simple and clear state transitions
- Reliable recovery from failures

---

## Submission Notes

This project is intentionally scoped to backend seat management only.
Authentication, payments, and UI concerns were excluded as per the problem statement
to keep the focus on correctness, concurrency handling, and system reliability.




