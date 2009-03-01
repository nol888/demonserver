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
using DemonServer.Handler;
using DemonServer.Queues;

namespace DemonServer
{
	class ServerCore
	{
		#region Dummy Constructor and Entry Point
		public ServerCore() { }

		private static ServerCore core;
		static void Main(string[] args)
		{
			ServerCore.core = new ServerCore();
			Environment.Exit(ServerCore.core.run(args));
		}

		public static ServerCore GetCore()
		{
			return ServerCore.core;
		}
		#endregion

		#region Version-specific constants
		public const string DAmnServerVersion = "0.3";
		public const string DAmnClientVersion = "0.3";
		#endregion

		#region MySQL public-access for DBC singleton.
		public static string MySQLHost;
		public static string MySQLUsername;
		public static string MySQLPassword;
		public static string MySQLDatabase;
		public static int MySQLPort;
		#endregion

		private int maxConnections;

		private Socket listenSocket;

		private AsyncCallback __clientConnected;
		private Net.DBConn DBConn;

		private Dictionary<int, Net.Socket> socketList = new Dictionary<int, Net.Socket>();
		private Stack<int> unusedSockets = new Stack<int>();

		private System.Timers.Timer pingTimer = new System.Timers.Timer(30000);
		private System.Timers.Timer errorTimer = new System.Timers.Timer(10000);
		private System.Timers.Timer mySQLPingTimer = new System.Timers.Timer(300000);

		private UserManageDaemon umd;
		private PacketProcessor packetProcess;

		private Dictionary<string, string> Configuration;

		internal List<DAmnUser> Clients = new List<DAmnUser>();
		internal Dictionary<string, DAmnUser> ClientNames = new Dictionary<string, DAmnUser>();

		private ServerSendQ sendQ;
		private ServerRecvQ recvQ;

		public bool Running = true;

		private int run(string[] args)
		{
			__clientConnected = new AsyncCallback(this.clientConnected);
			System.Console.CancelKeyPress += delegate { this.cleanUp(); };

			// Load config first.
			XmlConfigReader configReader = new XmlConfigReader("config.xml");
			this.Configuration = configReader.ReadConfig();
			Console.TimestampFormat = this.Configuration["timestamp"];
			this.maxConnections = int.Parse(this.Configuration["connlimit-main"]);

			Console.ShowInfo("Loaded configuration from 'config.xml'.");

			// Hook console events.
			Console.ControlEvent += new Console.ControlEventHandler(Console_ControlEvent);

			// Init the socket list.
			int i = maxConnections;
			while (i-- > 0)
			{
				unusedSockets.Push(i);
			}

			// Connect to the DB.
			#region Connect to the DB.
			ServerCore.MySQLHost = Configuration["mysql-host"];
			ServerCore.MySQLUsername = Configuration["mysql-user"];
			ServerCore.MySQLPassword = Configuration["mysql-pass"];
			ServerCore.MySQLDatabase = Configuration["mysql-database"];
			ServerCore.MySQLPort = ((Configuration["mysql-port"] != "") ? (int.Parse(Configuration["mysql-port"])) : (3306));
			DBConn = Net.ODBCFactory.Instance;
			ServerCore.MySQLPassword = "";

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
			this.listenSocket.Listen(4);

			// Start up the timers.
			pingTimer.Elapsed += new System.Timers.ElapsedEventHandler(Socket_PingTimer);
			errorTimer.Elapsed += new System.Timers.ElapsedEventHandler(Socket_ErrorCheck);
			mySQLPingTimer.Elapsed += new System.Timers.ElapsedEventHandler(MySQLPingTimer_Elapsed);

			// Start up the user manager.
			umd = new UserManageDaemon(this.Configuration);

			// Get a packet processor.
			this.packetProcess = PacketProcessor.getInstance();

			// Start queues.
			this.sendQ = new ServerSendQ(this);
			this.recvQ = new ServerRecvQ(this);

			// Start timers.
			this.pingTimer.Start();
			this.errorTimer.Start();
			this.mySQLPingTimer.Start();

			// Start listening.
			this.listenSocket.BeginAccept(__clientConnected, null);
			Console.ShowStatus("Demon is '\x1B[37mready\x1B[0m' and listening at " + listenSocket.LocalEndPoint + ".");

			while (Running)
			{
				this.listenSocket.Poll(40, SelectMode.SelectError);

				System.Threading.Thread.Sleep(100);
			}

			Console.ShowStatus("Demon is shutting down...disconnecting all users.");

			cleanUp();

			return 0;
		}

