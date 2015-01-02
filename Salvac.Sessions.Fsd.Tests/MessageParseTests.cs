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

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DotSpatial.Topology;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Salvac.Data.Types;
using Salvac.Sessions.Fsd.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salvac.Sessions.Fsd.Tests
{
    [TestClass]
    public class MessageParseTests
    {
        [TestCategory("FSD Parser")]
        [TestMethod]
        public void PiecewiseTest()
        {
            if (_parser == null) _parser = new MessageParser();

            Message message = _parser.Parse("&DA:B:0:C\r\n&DD:E:1:");
            Assert.IsNotNull(message);
            Assert.AreEqual("&D", message.Type);

            message = _parser.Parse("&DA:B:0:C\r\n&DD:E:1:F");
            Assert.IsNotNull(message);
            Assert.AreEqual("&D", message.Type);
        }


        [TestCategory("FSD Parser")]
        [TestMethod]
        public void PilotPositionTest()
        {
            PilotPositionMessage msg = this.ParseOne("@N:TEST123:1200:7:52.75:-8:20000:400:1073741826:100\r\n") as PilotPositionMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual("@", msg.Type);
            Assert.AreEqual(SquawkMode.Charlie, msg.SquawkMode);
            Assert.AreEqual("TEST123", msg.Source);
            Assert.AreEqual(new Squawk(0x280), msg.Squawk);
            Assert.AreEqual(7, msg.Rating);
            Assert.AreEqual(new Coordinate(-8d, 52.75d), msg.Position);
            Assert.AreEqual(Distance.FromFeet(20000d), msg.TrueAltitude);
            Assert.AreEqual(Speed.FromKnots(400d), msg.GroundSpeed);
            Assert.AreEqual(1073741826u, msg.PitchBankHeading);
            Assert.AreEqual(true, msg.OnGround);
            Assert.AreEqual(100, msg.AltitudeDifference);

            msg = this.ParseOne("@S:TEST123:7700:7:0052:-0008:5564:2450:1077591040:-45\r\n") as PilotPositionMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual(SquawkMode.Standby, msg.SquawkMode);
            Assert.AreEqual(Squawk.Emergency, msg.Squawk);
            Assert.AreEqual(new Coordinate(-8d, 52d), msg.Position);
            Assert.AreEqual(Distance.FromFeet(5564d), msg.TrueAltitude);
            Assert.AreEqual(Speed.FromKnots(2450d), msg.GroundSpeed);
            Assert.AreEqual(1077591040u, msg.PitchBankHeading);
            Assert.AreEqual(false, msg.OnGround);
            Assert.AreEqual(-45, msg.AltitudeDifference);

            msg = this.ParseOne("@Y:TEST123:7700:7:0052:-0008:5564:2450:1077591040:-45\r\n") as PilotPositionMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual(SquawkMode.Ident, msg.SquawkMode);

            msg = this.ParseOne("@Y:TEST123:741:7:0052:-0008:5564:2450:1077591040:-45\r\n") as PilotPositionMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual(new Squawk(0x1E1), msg.Squawk);

            // Check exceptions for missing data.
            try { this.ParseOne("@:A:7000:2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize missing squawk mode."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N::7000:2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize missing source."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A::2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize missing squawk."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:7000::0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize missing rating."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:7000:2::0:0:0:0:0\r\n"); Assert.Fail("Did not recognize missing latitude."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:7000:2:0::0:0:0:0\r\n"); Assert.Fail("Did not recognize missing longitude."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:7000:2:0:0::0:0:0\r\n"); Assert.Fail("Did not recognize missing true altitude."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:7000:2:0:0:0::0:0\r\n"); Assert.Fail("Did not recognize missing ground speed."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:7000:2:0:0:0:0::0\r\n"); Assert.Fail("Did not recognize missing pitchbankheading."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:7000:2:0:0:0:0:0:\r\n"); Assert.Fail("Did not recognize missing altitude difference."); }
            catch (AssertFailedException) { throw; }
            catch { }

            // Check exceptions for wrong data
            try { this.ParseOne("@B:A:7000:2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize wrong squawk mode."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@5:A:7000:2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize wrong squawk mode."); }
            catch (AssertFailedException) { throw; }
            catch { }

            try { this.ParseOne("@N:4D:7000:2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize wrong source."); }
            catch (AssertFailedException) { throw; }
            catch { }

            try { this.ParseOne("@N:A:7800:2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize wrong squawk."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:-0000:2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize wrong squawk."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("@N:A:77775:2:0:0:0:0:0:0\r\n"); Assert.Fail("Did not recognize wrong squawk."); }
            catch (AssertFailedException) { throw; }
            catch { }
        }

        [TestCategory("FSD Parser")]
        [TestMethod]
        public void WeatherDataTest()
        {
            WeatherDataMessage msg = this.ParseOne("&DAAA:BBB:0:DATA\r\n") as WeatherDataMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual("&D", msg.Type);
            Assert.AreEqual("AAA", msg.Source);
            Assert.AreEqual("BBB", msg.Destination);
            Assert.AreEqual(WeatherDataRequestType.Metar, msg.RequestType);
            Assert.AreEqual("DATA", msg.Data);

            msg = this.ParseOne("&DA:B:1:DATA AND MORE DATA /-\r\n") as WeatherDataMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual(WeatherDataRequestType.Taf, msg.RequestType);
            Assert.AreEqual("DATA AND MORE DATA /-", msg.Data);

            msg = this.ParseOne("&DA:B:0:12345\r\n") as WeatherDataMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual("12345", msg.Data);

            msg = this.ParseOne("&DA:B:0:-12.345\r\n") as WeatherDataMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual("-12.345", msg.Data);

            // Check exceptions for missing data.
            try { this.ParseOne("&D:5T:0:D\r\n"); Assert.Fail("Did not recognize missing source."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("&DA::0:D\r\n"); Assert.Fail("Did not recognize missing destination."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("&DA:B::D\r\n"); Assert.Fail("Did not recognize missing request type."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("&DA:B:0:\r\n"); Assert.Fail("Did not recognize missing data."); }
            catch (AssertFailedException) { throw; }
            catch { }

            // Check exceptions for wrong data.
            try { this.ParseOne("&DA:B:4:D\r\n"); Assert.Fail("Did not recognize wrong request type."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("&DA:B:0.1:D\r\n"); Assert.Fail("Did not recognize wrong request type."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("&D5A:B:0:D\r\n"); Assert.Fail("Did not recognize wrong FSD name."); }
            catch (AssertFailedException) { throw; }
            catch { }
            try { this.ParseOne("&DA:5T:0:D\r\n"); Assert.Fail("Did not recognize wrong FSD name."); }
            catch (AssertFailedException) { throw; }
            catch { }
        }

        [TestCategory("FSD Parser")]
        [TestMethod]
        public void DeletePilotTest()
        {
            DeletePilotMessage msg = this.ParseOne("#DPTEST123:45789\r\n") as DeletePilotMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual("#DP", msg.Type);
            Assert.AreEqual("TEST123", msg.Source);
            Assert.IsTrue(msg.IsBroadcast);

            msg = this.ParseOne("#DPTEST123") as DeletePilotMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual("TEST123", msg.Source);
            Assert.IsTrue(msg.IsBroadcast);

            try { this.ParseOne("#DP:0"); Assert.Fail("Did no recognize missing source."); }
            catch (AssertFailedException) { throw; }
            catch (Exception) { }
            try { this.ParseOne("#DP4D:0"); Assert.Fail("Did no recognize wrong source."); }
            catch (AssertFailedException) { throw; }
            catch (Exception) { }
            try { this.ParseOne("#DPTEST:"); Assert.Fail("Did no recognize missing unkown token."); }
            catch (AssertFailedException) { throw; }
            catch (Exception) { }
        }

        [TestCategory("FSD Parser")]
        [TestMethod]
        public void DeleteAtcTest()
        {
            DeleteAtcMessage msg = this.ParseOne("#DATEST123:45789\r\n") as DeleteAtcMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual("#DA", msg.Type);
            Assert.AreEqual("TEST123", msg.Source);
            Assert.IsTrue(msg.IsBroadcast);

            msg = this.ParseOne("#DATEST123") as DeleteAtcMessage;
            Assert.IsNotNull(msg);
            Assert.AreEqual("TEST123", msg.Source);
            Assert.IsTrue(msg.IsBroadcast);

            try { this.ParseOne("#DA:0"); Assert.Fail("Did no recognize missing source."); }
            catch (AssertFailedException) { throw; }
            catch (Exception) { }
            try { this.ParseOne("#DA4D:0"); Assert.Fail("Did no recognize wrong source."); }
            catch (AssertFailedException) { throw; }
            catch (Exception) { }
            try { this.ParseOne("#DATEST:"); Assert.Fail("Did no recognize missing unkown token."); }
            catch (AssertFailedException) { throw; }
            catch (Exception) { }
        }


        private MessageParser _parser;

        private Message ParseOne(string input)
        {
            if (_parser == null) _parser = new MessageParser();

            Message result = _parser.Parse(input);
            Assert.IsNotNull(result);
            return result;
        }
    }
}
