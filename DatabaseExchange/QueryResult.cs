using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExchange {

    /// <summary>
    /// Beschreibt das Ergebnis einer SQL-Abfrage.
    /// </summary>
    public abstract class QueryResult {
        
        #region Properties

        public abstract int NumberAffectedRows { get; }

        public virtual string ErrorDetails { get; protected set; }

        public bool HasError => ErrorDetails != null;

        public string Query { get; protected set; }

        #endregion

        #region Constructors

        public QueryResult(string query, string errorDetails = null) {

            Query = query;
            ErrorDetails = errorDetails;

        }

        #endregion

        #region Methods

        public override string ToString() {

            return
                $"Query: {Query}\n" +
                $"Rows Affected: {NumberAffectedRows}\n" +
                (HasError ? $"Error: {ErrorDetails}" : "");

        }

        #endregion

    }

}
