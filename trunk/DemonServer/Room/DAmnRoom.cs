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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using DemonServer.Net;
using DemonServer.Protocol;

namespace DemonServer.Room
{
	public class DAmnRoom
	{
		public uint RoomID { get; set; }
		public string RoomName { get; set; }

		public uint CreatorId { get; set; }
		public string CreatorName { get; set; }

		public RoomTopic Topic { get; set; }
		public RoomTitle Title { get; set; }

		public SendPrivs GlobalPerms { get; set; }

		private List<Socket> userList;
		public ReadOnlyCollection<Socket> UserList
		{
			get { return this.userList.AsReadOnly(); }
		}

		public List<Privclass> PrivclassList { get; set; }

		public DAmnRoom()
		{
			this.GlobalPerms = SendPrivs.All;
			this.Title = new RoomTitle();
			this.Topic = new RoomTopic();

			this.RoomID = 0;
			this.RoomName = "";

			this.CreatorId = 0;
			this.CreatorName = "";

			this.userList = new List<Socket>();
			this.PrivclassList = new List<Privclass>();
		}

		public void UserJoin(Socket user)
		{
			this.userList.Add(user);

			Packet recvPacket = new Packet("recv", RoomUtils.ParseNS(this.RoomName));
			recvPacket.body = new Packet("join", user.UserRef.Username).ToString(false);

			SendToAll(recvPacket);
		}

		public void SendToAll(Packet packet) { SendToAllButOne(Encoding.ASCII.GetBytes(packet.ToString()), null); }
		public void SendToAll(string packet) { SendToAllButOne(Encoding.ASCII.GetBytes(packet), null); }
		public void SendToAll(byte[] packet) { SendToAllButOne(packet, null); }

		public void SendToAllButOne(Packet packet, Socket one) { SendToAllButOne(Encoding.ASCII.GetBytes(packet.ToString()), one); }
		public void SendToAllButOne(string packet, Socket one) { SendToAllButOne(Encoding.ASCII.GetBytes(packet), one); }
		public void SendToAllButOne(byte[] packet, Socket one)
		{
			lock (this.userList)
			{
				foreach (Socket sock in this.userList)
				{
					if ((one != null) && (sock.SocketID == one.SocketID)) continue;

					ServerCore.GetCore().SendToSocketID(sock.SocketID, packet);
				}
			}
		}

		#region Overrides
		public override string ToString()
		{
			return this.RoomName;
		}
		#endregion

		#region Embedded Classes
		public class Privclass
		{
			#region Private Members
			private ushort _Promote;
			private ushort _Demote;

			private ushort _Order;
			#endregion

			#region Public Properties
			public PrivclassPrivs Permissions { get; set; }

			public string PrivclassName { get; set; }
			public uint PrivclassId { get; set; }

			public uint ParentRoomId { get; set; }

			public ushort Order
			{
				get { return this._Order; }
				set { if ((value > 0) && (value < 100)) this._Order = value; }
			}

			public ushort Promote
			{
				get { return this._Promote; }
				set { if (value < this._Demote) return; else this._Promote = value; }
			}
			public ushort Demote
			{
				get { return this._Demote; }
				set { if (value > this._Promote) return; else this._Demote = value; }
			}

			public short ImageLimit { get; set;}
			public short SmilieLimit { get; set;}
			public short EmoticonLimit { get; set; }
			public short ThumbLimit { get; set;}
			public short AvatarLimit { get; set; }
			public short WebsiteLimit { get; set; }
			public short ObjectLimit { get; set; }

			public bool Default { get; set; }
			#endregion

			#region Constructors
			public Privclass()
			{
				this.Permissions = PrivclassPrivs.Default;

				this.PrivclassName = "";
				this.PrivclassId = 0;

				this.ParentRoomId = 0;

				this.Order = 99;
				this._Promote = 0;
				this._Demote = 0;

				this.ImageLimit = this.SmilieLimit = this.EmoticonLimit = this.ThumbLimit = this.AvatarLimit = this.WebsiteLimit = this.ObjectLimit = -1;

				this.Default = false;
			}
			public Privclass(string name, uint id, uint parentId)
				: this()
			{
				this.PrivclassName = name;
				this.PrivclassId = id;
				this.ParentRoomId = parentId;
			}
			#endregion

			#region Overrides
			public override string ToString()
			{
				return String.Format("Order: {0}; Name: {1}", this.Order, this.PrivclassName);
			}
			#endregion
		}
		#endregion
	}
}
