using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    public class Baggage : INotifyPropertyChanged {


        #region Fields

        private decimal _weight;
        private int _id;
        private int _flightId;

        private decimal _weightLimit;
        private decimal _feePerExtraKilogram;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public int Id {
            get => _id;
            private set { _id = value; OnPropertyChanged(); }
        }

        public int FlightId {
            get => _flightId;
            set { _flightId = value; OnPropertyChanged(); }
        }

        public decimal Weight {
            get => _weight;
            set {
                _weight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Fee));
            }
        }

        public decimal WeightLimit {
            get => _weightLimit;
            set {
                _weightLimit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Fee));
            }

        }

        public decimal FeePerExtraKilogram {
            get => _feePerExtraKilogram;
            set {
                _feePerExtraKilogram = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Fee));
            }

        }

        public decimal Fee => Math.Max(Math.Ceiling(_weight - _weightLimit) * _feePerExtraKilogram, 0);

        #endregion

        #region Constructors

        public Baggage() {

        }

        #endregion

        #region Methods

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {

            PropertyChangedEventHandler handler = PropertyChanged;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        public void PrintLabel() => throw new NotImplementedException("Cannot print at the current state of development.");

        #endregion

    }

}
