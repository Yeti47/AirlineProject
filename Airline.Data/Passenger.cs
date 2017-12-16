using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    public class Passenger {

        #region Fields

        private List<Baggage> _baggage = new List<Baggage>();

        #endregion

        #region Properties

        public int Id { get; private set; }

        public string PassportId { get; set; }

        public Title Title { get; set; }
        public string TitleString => Title.ToString();

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public IEnumerable<Baggage> Baggage => _baggage;

        #endregion

        #region Constructors

        public Passenger(Title title, string firstName, string lastName) {

            Title = title;
            FirstName = firstName;
            LastName = lastName;
            
        }

        #endregion

        #region Methods

        public string GetFullName(bool includeTitle = true) => (includeTitle ? TitleString + " " : "") + $"{FirstName} {LastName}";

        public override string ToString() => GetFullName();

        #endregion

    }

}
