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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Airline.Data;
using DatabaseExchange;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

namespace Airline.CheckIn
{
    /// <summary>
    /// Interaktionslogik für CheckInWindow.xaml
    /// </summary>
    public partial class CheckInWindow : Window
    {
        #region Fields

        private ObservableCollection<Booking> _bookings = new ObservableCollection<Booking>();

        private DatabaseAccessor dbAccess = new DatabaseAccessor();

        #endregion

        #region Properties

        #endregion

        #region Constructors

        public CheckInWindow() {

            InitializeComponent();

            lvwBookings.Items.Clear();
            lvwBookings.ItemsSource = _bookings;

        }


        #endregion

        #region Methods

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {

            PopulateBookingListView();

        }

        private void PopulateBookingListView(string whereClause = null, SqlParameter[] sqlParams = null) {

            FetchResult<Booking> fetchResult = dbAccess.FetchBookings(whereClause, sqlParams);

            if (fetchResult.HasError)
                MessageBox.Show("Fehler beim Einholen der Buchungen. \r\n\r\nDetails:\r\n" + fetchResult.ErrorDetails, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            else {

                _bookings.Clear();

                foreach (Booking booking in fetchResult.RetrievedItems)
                    _bookings.Add(booking);

            }

        }

        private void OnClickButtonCreateBooking(object sender, RoutedEventArgs e) {

            ShowBookingWindow();


        }

        private void OnClickButtonCreateStandbyBooking(object sender, RoutedEventArgs e) {

            ShowBookingWindow(false);

        }

        private void ShowBookingWindow(bool isFirmBooking = true) {

            CreateBookingWindow bookingWindow = new CreateBookingWindow(isFirmBooking);

            bool? dialogResult = bookingWindow.ShowDialog();

            if (!dialogResult.HasValue)
                return;

            if (dialogResult.Value) {

                _bookings.Add(bookingWindow.Booking);

            }

        }

        private void OnClickButtonFetchAllBookings(object sender, RoutedEventArgs e) {

            ResetSearchFilters();
            PopulateBookingListView();

        }

        private void OnClickButtonShowWaitingList(object sender, RoutedEventArgs e) {

            ResetSearchFilters();
            PopulateBookingListView("bookings.IsWaiting<>0");

        }

        private void OnClickButtonSearch(object sender, RoutedEventArgs e) {

            List<string> whereClauses = new List<string>();
            List<SqlParameter> sqlParams = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(txtBookingId.Text)) {
                whereClauses.Add("bookings.Id=@SearchId");
                sqlParams.Add(new SqlParameter("@SearchId", txtBookingId.Text));
            }

            if (!string.IsNullOrWhiteSpace(txtFlightId.Text)) {
                whereClauses.Add("bookings.FlightId=@SearchFlightId");
                sqlParams.Add(new SqlParameter("@SearchFlightId", txtFlightId.Text));
            }

            if (!string.IsNullOrWhiteSpace(txtFirstName.Text)) {
                whereClauses.Add("passengers.FirstName LIKE @SearchFirstName");
                sqlParams.Add(new SqlParameter("@SearchFirstName", "%" + txtFirstName.Text + "%"));
            }

            if (!string.IsNullOrWhiteSpace(txtLastName.Text)) {
                whereClauses.Add("passengers.LastName LIKE @SearchLastName");
                sqlParams.Add(new SqlParameter("@SearchLastName", "%" + txtLastName.Text + "%"));
            }

            if(chkStandBy.IsChecked.HasValue && chkStandBy.IsChecked.Value) {
                whereClauses.Add("bookings.IsWaiting<>0");
            }

            string where = string.Join(" AND ", whereClauses);

            if (!string.IsNullOrWhiteSpace(where))
                PopulateBookingListView(where, sqlParams.ToArray());
            else
                PopulateBookingListView();

        }

        private void ResetSearchFilters() {

            txtBookingId.Text = string.Empty;
            txtFlightId.Text = string.Empty;
            txtFirstName.Text = string.Empty;
            txtLastName.Text = string.Empty;
            chkStandBy.IsChecked = false;

        }

        #endregion


    }
}
