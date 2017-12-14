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

    public class ObjectRelationalMapper<T> {

        #region Fields

        #endregion

        #region Properties

        public string ConnectionString { get; set; }

        public DatabaseTable SourceTable { get; set; }

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
        /// <param name="whereClause">Eine optionale Where-Klausel. Wird null übergeben, so wird keine Where-Klausel verwendet.</param>
        /// <param name="errorDetails">Ein Ausgabe-Parameter, welcher im Fehlerfall eine Beschreibung des Fehlers enthält, sofern 
        /// <see cref="StrictErrorHandling"/> nicht aktiviert wurde.</param>
        /// <returns>Eine Aufzählung von Objekten, welche mit den ausgelesenen Werten initialisiert wurden.</returns>
        public IEnumerable<T> Fetch(DatabaseObjectInitializer<T> initializer, string whereClause = null, out string errorDetails) {

            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer), "The initialization handler must not be null.");

            errorDetails = string.Empty;

            string where = String.IsNullOrWhiteSpace(whereClause) ? "" : $" WHERE {whereClause} ";

            List<T> dbObjects = new List<T>();

            using(SqlConnection connection = new SqlConnection(ConnectionString)) {

                SqlCommand command = new SqlCommand(SourceTable.SelectClause);

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
                catch (Exception e) {

                    if (StrictErrorHandling)
                        throw e;

                    errorDetails = e.Message;
                    return null;

                }

            }

            return dbObjects;

        }

        /// <summary>
        /// Ruft alle Datensätze aus der zuvor angegebenen <see cref="SourceTable"/> ab. Wird eine Where-Klausel übergeben,
        /// so werden nur übereinstimmende Datensätze abgerufen. Aus den ausgelesenen Datensätzen werden Objekte des
        /// über den Typ-Parameter bestimmten Typs erzeugt und als Aufzählung zurückgegeben.
        /// </summary>
        /// <param name="initializer">Eine Methode zur Erzeugung und Initialisierung der Objekte.</param>
        /// <param name="whereClause">Eine optionale Where-Klausel. Wird null übergeben, so wird keine Where-Klausel verwendet.</param>
        /// <returns>Eine Aufzählung von Objekten, welche mit den ausgelesenen Werten initialisiert wurden.</returns>
        public IEnumerable<T> Fetch(DatabaseObjectInitializer<T> initializer, string whereClause = null) {

            return Fetch(initializer, whereClause, out string temp);

        }

        #endregion

    }

}
