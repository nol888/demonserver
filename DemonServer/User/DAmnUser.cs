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
		#region Private Properties
		private List<Socket> _sockets;

		private int _userid;

		private string _username;
		private string _artistType;
		private string _realName;

		private GPC _gpc;

		private bool _authed;
		#endregion

		#region Public Properties
		public List<Socket> sockets
		{
			get { return this._sockets; }
			set { this._sockets = value; }
		}

		public int userID
		{
			get { return this._userid; }
			//set { this._userid = value; }
		}

		public string username
		{
			get { return _username; }
			//set { _username = value; }
		}
		public string artistType
		{
			get { return _artistType; }
			set { _artistType = value; }
		}
		public string realName
		{
			get { return _realName; }
			set { _realName = value; }
		}

		public GPC gpc
		{
			get { return this._gpc; }
			set { this._gpc = value; }
		}

		public bool authed
		{
			get { return this._authed; }
			//set { this._authed = value; }
		}
		#endregion

		#region Constructor
		public DAmnUser()
		{
		}
		#endregion

		#region Connection Functions
		public int disconnect(string reason) {
			int connDisconnected = 0;
			foreach (Socket sock in this._sockets)
			{
				Packet dAmnPacket = new Packet("disconnect", "");
				dAmnPacket.args.Add("e", reason);

				sock.SendPacket((string) dAmnPacket);
				sock.Close();
				connDisconnected++;
			}
			return connDisconnected;
		}
		#endregion
	}
}
