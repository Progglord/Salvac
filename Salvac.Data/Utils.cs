using OpenTK;
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

namespace Salvac.Data
{
    public static class Utils
    {
        public static bool FloatingEqual(double a, double b, double epsilon)
        {
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a == b)
                return true;
            else if (a == 0 || b == 0 || diff < double.Epsilon)
                return diff < (epsilon * double.Epsilon);
            else
                return diff / (absA + absB) < epsilon;
        }

        public static bool FloatingEqual2D(Vector2d a, Vector2d b, double epsilon)
        {
            return Utils.FloatingEqual(a.X, b.X, epsilon) && Utils.FloatingEqual(a.Y, b.Y, epsilon);
        }
    }
}
