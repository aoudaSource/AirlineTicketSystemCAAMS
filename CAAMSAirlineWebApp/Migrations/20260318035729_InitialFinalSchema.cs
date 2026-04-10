using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAAMSAirlineWebApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aircrafts",
                columns: table => new
                {
                    Aircraft_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aircrafts", x => x.Aircraft_id);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Airport_code = table.Column<string>(type: "char(3)", nullable: false),
                    Airport_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Airport_code);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    First_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DOB = table.Column<DateTime>(type: "date", nullable: false),
                    Created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Flight_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Flight_number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Aircraft_id = table.Column<int>(type: "int", nullable: false),
                    Base_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Flight_id);
                    table.ForeignKey(
                        name: "FK_Flight_Aircraft",
                        column: x => x.Aircraft_id,
                        principalTable: "Aircrafts",
                        principalColumn: "Aircraft_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Seat_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Aircraft_id = table.Column<int>(type: "int", nullable: false),
                    Seat_number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Seat_class = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Seat_id);
                    table.ForeignKey(
                        name: "FK_Seat_Aircraft",
                        column: x => x.Aircraft_id,
                        principalTable: "Aircrafts",
                        principalColumn: "Aircraft_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Customer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    First_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DOB = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Customer_id);
                    table.ForeignKey(
                        name: "FK_Customers_AppUsers",
                        column: x => x.Username,
                        principalTable: "AppUsers",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    Staff_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    First_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.Staff_id);
                    table.ForeignKey(
                        name: "FK_Staff_User",
                        column: x => x.Username,
                        principalTable: "AppUsers",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlightLegs",
                columns: table => new
                {
                    Leg_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Flight_id = table.Column<int>(type: "int", nullable: false),
                    Leg_number = table.Column<int>(type: "int", nullable: false),
                    Departure_airport = table.Column<string>(type: "char(3)", nullable: false),
                    Arrival_airport = table.Column<string>(type: "char(3)", nullable: false),
                    Departure_time = table.Column<DateTime>(type: "datetime", nullable: false),
                    Arrival_time = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightLegs", x => x.Leg_id);
                    table.ForeignKey(
                        name: "FK_FlightLeg_ArrivalAirport",
                        column: x => x.Arrival_airport,
                        principalTable: "Airports",
                        principalColumn: "Airport_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightLeg_DepartureAirport",
                        column: x => x.Departure_airport,
                        principalTable: "Airports",
                        principalColumn: "Airport_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightLeg_Flight",
                        column: x => x.Flight_id,
                        principalTable: "Flights",
                        principalColumn: "Flight_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlightStatuses",
                columns: table => new
                {
                    Status_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Flight_id = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightStatuses", x => x.Status_id);
                    table.ForeignKey(
                        name: "FK_FlightStatus_Flight",
                        column: x => x.Flight_id,
                        principalTable: "Flights",
                        principalColumn: "Flight_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Booking_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Customer_id = table.Column<int>(type: "int", nullable: false),
                    Booking_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Total_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Booking_id);
                    table.ForeignKey(
                        name: "FK_Booking_Customer",
                        column: x => x.Customer_id,
                        principalTable: "Customers",
                        principalColumn: "Customer_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlightStaffs",
                columns: table => new
                {
                    Flight_staff_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Flight_id = table.Column<int>(type: "int", nullable: false),
                    Staff_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightStaffs", x => x.Flight_staff_id);
                    table.ForeignKey(
                        name: "FK_FlightStaff_Flight",
                        column: x => x.Flight_id,
                        principalTable: "Flights",
                        principalColumn: "Flight_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightStaff_Staff",
                        column: x => x.Staff_id,
                        principalTable: "Staffs",
                        principalColumn: "Staff_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Passenger",
                columns: table => new
                {
                    PassengerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    First_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Passport_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DOB = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passenger", x => x.PassengerId);
                    table.ForeignKey(
                        name: "FK_Passenger_Booking",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Booking_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Booking_id = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Payment_date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Payment_id);
                    table.ForeignKey(
                        name: "FK_Payment_Booking",
                        column: x => x.Booking_id,
                        principalTable: "Bookings",
                        principalColumn: "Booking_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Ticket_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Passenger_id = table.Column<int>(type: "int", nullable: false),
                    Booking_id = table.Column<int>(type: "int", nullable: false),
                    Leg_id = table.Column<int>(type: "int", nullable: false),
                    Seat_id = table.Column<int>(type: "int", nullable: true),
                    Ticket_class = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Ticket_id);
                    table.ForeignKey(
                        name: "FK_Ticket_Booking",
                        column: x => x.Booking_id,
                        principalTable: "Bookings",
                        principalColumn: "Booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ticket_FlightLeg",
                        column: x => x.Leg_id,
                        principalTable: "FlightLegs",
                        principalColumn: "Leg_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Passenger",
                        column: x => x.Passenger_id,
                        principalTable: "Passenger",
                        principalColumn: "PassengerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Seat",
                        column: x => x.Seat_id,
                        principalTable: "Seats",
                        principalColumn: "Seat_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Baggages",
                columns: table => new
                {
                    Baggage_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticket_id = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baggages", x => x.Baggage_id);
                    table.ForeignKey(
                        name: "FK_Baggage_Ticket",
                        column: x => x.Ticket_id,
                        principalTable: "Tickets",
                        principalColumn: "Ticket_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Baggages_Ticket_id",
                table: "Baggages",
                column: "Ticket_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Customer_id",
                table: "Bookings",
                column: "Customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Username",
                table: "Customers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlightLegs_Arrival_airport",
                table: "FlightLegs",
                column: "Arrival_airport");

            migrationBuilder.CreateIndex(
                name: "IX_FlightLegs_Departure_airport",
                table: "FlightLegs",
                column: "Departure_airport");

            migrationBuilder.CreateIndex(
                name: "UQ_FlightLeg",
                table: "FlightLegs",
                columns: new[] { "Flight_id", "Leg_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Aircraft_id",
                table: "Flights",
                column: "Aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_FlightStaffs_Flight_id",
                table: "FlightStaffs",
                column: "Flight_id");

            migrationBuilder.CreateIndex(
                name: "IX_FlightStaffs_Staff_id",
                table: "FlightStaffs",
                column: "Staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_FlightStatuses_Flight_id",
                table: "FlightStatuses",
                column: "Flight_id");

            migrationBuilder.CreateIndex(
                name: "IX_Passenger_BookingId",
                table: "Passenger",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Booking_id",
                table: "Payments",
                column: "Booking_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Seat",
                table: "Seats",
                columns: new[] { "Aircraft_id", "Seat_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Username",
                table: "Staffs",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Leg_id",
                table: "Tickets",
                column: "Leg_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Passenger_id",
                table: "Tickets",
                column: "Passenger_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Seat_id",
                table: "Tickets",
                column: "Seat_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Ticket",
                table: "Tickets",
                columns: new[] { "Booking_id", "Passenger_id", "Leg_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Baggages");

            migrationBuilder.DropTable(
                name: "FlightStaffs");

            migrationBuilder.DropTable(
                name: "FlightStatuses");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "FlightLegs");

            migrationBuilder.DropTable(
                name: "Passenger");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Aircrafts");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
