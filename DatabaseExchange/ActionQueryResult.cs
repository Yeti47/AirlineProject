using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExchange {

    /// <summary>
    /// Beschreibt das Ergebnis einer SQL-Abfrage, welche keine SELECT-Anweisung ist, z. B. DELETE oder UPDATE.
    /// </summary>
    public class ActionQueryResult : QueryResult {

        #region Fields

        private int _numberAffectedRows;

        #endregion

        #region Properties

        public override int NumberAffectedRows { get => _numberAffectedRows; }

        #endregion

        #region Constructors

        public ActionQueryResult(string query, int numberAffectedRows, string errorDetails = null) 
            : base(query, errorDetails) {

            _numberAffectedRows = numberAffectedRows;

        }

        #endregion

    }

}
