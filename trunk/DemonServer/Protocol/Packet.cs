/*
+===========================================================================+
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
+===========================================================================+
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemonServer.Protocol
{
	public class Packet
	{
		#region Private Properties
		private string _cmd = "";
		private string _param = "";
		private Dictionary<string, string> _args;
		private string _body;
		#endregion

		#region Constructors
		public Packet() : this("", "")	{}
		public Packet(string cmd, string param){
			this._cmd = cmd;
			this._param = param;
			this._args = new Dictionary<string, string>();
			this._body = "";
		}

		public Packet(string data)
		{
			int lineLength, spacePos, valuePos;
			string firstLine, nextLines;

			lineLength = data.IndexOf('\n');
			if (lineLength < 0)
			{
				// Ugh, we got a bad one here.
				this._cmd = "";
				this._param = "";
				this._args = new Dictionary<string, string>();
				this._body = "";
				return;
			}

			firstLine = data.Substring(0, lineLength);

			this.cmd = firstLine.Split(' ')[0];

			spacePos = firstLine.IndexOf(' ');
			this.param = (spacePos > 0) ? (firstLine.Substring(spacePos + 1)) : ("");

			nextLines = data.Substring(lineLength + 1);

			this.args = new Dictionary<string, string>();
			this.body = null;

			while (true)
			{
				if ((nextLines.Length == 0) || (nextLines[0] == '\n') || (nextLines[0] == '\0')) break;

				lineLength = nextLines.IndexOf('\n');
				if (lineLength == -1) lineLength = nextLines.IndexOf('\0'); // Try null as terminator.
				valuePos = nextLines.IndexOf('=');

				if (valuePos > lineLength) break;

				this.args[nextLines.Substring(0, valuePos)] = nextLines.Substring(valuePos + 1, lineLength - (valuePos + 1));
				nextLines = nextLines.Substring(lineLength + 1);
			}

			if ((nextLines != null) && (nextLines.Length > 0) && (nextLines[0] != '\0')) this.body = nextLines.Substring(1);
			else this.body = "";
		}
		#endregion

		#region Public Properties
		public string cmd
		{
			get { return this._cmd; }
			set { this._cmd = value; }
		}
		public string param
		{
			get { return this._param; }
			set { this._param = value; }
		}
		public string body
		{
			get { return this._body; }
			set { this._body = value; }
		}
		public Dictionary<string, string> args
		{
			get { return this._args; }
			set { this._args = value; }
		}
		#endregion

		#region Public Methods
		public override string ToString()
		{
			string packetStr = "";

			packetStr += this.cmd;
			if (this.param != "") packetStr += " " + this.param + "\n";
			else packetStr += "\n";

			if (this.args.Count > 0)
			{
				foreach (KeyValuePair<string, string> arg in this.args)
				{
					packetStr += string.Format("{0}={1}\n", arg.Key, arg.Value);
				}
			}

			packetStr += "\n";

			packetStr += this.body;

			return packetStr;
		}
		#endregion

		#region Cast Operators
		public static explicit operator Packet(string data)
		{
			return new Packet(data);
		}
		public static explicit operator string(Packet packet)
		{
			return packet.ToString();
		}
		#endregion
	}
}
