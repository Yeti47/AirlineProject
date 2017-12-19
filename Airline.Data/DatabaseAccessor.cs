using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseExchange;
using System.Data.SqlClient;

namespace Airline.Data {

    public class DatabaseAccessor {

        #region Methods

        public IEnumerable<SeatNumber> FetchTakenSeats(int flightId) {

            DatabaseTable seatsTable = new DatabaseTable("seats");
            seatsTable.AddAttribute("PosX");
            seatsTable.AddAttribute("PosY");
            seatsTable.AddAttribute("Id");
            seatsTable.AddAttribute("FlightId");
            seatsTable.AddAttribute("PassengerId");

            ObjectRelationalMapper<SeatNumber> seatNumberMapper = new ObjectRelationalMapper<SeatNumber>(Config.DB_CONNECTION_STRING, seatsTable);

            string whereClause = "FlightId = " + flightId;
            FetchResult<SeatNumber> fetchResultSeatNumbers = seatNumberMapper.Fetch(attr => new SeatNumber((int)attr["PosX"], (int)attr["PosY"]), whereClause);

            if (fetchResultSeatNumbers.HasError) {

                return new SeatNumber[0];

            }

            return fetchResultSeatNumbers.RetrievedItems;

        }

        public FetchResult<Flight> FetchFlights(string whereClause = null, SqlParameter[] sqlParams = null) {

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

            ObjectRelationalMapper<Flight> flightsMapper = new ObjectRelationalMapper<Flight>(Config.DB_CONNECTION_STRING, flightSourceTable);

            FetchResult<Flight> flightsFetched = flightsMapper.Fetch(attr => {

                Airport departureAp = new Airport(attr["depAirport.Country"].ToString(), attr["depAirport.City"].ToString());
                Airport destinationAp = new Airport(attr["destAirport.Country"].ToString(), attr["destAirport.City"].ToString());

                return
                    new Flight((int)attr["flights.Id"],
                    (DateTime)attr["TimeOfDeparture"],
                    (DateTime)attr["TimeOfArrival"],
                    departureAp, destinationAp,
                    (int)attr["SeatRows"],
                    (int)attr["SeatsPerRow"]);

            }, whereClause, sqlParams);

            if (!flightsFetched.HasError) {

                foreach (Flight flight in flightsFetched.RetrievedItems)
                    flight.TakenSeatNumbers = FetchTakenSeats(flight.Id);

            }

            return flightsFetched;

        }

        public FetchResult<Booking> FetchBookings(string whereClause = null, SqlParameter[] sqlParams = null) {

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

            ObjectRelationalMapper<Booking> bookingMapper = new ObjectRelationalMapper<Booking>(Config.DB_CONNECTION_STRING, bookingSourceTable);

            FetchResult<Booking> fetchResult = bookingMapper.Fetch(CreateBookingInstance, whereClause, sqlParams);

            return fetchResult;

        }

        public ActionQueryResult WritePassengerToDatabase(Passenger passenger, out int outputId) {

            outputId = -1;

            DatabaseTable passengerTargetTable = new DatabaseTable("passengers");
            passengerTargetTable.AddAttribute("Id");
            passengerTargetTable.AddAttribute("Title");
            passengerTargetTable.AddAttribute("FirstName");
            passengerTargetTable.AddAttribute("LastName");
            passengerTargetTable.AddAttribute("PassportNumber");

            ObjectRelationalMapper<Passenger> passengerMapper = new ObjectRelationalMapper<Passenger>(Config.DB_CONNECTION_STRING, null, passengerTargetTable);

            List<SqlParameter> passengerSqlParams = new List<SqlParameter>();
            passengerSqlParams.Add(new SqlParameter("@Title", passenger.TitleString));
            passengerSqlParams.Add(new SqlParameter("@FirstName", passenger.FirstName));
            passengerSqlParams.Add(new SqlParameter("@LastName", passenger.LastName));

            if (passenger.PassportId != null)
                passengerSqlParams.Add(new SqlParameter("@PassportNumber", passenger.PassportId));

            ActionQueryResult passengerInsertResult = passengerMapper.Insert(passengerSqlParams.ToArray(), "Id", out outputId);

            return passengerInsertResult;

        }

