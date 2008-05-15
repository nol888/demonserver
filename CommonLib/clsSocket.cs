/*
+---------------------------------------------------------------------------+
|	Demon - dAmn Emulator													|
|===========================================================================|
|	Copyright © 2008 Nol888													|
|===========================================================================|
|	This file is part of Demon.												|
|																			|
|	Demon is free software: you can redistribute it and/or modify			|
|	it under the terms of the GNU General Public License as published by	|
|	the Free Software Foundation, either version 3 of the License, or		|
|	(at your option) any later version.										|
|																			|
|	Demon is distributed in the hope that it will be useful,				|
|	but WITHOUT ANY WARRANTY; without even the implied warranty of			|
|	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the			|
|	GNU General Public License for more details.							|
|																			|
|	You should have received a copy of the GNU General Public License		|
|	along with Foobar.  If not, see <http://www.gnu.org/licenses/>.			|
|===========================================================================|
|	> $Date$																|
|	> $Revision$															|
|	> $Author$																|
+---------------------------------------------------------------------------+
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;

namespace CommonLib
{
	public class Socket
	{
		#region Private Properties
		private object a = new object();

		private System.Net.Sockets.Socket _InternalSocket;

		private int SockID;
		private byte[] buffer = new byte[2048];

		private string AuthedName = "";
		private int AuthedID = 0;

		private bool _IsPinging = false;
		private int _PingTime = 0;
		#endregion

		#region Public Properties
		public System.Net.Sockets.Socket InternalSocket
		{
			get { return _InternalSocket; }
			set { _InternalSocket = value; }
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

		public string Name
		{
			get
			{
				if (this.AuthedName != "") { return this.AuthedName; }
				else { return this.InternalSocket.RemoteEndPoint.ToString(); }
			}
			set
			{
				if (this.AuthedName != "") { return; }
				else { this.AuthedName = value; }
			}
		}
		public int ClientID
		{
			get
			{
				return this.AuthedID;
			}
			set
			{
				if (this.AuthedID > 0) { return; }
				else { this.AuthedID = value; }
			}
		}
		public int SocketID
		{
			get
			{
				return this.SockID;
			}
		}
		#endregion

		#region Public Events
		public delegate void __OnDisconnect(int SocketID, SocketException Ex);
		public event __OnDisconnect OnDisconnect;

		public delegate void __OnError(int SocketID, Exception Ex);
		public event __OnError OnError;

		public delegate void __OnDataArrival(int SocketID, byte[] ByteArray, string HexData);
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
				string hex = EncryptLib.ByteArrayToHex(bytes);
				StartReceive();
				if (OnDataArrival != null) OnDataArrival(SockID, bytes, hex);
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