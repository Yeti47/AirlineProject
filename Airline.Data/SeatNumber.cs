using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airline.Data {

    public struct SeatNumber : IEquatable<SeatNumber> {

        private readonly int _x;
        private readonly int _y;

        public int X => _x;
        public int Y => _y;

        public SeatNumber(int x, int y) {
            _x = x;
            _y = y;
        }

        public static bool operator ==(SeatNumber a, SeatNumber b) => a.Equals(b);

        public static bool operator !=(SeatNumber a, SeatNumber b) => !a.Equals(b);

        public override bool Equals(object obj) => (obj is SeatNumber) && ((SeatNumber)obj).Equals(this);

        public bool Equals(SeatNumber other) => _x == other._x && _y == other._y;

        public override int GetHashCode() {

            unchecked {

                int hash = 17;

                hash = hash * 23 + _x.GetHashCode();
                hash = hash * 23 + _y.GetHashCode();

                return hash;

            }

        }

        public override string ToString() => $"({X}|{Y})";

    }

}
