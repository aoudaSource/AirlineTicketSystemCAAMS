using CAAMSAirlineWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CAAMSAirlineWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightStaff> FlightStaffs { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<FlightLeg> FlightLegs { get; set; }
        public DbSet<FlightStatus> FlightStatuses { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Baggage> Baggages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");

                entity.HasKey(e => e.Username);

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasColumnName("Password_hash")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.FirstName)
                    .HasColumnName("First_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasColumnName("Last_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.DOB)
                    .HasColumnName("DOB")
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("Created_at")
                    .HasColumnType("datetime")
                    .IsRequired();
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                entity.HasKey(e => e.CustomerId);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("Customer_id");

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.Property(e => e.FirstName)
                    .HasColumnName("First_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasColumnName("Last_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.Phone)
                    .HasMaxLength(20);

                entity.Property(e => e.DOB)
                    .HasColumnName("DOB")
                    .HasColumnType("date")
                    .IsRequired();

                entity.HasOne(e => e.AppUser)
                    .WithOne(u => u.Customer)
                    .HasForeignKey<Customer>(e => e.Username)
                    .HasPrincipalKey<AppUser>(u => u.Username)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Customers_AppUsers");

            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Staffs");

                entity.HasKey(e => e.StaffId);

                entity.Property(e => e.StaffId)
                    .HasColumnName("Staff_id");

                entity.Property(e => e.FirstName)
                    .HasColumnName("First_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasColumnName("Last_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.HasOne(e => e.AppUser)
                    .WithOne(u => u.Staff)
                    .HasForeignKey<Staff>(e => e.Username)
                    .HasPrincipalKey<AppUser>(u => u.Username)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Staff_User");
            });

            modelBuilder.Entity<Aircraft>(entity =>
            {
                entity.ToTable("Aircrafts");

                entity.HasKey(e => e.AircraftId);

                entity.Property(e => e.AircraftId)
                    .HasColumnName("Aircraft_id");

                entity.Property(e => e.Model)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Manufacturer)
                    .HasMaxLength(50);

                entity.Property(e => e.Capacity)
                    .IsRequired();
            });

            modelBuilder.Entity<Flight>(entity =>
            {
                entity.ToTable("Flights", tb => tb.UseSqlOutputClause(false));

                entity.HasKey(e => e.FlightId);

                entity.Property(e => e.FlightId)
                    .HasColumnName("Flight_id");

                entity.Property(e => e.FlightNumber)
                    .HasColumnName("Flight_number")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(e => e.AircraftId)
                    .HasColumnName("Aircraft_id")
                    .IsRequired();

                entity.Property(e => e.BasePrice)
                    .HasColumnName("Base_price")
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.HasOne(e => e.Aircraft)
                    .WithMany(a => a.Flights)
                    .HasForeignKey(e => e.AircraftId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Flight_Aircraft");
            });

            modelBuilder.Entity<FlightStaff>(entity =>
            {
                entity.ToTable("FlightStaffs");

                entity.HasKey(e => e.FlightStaffId);

                entity.Property(e => e.FlightStaffId)
                    .HasColumnName("Flight_staff_id");

                entity.Property(e => e.FlightId)
                    .HasColumnName("Flight_id")
                    .IsRequired();

                entity.Property(e => e.StaffId)
                    .HasColumnName("Staff_id")
                    .IsRequired();

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.FlightStaffs)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_FlightStaff_Flight");

                entity.HasOne(e => e.Staff)
                    .WithMany(s => s.FlightStaffs)
                    .HasForeignKey(e => e.StaffId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_FlightStaff_Staff");
            });

            modelBuilder.Entity<Airport>(entity =>
            {
                entity.ToTable("Airports");

                entity.HasKey(e => e.AirportCode);

                entity.Property(e => e.AirportCode)
                    .HasColumnName("Airport_code")
                    .HasColumnType("char(3)")
                    .IsRequired();

                entity.Property(e => e.AirportName)
                    .HasColumnName("Airport_name")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.State)
                    .HasMaxLength(50);

                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            modelBuilder.Entity<FlightLeg>(entity =>
            {
                entity.ToTable("FlightLegs");

                entity.HasKey(e => e.LegId);

                entity.Property(e => e.LegId)
                    .HasColumnName("Leg_id");

                entity.Property(e => e.FlightId)
                    .HasColumnName("Flight_id")
                    .IsRequired();

                entity.Property(e => e.LegNumber)
                    .HasColumnName("Leg_number")
                    .IsRequired();

                entity.Property(e => e.DepartureAirport)
                    .HasColumnName("Departure_airport")
                    .HasColumnType("char(3)")
                    .IsRequired();

                entity.Property(e => e.ArrivalAirport)
                    .HasColumnName("Arrival_airport")
                    .HasColumnType("char(3)")
                    .IsRequired();

                entity.Property(e => e.DepartureTime)
                    .HasColumnName("Departure_time")
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.Property(e => e.ArrivalTime)
                    .HasColumnName("Arrival_time")
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.HasIndex(e => new { e.FlightId, e.LegNumber })
                    .IsUnique()
                    .HasDatabaseName("UQ_FlightLeg");

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.FlightLegs)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_FlightLeg_Flight");

                entity.HasOne(e => e.DepartureAirportNavigation)
                    .WithMany(a => a.DepartureFlightLegs)
                    .HasForeignKey(e => e.DepartureAirport)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_FlightLeg_DepartureAirport");

                entity.HasOne(e => e.ArrivalAirportNavigation)
                    .WithMany(a => a.ArrivalFlightLegs)
                    .HasForeignKey(e => e.ArrivalAirport)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_FlightLeg_ArrivalAirport");
            });

            modelBuilder.Entity<FlightStatus>(entity =>
            {
                entity.ToTable("FlightStatuses");

                entity.HasKey(e => e.StatusId);

                entity.Property(e => e.StatusId)
                    .HasColumnName("Status_id");

                entity.Property(e => e.FlightId)
                    .HasColumnName("Flight_id")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("Updated_at")
                    .HasColumnType("datetime");

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.FlightStatuses)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_FlightStatus_Flight");
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");

                entity.HasKey(e => e.BookingId);

                entity.Property(e => e.BookingId)
                    .HasColumnName("Booking_id");

                entity.Property(e => e.CustomerId)
                    .HasColumnName("Customer_id")
                    .IsRequired();

                entity.Property(e => e.BookingDate)
                    .HasColumnName("Booking_date")
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.Property(e => e.TotalPrice)
                    .HasColumnName("Total_price")
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Active")
                    .IsRequired();

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Bookings)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Booking_Customer");
            });

            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.ToTable("Passenger");

                entity.HasKey(e => e.PassengerId);

                entity.Property(e => e.PassengerId)
                    .HasColumnName("PassengerId");

                entity.Property(e => e.BookingId)
                    .IsRequired();

                entity.Property(e => e.FirstName)
                    .HasColumnName("First_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasColumnName("Last_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.PassportNumber)
                    .HasColumnName("Passport_number")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.DOB)
                    .HasColumnType("date")
                    .IsRequired();

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.Passengers)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Passenger_Booking");
            });

            modelBuilder.Entity<Seat>(entity =>
            {
                entity.ToTable("Seats");

                entity.HasKey(e => e.SeatId);

                entity.Property(e => e.SeatId)
                    .HasColumnName("Seat_id");

                entity.Property(e => e.AircraftId)
                    .HasColumnName("Aircraft_id")
                    .IsRequired();

                entity.Property(e => e.SeatNumber)
                    .HasColumnName("Seat_number")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(e => e.SeatClass)
                    .HasColumnName("Seat_class")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(e => new { e.AircraftId, e.SeatNumber })
                    .IsUnique()
                    .HasDatabaseName("UQ_Seat");

                entity.HasOne(e => e.Aircraft)
                    .WithMany(a => a.Seats)
                    .HasForeignKey(e => e.AircraftId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Seat_Aircraft");
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Tickets", tb => tb.UseSqlOutputClause(false));

                entity.HasKey(e => e.TicketId);

                entity.Property(e => e.TicketId)
                    .HasColumnName("Ticket_id");

                entity.Property(e => e.PassengerId)
                    .HasColumnName("Passenger_id")
                    .IsRequired();

                entity.Property(e => e.BookingId)
                    .HasColumnName("Booking_id")
                    .IsRequired();

                entity.Property(e => e.LegId)
                    .HasColumnName("Leg_id")
                    .IsRequired();

                entity.Property(e => e.SeatId)
                    .HasColumnName("Seat_id");

                entity.Property(e => e.TicketClass)
                    .HasColumnName("Ticket_class")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.HasIndex(e => new { e.BookingId, e.PassengerId, e.LegId })
                    .IsUnique()
                    .HasDatabaseName("UQ_Ticket");

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.Tickets)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Ticket_Booking");

                entity.HasOne(e => e.Passenger)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(e => e.PassengerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Ticket_Passenger");

                entity.HasOne(e => e.FlightLeg)
                    .WithMany(fl => fl.Tickets)
                    .HasForeignKey(e => e.LegId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Ticket_FlightLeg");

                entity.HasOne(e => e.Seat)
                    .WithMany(s => s.Tickets)
                    .HasForeignKey(e => e.SeatId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Ticket_Seat");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");

                entity.HasKey(e => e.PaymentId);

                entity.Property(e => e.PaymentId)
                    .HasColumnName("Payment_id");

                entity.Property(e => e.BookingId)
                    .HasColumnName("Booking_id")
                    .IsRequired();

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.PaymentMethod)
                    .HasColumnName("Payment_method")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.PaymentDate)
                    .HasColumnName("Payment_date")
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.Payments)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Payment_Booking");
            });

            modelBuilder.Entity<Baggage>(entity =>
            {
                entity.ToTable("Baggages");

                entity.HasKey(e => e.BaggageId);

                entity.Property(e => e.BaggageId)
                    .HasColumnName("Baggage_id");

                entity.Property(e => e.TicketId)
                    .HasColumnName("Ticket_id")
                    .IsRequired();

                entity.Property(e => e.Weight)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasOne(e => e.Ticket)
                    .WithMany(t => t.Baggages)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Baggage_Ticket");
            });
        }
    }
}