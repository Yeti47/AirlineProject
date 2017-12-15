using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    /// <summary>
    /// Eine statische Klasse, welche der allgemeinen Konfiguration dient.
    /// </summary>
    public static class Config {

        public const string DB_CONNECTION_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\Databases\airline_db.mdf';Integrated Security=True";

        public const string AIRLINE_NAME = "Something Airlines";

        public const string AIRLINE_SLOGAN = "Wird schon schiefgehen!";


    }

}
