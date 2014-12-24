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

namespace Salvac.Sessions.Fsd.Messages
{
    public enum PilotRating
    {
        //Unknown = -1,  // We won't use them. So just throw an error if they occur.
        //Observer = 1,
        FS1 = 2,
        FS2 = 3,
        FS3 = 4,
        PP = 5,
        SPP = 6,
        CP = 7,
        ATP = 8,
        SFI = 9,
        CFI = 10,
        ShowAdministrator = 11,

        First = FS1,
        Last = ShowAdministrator
    }
}
