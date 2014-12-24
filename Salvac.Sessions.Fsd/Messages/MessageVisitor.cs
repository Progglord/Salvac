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
using System.Globalization;
using DotSpatial.Topology;
using Salvac.Data.Types;

namespace Salvac.Sessions.Fsd.Messages
{
    public sealed class MessageVisitor : FsdParserBaseVisitor<object>
    {
        public IList<Message> Messages
        { get; private set; }

        public MessageVisitor()
        {
            this.Messages = new List<Message>();
        }


        public override object Visit(Antlr4.Runtime.Tree.IParseTree tree)
        {
            this.Messages.Clear();
            return base.Visit(tree);
        }

        public override object VisitPilotPosition(FsdParser.PilotPositionContext context)
        {
            SquawkMode sqkMode = default(SquawkMode);
            switch (context.sqkMode.Text.ToUpper())
            {
                case "N": sqkMode = SquawkMode.Charlie; break;
                case "S": sqkMode = SquawkMode.Standby; break;
                case "Y": sqkMode = SquawkMode.Ident; break;
                default: throw new InvalidMessageException(string.Format("Invalid squawk mode token: \"{0}\"", context.sqkMode.Text));
            }
            Squawk squawk = default(Squawk);
            try { squawk = Squawk.Parse(context.squawk.Text); }
            catch (ArgumentException ex) { throw new InvalidMessageException(string.Format("Invalid squawk token: \"{0}\"", context.squawk.Text), ex); }
            PilotRating rating = (PilotRating)int.Parse(context.rating.Start.Text);
            Coordinate position = new Coordinate(double.Parse(context.lon.Start.Text, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(context.lat.Start.Text, CultureInfo.InvariantCulture.NumberFormat));
            Distance trueAltitude = Distance.FromFeet(int.Parse(context.altitude.Start.Text));
            Speed groundSpeed = Speed.FromKnots(int.Parse(context.speed.Start.Text));
            uint pbh = uint.Parse(context.pbh.Start.Text);
            int altitudeDiff = int.Parse(context.altitudeDiff.Start.Text);

            this.Messages.Add(new PilotPositionMessage(context.source.Text, sqkMode, squawk, rating, position, trueAltitude, 
                groundSpeed, pbh, altitudeDiff));
            return null;
        }

        public override object VisitWeatherData(FsdParser.WeatherDataContext context)
        {
            WeatherDataRequestType requestType = (WeatherDataRequestType)int.Parse(context.request.Start.Text);
            this.Messages.Add(new WeatherDataMessage(context.source.Text, context.destination.Text, requestType, context.data.Start.Text));
            return null;
        }

        public override object VisitDeletePilot(FsdParser.DeletePilotContext context)
        {
            this.Messages.Add(new DeletePilotMessage(context.source.Text));
            return null;
        }

        public override object VisitDeleteAtc(FsdParser.DeleteAtcContext context)
        {
            this.Messages.Add(new DeleteAtcMessage(context.source.Text));
            return null;
        }
    }
}