        public ActionQueryResult WriteSeatToDatabase(SeatNumber seat, int passengerId, int flightId) {

            DatabaseTable seatsTable = new DatabaseTable("seats");
            seatsTable.AddAttribute("PosX");
            seatsTable.AddAttribute("PosY");
            seatsTable.AddAttribute("Id");
            seatsTable.AddAttribute("FlightId");
            seatsTable.AddAttribute("PassengerId");

            ObjectRelationalMapper<Passenger> seatMapper = new ObjectRelationalMapper<Passenger>(Config.DB_CONNECTION_STRING, null, seatsTable);

            ActionQueryResult seatQueryResult = seatMapper.Insert(new SqlParameter[] {
                new SqlParameter("@PosX", seat.X),
                new SqlParameter("@PosY", seat.Y),
                new SqlParameter("@FlightId", flightId),
                new SqlParameter("@PassengerId", passengerId)
            });

            return seatQueryResult;

        }

        public ActionQueryResult WriteBaggageToDatabase(Baggage baggage, int passengerId) {

            DatabaseTable baggageTargetTable = new DatabaseTable("baggage");
            baggageTargetTable.AddAttribute("Id");
            baggageTargetTable.AddAttribute("FlightId");
            baggageTargetTable.AddAttribute("PassengerId");
            baggageTargetTable.AddAttribute("Weight");
            baggageTargetTable.AddAttribute("Fee");

            ObjectRelationalMapper<Baggage> baggageMapper = new ObjectRelationalMapper<Baggage>(Config.DB_CONNECTION_STRING, null, baggageTargetTable);

            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@FlightId", baggage.FlightId), // TODO: FlightId fehlt! Muss vor Übergabe zugewiesen werden!
                new SqlParameter("@PassengerId", passengerId),
                new SqlParameter("@Weight", baggage.Weight),
                new SqlParameter("@Fee", baggage.Fee)
            };

            ActionQueryResult baggageQueryResult = baggageMapper.Insert(sqlParams);

            return baggageQueryResult;

        }

        public ActionQueryResult WriteRawBookingToDatabase(int passengerId, int flightId, bool isWaiting, out int outputId) {

            DatabaseTable bookingTargetTable = new DatabaseTable("bookings");
            bookingTargetTable.AddAttribute("Id");
            bookingTargetTable.AddAttribute("PassengerId");
            bookingTargetTable.AddAttribute("FlightId");
            bookingTargetTable.AddAttribute("IsWaiting");

            ObjectRelationalMapper<Booking> bookingMapper = new ObjectRelationalMapper<Booking>(Config.DB_CONNECTION_STRING, null, bookingTargetTable);

            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@PassengerId", passengerId),
                new SqlParameter("@FlightId", flightId),
                new SqlParameter("@IsWaiting", isWaiting)
            };

            ActionQueryResult bookingQueryResult = bookingMapper.Insert(sqlParams, "Id", out outputId);

            return bookingQueryResult;

        }

        private Booking CreateBookingInstance(IReadOnlyDictionary<string, object> attributes) {

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

        public FetchResult<Baggage> FetchBaggage(string whereClause = null, SqlParameter[] sqlParams = null) {

            DatabaseTable baggageTargetTable = new DatabaseTable("baggage");
            baggageTargetTable.AddAttribute("Id");
            baggageTargetTable.AddAttribute("FlightId");
            baggageTargetTable.AddAttribute("PassengerId");
            baggageTargetTable.AddAttribute("Weight");
            baggageTargetTable.AddAttribute("Fee");

            ObjectRelationalMapper<Baggage> baggageMapper = new ObjectRelationalMapper<Baggage>(Config.DB_CONNECTION_STRING, baggageTargetTable);

            return baggageMapper.Fetch(attr => new Baggage((int)attr["Id"], (int)attr["FlightId"], (decimal)attr["Weight"], (decimal)attr["Fee"]), whereClause, sqlParams);

        }

        #endregion

    }

}
