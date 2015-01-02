// Salvac
// Copyright (C) 2014 Oliver Schmidt
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;

namespace Salvac
{
    public sealed class BufferedAverageDouble
    {
        private double[] _buffer;
        private int _nextIndex;

        public int BufferLength
        { get { return _buffer.Length; } }

        public double Average
        { get; private set; }


        public BufferedAverageDouble(int bufferLength)
        {
            if (bufferLength <= 1) throw new ArgumentOutOfRangeException("bufferLength");

            _buffer = new double[bufferLength];
            _nextIndex = 0;

            this.Average = 0d;
        }

        public void AddValue(double value)
        {
            double prev = _buffer[_nextIndex];
            _buffer[_nextIndex] = value;

            this.Average -= prev / (double)this.BufferLength;
            this.Average += value / (double)this.BufferLength;

            _nextIndex++;
            _nextIndex = _nextIndex % _buffer.Length;
        }
    }
}
