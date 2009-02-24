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
|	> $Date: 2009-02-23 16:34:44 -0500 (Mon, 23 Feb 2009) $
|	> $Revision: 23 $
|	> $Author: nol888 $
+---------------------------------------------------------------------------+
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using DemonServer.Net;
using DemonServer.Handler;
using DemonServer.Protocol;
using DemonServer.User;

namespace DemonServer
{
	class ServerRecvQ
	{
		private ServerCore _parent;

		private Thread workerThread;
		private Queue<QueueItem> recvQueue;

		private PacketProcessor process;

		public ServerRecvQ(ServerCore server)
		{
			this._parent = server;

			this.recvQueue = new Queue<QueueItem>();

			this.process = PacketProcessor.getInstance();

			this.workerThread = new Thread(new ThreadStart(this.queueProcessor));
			this.workerThread.Start();
		}

		private void queueProcessor()
		{
			Thread.CurrentThread.Name = "Server RecvQ Processor";

			while (true)
			{
				try
				{
					if (this.recvQueue.Count > 0)
					{
						lock (this.recvQueue)
						{
							while (this.recvQueue.Count > 0)
							{
								QueueItem item = this.recvQueue.Dequeue();
								Socket socket = this._parent.GetSocketById(item.SocketID);

								IPacketHandler handler = this.process.getHandler(item.Packet.cmd);
								if (handler == null)
								{
									socket.UserRef.disconnect("bad data", item.SocketID);
									continue;
								}

								if (handler.validateState(socket.UserRef, socket))
								{
									handler.handlePacket(item.Packet, socket.UserRef, item.SocketID);
								}
								else
								{
									socket.UserRef.disconnect("bad data", item.SocketID);
								}
							}
						}
					}
				}
				catch (ThreadAbortException)
				{
					// We were called to stop...just stop.
					return;
				}
				System.Threading.Thread.Sleep(100);
			}
		}

		public void AddQueue(Packet packet, int SocketID)
		{
			lock (this.recvQueue)
			{
				this.recvQueue.Enqueue(new QueueItem(packet, SocketID));
			}
		}

		public void Abort()
		{
			this.workerThread.Abort();
		}

		private struct QueueItem
		{
			public Packet Packet;
			public int SocketID;

			public QueueItem(Packet packet, int SocketID)
			{
				this.Packet = packet;
				this.SocketID = SocketID;
			}
		}
	}
}
