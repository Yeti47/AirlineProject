using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExchange {

    /// <summary>
    /// Beschreibt eine Datenbank-Tabelle, welche mittels Join-Operationen zusammengefügt werden kann.
    /// </summary>
    public class JoinableDatabaseTable : DatabaseTable {

        public List<Join> Joins { get; private set; }

        public override string SelectClause => base.SelectClause + JoinClause;

        public string JoinClause {

            get {

                string sql = "";

                foreach (Join join in Joins)
                    sql += " " + join.ToString();

                return sql;

            }

        }

        public override string FullPath => Name + JoinClause;

        public JoinableDatabaseTable(string name, IEnumerable<Join> joins = null) : base(name) {

            Joins = joins == null ? new List<Join>() : new List<Join>(joins);

        }

        public Join CreateJoin(string targetTable, string sourceColumn, string targetColumn, string targetTableAlias = null, JoinTypes type = JoinTypes.Inner, bool isEqui = true) {

            if (targetTable == null)
                throw new ArgumentNullException(nameof(targetTable));

            if (sourceColumn == null)
                throw new ArgumentNullException(nameof(sourceColumn));

            if (targetColumn == null)
                throw new ArgumentNullException(nameof(targetColumn));

            Join join = new Join(targetTable, sourceColumn, targetColumn, targetTableAlias, type, true);
            Joins.Add(join);

            return join;

        }


    }

}
