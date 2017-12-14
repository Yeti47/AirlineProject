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

namespace Airline.CheckIn {

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        #region Constructors

        public MainWindow() {

            InitializeComponent();

        }


        #endregion

        #region Methods

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {

            DatabaseTable dbTable = new DatabaseTable("airports");
            dbTable.AddAttribute("Id");
            dbTable.AddAttribute("Country");
            dbTable.AddAttribute("City");

            ObjectRelationalMapper<Airport> mapper = new ObjectRelationalMapper<Airport>(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\alexa\Documents\Visual Studio 2017\Projects\AirlineProject\Airline.Data\Databases\airline_db.mdf';Integrated Security=True", dbTable);

            IEnumerable<Airport> airports = mapper.Fetch(attr => new Airport(attr["Country"].ToString(), attr["City"].ToString()), out string errorDetails);

            if (airports == null)
                MessageBox.Show(errorDetails, "Fehler");
            else {

                string msg = airports.Select(a => a.ToString()).Aggregate((left, right) => left + "\n" + right);

                MessageBox.Show("Airports:\n" + msg, "Success!");
                
            }


        }

        #endregion

    }

}
