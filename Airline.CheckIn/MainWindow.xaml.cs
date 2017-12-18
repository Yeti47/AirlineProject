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

namespace Airline.CheckIn {

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Constructors

        public MainWindow() {

            InitializeComponent();

        }


        #endregion

        #region Methods

        private void OnClickButtonCheckIn(object sender, RoutedEventArgs e) {

            CheckInWindow checkInWindow = new CheckInWindow();

            checkInWindow.ShowDialog();

        }

        private void OnClickButtonPil(object sender, RoutedEventArgs e) {

            PassengerInformationListWindow pilWindow = new PassengerInformationListWindow();

            pilWindow.ShowDialog();

        }

        private void OnClickButtonBaggageScan(object sender, RoutedEventArgs e) {

            BaggageScanWindow baggageScanWindow = new BaggageScanWindow();

            baggageScanWindow.ShowDialog();

        }

        #endregion


    }

}
