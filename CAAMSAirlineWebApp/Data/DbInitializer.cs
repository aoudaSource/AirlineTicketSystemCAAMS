using CAAMSAirlineWebApp.Models;

namespace CAAMSAirlineWebApp.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Skip if already seeded
            if (context.AppUsers.Any(u => u.Username == "staff"))
                return;

            // ── Clear all existing data (children before parents) ──────────────
            context.Baggages.RemoveRange(context.Baggages);
            context.Tickets.RemoveRange(context.Tickets);
            context.Payments.RemoveRange(context.Payments);
            context.Passengers.RemoveRange(context.Passengers);
            context.Bookings.RemoveRange(context.Bookings);
            context.FlightStatuses.RemoveRange(context.FlightStatuses);
            context.FlightStaffs.RemoveRange(context.FlightStaffs);
            context.FlightLegs.RemoveRange(context.FlightLegs);
            context.Flights.RemoveRange(context.Flights);
            context.Seats.RemoveRange(context.Seats);
            context.Aircrafts.RemoveRange(context.Aircrafts);
            context.Airports.RemoveRange(context.Airports);
            context.Customers.RemoveRange(context.Customers);
            context.Staffs.RemoveRange(context.Staffs);
            context.AppUsers.RemoveRange(context.AppUsers);
            context.SaveChanges();

            // ── AppUsers ──────────────────────────────────────────────────────
            var adminUser = new AppUser
            {
                Username = "admin",
                PasswordHash = "Admin@123",
                Role = "Admin",
                FirstName = "Ahmad",
                LastName = "Razali",
                DOB = new DateTime(1985, 6, 15),
                CreatedAt = DateTime.UtcNow
            };

            var staffUser = new AppUser
            {
                Username = "staff",
                PasswordHash = "Staff@123",
                Role = "Staff",
                FirstName = "Sara",
                LastName = "Hassan",
                DOB = new DateTime(1992, 8, 10),
                CreatedAt = DateTime.UtcNow
            };

            var customerUser = new AppUser
            {
                Username = "customer",
                PasswordHash = "Customer@123",
                Role = "Customer",
                FirstName = "John",
                LastName = "Doe",
                DOB = new DateTime(1990, 3, 22),
                CreatedAt = DateTime.UtcNow
            };

            context.AppUsers.AddRange(adminUser, staffUser, customerUser);
            context.SaveChanges();

            // ── Staff & Customer profiles ─────────────────────────────────────
            context.Staffs.Add(new Staff
            {
                FirstName = "Ahmad",
                LastName = "Razali",
                Role = "Admin",
                Username = "admin"
            });

            context.Staffs.Add(new Staff
            {
                FirstName = "Sara",
                LastName = "Hassan",
                Role = "Staff",
                Username = "staff"
            });

            context.Customers.Add(new Customer
            {
                Username = "customer",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "0123456789",
                DOB = new DateTime(1990, 3, 22)
            });

            context.SaveChanges();

            // ── Airports ──────────────────────────────────────────────────────
            context.Airports.AddRange(
                new Airport { AirportCode = "KUL", AirportName = "Kuala Lumpur International Airport", City = "Kuala Lumpur", Country = "Malaysia" },
                new Airport { AirportCode = "SIN", AirportName = "Singapore Changi Airport", City = "Singapore", Country = "Singapore" },
                new Airport { AirportCode = "BKK", AirportName = "Suvarnabhumi Airport", City = "Bangkok", Country = "Thailand" },
                new Airport { AirportCode = "HKG", AirportName = "Hong Kong International Airport", City = "Hong Kong", Country = "China" },
                new Airport { AirportCode = "NRT", AirportName = "Narita International Airport", City = "Tokyo", Country = "Japan" },
                new Airport { AirportCode = "SYD", AirportName = "Sydney Kingsford Smith Airport", City = "Sydney", Country = "Australia" }
            );
            context.SaveChanges();

            // ── Aircrafts ─────────────────────────────────────────────────────
            var b737 = new Aircraft { Model = "Boeing 737-800", Manufacturer = "Boeing", Capacity = 162 };
            var a320 = new Aircraft { Model = "Airbus A320neo", Manufacturer = "Airbus", Capacity = 165 };
            var b777 = new Aircraft { Model = "Boeing 777-300ER", Manufacturer = "Boeing", Capacity = 396 };

            context.Aircrafts.AddRange(b737, a320, b777);
            context.SaveChanges();

            // ── Seats ─────────────────────────────────────────────────────────
            context.Seats.AddRange(GenerateSeats(b737.AircraftId, firstRows: 1, businessRows: 3, economyRows: 8));
            context.Seats.AddRange(GenerateSeats(a320.AircraftId, firstRows: 1, businessRows: 3, economyRows: 8));
            context.Seats.AddRange(GenerateSeats(b777.AircraftId, firstRows: 2, businessRows: 6, economyRows: 14));
            context.SaveChanges();

            // ── Flights & FlightLegs ──────────────────────────────────────────
            var today = DateTime.Today;
            var flights = new List<Flight>();

            for (int day = 1; day <= 14; day++)
            {
                var d = today.AddDays(day);

                // MH370: KUL → SIN  (1h 30m, morning)
                flights.Add(MakeFlight("MH370", b737.AircraftId, 299m, "KUL", "SIN",
                    d.AddHours(8), d.AddHours(9).AddMinutes(30)));

                // MH371: SIN → KUL  (1h 30m, afternoon)
                flights.Add(MakeFlight("MH371", b737.AircraftId, 299m, "SIN", "KUL",
                    d.AddHours(14), d.AddHours(15).AddMinutes(30)));

                // MH606: KUL → BKK  (2h)
                flights.Add(MakeFlight("MH606", a320.AircraftId, 449m, "KUL", "BKK",
                    d.AddHours(9), d.AddHours(11)));

                // MH607: BKK → KUL  (2h)
                flights.Add(MakeFlight("MH607", a320.AircraftId, 449m, "BKK", "KUL",
                    d.AddHours(13), d.AddHours(15)));

                // MH70: KUL → HKG  (3h 30m) — every other day
                if (day % 2 == 0)
                {
                    flights.Add(MakeFlight("MH70", b777.AircraftId, 699m, "KUL", "HKG",
                        d.AddHours(10), d.AddHours(13).AddMinutes(30)));

                    // MH71: HKG → KUL  (3h 30m)
                    flights.Add(MakeFlight("MH71", b777.AircraftId, 699m, "HKG", "KUL",
                        d.AddHours(16), d.AddHours(19).AddMinutes(30)));
                }

                // MH88: KUL → NRT  (7h) — every 3 days
                if (day % 3 == 0)
                {
                    flights.Add(MakeFlight("MH88", b777.AircraftId, 1299m, "KUL", "NRT",
                        d.AddHours(1), d.AddHours(8)));
                }

                // MH122: KUL → SYD  (8h) — every 3 days (offset)
                if (day % 3 == 1)
                {
                    flights.Add(MakeFlight("MH122", b777.AircraftId, 1499m, "KUL", "SYD",
                        d.AddHours(23).AddMinutes(55), d.AddDays(1).AddHours(8)));
                }

                // MH200: KUL → SIN → SYD (2 legs with stop) — every 3 days
                if (day % 3 == 2)
                {
                    flights.Add(new Flight
                    {
                        FlightNumber = "MH200",
                        AircraftId = b777.AircraftId,
                        BasePrice = 1299m,
                        FlightLegs = new List<FlightLeg>
                        {
                            new FlightLeg { LegNumber = 1, DepartureAirport = "KUL", ArrivalAirport = "SIN", DepartureTime = d.AddHours(6), ArrivalTime = d.AddHours(7).AddMinutes(30) },
                            new FlightLeg { LegNumber = 2, DepartureAirport = "SIN", ArrivalAirport = "SYD", DepartureTime = d.AddHours(9), ArrivalTime = d.AddHours(17) }
                        }
                    });
                }
            }

            context.Flights.AddRange(flights);
            context.SaveChanges();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Flight MakeFlight(
            string flightNumber, int aircraftId, decimal basePrice,
            string depAirport, string arrAirport,
            DateTime depTime, DateTime arrTime)
        {
            return new Flight
            {
                FlightNumber = flightNumber,
                AircraftId = aircraftId,
                BasePrice = basePrice,
                FlightLegs = new List<FlightLeg>
                {
                    new FlightLeg
                    {
                        LegNumber = 1,
                        DepartureAirport = depAirport,
                        ArrivalAirport = arrAirport,
                        DepartureTime = depTime,
                        ArrivalTime = arrTime
                    }
                }
            };
        }

        private static List<Seat> GenerateSeats(int aircraftId, int firstRows, int businessRows, int economyRows)
        {
            var seats = new List<Seat>();
            char[] cols = { 'A', 'B', 'C', 'D' };
            int row = 1;

            for (int r = 0; r < firstRows; r++, row++)
                foreach (var col in cols)
                    seats.Add(new Seat { AircraftId = aircraftId, SeatNumber = $"{row}{col}", SeatClass = "First" });

            for (int r = 0; r < businessRows; r++, row++)
                foreach (var col in cols)
                    seats.Add(new Seat { AircraftId = aircraftId, SeatNumber = $"{row}{col}", SeatClass = "Business" });

            for (int r = 0; r < economyRows; r++, row++)
                foreach (var col in cols)
                    seats.Add(new Seat { AircraftId = aircraftId, SeatNumber = $"{row}{col}", SeatClass = "Economy" });

            return seats;
        }
    }
}
