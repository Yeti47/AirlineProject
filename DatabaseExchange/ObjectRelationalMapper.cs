using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

namespace DatabaseExchange {

    /// <summary>
    /// Stellt eine Methode zum Erzeugen eines Objektes dar, welches aus einer Datenbank gelesen werden kann.
    /// Dient der Initialisierung des Objekts mittels Zuweisung der ausgelesenen Tabellen-Attribute.
    /// </summary>
    /// <typeparam name="T">Der Typ des zu erzeugenden Objekts.</typeparam>
    /// <param name="attributes">Eine Map, welche den Zugriff auf die Attributwerte mittels des jeweiligen Attribut-Bezeichners ermöglicht.</param>
    /// <returns>Das erzeugte und initialisierte Objekt.</returns>
    public delegate T DatabaseObjectInitializer<out T>(IReadOnlyDictionary<string, object> attributes);

    /// <summary>
    /// Stellt generische Methoden zum Lesen und Schreiben von Datensätzen zur Verfügung.
    /// </summary>
    /// <typeparam name="T">Der Typ der Klasse, welche die zu lesenden bzw. zu schreibenden Datensätze repräsentiert.</typeparam>
    public class ObjectRelationalMapper<T> {

        #region Fields

        #endregion

        #region Properties

        public string ConnectionString { get; set; }

        /// <summary>
        /// Die Quell-Tabelle, aus welcher die Datensätze gelesen werden sollen.
        /// </summary>
        public DatabaseTable SourceTable { get; set; }

        /// <summary>
        /// Die Ziel-Tabelle, in welche Datensätze geschrieben werden sollen.
        /// </summary>
        public DatabaseTable TargetTable { get; set; }
        
        /// <summary>
        /// Legt fest, ob beim Ausführen von SQL-Befehlen aufgetretene Exceptions 
        /// ausgeworfen (true) oder ignoriert (false) werden sollen.
        /// 
        /// </summary>
        public bool StrictErrorHandling { get; set; } = false;

        #endregion

        #region Constructors

        public ObjectRelationalMapper(string connectionString, DatabaseTable sourceTable = null, DatabaseTable targetTable = null) {
            
            ConnectionString = connectionString;
            SourceTable = sourceTable;
            TargetTable = targetTable;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Ruft alle Datensätze aus der zuvor angegebenen <see cref="SourceTable"/> ab. Wird eine Where-Klausel übergeben,
        /// so werden nur übereinstimmende Datensätze abgerufen. Aus den ausgelesenen Datensätzen werden Objekte des
        /// über den Typ-Parameter bestimmten Typs erzeugt und als Aufzählung zurückgegeben.
        /// </summary>
        /// <param name="initializer">Eine Methode zur Erzeugung und Initialisierung der Objekte.</param>
        /// <param name="whereClause">Eine optionale Where-Klausel. Wird null übergeben, so wird keine Where-Klausel verwendet. Kann auch
        /// Platzhalter in Form von @name enthalten.</param>
        /// <returns>Eine Instanz von <see cref="FetchResult{T}"/>, welche eine Aufzählung der eingeholten Objekte und/oder
        /// eine Beschreibung des aufgetretenen Fehlers enthält.</returns>
        public FetchResult<T> Fetch(DatabaseObjectInitializer<T> initializer, string whereClause = null, SqlParameter[] sqlParams = null) {

            if (SourceTable == null)
                throw new InvalidOperationException("Cannot perform fetch operation, since the SourceTable has not been defined.");

            if (SourceTable.AttributeCount <= 0)
                throw new InvalidOperationException("Cannot perform fetch operation due to the SourceTable not containing any attributes.");

            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer), "The initialization handler must not be null.");

            string errorDetails = null;

            string where = String.IsNullOrWhiteSpace(whereClause) ? "" : $" WHERE {whereClause} ";
            string sql = SourceTable.SelectClause + where;

            List<T> dbObjects = new List<T>();

