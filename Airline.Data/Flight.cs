using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    public class Flight {

        #region Fields

        private List<SeatNumber> _allSeatNumbers;

        #endregion

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

                return DepartureAirport.Country != DestinationAirport.Country;

            }

        }

        public string FlightTypeText => IsInternational ? "International" : "Inland";

        public IEnumerable<SeatNumber> TakenSeatNumbers { get; set; }

        public IEnumerable<SeatNumber> AllSeatNumbers => _allSeatNumbers;

        public IEnumerable<SeatNumber> AvailableSeatNumbers => _allSeatNumbers.Except(TakenSeatNumbers);

        public int AvailableSeatCount => _allSeatNumbers.Count - TakenSeatNumbers.Count();

        #endregion

        #region Constructors

        public Flight(int id, DateTime timeOfDeparture, DateTime timeOfArrival,
            Airport departureAirport, Airport destinationAirport, int seatsRowCount, int seatsPerRow, IEnumerable<SeatNumber> takenSeats = null) {

            Id = id;
            TimeOfDeparture = timeOfDeparture;
            TimeOfArrival = timeOfArrival;
            DepartureAirport = departureAirport;
            DestinationAirport = destinationAirport;
            SeatRowsCount = seatsRowCount;
            SeatsPerRow = seatsPerRow;

            _allSeatNumbers = new List<SeatNumber>();

            for(int y = 1; y <= SeatRowsCount; y++) {

                for(int x = 1; x <= SeatsPerRow; x++) {

                    _allSeatNumbers.Add(new SeatNumber(x, y));

                }

            }

            TakenSeatNumbers = takenSeats ?? new SeatNumber[0];

        }

        #endregion



    }

}
