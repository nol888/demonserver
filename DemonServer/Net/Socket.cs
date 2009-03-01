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
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using DemonServer.User;

namespace DemonServer.Net
{
	public class Socket
	{
		#region Private Properties
		private object objLock = new object();

		private System.Net.Sockets.Socket _InternalSocket;

		private byte[] buffer = new byte[2048];

		private DAmnUser _userRef;

		private DateTime _timeConnected;
		private DateTime _lastData;

		private bool _raisedDisconnect = true;
		private bool _socketDisposed = false;
		#endregion

		#region Public Properties
		public System.Net.Sockets.Socket InternalSocket
		{
			get { return _InternalSocket; }
			set { _InternalSocket = value; }
		}

		public string Name
		{
			get
			{
				if (!this.InternalSocket.Connected) return "Disconnected socket";
				return this.InternalSocket.RemoteEndPoint.ToString();
			}
		}

		public int PingTime { get; set; }
		public bool IsPinging { get; set; }

		public int SocketID { get; private set; }

		public int SecondsSinceConnected
		{
			get
			{
				TimeSpan timeSinceConnected = DateTime.Now - this._timeConnected;
				return (int) Math.Round(timeSinceConnected.TotalSeconds);
			}
		}
		public int SecondsSinceLast
		{
			get
			{
				TimeSpan timeSinceLast = DateTime.Now - this._lastData;
				return (int) Math.Round(timeSinceLast.TotalSeconds);
			}
		}

		public DAmnUser UserRef
		{
			get
			{
				return this._userRef;
			}
			set
			{
				if (this._userRef == null) this._userRef = value;
			}
		}
		#endregion

		#region Public Events
		public delegate void __OnDisconnect(int SocketID, SocketException Ex);
		public event __OnDisconnect OnDisconnect;

		public delegate void __OnError(int SocketID, Exception Ex);
		public event __OnError OnError;

		public delegate void __OnDataArrival(int SocketID, byte[] ByteArray);
		public event __OnDataArrival OnDataArrival;

		#endregion

		#region Constructor
		public Socket(int SocketID, System.Net.Sockets.Socket ConnectedSocket)
		{
			this.SocketID = SocketID;
			this.InternalSocket = ConnectedSocket;
			this._raisedDisconnect = false;
		}
		#endregion

		#region Connection Functions
		public void Close() { Close(0); }
		public void Close(int Reason) { Close(Reason, ""); }
		[MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
		public void Close(int Reason, string ReasonString)
		{
			if (this._InternalSocket == null) return;
			if (!this._InternalSocket.Connected) return;

			try
			{
				if (!this._raisedDisconnect)
				{
					this._raisedDisconnect = true;
					if (OnDisconnect != null) OnDisconnect(SocketID, new SocketException(Reason, ReasonString));
				}

				this._InternalSocket.Shutdown(SocketShutdown.Both);
				this._InternalSocket.Disconnect(false);
			}
			catch (Exception Ex)
			{
				if (OnError != null) OnError(SocketID, Ex);
			}
			finally
			{
				this._InternalSocket.Close();
				this._socketDisposed = true;
			}
		}
		#endregion

		#region Public Methods
		public void StartReceive()
		{
			buffer = new byte[2048];
			try
			{
				if (this._InternalSocket.Connected == false) return;
				this._InternalSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ClientDataSent), null);
			}
			catch (Exception)
			{
				return;
			}
		}
		public int SendPacket(byte[] PacketByteArray)
		{
			try
			{
				if (PacketByteArray.Length == 0) return 0;
				if (this._InternalSocket.Connected == false) return -1;
				return this._InternalSocket.Send(PacketByteArray);
			}
			catch (SocketException Ex)
			{
				if (!this._raisedDisconnect && !this._InternalSocket.Connected)
				{
					this._raisedDisconnect = true;
					if (OnDisconnect != null) OnDisconnect(SocketID, Ex);
				}
				return -1;
			}
		}
		public int SendPacket(string PacketString)
		{
			if (PacketString.Length == 0) return 0;
			if (this._InternalSocket.Connected == false) return -1;

			return this.SendPacket(Encoding.ASCII.GetBytes(PacketString));
		}
		public int SendPacket(DemonServer.Protocol.Packet Packet)
		{
			return this.SendPacket(Packet.ToString());
		}
		#endregion

		#region Private Methods
		private void ClientDataSent(IAsyncResult Result)
		{
			try
			{
				// First sanity check.
				if (this._socketDisposed) return;
				if (!this._InternalSocket.Connected) return;

				int l = 0;
				l = this._InternalSocket.EndReceive(Result);

				// More state validation.
				if (l <= 0)
				{
					this.Close(0);
					return;
				}

				byte[] bytes = new byte[l];
				System.Buffer.BlockCopy(buffer, 0, bytes, 0, l);
				StartReceive();
				if (OnDataArrival != null) OnDataArrival(SocketID, bytes);
			}
			catch (System.Net.Sockets.SocketException se)
			{
				// Ensure the OnDisconnect event is never triggered more than once.
				// Also ensure that the socket actually disconnected...
				if (!this._raisedDisconnect && !this._InternalSocket.Connected)
				{
					this._raisedDisconnect = true;
					if (OnDisconnect != null) OnDisconnect(SocketID, new SocketException(se.ErrorCode, "Error reading from socket."));
				}
			}
			catch (Exception Ex)
			{
				if (OnError != null) OnError(SocketID, Ex);
			}
		}
		#endregion
	}

	public class SocketException : System.Net.Sockets.SocketException
	{
		public SocketException() : base() { }
		public SocketException(int errorCode) : base(errorCode) { }
		public SocketException(int errorCode, string errorMessage)
			: base(errorCode)
		{
			this.Message = errorMessage;
		}

		public new string Message { get; protected set; }
	}
}