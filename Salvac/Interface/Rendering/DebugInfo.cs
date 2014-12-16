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

#if DEBUG

using System;
using System.Diagnostics;

namespace Salvac.Interface.Rendering
{

    public static class DebugInfo
    {
        public static int DrawnSectorBackgrounds
        { get; set; }

        public static int DrawnSectorBoundaries
        { get; set; }

        public static bool DrawBoundingBoxes
        { get; set; }

        public static double LastFrameTime
        { get; set; }
    }

}

#endif