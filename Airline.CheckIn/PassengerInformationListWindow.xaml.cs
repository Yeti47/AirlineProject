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
using System.Data.SqlClient;

namespace Airline.CheckIn
{
    /// <summary>
    /// Interaktionslogik für PassengerInformationListWindow.xaml
    /// </summary>
    public partial class PassengerInformationListWindow : Window {

        #region Fields

        private ObservableCollection<Flight> _flights = new ObservableCollection<Flight>();
        private ObservableCollection<Booking> _bookings = new ObservableCollection<Booking>();

        private DatabaseAccessor dbAccess = new DatabaseAccessor();

        #endregion

        #region Properties

        public Flight SelectedFlight => lvwFlights.SelectedItem as Flight;

        #endregion

        #region Constructors

        public PassengerInformationListWindow() {

            InitializeComponent();

            lvwBoardingCards.ItemsSource = _bookings;
            lvwFlights.ItemsSource = _flights;

        }

        #endregion

        #region Methods

        private void PopulateFlightList() {

            FetchResult<Flight> flightsFetched = dbAccess.FetchFlights();

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

        private void OnClickButtonGeneratePil(object sender, RoutedEventArgs e) {

            if (SelectedFlight == null) {

                MessageBox.Show("Bitte zunächst einen Flug auswählen.", "Kein Flug ausgewählt");
                return;

            }

            FetchResult<Booking> fetchResult = dbAccess.FetchBookings("bookings.FlightId=@FlightId AND bookings.IsWaiting=0", new SqlParameter[] { new SqlParameter("@FlightId", SelectedFlight.Id) });

            if (fetchResult.HasError)
                MessageBox.Show("Fehler beim Einholen der Buchungen. \r\n\r\nDetails:\r\n" + fetchResult.ErrorDetails, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            else {

                _bookings.Clear();

                foreach (Booking booking in fetchResult.RetrievedItems)
                    _bookings.Add(booking);

            }

        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {

            PopulateFlightList();

        }

        private void OnClickButtonFetchAllFlights(object sender, RoutedEventArgs e) {

            PopulateFlightList();

        }

        private void OnClickButtonPrintPil(object sender, RoutedEventArgs e) {

            if(lvwBoardingCards.Items.Count <= 0) {

                MessageBox.Show("Die PIL ist aktuell leer. Bitte zuerst eine PIL erzeugen.", "Keine Daten");
                return;

            }

            MessageBox.Show("Der Drucker reagiert leider nicht. Hoffentlich haben Sie Zettel und Stift parat.\r\n\r\n¯\\_(ツ)_/¯", "Druckvorgang abgebrochen");

        }

        #endregion


    }

}
