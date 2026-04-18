CAAMS Airline Web Application

A full-stack airline ticket booking system built with ASP.NET Core 8 (Razor Pages) and SQL Server. Developed as part of a university database systems project.

Features
Customers
Register and log in
Search and book one-way or round-trip flights
Select seat class (Economy, Business, First)
Enter passenger details and complete payment
View booking confirmation
Admin / Staff
Dashboard with booking and revenue statistics
Manage flights, airports, and aircraft (create, edit, delete)
View and manage bookings (edit passengers, change seats)
Reports:
Daily revenue
Route profitability
Most booked flights
Customer loyalty
Tech Stack
Framework: ASP.NET Core 8 (Razor Pages)
Database: SQL Server (LocalDB for development)
ORM: Entity Framework Core 8
Authentication: Cookie-based with role authorization
Frontend: Bootstrap 5, vanilla JavaScript
Prerequisites
.NET 8 SDK
SQL Server LocalDB (included with Visual Studio 2022)
Git

If using a full SQL Server instance, update the connection string accordingly.

Setup Instructions
1. Clone the repository
git clone <your-repo-url>
cd CAAMSAirlineWebApp
2. Restore dependencies
dotnet restore
3. Apply database migrations
dotnet tool install --global dotnet-ef
dotnet ef database update
4. Run the application
dotnet run

The application will start at:

https://localhost:<port>

The database is seeded automatically on first launch.

Connection String

Default configuration uses SQL Server LocalDB:

"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CAAMS;Trusted_Connection=True;MultipleActiveResultSets=true"
}

To use another SQL Server instance, update the connection string in appsettings.json and run migrations again.

Test Accounts
Role	Username	Password
Admin	admin	Admin@123
Staff	staff	Staff@123
Customer	customer	Customer@123

Users can also register a new account through the /Register page.

Seeded Data
Airports
KUL – Kuala Lumpur
SIN – Singapore
BKK – Bangkok
HKG – Hong Kong
NRT – Tokyo
SYD – Sydney
Aircraft
Boeing 737-800
Airbus A320neo
Boeing 777-300ER
Flights
Approximately 80 flights over a 14-day period
Example routes:
KUL ↔ SIN (daily)
KUL ↔ BKK (daily)
KUL ↔ HKG (every 2 days)
KUL → NRT (every 3 days)
KUL → SYD (every 3 days)
Key Pages
Route	Description
/	Landing page
/Login	Login
/Register	Registration
/BookFlight	Search and book flights
/BookingConfirmation	Booking summary
/Admin	Dashboard
/Admin/Flights	Manage flights
/Admin/Bookings	Manage bookings
/Admin/Reports	Reports
Resetting the Database

To reset and reseed the database:

dotnet ef database drop
dotnet ef database update
dotnet run

The database will be recreated and seeded automatically on startup.
