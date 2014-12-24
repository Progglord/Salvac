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
using System.Collections.Generic;

namespace Salvac.Sessions.Fsd.Messages
{
    public enum WeatherDataRequestType
    {
        Metar = 0,
        Taf = 1,
        ShortTaf = 2,

        First = Metar,
        Last = ShortTaf
    }

    public sealed class WeatherDataMessage : Message
    {
        public const string TYPE = "&D";

        public WeatherDataRequestType RequestType
        { get; private set; }

        public string Data
        { get; private set; }

        
        public WeatherDataMessage(string source, string destination, WeatherDataRequestType requestType, string data) :
            base(TYPE, source, destination)
        {
            if (string.IsNullOrEmpty(destination)) throw new ArgumentNullException("destination");
            if (requestType < WeatherDataRequestType.First || requestType > WeatherDataRequestType.Last) throw new ArgumentException("requestType is invalid.", "requestType");
            if (string.IsNullOrEmpty(data)) throw new ArgumentNullException("data");

            this.RequestType = requestType;
            this.Data = data;
        }


        protected override IEnumerable<string> GetTokens()
        {
            yield return this.Source;
            yield return this.Destination;
            yield return ((int)this.RequestType).ToString();
            yield return this.Data;
        }
    }
}
