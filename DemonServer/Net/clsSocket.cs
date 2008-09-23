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
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;

using DemonServer.User;

namespace DemonServer.Net
{
	public class Socket
	{
		#region Private Properties
		private object a = new object();

		private System.Net.Sockets.Socket _InternalSocket;

		private int SockID;
		private byte[] buffer = new byte[2048];

		private bool _IsPinging = false;
		private int _PingTime = 0;

		private DAmnUser _userRef;

		private DateTime _timeConnected;
		private DateTime _lastData;
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
				return this.InternalSocket.RemoteEndPoint.ToString();
			}
		}

		public int PingTime
		{
			get { return _PingTime; }
			set { _PingTime = value; }
		}
		public bool IsPinging
		{
			get { return _IsPinging; }
			set { _IsPinging = value; }
		}

		public int SocketID
		{
			get
			{
				return this.SockID;
			}
		}

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
			SockID = SocketID;
			this.InternalSocket = ConnectedSocket;
		}
		#endregion

		#region Connection Functions
		public void Close() { Close(0); }
		public void Close(int Reason)
		{
			if (this._InternalSocket == null) return;
			try
			{
				this._InternalSocket.Shutdown(SocketShutdown.Both);
				this._InternalSocket.Disconnect(false);

				if (OnDisconnect != null) OnDisconnect(SockID, new SocketException(Reason));
			}
			catch (System.ObjectDisposedException) { }
			catch (Exception Ex)
			{
				if (OnError != null) OnError(SockID, Ex);
			}
			finally
			{
				try { this._InternalSocket.Close(); }
				catch { }
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
				if (OnDisconnect != null) OnDisconnect(SockID, Ex);
				return -1;
			}
		}
		public int SendPacket(string PacketString)
		{
			if (PacketString.Length == 0) return 0;
			if (this._InternalSocket.Connected == false) return -1;

			byte[] stringBytes = new byte[PacketString.Length];
			// Hackhack.
			for (int i = 0; i < (PacketString.Length - 1); i++)
			{
				if (PacketString[i] > 255)
				{
					// Should never happen...but...
					throw new ArgumentException("PacketString");
				}
				stringBytes[i] = (byte) PacketString[i];
			}

			return this.SendPacket(stringBytes);
		}
		#endregion

		#region Private Methods
		private void ClientDataSent(IAsyncResult Result)
		{
			try
			{
				int l = this._InternalSocket.EndReceive(Result);
				if (l <= 0)
				{
					// Disconnected.
					this.Close(0);
					return;
				}
				byte[] bytes = new byte[l];
				System.Buffer.BlockCopy(buffer, 0, bytes, 0, l);
				StartReceive();
				if (OnDataArrival != null) OnDataArrival(SockID, bytes);
			}
			catch (SocketException Ex)
			{
				if (OnDisconnect != null) OnDisconnect(SockID, Ex);
			}
			catch (ObjectDisposedException) { }
			catch (Exception Ex)
			{
				if (OnError != null) OnError(SockID, Ex);
			}
		}
		#endregion
	}
}