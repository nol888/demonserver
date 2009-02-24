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

using DemonServer.Net;
using DemonServer.User;
using DemonServer.Protocol;

using MySql.Data.MySqlClient;

namespace DemonServer.Handler.Handlers
{
	public class LoginHandler : IPacketHandler
	{
		public LoginHandler()
		{
		}

		public void handlePacket(Packet origPacket, DAmnUser user, int socketID)
		{
			Packet respPacket;

			if (!origPacket.args.ContainsKey("pk") || (origPacket.param == ""))
			{
				user.disconnect("bad data", socketID);
				return;
			}

			// Look up the data...yawn...
			DBConn conn = ODBCFactory.Instance;
			MySqlCommand ps = conn.Prepare("SELECT `user_id`, `authtoken`, `gpc`, `user_realname`, `user_dtype`, `user_symbol` FROM `users` WHERE `user_name` = @name LIMIT 1;");
			ps.Parameters.AddWithValue("@name", origPacket.param);
			DBResult result = conn.Query(ps);

			if (result.GetNumRows() != 1)
			{
				// Epic fail.
				respPacket = new Packet("login", origPacket.param);
				respPacket.args.Add("e", "authentication failed");
				user.send(respPacket.ToString(), socketID); // Socket specific.
				return;
			}

			Dictionary<string, object> row = result.FetchRow();

			if (((string) row["authtoken"]).Trim() != origPacket["pk"])
			{
				// Epic fail.
				respPacket = new Packet("login", origPacket.param);
				respPacket.args.Add("e", "authentication failed");
				user.send(respPacket.ToString(), socketID); // Socket specific.
				return;
			}

			// We're good...check if a user already exists...
			ServerCore core = ServerCore.GetCore();
			if(core.ClientNames.ContainsKey(origPacket.param)) {
				core.ClientNames[origPacket.param].Sockets.Add(core.GetSocketById(socketID));

				core.Clients.Remove(user);
				user = null;
			} else {
				user.Username = origPacket.param;
				user.GPC = (GPC) (byte) row["gpc"];
				user.RealName = (string) row["user_realname"];
				user.UserID = (int) row["user_id"];
				user.ArtistType = (string) row["user_dtype"];
				user.DeviantSymbol = (string) row["user_symbol"];

				core.ClientNames.Add(origPacket.param, user);
			}

			respPacket = new Packet("login", origPacket.param);
			respPacket.args.Add("e", "ok");
			respPacket.args.Add("symbol", (string) row["user_symbol"]);
			respPacket.args.Add("realname", (string) row["user_realname"]);
			respPacket.args.Add("typename", (string) row["user_dtype"]);
			respPacket.args.Add("gpc", Enum.GetName(typeof(GPC), (int) (byte) row["gpc"]));

			user.send(respPacket.ToString(), socketID); // Socket specific.
		}

		public bool validateState(DAmnUser user, Socket userSocket)
		{
			// Only available if we haven't logged in before.

			return user.Username == null;
		}
	}
}
