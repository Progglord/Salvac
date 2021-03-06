﻿// Salvac
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
using System.Collections.Generic;

namespace Salvac.Sessions.Fsd.Messages
{
    public sealed class DeletePlaneMessage : Message
    {
        public const string TYPE = "#DP";

        public long Unkown
        { get; set; }


        public DeletePlaneMessage(string source, long unkown = 0) :
            base(TYPE, source, null)
        {
            Unkown = unkown;
        }

        protected override IEnumerable<string> GetTokens()
        {
            yield return Source;
            yield return Unkown.ToString(); // return dummy for unkown integer field.
        }
    }
}
