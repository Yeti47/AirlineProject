using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExchange {

    public enum JoinTypes { Inner, Left, Right }

    /// <summary>
    /// Beschreibt eine SQL-Join-Operation zwischen zwei Datenbank-Tabellen.
    /// </summary>
    public class Join {

        #region Fields

        private string _targetTable;
        private string _sourceColumn;
        private string _targetColumn;

        #endregion

        #region Properties

        public JoinTypes JoinType { get; set; }

        public bool IsEqui { get; set; } = true;

        public string TargetTable {

            get => _targetTable;

            set {

                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("The target table must not be empty.");

                _targetTable = value;

            }

        }

        public string SourceColumn {

            get => _sourceColumn;

            set {

                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("The source column must not be empty.");

                _sourceColumn = value;

            }

        }

        public string TargetColumn {

            get => _targetColumn;

            set {

                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("The target column must not be empty.");

                _targetColumn = value;

            }

        }

        internal string ComparisonOperator => IsEqui ? "=" : "<>";

        #endregion

        #region Constructors

        public Join(string targetTable, string sourceColumn, string targetColumn, JoinTypes type = JoinTypes.Inner, bool isEqui = true) {

            _targetTable = targetTable;
            _sourceColumn = sourceColumn;
            _targetColumn = targetColumn;

            JoinType = type;
            IsEqui = isEqui;

        }

        #endregion

        #region Methods

        public override string ToString() {

            return $"{JoinType.ToString().ToUpper()} JOIN {_targetTable} ON " +
                $"{_sourceColumn} {ComparisonOperator} {_targetColumn}";

        }

        #endregion

    }

}