		private void cleanUp()
		{
			this.Running = false;

			try
			{
				// Stop timers.
				pingTimer.Stop();
				errorTimer.Stop();
				mySQLPingTimer.Stop();

				// Disconnect all sockets.
				foreach (DAmnUser CurrentClient in Clients)
				{
					if (CurrentClient == null) continue;
					CurrentClient.disconnect("shutdown");
				}

				// Flush SendQ.
				this.sendQ.Abort();
				this.recvQ.Abort();

				// Close DBC.
				this.DBConn.Close();
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

					lock (this.socketList)
					{
						// Set up the event listeners.
						socketList.Add(SocketID, null);
						socketList[SocketID] = new Net.Socket(SocketID, listenSocket.EndAccept(Result));
						socketList[SocketID].OnDataArrival += new Net.Socket.__OnDataArrival(Program_OnDataArrival);
						socketList[SocketID].OnDisconnect += new Net.Socket.__OnDisconnect(Program_OnDisconnect);
						socketList[SocketID].OnError += new Net.Socket.__OnError(Program_OnError);

						// Start listening for the handshake.
						socketList[SocketID].StartReceive();

						// Create a DAmnUser.
						DAmnUser user = new DAmnUser();
						user.Sockets.Add(socketList[SocketID]);
						socketList[SocketID].UserRef = user;

						lock (this.Clients)
						{
							Clients.Add(user);
						}
					}



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

			Packet origPacket = (Packet) System.Text.Encoding.ASCII.GetString(ByteArray);

			if (origPacket.cmd.Length < 1)
			{
				socketList[SocketID].UserRef.disconnect("bad data", SocketID);
			}
			else
			{
				this.recvQ.AddQueue(origPacket, SocketID);
			}
		}
		void Program_OnDisconnect(int SocketID, Net.SocketException Ex)
		{
			if (!socketList.ContainsKey(SocketID)) return;
			lock (this.socketList)
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
					Console.ShowInfo("\x1B[37m" + socketList[SocketID].Name + "\x1B[0m disconnected. Reason given: " + Ex.Message);
				}

				// Remove all references to the socket.
				DAmnUser user = socketList[SocketID].UserRef;

				user.Sockets.Remove(socketList[SocketID]);
				socketList[SocketID] = null;
				socketList.Remove(SocketID);
				unusedSockets.Push(SocketID);

				// Clean up if the user is gone.
				if (user.Sockets.Count == 0)
				{
					lock (this.Clients)
					{
						this.Clients.Remove(user);
					}

					if ((user.Username != null) && this.ClientNames.ContainsKey(user.Username))
					{
						lock (this.ClientNames)
						{
							this.ClientNames.Remove(user.Username);
						}
					}
				}
			}
		}
		void Program_OnError(int SocketID, Exception Ex)
		{
			Console.ShowError("An error occurred\n" + Ex.StackTrace + "\n" + Ex.Message);
		}
		#endregion

		#region Public ServerCore API
		public Net.Socket GetSocketById(int socketId)
		{
			if (this.socketList.ContainsKey(socketId))
				return this.socketList[socketId];
			else
				return null;
		}

		public void SendToSocket(Net.Socket sock, Packet packet)
		{
			this.sendQ.AddQueue(packet, sock.SocketID);
		}
		public void SendToSocket(Net.Socket sock, string packet)
		{
			this.sendQ.AddQueue(packet, sock.SocketID);
		}
		public void SendToSocket(Net.Socket sock, byte[] packet)
		{
			this.sendQ.AddQueue(packet, sock.SocketID);
		}

		public void SendToSocketID(int SocketID, Packet packet)
		{
			this.sendQ.AddQueue(packet, SocketID);
		}
		public void SendToSocketID(int SocketID, string packet)
		{
			this.sendQ.AddQueue(packet, SocketID);
		}
		public void SendToSocketID(int SocketID, byte[] packet)
		{
			this.sendQ.AddQueue(packet, SocketID);
		}
		#endregion

		#region Timed Events
		[MethodImpl(MethodImplOptions.Synchronized)]
		private void Socket_PingTimer(object sender, System.Timers.ElapsedEventArgs e)
		{
			//Console.ShowDebug("Pinging all clients...");
			Packet pingPacket = new Packet("ping", "");

			lock (this.Clients)
			{
				foreach (DAmnUser user in this.Clients)
				{
					if (user == null)
					{
						continue;
					}

					foreach (Net.Socket socket in user.Sockets)
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
		private void Socket_ErrorCheck(object sender, System.Timers.ElapsedEventArgs e)
		{
			//Console.ShowDebug("Cleaning up dead sockets...");

			lock (this.Clients)
			{
				Stack<Net.Socket> timedOutSockets = new Stack<Net.Socket>();

				foreach (DAmnUser user in this.Clients)
				{
					if (user == null)
					{
						continue;
					}


					foreach (Net.Socket socket in user.Sockets)
					{
						if (!socket.IsPinging) continue;
						if ((ServerCore.time() - socket.PingTime) >= 48)
						{
							socket.IsPinging = false;
							timedOutSockets.Push(socket);
						}
					}
				}

				while (timedOutSockets.Count > 0)
				{
					Net.Socket temp = timedOutSockets.Pop();
					temp.UserRef.disconnect("ping timeout", temp.SocketID);
				}
			}
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		private void MySQLPingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
		private void Console_ControlEvent(Console.ConsoleEvent consoleEvent)
		{
			Console.ShowInfo("Caught signal, preparing to shut down...");

			this.Running = false;

			System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
		}

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
