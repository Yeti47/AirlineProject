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
using System.Data.SqlClient;

namespace Airline.CheckIn {

    /// <summary>
    /// Interaktionslogik für BookingWindow.xaml
    /// </summary>
    public partial class CreateBookingWindow : Window {

        #region Constants

        private const string ERROR_NO_BAGGAGE_SELECTED = "Es wurde kein Gepäckstück ausgewählt.";
        private const string ERROR_BAGGAGE_REMOVAL = "Aufgrund eines unbekannten Fehlers, konnte das Gepäckstück nicht entfernt werden.";
        private const string ERROR_NO_PRINTER = "Es konnte keine Verbindung zum Druck-Server aufgebaut werden. Tja, schade! Dann benutzen Sie doch einfach Stift und Papier!";

        private const int PASSPORT_ID_MIN_LENGTH = 6;

        #endregion

        #region Fields

        private ObservableCollection<Flight> _flights = new ObservableCollection<Flight>();

        private DatabaseAccessor dbAccess = new DatabaseAccessor();

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

        public bool HasValidInput {

            get {

                return cmbTitle.SelectedIndex >= 0
                    && !string.IsNullOrWhiteSpace(txtFirstName.Text)
                    && !string.IsNullOrWhiteSpace(txtLastName.Text)
                    && (SelectedFlight != null)
                    && (!SelectedFlight.IsInternational || (!string.IsNullOrWhiteSpace(txtPassportId.Text) && txtPassportId.Text.Trim().Length >= PASSPORT_ID_MIN_LENGTH))
                    && (Booking.IsWaiting || (cmbSeat.SelectedItem as SeatNumber?) != null);

            }

        }

        #endregion

        #region Constructors

        public CreateBookingWindow(bool isFirm = true) {

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

            txtPassportId.IsEnabled = lblPassportId.IsEnabled = false;

            PopulateFlightList();
            PopulateTitleListBox();

        }

        private void PopulateTitleListBox() {

            cmbTitle.Items.Clear();

            foreach(string title in Enum.GetNames(typeof(Title)))
                cmbTitle.Items.Add(title);

            if (cmbTitle.Items.Count > 0)
                cmbTitle.SelectedIndex = 0;

        }

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

        private void OnClickButtonFetchFlights(object sender, RoutedEventArgs e) {

            PopulateFlightList();


        }

        private void OnClickButtonCancel(object sender, RoutedEventArgs e) {

            DialogResult = false;

        }

        private bool InsertBooking(out int newBookingId) {

            newBookingId = -1;

            txtFirstName.Text = txtFirstName.Text?.Trim();
            txtLastName.Text = txtLastName.Text?.Trim();
            txtPassportId.Text = txtPassportId.Text?.Trim();

            if (!HasValidInput) {

                if(SelectedFlight != null && SelectedFlight.IsInternational && txtPassportId.Text.Length < PASSPORT_ID_MIN_LENGTH) {
                    MessageBox.Show("Die Reisepassnummer muss bei Auslandsreisen mindestens " + PASSPORT_ID_MIN_LENGTH + " Zeichen lang sein.", "Ungültige Reisepassnummer", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                MessageBox.Show("Bitte zunächst alle erforderlichen Daten eingeben.", "Fehlende Eingaben", MessageBoxButton.OK, MessageBoxImage.Warning);

                return false;

            }

            if (!Booking.IsWaiting && Booking.Passenger.Baggage.Count() < 1) {

                MessageBoxResult messageBoxResult = MessageBox.Show("Sind Sie sicher, dass Sie für diese Buchung keine Gepäckstücke hinterlegen möchten?", "Kein Gepäck angegeben", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (messageBoxResult != MessageBoxResult.Yes)
                    return false;

            }

            if (!Enum.TryParse(cmbTitle.SelectedItem?.ToString(), out Title passengerTitle)) {
                MessageBox.Show("Kann Fluggast nicht in die Datenbank schreiben. Fehler beim Parsen der Anrede.");
                return false;
            }

            ActionQueryResult passengerQueryResult = dbAccess.WritePassengerToDatabase(new Passenger(passengerTitle, txtFirstName.Text, txtLastName.Text, SelectedFlight.IsInternational ? txtPassportId.Text : null), out int passengerId);

            if(passengerQueryResult.HasError) { 
                MessageBox.Show("Fehler beim Anlegen des Fluggastes.\r\n\r\nDetails:r\n" + passengerQueryResult.ErrorDetails, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if(!Booking.IsWaiting) {

                SeatNumber? seatNumber = cmbSeat.SelectedItem as SeatNumber?;

                if (!seatNumber.HasValue) {
                    MessageBox.Show("Sitznummer konnte nicht in die Datenbank geschrieben werden. Es wurde keine Sitznummer ausgewählt.");
                    return false;
                }

                SeatNumber seat = seatNumber.Value;

                ActionQueryResult seatQueryResult = dbAccess.WriteSeatToDatabase(seat, passengerId, SelectedFlight.Id);

                if (seatQueryResult.HasError) {
                    MessageBox.Show("Fehler beim Schreiben des Sitzokatzes in die Datenbank.\r\n\r\nDetails:\r\n" + seatQueryResult, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                foreach (Baggage bg in Booking.Passenger.Baggage) {

                    bg.FlightId = SelectedFlight.Id;
                    ActionQueryResult baggageQueryResult = dbAccess.WriteBaggageToDatabase(bg, passengerId);

                    if(baggageQueryResult.HasError) {
                        MessageBox.Show("Fehler beim Schreiben eines Gepäckstücks in die Datenbank.\r\n\r\nDetails:\r\n" + baggageQueryResult, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                }

            }

            ActionQueryResult bookingQueryResult = dbAccess.WriteRawBookingToDatabase(passengerId, SelectedFlight.Id, Booking.IsWaiting, out newBookingId);

            if (bookingQueryResult.HasError) {

                MessageBox.Show("Fehler beim Schreiben der Buchungsdaten in die Datenbank.\r\n\r\nDetails:\r\n" + bookingQueryResult, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;

            }

            return true;

        }

        private void OnClickButtonOkay(object sender, RoutedEventArgs e) {

            if(InsertBooking(out int bookId)) {

                MessageBox.Show("Die Buchung wurde erfolgreich unter der ID " + bookId + " angelegt.", "Erfolg");

                // eingegebene Daten für lokale Booking-Instanz übernehmen, damit Änderungen direkt sichtbar sind
                if (Enum.TryParse(cmbTitle.SelectedItem.ToString(), out Title passengerTitle))
                    Booking.Passenger.Title = passengerTitle;

                if (SelectedFlight.IsInternational)
                    Booking.Passenger.PassportId = txtPassportId.Text;

                Booking.Passenger.FirstName = txtFirstName.Text;
                Booking.Passenger.LastName = txtLastName.Text;

                Booking.Id = bookId;
                Booking.Flight = SelectedFlight;

                Booking.SeatNumber = cmbSeat.SelectedItem as SeatNumber?;

                // Fenster mit Erfolg-Meldung schließen
                DialogResult = true;

            }   

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

        private void OnClickButtonPrintLabel(object sender, RoutedEventArgs e) {

            if (lvwBaggage.SelectedIndex < 0)
                MessageBox.Show(ERROR_NO_BAGGAGE_SELECTED, "Keine Auswahl", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(ERROR_NO_PRINTER, "Drucken nicht möglich", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        #endregion

    }

}
