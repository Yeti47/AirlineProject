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
using DatabaseExchange;
using Airline.Data;
using System.ComponentModel;

namespace Airline.CheckIn {

    /// <summary>
    /// Interaktionslogik für BaggageWindow.xaml
    /// </summary>
    public partial class BaggageWindow : Window {

        /// <summary>
        /// Hilfsklasse zum Speichern der Gepäckgebühr-Informationen aus der Datenbank.
        /// </summary>
        private class BaggageFeeInfo {

            public decimal WeightLimit { get; set; }
            public decimal FeePerExtraKilogram { get; set; }

        }

        #region Constants

        private const string ERROR_FEE_INFO = "Leider ist beim Einholen der Gepäckgebühr-Informationen ein Fehler aufgetreten. Es werden stattdessen Standard-Werte verwendet.";

        private const decimal MIN_RANDOM_WEIGHT = 3m;
        private const decimal MAX_RANDOM_WEIGHT = 38m;

        #endregion

        #region Fields

        private ObjectRelationalMapper<BaggageFeeInfo> _baggageFeeMapper;

        private BaggageFeeInfo _baggageFeeInfo = new BaggageFeeInfo { WeightLimit = 30m, FeePerExtraKilogram = 3m };

        private Random _random = new Random();

        #endregion

        #region Properties

        public Baggage PreviewBaggage { get; private set; }

        public Baggage Baggage { get; set; }

        public bool HasValidInput => PreviewBaggage != null && PreviewBaggage.Weight > 0;

        #endregion

        #region Constructors

        public BaggageWindow(Baggage baggage = null) {

            InitializeComponent();

            PreviewBaggage = new Baggage();

            PreviewBaggage.PropertyChanged += (sender, e) => btnOkay.IsEnabled = HasValidInput;

            if (baggage != null) {

                Baggage = baggage;

                PreviewBaggage.Weight = baggage.Weight;
                PreviewBaggage.WeightLimit = baggage.WeightLimit;
                PreviewBaggage.FeePerExtraKilogram = baggage.FeePerExtraKilogram;

                Title = "Gepäckstück bearbeiten";

            }
            else {

                Baggage = PreviewBaggage;

                Title = "Gepäckstück hinzufügen";

            }

            DataContext = PreviewBaggage;

            _baggageFeeMapper = new ObjectRelationalMapper<BaggageFeeInfo>(Config.DB_CONNECTION_STRING);

            DatabaseTable baggageFeeTable = new DatabaseTable("baggagefees");
            baggageFeeTable.AddAttribute("Limit");
            baggageFeeTable.AddAttribute("FeePerKilogram");

            _baggageFeeMapper.SourceTable = baggageFeeTable;

            //btnOkay.IsEnabled = HasValidInput;

        }

        #endregion

        #region Methods

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {

            FetchBaggageFeeInfo();

        }

        private void FetchBaggageFeeInfo() {

            FetchResult<BaggageFeeInfo> baggageFeeFetchResult = _baggageFeeMapper.Fetch(attr => new BaggageFeeInfo { WeightLimit = (decimal)attr["Limit"], FeePerExtraKilogram = (decimal)attr["FeePerKilogram"] });

            if(baggageFeeFetchResult.HasError || baggageFeeFetchResult.RetrievedItems.FirstOrDefault() == null) {

                MessageBox.Show(ERROR_FEE_INFO + "\r\n\r\nFehler-Details:\r\n" + baggageFeeFetchResult.ErrorDetails, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else {

                _baggageFeeInfo = baggageFeeFetchResult.RetrievedItems.FirstOrDefault();

            }

            PreviewBaggage.WeightLimit = _baggageFeeInfo.WeightLimit;
            PreviewBaggage.FeePerExtraKilogram = _baggageFeeInfo.FeePerExtraKilogram;

        }

        private void OnClickButtonOkay(object sender, RoutedEventArgs e) {

            if(!HasValidInput) {

                MessageBox.Show("Es wurde kein gültiges Gewicht eingegeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Baggage != null) {

                Baggage.Weight = PreviewBaggage.Weight;
                Baggage.WeightLimit = PreviewBaggage.WeightLimit;
                Baggage.FeePerExtraKilogram = PreviewBaggage.FeePerExtraKilogram;

            }

            DialogResult = true;


        }

        private void OnClickButtonCancel(object sender, RoutedEventArgs e) {

            DialogResult = false;

        }

        private void OnClickButtonWeigh(object sender, RoutedEventArgs e) {

            if (PreviewBaggage == null || _random == null)
                return;

            PreviewBaggage.Weight = (decimal)_random.NextDouble() * (MAX_RANDOM_WEIGHT - MIN_RANDOM_WEIGHT) + MIN_RANDOM_WEIGHT;

        }


        #endregion


    }

}
