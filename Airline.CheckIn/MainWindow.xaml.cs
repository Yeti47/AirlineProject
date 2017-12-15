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

            ObjectRelationalMapper<Airport> mapper = new ObjectRelationalMapper<Airport>(Config.DB_CONNECTION_STRING, dbTable, dbTable);


            ActionQueryResult actionQueryResult = mapper.Delete("City=@City", new SqlParameter[] {
                new SqlParameter("@City", "Kabul")
            });

            if (actionQueryResult.HasError) {

                MessageBox.Show(actionQueryResult.ToString(), "Fehler!");

            }
            else
                MessageBox.Show("Delete success! " + actionQueryResult.ToString());


            FetchResult<Airport> airportFetchResult = mapper.Fetch(attr => new Airport(attr["Country"].ToString(), attr["City"].ToString()));
            
            if (airportFetchResult.HasError)
                MessageBox.Show(airportFetchResult.ErrorDetails, "Fehler");
            else {

                string msg = airportFetchResult.RetrievedItems.Select(a => a.ToString()).Aggregate((left, right) => left + "\n" + right);

                MessageBox.Show("Airports:\n" + msg, "Success!");
                
            }

            //ActionQueryResult actionQueryResult = mapper.Update(new SqlParameter[] {
            //    new SqlParameter("@City", "Kabul")
            //}, "City='Test'");

            //if (actionQueryResult.HasError) {

            //    MessageBox.Show(actionQueryResult.ToString(), "Fehler!");

            //}
            //else
            //    MessageBox.Show("Update success! " + actionQueryResult.ToString());


            //ActionQueryResult actionQueryResult = mapper.Insert(new SqlParameter[] {
            //    new SqlParameter("@Country", "Schweiz"), new SqlParameter("@City", "Bern")
            //});

            //if(actionQueryResult.HasError) {

            //    MessageBox.Show(actionQueryResult.ToString(), "Fehler!");

            //}
            //else {

            //    MessageBox.Show("Insert successfull!");

            //}




        }

        #endregion

    }

}
