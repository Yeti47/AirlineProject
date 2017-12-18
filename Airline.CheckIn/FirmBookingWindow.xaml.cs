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

        private const string ERROR_NO_BAGGAGE_SELECTED = "Es wurde kein Gepäckstück asugewählt.";
        private const string ERROR_BAGGAGE_REMOVAL = "Aufgrund eines unbekannten Fehlers, konnte das Gepäckstück nicht entfernt werden.";
        private const string ERROR_NO_PRINTER = "Es konnte keine Verbindung zum Druck-Server aufgebaut werden. Tja, schade! Dann benutzen Sie doch einfach Stift und Papier!";

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

        public bool HasValidInput {

            get {

                return cmbTitle.SelectedIndex >= 0
                    && !string.IsNullOrWhiteSpace(txtFirstName.Text)
                    && !string.IsNullOrWhiteSpace(txtLastName.Text)
                    && (SelectedFlight != null)
                    && (!SelectedFlight.IsInternational || !string.IsNullOrWhiteSpace(txtPassportId.Text))
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

            if (lvwFlights.Items.Count > 0)
                lvwFlights.SelectedIndex = 0;

        }

        private void OnClickButtonFetchFlights(object sender, RoutedEventArgs e) {

            PopulateFlightList();


        }

        private void OnClickButtonCancel(object sender, RoutedEventArgs e) {

            DialogResult = false;

        }

        private void OnClickButtonOkay(object sender, RoutedEventArgs e) {

            txtFirstName.Text = txtFirstName.Text?.Trim();
            txtLastName.Text = txtLastName.Text?.Trim();
            txtPassportId.Text = txtPassportId.Text?.Trim();

            if (!HasValidInput) {

                MessageBox.Show("Bitte zunächst alle erforderlichen Daten eingeben.", "Fehlende Eingaben", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;

            }

            if(!Booking.IsWaiting && Booking.Passenger.Baggage.Count() < 1) {

                MessageBoxResult messageBoxResult = MessageBox.Show("Sind Sie sicher, dass Sie für diese Buchung keine Gepäckstücke hinterlegen möchten?", "Kein Gepäck angegeben", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (messageBoxResult != MessageBoxResult.Yes)
                    return;

            }

            // In Datenbank schreiben
            int? newBookingId = WriteBookingToDatabase();

            if(!newBookingId.HasValue) {

                MessageBox.Show("Die Buchung ist leider fehlgeschlagen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
            else {

                MessageBox.Show("Die Buchung wurde erfolgreich unter der ID " + newBookingId.Value + " angelegt.", "Erfolg");

            }

            // eingegebene Daten für lokale Booking-Instanz übernehmen, damit Änderungen direkt sichtbar sind
            if (Enum.TryParse(cmbTitle.SelectedItem.ToString(), out Title passengerTitle))
                Booking.Passenger.Title = passengerTitle;

            if(SelectedFlight.IsInternational)
                Booking.Passenger.PassportId = txtPassportId.Text;

            Booking.Passenger.FirstName = txtFirstName.Text;
            Booking.Passenger.LastName = txtLastName.Text;

            Booking.Id = newBookingId.Value;
            Booking.Flight = SelectedFlight;

            Booking.SeatNumber = cmbSeat.SelectedItem as SeatNumber?;

            // Fenster mit Erfolg-Meldung schließen
            DialogResult = true;


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

        private int? WritePassengerToDatabase() {

            if (!Enum.TryParse(cmbTitle.SelectedItem?.ToString(), out Title passengerTitle)) {

                MessageBox.Show("Kann Fluggast nicht in die Datenbank schreiben. Fehler beim Parsen der Anrede.");
                return null;

            }

            if(SelectedFlight == null) {

                MessageBox.Show("Kann Fluggast nicht in die Datenbank schreiben. Es wurde kein Flug ausgewählt.");
                return null;
            }

            if (SelectedFlight.IsInternational)
                Booking.Passenger.PassportId = txtPassportId.Text;
                
            ObjectRelationalMapper<Passenger> passengerMapper = new ObjectRelationalMapper<Passenger>(Config.DB_CONNECTION_STRING, null, Config.PassengerTargetTable);

            List<SqlParameter> passengerSqlParams = new List<SqlParameter>();
            passengerSqlParams.Add(new SqlParameter("@Title", passengerTitle));
            passengerSqlParams.Add(new SqlParameter("@FirstName", txtFirstName.Text));
            passengerSqlParams.Add(new SqlParameter("@LastName", txtLastName.Text));

            if(SelectedFlight.IsInternational)
                passengerSqlParams.Add(new SqlParameter("@PassportNumber", Booking.Passenger.PassportId));

            ActionQueryResult passengerInsertResult = passengerMapper.Insert(passengerSqlParams.ToArray(), "Id", out int outputId);

            if(passengerInsertResult.HasError) {

                MessageBox.Show("Fehler beim Schreiben des Fluggastes in die Datenbank.\r\n\r\nDetails:\r\n" + passengerInsertResult, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;

            }

            return outputId;

        }

        private bool WriteSeatToDatabase(int passengerId) {

            if(SelectedFlight == null) {

                MessageBox.Show("Sitzplatz kann nicht in die Datenbank geschrieben werden, da kein Flug ausgewählt wurde.");

                return false;

            }

            SeatNumber? seatNumber = cmbSeat.SelectedItem as SeatNumber?;

            if(!seatNumber.HasValue) {

                MessageBox.Show("Sitznummer konnte nicht in die Datenbank geschrieben werden. Es wurde keine Sitznummer ausgewählt.");
                return false;

            }

            SeatNumber seat = seatNumber.Value;

            ObjectRelationalMapper<Passenger> seatMapper = new ObjectRelationalMapper<Passenger>(Config.DB_CONNECTION_STRING, null, Config.SeatsTable);

            ActionQueryResult seatQueryResult = seatMapper.Insert(new SqlParameter[] {
                new SqlParameter("@PosX", seat.X),
                new SqlParameter("@PosY", seat.Y),
                new SqlParameter("@FlightId", SelectedFlight.Id),
                new SqlParameter("@PassengerId", passengerId)
            });

            if(seatQueryResult.HasError) {

                MessageBox.Show("Fehler beim Schreiben des Sitzokatzes in die Datenbank.\r\n\r\nDetails:\r\n" + seatQueryResult, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;

            }

            return true;

        }

        private int? WriteRawBookingToDatabase(int passengerId) {

            if (SelectedFlight == null) {

                MessageBox.Show("Buchung kann nicht in die Datenbank geschrieben werden, da kein FLug ausgewählt wurde.");
                return null;

            }

            ObjectRelationalMapper<Booking> bookingMapper = new ObjectRelationalMapper<Booking>(Config.DB_CONNECTION_STRING, null, Config.BookingTargetTable);

            SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@PassengerId", passengerId),
                new SqlParameter("@FlightId", SelectedFlight.Id),
                new SqlParameter("@IsWaiting", Booking.IsWaiting)
            };

            ActionQueryResult bookingQueryResult = bookingMapper.Insert(sqlParams, "Id", out int outputId);

            if (bookingQueryResult.HasError) {

                MessageBox.Show("Fehler beim Schreiben der Buchungsdaten in die Datenbank.\r\n\r\nDetails:\r\n" + bookingQueryResult, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;

            }

            return outputId;

        }

        private bool WriteBaggageToDatabase(int passengerId) {

            if (SelectedFlight == null) {

                MessageBox.Show("Die Gepäcksücke können nicht in die Datenbank geschrieben werden, da kein Flug ausgewählt wurde.");
                return false;

            }

            ObjectRelationalMapper<Baggage> baggageMapper = new ObjectRelationalMapper<Baggage>(Config.DB_CONNECTION_STRING, null, Config.BaggageTargetTable);

            foreach(Baggage bg in Booking.Passenger.Baggage) {

                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@FlightId", SelectedFlight.Id),
                    new SqlParameter("@PassengerId", passengerId),
                    new SqlParameter("@Weight", bg.Weight),
                    new SqlParameter("@Fee", bg.Fee)
                };

                ActionQueryResult baggageQueryResult = baggageMapper.Insert(sqlParams);

                if(baggageQueryResult.HasError) {

                    MessageBox.Show("Fehler beim Schreiben eines Gepäckstücks in die Datenbank.\r\n\r\nDetails:\r\n" + baggageQueryResult, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;

                }

            }

            return true;

        }

        private int? WriteBookingToDatabase() {

            if(SelectedFlight == null) {

                MessageBox.Show("Buchung kann nicht erstellt werden. Es wurde kein Flug ausgewählt.");

                return null;
            }

            int? passengerId = WritePassengerToDatabase();

            if (!passengerId.HasValue)
                return null;

            if(!Booking.IsWaiting) {

                if (!WriteSeatToDatabase(passengerId.Value))
                    return null;

            }

            if(Booking.Passenger.Baggage.Count() > 0) {

                if (!WriteBaggageToDatabase(passengerId.Value))
                    return null;

            }

            int? bookingId = WriteRawBookingToDatabase(passengerId.Value);

            if (!bookingId.HasValue)
                return null;

            return bookingId;

        }

        private void OnClickButtonPrintLabel(object sender, RoutedEventArgs e) {

            MessageBox.Show(ERROR_NO_PRINTER, "Drucken nicht möglich", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        #endregion

    }

}
