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
using System;
using System.Collections.Generic;

namespace Salvac.Sessions.Fsd.Messages
{
    public sealed class MessageParser
    {
        private FsdLexer _lexer;
        private FsdParser _parser;
        private MessageVisitor _visitor;


        public MessageParser()
        {
            _lexer = new FsdLexer(new AntlrInputStream(string.Empty));
            _lexer.RemoveErrorListeners();
            _parser = new FsdParser(new UnbufferedTokenStream(_lexer));
            _parser.ErrorHandler = new MessageErrorStrategy();
            _parser.RemoveErrorListeners();
            _visitor = new MessageVisitor();
        }


        /// <returns>The parsed length in input.</returns>
        public Message Parse(string input)
        {
            _lexer.SetInputStream(new AntlrInputStream(input));
            _parser.SetInputStream(new UnbufferedTokenStream(_lexer));

            var ast = _parser.message();
            if (ast.Start.StartIndex != 0) throw new InvalidMessageException("Parser output does not start at 0: '" + input + "'");

            _visitor.Visit(ast);
            if (_visitor.Messages.Count != 1)
                throw new InvalidMessageException("There are no messages: '" + input + "'");
            return _visitor.Messages[0];
        }

    }
}
