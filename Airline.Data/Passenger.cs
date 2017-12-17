using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Airline.Data {

    public class Passenger : INotifyPropertyChanged {

        #region Fields

        private ObservableCollection<Baggage> _baggage = new ObservableCollection<Baggage>();

        private int _id;
        private string _firstName;
        private string _lastName;
        private string _passportId;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public int Id {

            get => _id;
            private set { _id = value; OnPropertyChanged(); }

        }

        public string PassportId {

            get => _passportId;
            set { _passportId = value; OnPropertyChanged(); }

        }

        public Title Title { get; set; }
        public string TitleString => Title.ToString();

        public string FirstName {

            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }

        }

        public string LastName {

            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public IEnumerable<Baggage> Baggage => _baggage;

        public decimal TotalBaggageWeight => _baggage.Sum(b => b.Weight);

        public decimal TotalBaggageFee => _baggage.Sum(b => b.Fee);

        #endregion

        #region Constructors

        public Passenger(Title title, string firstName, string lastName, string passportId = null) {

            Title = title;
            FirstName = firstName;
            LastName = lastName;
            PassportId = passportId;

            _baggage.CollectionChanged += (sender, e) => {
                OnPropertyChanged(nameof(TotalBaggageWeight));
                OnPropertyChanged(nameof(TotalBaggageFee));
            };
            
        }

        #endregion

        #region Methods

        public string GetFullName(bool includeTitle = true) => (includeTitle ? TitleString + " " : "") + $"{FirstName} {LastName}";

        public override string ToString() => GetFullName();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {

            PropertyChangedEventHandler handler = PropertyChanged;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        public bool AddBaggage(Baggage baggage) {

            if (_baggage.Contains(baggage))
                return false;

            baggage.PropertyChanged += OnBaggagePropertyChanged;

            _baggage.Add(baggage);

            return true;

        }

        public bool AddBaggage(IEnumerable<Baggage> baggage) {

            bool result = true;

            foreach(Baggage bg in baggage) {

                if (!_baggage.Contains(bg)) {
                    bg.PropertyChanged += OnBaggagePropertyChanged;
                    _baggage.Add(bg);
                }
                else
                    result = false;
                
            }

            return result;

        }

        public bool RemoveBaggage(Baggage baggage) {

            bool success = _baggage.Remove(baggage);

            if (success)
                baggage.PropertyChanged -= OnBaggagePropertyChanged;

            return success;

        }

        public void ClearBaggage() {

            foreach (Baggage bg in _baggage)
                bg.PropertyChanged -= OnBaggagePropertyChanged;

            _baggage.Clear();

        }

        private void OnBaggagePropertyChanged(object sender, PropertyChangedEventArgs e) {

            Baggage baggage = sender as Baggage;

            if (baggage == null || e == null)
                return;

            if (e.PropertyName == nameof(baggage.Weight))
                OnPropertyChanged(nameof(TotalBaggageWeight));

            if(e.PropertyName == nameof(baggage.Fee))
                OnPropertyChanged(nameof(TotalBaggageFee));

        }


        #endregion

    }

}
