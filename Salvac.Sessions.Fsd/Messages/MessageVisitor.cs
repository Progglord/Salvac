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
using Salvac.Data.Types;
using OpenTK;

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

        public override object VisitPlanePosition(FsdParser.PlanePositionContext context)
        {
            SquawkMode sqkMode = default(SquawkMode);
            switch (context.sqkMode.Text.ToUpper())
            {
                case "N": sqkMode = SquawkMode.Charlie; break;
                case "S": sqkMode = SquawkMode.Standby; break;
                case "Y": sqkMode = SquawkMode.Ident; break;
                default: throw new InvalidMessageException(string.Format("Invalid squawk mode token: \"{0}\"", context.sqkMode.Text));
            }
            var squawk = Squawk.Parse(context.squawk.Text);
            var rating = int.Parse(context.rating.Start.Text);
            var pbh = uint.Parse(context.pbh.Start.Text);

            var pressAltDiff = Distance.FromFeet(int.Parse(context.pressAltDiff.Start.Text));
            var position = new PlanePosition();
            position.Position = new Vector2d(double.Parse(context.lon.Start.Text, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(context.lat.Start.Text, CultureInfo.InvariantCulture.NumberFormat));
            position.GroundSpeed = Speed.FromKnots(int.Parse(context.speed.Start.Text));
            position.Elevation = Distance.FromFeet(int.Parse(context.elevation.Start.Text));
            position.PressureAltitude = position.Elevation + pressAltDiff;
            position.OnGround = (pbh & 0x2) > 0;
            position.TrueHeading = Angle.FromDegrees((double)((pbh >> 2) & 0x3FF) * 360d / 1024d);

            this.Messages.Add(new PlanePositionMessage(context.source.Text, sqkMode, squawk, rating, position, pbh));
            return null;
        }

        public override object VisitWeatherData(FsdParser.WeatherDataContext context)
        {
            WeatherDataRequestType requestType = (WeatherDataRequestType)int.Parse(context.request.Start.Text);
            this.Messages.Add(new WeatherDataMessage(context.source.Text, context.destination.Text, requestType, context.data.Start.Text));
            return null;
        }

        public override object VisitDeletePlane(FsdParser.DeletePlaneContext context)
        {
            long unkown = 0;
            if (context.unkown != null) unkown = long.Parse(context.unkown.Start.Text);

            this.Messages.Add(new DeletePlaneMessage(context.source.Text, unkown));
            return null;
        }

        public override object VisitDeleteAtc(FsdParser.DeleteAtcContext context)
        {
            long unkown = 0;
            if (context.unkown != null) unkown = long.Parse(context.unkown.Start.Text);

            this.Messages.Add(new DeleteAtcMessage(context.source.Text, unkown));
            return null;
        }
    }
}
