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

using DemonServer.User;
using DemonServer.Protocol;

namespace DemonServer.Handler.Handlers
{
	public class DAmnClientHandler : IPacketHandler
	{
		public DAmnClientHandler()
		{
		}

		public void handlePacket(Packet origPacket, DAmnUser user, int socketID)
		{
			if (origPacket.param.Trim() != ServerCore.DAmnClientVersion.Trim())
			{
				user.disconnect("wrong version", socketID);
				return;
			}

			// We're good, send them the response...
			Packet respPacket = new Packet("dAmnServer", ServerCore.DAmnServerVersion);
			respPacket.args.Add("emulatedBy", "Demon");

			user.send(respPacket.ToString(), socketID); // Socket specific.
		}

		public bool validateState(DAmnUser user)
		{
			// You can technically use this anytime...
			// Though it's not much of a help.
			// I think.
			return true;
		}
	}
}
