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
using Antlr4.Runtime;

namespace Salvac.Sessions.Fsd.Messages
{
    partial class FsdLexer
    {
        public override void Recover(LexerNoViableAltException e)
        {
            string message = string.Format("Unrecognized token at '{0}'.", (char)e.InputStream.La(1));
            throw new InvalidMessageException(message, e);
        }

        public override void Recover(RecognitionException e)
        {
            string message = string.Format("Unrecognized token at '{0}'.", (char)e.InputStream.La(1));
            throw new InvalidMessageException(message, e);
        }
    }
}
