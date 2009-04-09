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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using DemonServer.Net;
using DemonServer.Handler;
using DemonServer.Protocol;
using DemonServer.User;

namespace DemonServer.Queues
{
	class ServerRecvQ
	{
		private ServerCore _parent;
		private int numThreads;

		private List<Thread> workerThreads;
		private Thread threadManagerThread;

		private AutoResetEvent are;
		private AutoResetEvent abortEvent;

		private Queue<QueueItem> recvQueue;

		private PacketProcessor process;

		public ServerRecvQ(ServerCore server) : this(server, 5) { }
		public ServerRecvQ(ServerCore server, int numThreads)
		{
			this._parent = server;

			this.recvQueue = new Queue<QueueItem>();

			this.process = PacketProcessor.getInstance();
			this.numThreads = numThreads;

			this.are = new AutoResetEvent(false);
			this.abortEvent = new AutoResetEvent(false);

			this.workerThreads = new List<Thread>(numThreads);

			this.threadManagerThread = new Thread(new ThreadStart(this.threadManager));
			this.threadManagerThread.Start();
		}

		private void queueProcessor()
		{
			Thread.CurrentThread.Name = "Server RecvQ Processor";

			for (; ; )
			{
				// Wait for an event to be signalled.
				are.WaitOne();

				try
				{
					QueueItem item;
					lock (this.recvQueue)
					{
						try
						{
							item = this.recvQueue.Dequeue();
						}
						catch (InvalidOperationException) { continue; }
					}

					Socket socket = this._parent.GetSocketById(item.SocketID);

					if (socket == null)
					{
						// The user must have disconnected already. :/ Discard.
						continue;
					}

					IPacketHandler handler = this.process.getHandler(item.Packet.cmd);
					if (handler == null)
					{
						socket.UserRef.disconnect("bad data", item.SocketID);
						continue;
					}

					if (handler.validateState(socket.UserRef, socket))
						handler.handlePacket(item.Packet, socket.UserRef, item.SocketID);
					else
						socket.UserRef.disconnect("bad data", item.SocketID);
				}
				catch (ThreadAbortException)
				{
					// We were called to stop...just stop.
					return;
				}
			}
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		private void threadManager()
		{
			Thread.CurrentThread.Name = "Server RecvQ Threadpool Manager";

			while (!abortEvent.WaitOne(1000))
			{
				while (this.workerThreads.Count < this.numThreads)
				{
					Thread thread = new Thread(new ThreadStart(this.queueProcessor));
					this.workerThreads.Add(thread);
					thread.Start();
				}

				for (int i = 0; i < this.workerThreads.Count; i++)
				{
					{
						switch (this.workerThreads[i].ThreadState)
						{
							case ThreadState.Running:
							case ThreadState.WaitSleepJoin:
							case ThreadState.Background:
								break;
							case ThreadState.Unstarted:
								this.workerThreads[i].Start();
								break;
							default:
								try { this.workerThreads[i].Abort(); }
								catch { }

								this.workerThreads[i] = new Thread(new ThreadStart(this.queueProcessor));
								this.workerThreads[i].Start();
								break;
						}
					}
				}
			}
		}

		public void AddQueue(Packet packet, int SocketID)
		{
			lock (this.recvQueue)
			{
				this.recvQueue.Enqueue(new QueueItem(packet, SocketID));
			}
			this.are.Set();
		}

		public void Abort()
		{
			this.abortEvent.Set();
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
