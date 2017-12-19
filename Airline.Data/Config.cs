using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseExchange;

namespace Airline.Data {

    /// <summary>
    /// Eine statische Klasse, welche der allgemeinen Konfiguration dient.
    /// </summary>
    public static class Config {

        #region Constants

        public const string DB_CONNECTION_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\Databases\airline_db.mdf';Integrated Security=True";

        public const string AIRLINE_NAME = "CyanAir";

        public const string AIRLINE_SLOGAN = "Pretty fly for a cyan guy!";

        #endregion

    }

}
