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

        #region Fields

        private ObservableCollection<Flight> _flights = new ObservableCollection<Flight>();

        #endregion

        #region Constructors

        public BookingWindow() {

            InitializeComponent();

            lvwFlights.Items.Clear();
            lvwFlights.ItemsSource = _flights;

        }

        #endregion

        #region Methods

        private void PopulateFlightList() {

            ObjectRelationalMapper<Flight> orm = new ObjectRelationalMapper<Flight>(Config.DB_CONNECTION_STRING, Config.FlightSourceTable);

            FetchResult<Flight> flightsFetched = orm.Fetch(attr => {

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

                MessageBox.Show("Leider ist ein Fehler beim Abrufen der Flüge aufgetreten.\r\n\r\n Details:\r\n" + flightsFetched.ToString(), "Fehler");

            }
            else {

                _flights.Clear();

                foreach (Flight flight in flightsFetched.RetrievedItems)
                    _flights.Add(flight);

            }

        }

        private void OnClickButtonFetchFlights(object sender, RoutedEventArgs e) {

            PopulateFlightList();

        }

        #endregion


    }

}
