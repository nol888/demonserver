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
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using DemonServer.Protocol;
using DemonServer.User;

namespace DemonServer
{
	class ServerCore
	{
		#region Dummy Constructor and Entry Point
		public ServerCore() { }
		static void Main(string[] args)
		{
			ServerCore mainProg = new ServerCore();
			Environment.Exit(mainProg.run(args));
		}
		#endregion
		private int maxConnections;

		private Socket listenSocket;

		private AsyncCallback __clientConnected;
		private Net.DBConn DBConn;

		private List<DAmnUser> clients = new List<DAmnUser>();
		private Dictionary<int, Net.Socket> socketList = new Dictionary<int, Net.Socket>();
		private Stack<int> unusedSockets = new Stack<int>();

		private System.Timers.Timer pingTimer = new System.Timers.Timer(30000);
		private System.Timers.Timer errorTimer = new System.Timers.Timer(10000);
		private System.Timers.Timer mySQLPingTimer = new System.Timers.Timer(300000);

		private Dictionary<string, string> Configuration;

		public bool Running = true;
		int run(string[] args)
		{
			__clientConnected = new AsyncCallback(this.clientConnected);
			System.Console.CancelKeyPress += delegate { this.cleanUp(); };

			// Load config first.
			XmlConfigReader configReader = new XmlConfigReader("config.xml");
			this.Configuration = configReader.ReadConfig();
			Console.TimestampFormat = this.Configuration["timestamp"];
			this.maxConnections = int.Parse(this.Configuration["connlimit-main"]);

			Console.ShowInfo("Loaded configuration from 'config.xml'.");

			// Init the socket list.
			int i = maxConnections;
			while (i-- > 0)
			{
				unusedSockets.Push(i);
			}

			// Connect to the DB.
			#region Connect to the DB.
			DBConn = new Net.DBConn(Configuration["mysql-host"], Configuration["mysql-user"], Configuration["mysql-pass"],
				Configuration["mysql-database"], ((Configuration["mysql-port"] != "") ? (int.Parse(Configuration["mysql-port"])) : (3306)));
			lock (DBConn)
			{
				try
				{
					while (true)
					{
						Console.ShowInfo(string.Format("Attempting to connect to MySQL on '{0}:{1}'.", DBConn.sqlServer, DBConn.sqlPort));
						if (DBConn.Connect() == false)
						{
							Console.ShowError("Unable to connect to MySQL server!  Error: " + DBConn.MySQL_Error());
							System.Threading.Thread.Sleep(5000);
						}
						else
						{
							Console.ShowStatus("Successfully connected to MySQL.");
							break;
						}
					}
				}
				catch (Exception)
				{
					Console.ShowError("Unable to connect to MySQL server!  Error: " + DBConn.MySQL_Error());
				}
			}
			#endregion

			// Start up the main socket.
			this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.listenSocket.Bind(new IPEndPoint(IPAddress.Parse(this.Configuration["bind-ip"]), int.Parse(this.Configuration["bind-port"])));

			// Start up the timers.
			pingTimer.Elapsed += new System.Timers.ElapsedEventHandler(Socket_PingTimer);
			errorTimer.Elapsed += new System.Timers.ElapsedEventHandler(Socket_ErrorCheck);
			mySQLPingTimer.Elapsed += new System.Timers.ElapsedEventHandler(MySQLPingTimer_Elapsed);

			string hash = Crypto.hash("12345", "salt12");
			string salt1 = Crypto.genSalt();
			string salt2 = Crypto.genSalt();
			string pk1 = Crypto.genAuthToken();
			string pk2 = Crypto.genAuthToken();
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

				// Disconnect all sockets.
				foreach (DAmnUser CurrentClient in clients)
				{
					if (CurrentClient == null) continue;
					CurrentClient.disconnect("shutdown");
				}
			}
			catch { }
		}

		#region Client Connection Handling
		public void clientConnected(IAsyncResult Result)
		{
			lock (this.listenSocket)
			{
				try
				{
					int SocketID = unusedSockets.Pop();

					// Set up the event listeners.
					socketList.Add(SocketID, null);
					socketList[SocketID] = new Net.Socket(SocketID, listenSocket.EndAccept(Result));
					socketList[SocketID].OnDataArrival += new Net.Socket.__OnDataArrival(Program_OnDataArrival);
					socketList[SocketID].OnDisconnect += new Net.Socket.__OnDisconnect(Program_OnDisconnect);
					socketList[SocketID].OnError += new Net.Socket.__OnError(Program_OnError);

					// Start listening for the handshake.
					socketList[SocketID].StartReceive();

					Console.ShowInfo(string.Format("New connection from \x1B[37m{0}\x1B[0m.", socketList[SocketID].Name));
				}
				catch (InvalidOperationException)
				{
					// _Probably_ we ran out of sockets.
					// Turn 'em down, sorry.
					Console.ShowError("Error accepting connection - maximum limit of " + maxConnections.ToString() + " connections reached.");

					listenSocket.EndAccept(Result).Close(1);
					return;
				}
				catch (SocketException Ex)
				{
					Console.ShowError("Socket Exception: " + Ex.Message);
				}
				catch (Exception Ex)
				{
					Console.ShowError("Error accepting connection: " + Ex.Message);
				}
				finally
				{
					// Yea, let's stop hogging the main socket.
					listenSocket.BeginAccept(__clientConnected, null);
				}
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
					Console.ShowInfo("\x1B[37m" + socketList[SocketID].Name + "\x1B[0m disconnected: " + ExceptionName);
				}
				else
				{
					Console.ShowInfo("\x1B[37m" + socketList[SocketID].Name + "\x1B[0m disconnected: Connection closed.");
				}
				clients[SocketID] = null;
				unusedSockets.Push(SocketID);
			}
		}
		void Program_OnError(int SocketID, Exception Ex)
		{
			Console.ShowError("An error occurred\n" + Ex.StackTrace + "\n" + Ex.Message);
		}
		#endregion

		#region Timed Events
		[MethodImpl(MethodImplOptions.Synchronized)]
		void Socket_PingTimer(object sender, System.Timers.ElapsedEventArgs e)
		{
			Packet pingPacket = new Packet("ping", "");

			lock (this.clients)
			{
				foreach (DAmnUser user in this.clients)
				{
					if (user == null)
					{
						continue;
					}
					foreach (Net.Socket socket in user.sockets)
					{
						if (socket.IsPinging)
						{
							continue;
						}

						socket.IsPinging = true;
						socket.PingTime = ServerCore.time();
					}
				}
			}
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		void Socket_ErrorCheck(object sender, System.Timers.ElapsedEventArgs e)
		{
			lock (this.clients)
			{
				foreach (DAmnUser user in this.clients)
				{
					if (user == null)
					{
						continue;
					}
					foreach (Net.Socket socket in user.sockets)
					{
						if (!socket.IsPinging) continue;
						if ((ServerCore.time() - socket.PingTime) >= 48)
						{
							socket.IsPinging = false;
							user.disconnect("ping timeout", socket.SocketID);
						}
					}
				}
			}
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		void MySQLPingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				lock (DBConn)
				{
					DBConn.Ping();
				}
				Console.ShowInfo("Pinging MySQL server to keep connection alive...");
			}
			catch (Exception) { }
		}
		#endregion

		#region Misc
		public static int time()
		{
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
			int timestamp = (int) t.TotalSeconds;
			return timestamp;
		}
		public static long microtime()
		{
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
			long timestamp = (long) t.TotalMilliseconds;
			return timestamp;
		}
		#endregion
	}
}
