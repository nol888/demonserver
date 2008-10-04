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
using System.Runtime.CompilerServices;

namespace DemonServer.Handler
{
	public class PacketProcessor
	{
		#region Private Properties
		private static PacketProcessor _instance;
		private Dictionary<string, IPacketHandler> _handlers;
		#endregion

		private PacketProcessor()
		{
			this._handlers = new Dictionary<string, IPacketHandler>();

			// Register the handlers here.
			this.registerHandler("dAmnClient", new Handlers.DAmnClientHandler());
		}

		public IPacketHandler getHandler(string packetType)
		{
			if (!this._handlers.ContainsKey(packetType))
			{
				return null;
			}

			IPacketHandler handler = this._handlers[packetType];

			if (handler != null) return handler;

			return null;
		}
		public void registerHandler(string packetType, IPacketHandler packetHandler)
		{
			this._handlers.Add(packetType, packetHandler);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static PacketProcessor getInstance() {
			if (_instance == null)
			{
				_instance = new PacketProcessor();
			}
			return _instance;
		}
	}
}
