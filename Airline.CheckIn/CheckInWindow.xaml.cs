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

        private ObjectRelationalMapper<Booking> _bookingMapper = new ObjectRelationalMapper<Booking>(Config.DB_CONNECTION_STRING, Config.BookingSourceTable);

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

        private void PopulateBookingListView() {

            FetchResult<Booking> fetchResult = _bookingMapper.Fetch(Booking.CreateFromDataRecord);

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

            PopulateBookingListView();

        }

        #endregion
    }
}
