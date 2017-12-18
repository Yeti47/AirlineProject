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
using Airline.Data;
using DatabaseExchange;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;

namespace Airline.CheckIn
{
    /// <summary>
    /// Interaktionslogik für BaggageScanWindow.xaml
    /// </summary>
    public partial class BaggageScanWindow : Window, INotifyPropertyChanged
    {

        #region Constants

        private const string ERROR_FETCH_BAGGAGE = "Beim Einholen der Gepäckstücke aus der Datenbank ist ein Fehler aufgetreten.";
        private const string ERROR_SCAN = "Leider ist ein technischer Fehler aufgetreten. Entschuldigung! :(";

        private const string SCAN_POSITIVE = "Das eingescannte Gepäckstück gehört zu diesem Flug.";
        private const string SCAN_NEGATIVE = "Das eingescannte Gepäckstück ist ein Irrläufer.";

        private const string ERROR_INVALID_INPUT = "Bitte entweder Scan-Gerät anschließen oder gültige Gepäck-ID manuell eingeben.";
        private const string ERROR_NO_FLIGHT_SELECTED = "Bitte einen Flug auswählen.";

        #endregion

        #region Fields

        private int? _baggageId;

        private ObservableCollection<Baggage> _baggage = new ObservableCollection<Baggage>();
        private ObservableCollection<Flight> _flights = new ObservableCollection<Flight>();

        private ObjectRelationalMapper<Baggage> _baggageMapper;
        private ObjectRelationalMapper<Flight> _flightsMapper;

        #endregion

        #region Properties

        public int? BaggageId {

            get => _baggageId;
            set {
                _baggageId = value;
                OnPropertyChanged();
            }

        }

        public Flight SelectedFlight => lvwFlights?.SelectedItem as Flight;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public BaggageScanWindow() {

            InitializeComponent();

            _baggageMapper = new ObjectRelationalMapper<Baggage>(Config.DB_CONNECTION_STRING, Config.BaggageTargetTable);
            _flightsMapper = new ObjectRelationalMapper<Flight>(Config.DB_CONNECTION_STRING, Config.FlightSourceTable);

            lvwBaggage.Items.Clear();
            lvwBaggage.ItemsSource = _baggage;

            lvwFlights.Items.Clear();
            lvwFlights.ItemsSource = _flights;

            DataContext = this;

        }

        #endregion

        #region Methods

        private void OnClickButtonFetchAll(object sender, RoutedEventArgs e) {

            PopulateBaggageList();

        }

        private void PopulateBaggageList(string whereClause = null) {

            if (_baggageMapper == null)
                return;

            FetchResult<Baggage> baggageFetchResult = _baggageMapper.Fetch(attr => new Baggage((int)attr["Id"], (int)attr["FlightId"], (decimal)attr["Weight"], (decimal)attr["Fee"]), whereClause);

            if(baggageFetchResult.HasError) {

                MessageBox.Show(ERROR_FETCH_BAGGAGE + "\r\n\r\nDetails:\r\n" + baggageFetchResult.ErrorDetails, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _baggage.Clear();

            foreach (Baggage bag in baggageFetchResult.RetrievedItems)
                _baggage.Add(bag);


        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {

            PropertyChangedEventHandler handler = PropertyChanged;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private void OnClickButtonScan(object sender, RoutedEventArgs e) {

            if (_baggageMapper == null)
                return;

            if(SelectedFlight == null) {

                MessageBox.Show(ERROR_NO_FLIGHT_SELECTED, "Kein Flug gewählt");
                return;

            }

            if(BaggageId == null || txtBaggageId.GetBindingExpression(TextBox.TextProperty).HasError) {

                MessageBox.Show(ERROR_INVALID_INPUT, "Scan nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;

            }

            FetchResult<Baggage> baggageFetchResult = _baggageMapper.Fetch(attr => new Baggage((int)attr["Id"], (int)attr["FlightId"], (decimal)attr["Weight"], (decimal)attr["Fee"]), 
                "Id=@Id AND FlightId=@FlightID", 
                new SqlParameter[] { new SqlParameter("@Id", BaggageId), new SqlParameter("@FlightId", SelectedFlight.Id) }
            );

            if (baggageFetchResult.HasError) {

                MessageBox.Show(ERROR_SCAN + "\r\n\r\nDetails:\r\n" + baggageFetchResult.ErrorDetails, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(baggageFetchResult.NumberAffectedRows > 0)
                MessageBox.Show(SCAN_POSITIVE, "Scan abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(SCAN_NEGATIVE, "Scan abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void OnClickButtonReset(object sender, RoutedEventArgs e) => _baggage?.Clear();

        private void OnWindowLoaded(object sender, RoutedEventArgs e) => PopulateFlightList();

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

                _flights.Clear();

                foreach (Flight flight in flightsFetched.RetrievedItems)
                    _flights.Add(flight);

            }

            if (lvwFlights.Items.Count > 0)
                lvwFlights.SelectedIndex = 0;

        }

        private void OnClickButtonRefreshFlights(object sender, RoutedEventArgs e) => PopulateFlightList();

        #endregion


    }
}
