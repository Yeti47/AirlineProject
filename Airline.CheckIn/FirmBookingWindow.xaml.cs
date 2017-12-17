using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Airline.Data;
using DatabaseExchange;

namespace Airline.CheckIn {
    /// <summary>
    /// Interaktionslogik für BookingWindow.xaml
    /// </summary>
    public partial class BookingWindow : Window {

        #region Constants

        private const string ERROR_NO_BAGGAGE_SELECTED = "Es wurde kein Gepäckstück asugewählt.";
        private const string ERROR_BAGGAGE_REMOVAL = "Aufgrund eines unbekannten Fehlers, konnte das Gepäckstück nicht entfernt werden.";

        #endregion

        #region Fields

        private ObservableCollection<Flight> _flights = new ObservableCollection<Flight>();

        private ObjectRelationalMapper<Flight> _flightsMapper = new ObjectRelationalMapper<Flight>(Config.DB_CONNECTION_STRING, Config.FlightSourceTable);

        private ObjectRelationalMapper<SeatNumber> _seatNumberMapper = new ObjectRelationalMapper<SeatNumber>(Config.DB_CONNECTION_STRING, Config.SeatsTable);

        #endregion

        #region Properties

        public Booking Booking { get; set; }

        public string SelectedFlightId {

            get {

                Flight selFlight = lvwFlights.SelectedItem as Flight;

                return selFlight?.Id.ToString();

            }

        }

        public Flight SelectedFlight => lvwFlights.SelectedItem as Flight;

        #endregion

        #region Constructors

        public BookingWindow(bool isFirm = true) {

            InitializeComponent();

            //Booking = booking ?? new Booking { Passenger = new Passenger(Data.Title.Mr, string.Empty, string.Empty) };
            Booking = new Booking { Passenger = new Passenger(Data.Title.Mr, string.Empty, string.Empty), IsWaiting = !isFirm };

            DataContext = Booking;
            
            lvwFlights.Items.Clear();
            lvwFlights.ItemsSource = _flights;

            lvwBaggage.Items.Clear();
            lvwBaggage.ItemsSource = Booking.Passenger.Baggage;

            grpSeat.IsEnabled = grpBaggage.IsEnabled = isFirm;

            Title = isFirm ? "Festbuchung anlegen" : "Stand-by-Buchung anlegen";

        }

        #endregion

        #region Methods

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {

            PopulateTitleListBox();

        }

        private IEnumerable<SeatNumber> FetchTakenSeats(int flightId) {

            string whereClause = "FlightId = " + flightId;
            FetchResult<SeatNumber> fetchResultSeatNumbers = _seatNumberMapper.Fetch(attr => new SeatNumber((int)attr["PosX"], (int)attr["PosY"]), whereClause);

            if(fetchResultSeatNumbers.HasError) {

                return new SeatNumber[0];

            }

            return fetchResultSeatNumbers.RetrievedItems;


        }

        private void PopulateTitleListBox() {

            cmbTitle.Items.Clear();

            foreach(string title in Enum.GetNames(typeof(Title)))
                cmbTitle.Items.Add(title);

            if (cmbTitle.Items.Count > 0)
                cmbTitle.SelectedIndex = 0;

        }

        private void PopulateFlightList() {

            FetchResult<Flight> flightsFetched = _flightsMapper.Fetch(attr => {

                Airport departureAp = new Airport(attr["depAirport.Country"].ToString(), attr["depAirport.City"].ToString());
                Airport destinationAp = new Airport(attr["destAirport.Country"].ToString(), attr["destAirport.City"].ToString());

                return
                    new Flight((int)attr["flights.Id"],
                    (DateTime)attr["TimeOfDeparture"],
                    (DateTime)attr["TimeOfArrival"],
                    departureAp, destinationAp,
                    (int)attr["SeatRows"],
                    (int)attr["SeatsPerRow"]);

            });

            if (flightsFetched.HasError) {

                MessageBox.Show("Leider ist ein Fehler beim Abrufen der Flüge aufgetreten.\r\n\r\n Details:\r\n" + flightsFetched.ErrorDetails, "Fehler");

            }
            else {

                foreach(Flight flight in flightsFetched.RetrievedItems)
                    flight.TakenSeatNumbers = FetchTakenSeats(flight.Id);

                _flights.Clear();

                foreach (Flight flight in flightsFetched.RetrievedItems)
                    _flights.Add(flight);

            }

        }

        private void OnClickButtonFetchFlights(object sender, RoutedEventArgs e) {

            PopulateFlightList();


        }

        private void OnClickButtonCancel(object sender, RoutedEventArgs e) {

            DialogResult = false;

        }

        private void OnClickButtonOkay(object sender, RoutedEventArgs e) {

        }

        private void OnClickButtonAddBaggage(object sender, RoutedEventArgs e) {

            BaggageWindow baggageWindow = new BaggageWindow();

            bool? dialogResult = baggageWindow.ShowDialog();

            if (!dialogResult.HasValue)
                return;

            if(dialogResult.Value) {

                Booking.Passenger.AddBaggage(baggageWindow.Baggage);

            }

        }

        private void OnClickButtonEditBaggage(object sender, RoutedEventArgs e) {

            Baggage selectedBaggage = lvwBaggage.SelectedItem as Baggage;

            if (selectedBaggage == null) {

                MessageBox.Show(ERROR_NO_BAGGAGE_SELECTED, "Bearbeiten nicht möglich", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            }

            BaggageWindow baggageWindow = new BaggageWindow(selectedBaggage);

            bool? dialogResult = baggageWindow.ShowDialog();

        }

        private void OnClickButtonRemoveBaggage(object sender, RoutedEventArgs e) {

            Baggage selectedBaggage = lvwBaggage.SelectedItem as Baggage;

            if (selectedBaggage == null) {

                MessageBox.Show(ERROR_NO_BAGGAGE_SELECTED, "Entfernen nicht möglich", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

            }

            if(!Booking.Passenger.RemoveBaggage(selectedBaggage))
                MessageBox.Show(ERROR_BAGGAGE_REMOVAL, "Entfernen nicht möglich", MessageBoxButton.OK, MessageBoxImage.Error);


        }

        private void OnFlightsSelectionChanged(object sender, SelectionChangedEventArgs e) {

            cmbSeat.ItemsSource = SelectedFlight?.AvailableSeatNumbers;
            txtPassportId.IsEnabled = SelectedFlight?.IsInternational ?? false;
            lblPassportId.IsEnabled = txtPassportId.IsEnabled;

        }

        #endregion


    }

}
