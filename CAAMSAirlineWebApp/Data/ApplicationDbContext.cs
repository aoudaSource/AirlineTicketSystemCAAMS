using CAAMSAirlineWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        // Renamed from `Users` to `AppUsers` to avoid hiding Identity's `Users` DbSet (fixes CS0114).
        public DbSet<Users> AppUsers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<FlightStaff> FlightStaffs { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<FlightLeg> FlightLegs { get; set; }
        public DbSet<FlightStatus> FlightStatuses { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Baggage> Baggages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Flight)
                .WithMany(f => f.Seats) // add ICollection<seat> Seats to flight class so that EF sees Seat as a child of Flight
                .HasForeignKey(s => s.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FlightLeg>()
                .HasOne(f => f.DepartureAirport)
                .WithMany()
                .HasForeignKey(f => f.DepartureAirportCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FlightLeg>()
                .HasOne(f => f.ArrivalAirport)
                .WithMany()
                .HasForeignKey(f => f.ArrivalAirportCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.PassportNumber)
                .IsUnique();


            //seed data?
            // First, seed Aircrafts
            modelBuilder.Entity<Aircraft>().HasData(
                new Aircraft { AircraftId = 1, Model = "Boeing 737", Manufacturer = "Boeing", Capacity = 160 },
                new Aircraft { AircraftId = 2, Model = "Airbus A320", Manufacturer = "Airbus", Capacity = 150 }
            );

            // ----- Seed Flights -----
            modelBuilder.Entity<Flight>().HasData(
                new Flight { FlightId = 1, FlightNumber = "AA100", AircraftId = 1 },
                new Flight { FlightId = 2, FlightNumber = "DL200", AircraftId = 2 }
            );

            // ----- Seed Seats -----
            modelBuilder.Entity<Seat>().HasData(
                new Seat { SeatId = 1, AircraftId = 1, FlightId = 1, SeatNumber = "1A", SeatClass = "Economy", IsBooked = false, Aircraft = null!, Flight = null! },
                new Seat { SeatId = 2, AircraftId = 1, FlightId = 1, SeatNumber = "1B", SeatClass = "Economy", IsBooked = false, Aircraft = null!, Flight = null! },
                new Seat { SeatId = 3, AircraftId = 2, FlightId = 2, SeatNumber = "1A", SeatClass = "Economy", IsBooked = false, Aircraft = null!, Flight = null! },
                new Seat { SeatId = 4, AircraftId = 2, FlightId = 2, SeatNumber = "1B", SeatClass = "Economy", IsBooked = false, Aircraft = null!, Flight = null! }
            );

            // ----- Seed Customers -----
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Phone = "1234567890", PassportNumber = "P10001", DOB = DateTime.Parse("1985-05-01") },
                new Customer { CustomerId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Phone = "1234567891", PassportNumber = "P10002", DOB = DateTime.Parse("1990-07-12") }
            );


            base.OnModelCreating(modelBuilder);
        }
    }
}
