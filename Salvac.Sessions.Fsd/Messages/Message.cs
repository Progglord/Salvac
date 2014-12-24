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
using System.Collections.Generic;

namespace Salvac.Sessions.Fsd.Messages
{
    public abstract class Message
    {
        public static readonly string SEPERATOR = ":";
        public static readonly string END = "\r\n";


        public string Type
        { get; protected set; }

        public string Source
        { get; protected set; }

        public string Destination
        { get; protected set; }

        public bool IsBroadcast
        { get { return string.IsNullOrEmpty(this.Destination); } }

        
        public Message(string type, string source, string destination)
        {
            if (string.IsNullOrEmpty(type)) throw new ArgumentNullException("type");
            if (!IsValidFsdName(source)) throw new ArgumentException("source is no valid FSD name.", "source");
            if (!string.IsNullOrEmpty(destination) && !IsValidFsdName(destination)) throw new ArgumentException("destination is no valid FSD name.", "source");

            this.Type = type;
            this.Source = source;
            this.Destination = destination;
        }

        public string Decompose()
        {
            return string.Concat(this.Type, String.Join(SEPERATOR, this.GetTokens()), END);
        }

        protected abstract IEnumerable<string> GetTokens();


        protected bool IsValidFsdName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.All(c => char.IsLetterOrDigit(c) || c == '_') && char.IsLetter(name[0]);
        }
    }
}
