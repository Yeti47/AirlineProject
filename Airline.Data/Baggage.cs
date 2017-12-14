using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    public class Baggage {

        #region Constants

        /// <summary>
        /// Das größte zulässige Gewicht in Kilogramm (kg).
        /// </summary>
        public const float WEIGHT_LIMIT = 25f;

        #endregion

        #region Fields

        #endregion

        #region Properties

        public int Id { get; private set; }

        public int FlightId { get; set; }

        public float Weight { get; set; }

        public bool IsOverweight => Weight > WEIGHT_LIMIT;

        #endregion

        #region Constructors

        public Baggage() {

        }

        #endregion

        #region Methods



        #endregion

    }

}
