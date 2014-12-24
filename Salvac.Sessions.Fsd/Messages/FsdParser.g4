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

parser grammar FsdParser;

options {
	tokenVocab = FsdLexer ;
}

message : pilotPosition END?
		| weatherData END?
		| deletePilot END?
		| deleteAtc END?
		;

		 
weatherData		: type=WEATHERDATA source=FSDNAME SEP destination=FSDNAME SEP request=integer SEP data=string
				;
// request cannot be Squawk, shall be 0, 1 or 2
// allow numbers for data, too

pilotPosition	: type=PILOTPOSITION sqkMode=FSDNAME SEP source=FSDNAME SEP squawk=SQUAWK SEP rating=integer SEP lat=float SEP lon=float SEP 
					altitude=integer SEP speed=integer SEP pbh=integer SEP altitudeDiff=integer
				;

deletePilot		: type=DELETEPILOT source=FSDNAME (SEP unkown=integer)?
				;
deleteAtc		: type=DELETEATC source=FSDNAME (SEP unkown=integer)?
				;

// Basic string and number "tokens". Have to be used, as e.g. 0145 will be interpreted as SQUAWK but might want
// to be interpreted as INT. integer has to be used instead of INT.
// These rules always consist of ONE token.
integer	: SQUAWK
		| INT
		;

float	: integer
		| FLOAT
		;

string	: STRING
		| FSDNAME
		| float
		;