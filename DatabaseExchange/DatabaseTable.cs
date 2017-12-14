using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExchange {

    /// <summary>
    /// Beschreibt eine Datenbank-Tabelle.
    /// </summary>
    public class DatabaseTable {

        #region Fields

        protected Dictionary<string, string> _attributes = new Dictionary<string, string>();

        #endregion

        #region Properties

        public string Name { get; set; }

        public IEnumerable<string> AttributeNames => _attributes.Keys;

        public IEnumerable<string> AttributeAliases => _attributes.Values;

        /// <summary>
        /// Eine Aufzählung aller effektiven, für ein SELECT-Statement verwendeten 
        /// Attribut-Bezeichner. Dabei handelt es sich entweder um den tatsächlichen Namen
        /// des Attributs oder um dessen Alias, falls vorhanden.
        /// </summary>
        public IEnumerable<string> AttributeIdentifiers {

            get {

                IEnumerable<string> attrWithoutAliases = _attributes.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key);
                IEnumerable<string> attrAliases = _attributes.Values.Where(v => v != null);

                return attrWithoutAliases.Union(attrAliases);

            }

        }

        public virtual string SelectClause {

            get {

                string sql = "SELECT ";

                foreach (KeyValuePair<string, string> kvp in _attributes) {

                    sql += kvp.Key;

                    if (kvp.Value != null)
                        sql += $" AS {kvp.Value} ";

                }

                sql += $"FROM {Name}";

                return sql;


            }

        }

        public int AttributesCount => _attributes.Count;

        #endregion

        #region Constructors

        public DatabaseTable(string name) {

            Name = name;

        }

        #endregion

        #region Methods

        public virtual bool AddAttribute(string name, string alias = null) {

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The attribute name must not be empty or null.");

            if (name == alias)
                throw new ArgumentException("The attribute's name and its alias must not be the same.");

            if(alias != null && alias.Trim().Length <= 0)
                throw new ArgumentException("The alias must not be empty. To use no alias, pass null.");

            if (_attributes.ContainsKey(name))
                return false;

            if (_attributes.Values.Any(v => v == alias))
                return false;

            _attributes.Add(name, alias);

            return true;

        }

        public virtual bool RemoveAttribute(string name) => _attributes.Remove(name);

        #endregion



    }

}
