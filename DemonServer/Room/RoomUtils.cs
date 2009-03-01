/*
+---------------------------------------------------------------------------+
|	Demon - dAmn Emulator													|
|===========================================================================|
|	Copyright © 2008 Nol888													|
|===========================================================================|
|	This file is part of Demon.												|
|																			|
|	Demon is free software: you can redistribute it and/or modify			|
|	it under the terms of the GNU Affero General Public License as			|
|	published by the Free Software Foundation, either version 3 of the		|
|	License, or (at your option) any later version.							|
|																			|
|	This program is distributed in the hope that it will be useful,			|
|	but WITHOUT ANY WARRANTY; without even the implied warranty of			|
|	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the			|
|	GNU Affero General Public License for more details.						|
|																			|
|	You should have received a copy of the GNU Affero General Public License|
|	along with this program.  If not, see <http://www.gnu.org/licenses/>.	|
|																			|
|===========================================================================|
|	> $Date$
|	> $Revision$
|	> $Author$
+---------------------------------------------------------------------------+
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemonServer.Room
{
	public static class RoomUtils
	{
		public static string ParseNS(string input)
		{
			if (input.StartsWith("chat:"))
			{
				return input;
			}
			if (input.StartsWith("#"))
			{
				input = input.Remove(0, 1);
				input = "chat:" + input;
				return input;
			}
			return ("chat:" + input);
		}
		public static string DeparseNS(string input)
		{
			if (input.StartsWith("#"))
			{
				return input;
			}
			if (input.StartsWith("chat:"))
			{
				input = input.Remove(0, 5);
				input = "#" + input;
				return input;
			}
			return ("#" + input);
		}
	}
}
