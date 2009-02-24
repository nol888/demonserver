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

using DemonServer.Protocol;
using DemonServer.Net;

namespace DemonServer.User
{
	public class DAmnUser
	{
		#region Public Properties
		public List<Socket> Sockets { get; set; }

		public uint UserID { get; set; }

		public string Username { get; set; }

		public string ArtistType { get; set; }
		public string RealName { get; set; }
		public string DeviantSymbol { get; set; }

		public Dictionary<string, string> HandshakeVars { get; set; }

		public GPC GPC { get; set; }

		public bool Authed { get; set; }
		#endregion

		#region Constructor
		public DAmnUser()
		{
			this.HandshakeVars = new Dictionary<string, string>();
			this.Sockets = new List<Socket>();
		}
		#endregion

		#region Connection Functions
		public void send(string packet) { this.send(packet, -1); }
		public void send(string packet, int socketID)
		{
			lock (this.Sockets)
			{
				foreach (Socket sock in this.Sockets)
				{
					if (socketID > 0)
					{
						if (sock.SocketID != socketID) continue;
					}

					ServerCore.GetCore().SendToSocket(sock, packet);
				}
			}
		}

		public int disconnect(string reason) { return this.disconnect(reason, -1); }
		public int disconnect(string reason, int socketID)
		{
			int connDisconnected = 0;
			lock (this.Sockets)
			{
				for (int i = 0; i < this.Sockets.Count; i++)
				{
					if (socketID > 0)
					{
						if (this.Sockets[i].SocketID != socketID) continue;
					}
					Packet dAmnPacket = new Packet("disconnect", "");
					dAmnPacket.args.Add("e", reason);

					ServerCore.GetCore().SendToSocket(this.Sockets[i], dAmnPacket);

					i--; // Removing a socket pushes all other sockets after that back 1 spot.
					connDisconnected++;
				}
			}
			return connDisconnected;
		}
		#endregion
	}
}
