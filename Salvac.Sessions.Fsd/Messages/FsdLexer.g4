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

lexer grammar FsdLexer;

// FSD message types 

WEATHERDATA		: '&D'	-> pushMode(Data) ;
PLANEPOSITION	: '@'	-> pushMode(Data) ;
DELETEPLANE		: '#DP'	-> pushMode(Data) ;
DELETEATC		: '#DA' -> pushMode(Data) ;

// Data types

mode Data;

SEP			: ':' ;
END			: '\r'? '\n'	-> popMode;
SQUAWK		: [0-7][0-7]?[0-7]?[0-7]? ;
INT			: ('-'|'+')? [0-9]+ ('l'|'L')? ;
FLOAT		: ('-'|'+')? [0-9]+ '.' [0-9]+ 
			| ('-'|'+')? '.' [0-9]+
			;
FSDNAME		: [a-zA-z] [a-zA-Z0-9_]* ;
STRING		: ~[:\r\n]+
			;
