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

        private const string ERROR_INVALID_INPUT = "Bitte entweder Scan-Gerät anschließen oder gültige Daten manuell eingeben.";

        #endregion

        #region Fields

        private int? _baggageId;
        private int? _flightId;

        private ObservableCollection<Baggage> _baggage = new ObservableCollection<Baggage>();

        private ObjectRelationalMapper<Baggage> _baggageMapper;

        #endregion

        #region Properties

        public int? BaggageId {

            get => _baggageId;
            set {
                _baggageId = value;
                OnPropertyChanged();
            }

        }

        public int? FlightId {

            get => _flightId;
            set {
                _flightId = value;
                OnPropertyChanged();
            }

        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public BaggageScanWindow() {

            InitializeComponent();

            _baggageMapper = new ObjectRelationalMapper<Baggage>(Config.DB_CONNECTION_STRING, Config.BaggageTargetTable);

            lvwBaggage.Items.Clear();
            lvwBaggage.ItemsSource = _baggage;

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

            

            if(BaggageId == null || FlightId == null || txtBaggageId.GetBindingExpression(TextBox.TextProperty).HasError || txtFlightId.GetBindingExpression(TextBox.TextProperty).HasError) {

                MessageBox.Show(ERROR_INVALID_INPUT, "Scan nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;

            }

            FetchResult<Baggage> baggageFetchResult = _baggageMapper.Fetch(attr => new Baggage((int)attr["Id"], (int)attr["FlightId"], (decimal)attr["Weight"], (decimal)attr["Fee"]), 
                "Id=@Id AND FlightId=@FlightID", 
                new SqlParameter[] { new SqlParameter("@Id", BaggageId), new SqlParameter("@FlightId", FlightId) }
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

        #endregion


    }
}
