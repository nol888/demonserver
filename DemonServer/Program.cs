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
using System.Collections;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace DemonServer
{
	class Program
	{
		#region Dummy Constructor and Entry Point
		public Program() { }
		static void Main(string[] args)
		{
			Program mainProg = new Program();
			Environment.Exit(mainProg.run(args));
		}
		#endregion
		private const int maxConnections = 200;

		private Socket listenSocket;

		private AsyncCallback __clientConnected;
		private CommonLib.DBConn DBConn;

		private CommonLib.Socket[] clients = new CommonLib.Socket[maxConnections];
		private Stack<int> unusedSockets = new Stack<int>(maxConnections);

		private System.Timers.Timer pingTimer = new System.Timers.Timer(30000);
		private System.Timers.Timer errorTimer = new System.Timers.Timer(10000);
		private System.Timers.Timer mySQLPingTimer = new System.Timers.Timer(300000);

		private Dictionary<string, string> Configuration;

		public bool Running = true;
		int run(string[] args)
		{
			__clientConnected = new AsyncCallback(this.clientConnected);
			Console.CancelKeyPress += delegate { this.cleanUp(); };

			// Init the socket list.
			int i = maxConnections;
			while (i-- > 0)
			{
				unusedSockets.Push(i);
			}


			return 0;
		}
		void cleanUp()
		{
			this.Running = false;

			try
			{
				// Stop timers.
				pingTimer.Stop();
				errorTimer.Stop();
				mySQLPingTimer.Stop();

				// Disconnect all users.
				foreach (CommonLib.Socket CurrentClient in clients)
				{
					if ((CurrentClient == null) || !(CurrentClient.InternalSocket.Connected))
					{
						continue;
					}
					CurrentClient.Close(10053);
				}
			}
			catch { }
		}

		#region Client Connection Handling
		public void clientConnected(IAsyncResult Result)
		{
			int SocketID = unusedSockets.Pop();
			try
			{
				// Set up the event listeners.
				clients[SocketID] = new CommonLib.Socket(SocketID, listenSocket.EndAccept(Result));
				clients[SocketID].OnDataArrival += new CommonLib.Socket.__OnDataArrival(Program_OnDataArrival);
				clients[SocketID].OnDisconnect += new CommonLib.Socket.__OnDisconnect(Program_OnDisconnect);
				clients[SocketID].OnError += new CommonLib.Socket.__OnError(Program_OnError);

				// Start listening for the handshake.
				clients[SocketID].StartReceive();

				CommonLib.Console.ShowInfo(string.Format("New connection from \x1B[37m{0}\x1B[0m.", clients[SocketID].Name));
			}
			catch (SocketException Ex)
			{
				CommonLib.Console.ShowError("Socket Exception: " + Ex.Message);
			}
			catch (Exception Ex)
			{
				CommonLib.Console.ShowError("Error accepting connection: " + Ex.Message);
			}
			finally
			{
				// Yea, let's stop hogging the main socket.
				listenSocket.BeginAccept(__clientConnected, null);
			}
		}
		void Program_OnDataArrival(int SocketID, byte[] ByteArray)
		{
			string packetText = "";
			foreach (byte dataByte in ByteArray)
			{
				packetText += ((char) dataByte).ToString();
			}

			// Do something with it later.
		}
		void Program_OnDisconnect(int SocketID, SocketException Ex)
		{
			if (clients[SocketID] == null) return;
			lock (this.clients[SocketID])
			{
				if (Ex.ErrorCode > 0)
				{
					string ExceptionName = "";
					switch (Ex.ErrorCode)
					{
						case 10060:
							ExceptionName = "Connection timed out.";
							break;
						case 10054:
							ExceptionName = "Connection reset by peer.";
							break;
						case 10053:
							ExceptionName = "Software caused connection abort.";
							break;
						case 10052:
							ExceptionName = "Network dropped connection on reset.";
							break;
						default:
							ExceptionName = Ex.Message;
							break;
					}
					CommonLib.Console.ShowInfo("\x1B[37m" + clients[SocketID].Name + "\x1B[0m disconnected: " + ExceptionName);
				}
				else
				{
					CommonLib.Console.ShowInfo("\x1B[37m" + clients[SocketID].Name + "\x1B[0m disconnected: Connection closed.");
				}
				clients[SocketID] = null;
				unusedSockets.Push(SocketID);
			}
		}
		void Program_OnError(int SocketID, Exception Ex)
		{
			CommonLib.Console.ShowError("An error occurred\n" + Ex.StackTrace + "\n" + Ex.Message);
		}
		#endregion
	}
}
