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
using System.Threading;

using DemonServer.Net;
using DemonServer.Handler;
using DemonServer.Protocol;
using DemonServer.User;

namespace DemonServer
{
	class ServerSendQ
	{
		private ServerCore _parent;

		private Thread workerThread;
		private Queue<QueueItem> sendQueue;

		public ServerSendQ(ServerCore server)
		{
			this._parent = server;

			this.sendQueue = new Queue<QueueItem>();

			this.workerThread = new Thread(new ThreadStart(this.queueProcessor));
			this.workerThread.Start();
		}

		private void queueProcessor()
		{
			Thread.CurrentThread.Name = "Server SendQ Processor";

			while (true)
			{
				try
				{
					if (this.sendQueue.Count > 0)
					{
						lock (this.sendQueue)
						{
							while (this.sendQueue.Count > 0)
							{
								QueueItem item = this.sendQueue.Dequeue();
								Socket socket = this._parent.GetSocketById(item.SocketID);

								if (socket == null)
								{
									// Lololo, the person got disconnected. Discard the packet.
									continue;
								}
								try
								{
									socket.SendPacket(item.PacketB);

									if (item.PacketP.cmd == "disconnect")
									{
										socket.Close(0, item.PacketP.args["e"]);
									}
								}
								catch (Exception e)
								{
									Console.ShowWarning("Error sending queued message to SockID " + item.SocketID.ToString() + ":\n" + e.Message);
								}
							}
						}
					}
				}
				catch (ThreadAbortException)
				{
					// Flush the queue.
					lock (this.sendQueue)
					{
						while (this.sendQueue.Count > 0)
						{
							QueueItem item = this.sendQueue.Dequeue();
							Socket socket = this._parent.GetSocketById(item.SocketID);

							if (socket == null)
							{
								// Lololo, the person got disconnected. Discard the packet.
								continue;
							}
							try
							{
								socket.SendPacket(item.PacketB);

								if (item.PacketP.cmd == "disconnect")
								{
									socket.Close(0, item.PacketP.args["e"]);
								}
							}
							catch (Exception e)
							{
								Console.ShowWarning("Error sending queued message to SockID " + item.SocketID.ToString() + ":\n" + e.Message);
							}
						}
					}

					return;
				}
				System.Threading.Thread.Sleep(100);
			}
		}

		public void AddQueue(Packet packet, int SocketID)
		{
			lock (this.sendQueue)
			{
				this.sendQueue.Enqueue(new QueueItem(packet, SocketID));
			}
		}
		public void AddQueue(string packet, int SocketID)
		{
			lock (this.sendQueue)
			{
				this.sendQueue.Enqueue(new QueueItem(packet, SocketID));
			}
		}
		public void AddQueue(byte[] packet, int SocketID)
		{
			lock (this.sendQueue)
			{
				this.sendQueue.Enqueue(new QueueItem(packet, SocketID));
			}
		}

		public void Abort()
		{
			this.workerThread.Abort();
		}

		private struct QueueItem
		{
			private byte[] _packet;
			public Packet PacketP
			{
				get
				{
					return new Packet(new string(Encoding.ASCII.GetChars(_packet)));
				}
			}
			public string PacketS
			{
				get
				{
					return new string(Encoding.ASCII.GetChars(_packet));
				}
			}
			public byte[] PacketB
			{
				get
				{
					return (byte[]) this._packet.Clone();
				}
			}

			public int SocketID;

			public QueueItem(Packet packet, int SocketID)
			{
				this._packet = Encoding.ASCII.GetBytes(packet.ToString());
				this.SocketID = SocketID;
			}
			public QueueItem(string packet, int SocketID)
			{
				this._packet = Encoding.ASCII.GetBytes(packet);
				this.SocketID = SocketID;
			}
			public QueueItem(byte[] packet, int SocketID)
			{
				this._packet = packet;
				this.SocketID = SocketID;
			}
		}
	}
}