            using(SqlConnection connection = new SqlConnection(ConnectionString)) {

                SqlCommand command = new SqlCommand(sql, connection);
                
                if (sqlParams != null)
                    command.Parameters.AddRange(sqlParams);

                try {

                    connection.Open();
                    
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read()) {

                        Dictionary<string, object> attributes = new Dictionary<string, object>();

                        foreach (string attrIdentifier in SourceTable.AttributeIdentifiers)
                            attributes.Add(attrIdentifier, reader[attrIdentifier]);

                        T dbObj = initializer(new ReadOnlyDictionary<string, object>(attributes));

                        dbObjects.Add(dbObj);

                    }

                }
                catch(IndexOutOfRangeException e) {

                    string errMsg = "Attempt to read a non-existent table attribute: " + e.Message;

                    if (StrictErrorHandling)
                        throw new Exception(errMsg);

                    errorDetails = errMsg;

                }
                catch (Exception e) {

                    if (StrictErrorHandling)
                        throw e;

                    errorDetails = e.Message;

                }

            }

            return new FetchResult<T>(dbObjects, sql, errorDetails);

        }

        public ActionQueryResult Insert(SqlParameter[] sqlParams) {

            if(TargetTable == null)
                throw new InvalidOperationException("Cannot perform insert operation, since the TargetTable has not been defined.");


            int numAffectedRows = 0;
            string errorDetails = null;

            string sql = $"INSERT INTO {TargetTable.Name} (";

            IEnumerable<string> attrNames = sqlParams.Select(p => p.ParameterName.TrimStart('@')).Intersect(TargetTable.AttributeIdentifiers, StringComparer.OrdinalIgnoreCase);

            string attrSql = string.Join(", ", attrNames);

            sql += attrSql + ") VALUES (@" + string.Join(", @", attrNames) + ")";

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {

                SqlCommand command = new SqlCommand(sql, connection);

                if (sqlParams != null)
                    command.Parameters.AddRange(sqlParams);

                try {

                    connection.Open();

                    numAffectedRows = command.ExecuteNonQuery();

                }
                catch (Exception e) {

                    if (StrictErrorHandling)
                        throw e;

                    errorDetails = e.Message;

                }

            }

            return new ActionQueryResult(sql, numAffectedRows, errorDetails);

        }

        public ActionQueryResult Update(SqlParameter[] sqlParams, string whereClause, SqlParameter[] whereParams = null) {

            if (TargetTable == null)
                throw new InvalidOperationException("Cannot perform update operation, since the TargetTable has not been defined.");

            int numAffectedRows = 0;
            string errorDetails = null;

            string sql = $"UPDATE {TargetTable.Name} SET";

            IEnumerable<string> attrNames = sqlParams.Select(p => p.ParameterName.TrimStart('@')).Intersect(TargetTable.AttributeIdentifiers, StringComparer.OrdinalIgnoreCase);

            foreach(string attrName in attrNames) {

                sql += $" {attrName}=@{attrName},";

            }

            sql = sql.TrimEnd(',');

            if(whereClause != null) {

                sql += " WHERE " + whereClause;

            }

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {

                SqlCommand command = new SqlCommand(sql, connection);

                if (sqlParams != null)
                    command.Parameters.AddRange(sqlParams);

                if (whereParams != null)
                    command.Parameters.AddRange(whereParams);

                try {

                    connection.Open();

                    numAffectedRows = command.ExecuteNonQuery();

                }
                catch (Exception e) {

                    if (StrictErrorHandling)
                        throw e;

                    errorDetails = e.Message;

                }

            }

            return new ActionQueryResult(sql, numAffectedRows, errorDetails);

        }

        public ActionQueryResult Delete(string whereClause, SqlParameter[] sqlParams = null) {

            if (TargetTable == null)
                throw new InvalidOperationException("Cannot perform delete operation, since the TargetTable has not been defined.");

            int numAffectedRows = 0;
            string errorDetails = null;

            string sql = $"DELETE FROM {TargetTable.Name}";

            if(whereClause != null) {

                sql += " WHERE " + whereClause;

            }

            using (SqlConnection connection = new SqlConnection(ConnectionString)) {

                SqlCommand command = new SqlCommand(sql, connection);

                if (sqlParams != null)
                    command.Parameters.AddRange(sqlParams);

                try {

                    connection.Open();

                    numAffectedRows = command.ExecuteNonQuery();

                }
                catch (Exception e) {

                    if (StrictErrorHandling)
                        throw e;

                    errorDetails = e.Message;

                }

            }

            return new ActionQueryResult(sql, numAffectedRows, errorDetails);

        }

        #endregion

    }

}
