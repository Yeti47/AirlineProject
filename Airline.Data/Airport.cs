using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    public class Airport {

        #region Properties

        public string Country { get; set; }
        public string City { get; set; }

        #endregion

        #region Constructors

        public Airport(string country, string city) {

            Country = country;
            City = city;

        }

        #endregion

        #region Methods

        public override string ToString() => $"{City}, {Country}";

        #endregion


    }

}
