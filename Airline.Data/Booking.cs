using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    /// <summary>
    /// Beschreibt eine Flug-Buchung.
    /// </summary>
    public class Booking {

        #region Fields

        #endregion

        #region Properties

        public int Id { get; set; }

        public Flight Flight { get; set; }

        public Passenger Passenger { get; set; }

        public bool IsWaiting { get; set; }

        public string StatusText => IsWaiting ? "Wartet" : "Fest";

        public SeatNumber? SeatNumber { get; set; }

        #endregion

        #region Constructors

        public Booking() { }

        public Booking(int id, Flight flight, Passenger passenger) {

            Id = id;
            Flight = flight;
            Passenger = passenger;

        }

        #endregion

        #region Methods

        public static Booking CreateFromDataRecord(IReadOnlyDictionary<string, object> attributes) {

            Airport departureAp = new Airport(attributes["depAirport.Country"].ToString(), attributes["depAirport.City"].ToString());
            Airport destinationAp = new Airport(attributes["destAirport.Country"].ToString(), attributes["destAirport.City"].ToString());

            int flightId = (int)attributes["bookings.FlightId"];
            DateTime timeOfDeparture = (DateTime)attributes["flights.TimeOfDeparture"];
            DateTime timeOfArrival = (DateTime)attributes["flights.TimeOfArrival"];
            int seatRows = (int)attributes["flights.SeatRows"];
            int seatsPerRow = (int)attributes["flights.SeatsPerRow"];

            Flight flight = new Flight(flightId, timeOfDeparture, timeOfArrival, departureAp, destinationAp, seatRows, seatsPerRow);

            int passengerId = (int)attributes["bookings.PassengerId"];

            if (!Enum.TryParse<Title>(attributes["passengers.Title"].ToString(), true, out Title title))
                title = Title.Mr;

            string firstName = attributes["passengers.FirstName"].ToString();
            string lastName = attributes["passengers.LastName"].ToString();

            Passenger passenger = new Passenger(title, firstName, lastName);

            int bookingId = (int)attributes["bookings.Id"];
            bool isWaiting = (int)attributes["bookings.IsWaiting"] != 0;

            Booking booking = new Booking(bookingId, flight, passenger) {
                IsWaiting = isWaiting
            };

            if (!isWaiting)
                booking.SeatNumber = new SeatNumber((int)attributes["seats.PosX"], (int)attributes["seats.PosY"]);
            else
                booking.SeatNumber = null;

            return booking;

        }

        #endregion

    }

}
