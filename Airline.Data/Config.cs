using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseExchange;

namespace Airline.Data {

    /// <summary>
    /// Eine statische Klasse, welche der allgemeinen Konfiguration dient.
    /// </summary>
    public static class Config {

        #region Constants

        public const string DB_CONNECTION_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\Databases\airline_db.mdf';Integrated Security=True";

        public const string AIRLINE_NAME = "CyanAir";

        public const string AIRLINE_SLOGAN = "Pretty fly for a cyan guy!";

        #endregion

        #region Static Readonly Fields

        public static readonly JoinableDatabaseTable BookingSourceTable;
        public static readonly DatabaseTable BookingTargetTable;

        public static readonly JoinableDatabaseTable FlightSourceTable;

        public static readonly DatabaseTable SeatsTable;

        public static readonly DatabaseTable PassengerTargetTable;

        public static readonly JoinableDatabaseTable BaggageSourceTable;
        public static readonly DatabaseTable BaggageTargetTable;

        #endregion

        #region Static Constructor

        static Config() {

            FlightSourceTable = InitializeFlightSourceTable();
            BookingSourceTable = InitializeBookingSourceTable();
            SeatsTable = InitializeSeatsTable();

            BookingTargetTable = InitializeBookingTargetTable();

            PassengerTargetTable = InitializePassengerTargetTable();

            BaggageTargetTable = InitializeBaggageTargetTable();

            BaggageSourceTable = InitializeBaggageSourceTable();

        }

        private static JoinableDatabaseTable InitializeFlightSourceTable() {

            JoinableDatabaseTable flightSourceTable = new JoinableDatabaseTable("flights");

            flightSourceTable.AddAttribute("flights.Id");
            flightSourceTable.AddAttribute("depAirport.Country");
            flightSourceTable.AddAttribute("depAirport.City");
            flightSourceTable.AddAttribute("destAirport.Country");
            flightSourceTable.AddAttribute("destAirport.City");
            flightSourceTable.AddAttribute("TimeOfDeparture");
            flightSourceTable.AddAttribute("TimeOfArrival");
            flightSourceTable.AddAttribute("SeatRows");
            flightSourceTable.AddAttribute("SeatsPerRow");

            flightSourceTable.CreateJoin("airports", "DepartureAirportId", "Id", "depAirport");
            flightSourceTable.CreateJoin("airports", "DestinationAirportId", "Id", "destAirport");

            return flightSourceTable;

        }

        private static JoinableDatabaseTable InitializeBookingSourceTable() {

            JoinableDatabaseTable bookingSourceTable = new JoinableDatabaseTable("bookings");

            bookingSourceTable.AddAttribute("bookings.Id");
            bookingSourceTable.AddAttribute("bookings.PassengerId");
            bookingSourceTable.AddAttribute("passengers.Title");
            bookingSourceTable.AddAttribute("passengers.FirstName");
            bookingSourceTable.AddAttribute("passengers.LastName");
            bookingSourceTable.AddAttribute("bookings.FlightId");
            bookingSourceTable.AddAttribute("depAirport.Country");
            bookingSourceTable.AddAttribute("depAirport.City");
            bookingSourceTable.AddAttribute("destAirport.Country");
            bookingSourceTable.AddAttribute("destAirport.City");
            bookingSourceTable.AddAttribute("flights.TimeOfDeparture");
            bookingSourceTable.AddAttribute("flights.TimeOfArrival");
            bookingSourceTable.AddAttribute("flights.SeatRows");
            bookingSourceTable.AddAttribute("flights.SeatsPerRow");
            bookingSourceTable.AddAttribute("seats.PosX");
            bookingSourceTable.AddAttribute("seats.PosY");
            bookingSourceTable.AddAttribute("bookings.IsWaiting");

            Join j = bookingSourceTable.CreateJoin("seats", "bookings.PassengerId", "PassengerId", null, JoinTypes.Left);
            j.CustomCondition = "AND bookings.FlightId = seats.FlightId";

            bookingSourceTable.CreateJoin("passengers", "bookings.PassengerId", "Id");
            bookingSourceTable.CreateJoin("flights", "bookings.FlightId", "Id");
            bookingSourceTable.CreateJoin("airports", "flights.DepartureAirportId", "Id", "depAirport");
            bookingSourceTable.CreateJoin("airports", "flights.DestinationAirportId", "Id", "destAirport");

            return bookingSourceTable;

        }

        private static DatabaseTable InitializeSeatsTable() {

            DatabaseTable seatsTable = new DatabaseTable("seats");
            seatsTable.AddAttribute("PosX");
            seatsTable.AddAttribute("PosY");
            seatsTable.AddAttribute("Id");
            seatsTable.AddAttribute("FlightId");
            seatsTable.AddAttribute("PassengerId");

            return seatsTable;

        }

        private static DatabaseTable InitializeBookingTargetTable() {

            DatabaseTable bookingTargetTable = new DatabaseTable("bookings");
            bookingTargetTable.AddAttribute("Id");
            bookingTargetTable.AddAttribute("PassengerId");
            bookingTargetTable.AddAttribute("FlightId");
            bookingTargetTable.AddAttribute("IsWaiting");

            return bookingTargetTable;

        }

        private static DatabaseTable InitializePassengerTargetTable() {

            DatabaseTable passengerTargetTable = new DatabaseTable("passengers");
            passengerTargetTable.AddAttribute("Id");
            passengerTargetTable.AddAttribute("Title");
            passengerTargetTable.AddAttribute("FirstName");
            passengerTargetTable.AddAttribute("LastName");
            passengerTargetTable.AddAttribute("PassportNumber");

            return passengerTargetTable;

        }

        private static DatabaseTable InitializeBaggageTargetTable() {

            DatabaseTable baggageTargetTable = new DatabaseTable("baggage");
            baggageTargetTable.AddAttribute("Id");
            baggageTargetTable.AddAttribute("FlightId");
            baggageTargetTable.AddAttribute("PassengerId");
            baggageTargetTable.AddAttribute("Weight");
            baggageTargetTable.AddAttribute("Fee");

            return baggageTargetTable;

        }

        private static JoinableDatabaseTable InitializeBaggageSourceTable() {

            JoinableDatabaseTable baggageSourceTable = new JoinableDatabaseTable("baggage");
            baggageSourceTable.AddAttribute("baggage.Id");
            baggageSourceTable.AddAttribute("baggage.FlightId");
            baggageSourceTable.AddAttribute("baggage.Weight");
            baggageSourceTable.AddAttribute("baggage.Fee");
            baggageSourceTable.AddAttribute("passengers.Title");
            baggageSourceTable.AddAttribute("passengers.FirstName");
            baggageSourceTable.AddAttribute("passengers.LastName");

            baggageSourceTable.CreateJoin("passengers", "baggage.PassengerId", "passengers.Id");

            return baggageSourceTable;

        }

        #endregion



    }

}
