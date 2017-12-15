using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExchange {

    /// <summary>
    /// Beschreibt das Ergebnis einer SQL-Fetch-Abfrage.
    /// </summary>
    /// <typeparam name="T">Der Typ der aus der Datenbank eingeholten Objekte.</typeparam>
    public class FetchResult<T> : QueryResult {

        #region Properties

        public IEnumerable<T> RetrievedItems { get; private set; }

        public override int NumberAffectedRows => RetrievedItems?.Count() ?? 0;

        #endregion

        #region Constructors

        public FetchResult(IEnumerable<T> retrievedItems, string query, string errorDetails = null) 
            : base(query, errorDetails) {

            RetrievedItems = retrievedItems;

        }

        #endregion

    }

}
