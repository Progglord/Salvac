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
using Antlr4.Runtime;

namespace Salvac.Sessions.Fsd.Messages
{
    public sealed class MessageErrorStrategy : DefaultErrorStrategy
    {
        public override void Recover(Parser recognizer, RecognitionException e)
        {
            string message = string.Format("Unexpected token '{0}'. Expected tokens: {1}.", e.OffendingToken.ToString(), String.Join(", ", e.GetExpectedTokens().ToArray().Select(i => recognizer.TokenNames[i])));
            throw new InvalidMessageException(message, e);
        }

        public override IToken RecoverInline(Parser recognizer)
        {
            InputMismatchException e = new InputMismatchException(recognizer);
            string message = string.Format("Unexpected token '{0}'. Expected tokens: {1}.", e.OffendingToken.ToString(), "{ " + String.Join(", ", e.GetExpectedTokens().ToArray().Select(i => recognizer.TokenNames[i])) + " }");
            throw new InvalidMessageException(message, e);
        }

        public override void Sync(Parser recognizer)
        { }
    }
}
