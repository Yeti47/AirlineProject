using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    public class Flight { 

        #region Properties

        public int Id { get; set; }

        public DateTime TimeOfDeparture { get; set; }
        public DateTime TimeOfArrival { get; set; }

        public Airport DepartureAirport { get; private set; }
        public Airport DestinationAirport { get; private set; }

        public int TotalNumberSeats => SeatRowsCount * SeatsPerRow; 

        public int SeatRowsCount { get; private set; }
        public int SeatsPerRow { get; private set; }

        public bool IsInternational {

            get {

                if (DepartureAirport == null || DestinationAirport == null)
                    return false;

                return DepartureAirport.Country == DestinationAirport.Country;

            }

        }

        #endregion

        #region Constructors

        public Flight(int id, DateTime timeOfDeparture, DateTime timeOfArrival,
            Airport departureAirport, Airport destinationAirport, int seatsRowCount, int seatsPerRow) {

            Id = id;
            TimeOfDeparture = timeOfDeparture;
            TimeOfArrival = timeOfArrival;
            DepartureAirport = departureAirport;
            DestinationAirport = destinationAirport;
            SeatRowsCount = seatsRowCount;
            SeatsPerRow = seatsPerRow;

        }

        #endregion



    }

}
