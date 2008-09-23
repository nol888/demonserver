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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace DemonServer.Net
{

	public sealed class DBConn
	{
		#region Private Properties
		private MySqlConnection InternalDBC;
		private MySqlTransaction CurrentTransaction;

		private MySqlException LastException;

		private string _sqlServer;
		private string _sqlDatabase;
		private string _sqlUsername;
		private int _sqlPort;
		#endregion

		#region Public Properties
		public string sqlServer
		{
			get { return this._sqlServer; }
		}
		public string sqlUsername
		{
			get { return this._sqlUsername; }
		}
		public string sqlDatabase
		{
			get { return this._sqlDatabase; }
		}
		public int sqlPort
		{
			get { return this._sqlPort; }
		}
		#endregion

		#region Constructors
		public DBConn(string Server, string Username, string Password, string Database) : this(Server, Username, Password, Database, 3306) { }
		public DBConn(string Server, string Username, string Password) : this(Server, Username, Password, "Test", 3306) { }
		public DBConn(string Server, string Username, string Password, string Database, int Port)
		{
			this._sqlServer = Server;
			this._sqlUsername = Username;
			this._sqlDatabase = Database;
			this._sqlPort = Port;
			this.InternalDBC = new MySqlConnection("Database=" + Database + ";Data Source=" + Server + ";User Id=" + Username + ";Password=" + Password + ";");
		}
		#endregion

		#region Public Functions
		public bool Connect()
		{
			if (this.InternalDBC == null) throw new ObjectDisposedException("CommonLib.DBConn");
			lock (this.InternalDBC)
			{
				try
				{
					this.InternalDBC.Open();
				}
				catch (MySqlException Ex)
				{
					this.LastException = Ex;
					return false;
				}
				return true;
			}
		}
		public void Close()
		{
			if (this.InternalDBC == null) throw new ObjectDisposedException("CommonLib.DBConn");

			lock (this.InternalDBC)
			{
				try
				{
					this.InternalDBC.Close();
				}
				catch (MySqlException Ex)
				{
					this.LastException = Ex;
				}
			}
		}
		public DBResult Query(string QueryString)
		{
			if (this.InternalDBC == null) throw new ObjectDisposedException("CommonLib.DBConn");

			lock (this.InternalDBC)
			{
				if (InternalDBC.State == System.Data.ConnectionState.Closed)
				{
					Console.ShowError("MySQL Error!  MySQL server has gone away.  Query: " + QueryString);
					return new DBResult();
				}
				MySqlCommand Command;
				MySqlDataReader Result;
				try
				{
					Command = new MySqlCommand(QueryString, this.InternalDBC);

					Result = Command.ExecuteReader();
					return new DBResult(Result);
				}
				catch (MySqlException Ex)
				{
					this.LastException = Ex;
					return new DBResult();
				}
			}
		}
		public DBResult Query(MySqlCommand Command)
		{
			if (this.InternalDBC == null) throw new ObjectDisposedException("CommonLib.DBConn");

			MySqlDataReader Result;

			try
			{
				Result = Command.ExecuteReader();
				return new DBResult(Result);
			}
			catch (MySqlException Ex)
			{
				this.LastException = Ex;
				return new DBResult();
			}
		}

		public MySqlCommand Prepare(string QueryString)
		{
			MySqlCommand Command = new MySqlCommand(QueryString, this.InternalDBC);
			Command.Prepare();

			return Command;
		}

		public bool IsConnected()
		{
			return (this.InternalDBC != null) ? (this.InternalDBC.State == System.Data.ConnectionState.Open) : false;
		}

		public void StartTransaction()
		{
			if (this.CurrentTransaction != null) return;

			lock (this.InternalDBC)
			{
				this.CurrentTransaction = InternalDBC.BeginTransaction();
			}
		}
		public void CommitTransaction()
		{
			if (this.CurrentTransaction == null) return;

			lock (this.InternalDBC)
			{
				lock (this.CurrentTransaction)
				{
					this.CurrentTransaction.Commit();
					this.CurrentTransaction = null;
				}
			}
		}
		public void RollbackTransaction()
		{
			if (this.CurrentTransaction == null) return;

			lock (this.InternalDBC)
			{
				lock (this.CurrentTransaction)
				{
					this.CurrentTransaction.Rollback();
					this.CurrentTransaction = null;
				}
			}
		}

		public int LastAutoIncrement()
		{
			throw new NotImplementedException();
		}

		public void Ping()
		{
			InternalDBC.Ping();
		}
		public string MySQL_Error(bool Keep)
		{
			if (this.LastException != null)
			{
				string Message = this.LastException.Message;
				if (!Keep) this.LastException = null;
				return Message;
			}
			else return "";
		}
		public string MySQL_Error()
		{
			return MySQL_Error(false);
		}

		public string EscapeString(string StringToEscape)
		{
			// Do a little replacing with the string.
			string ReturnValue = StringToEscape.Replace("'", "&#39;");

			ReturnValue = System.Text.RegularExpressions.Regex.Replace(StringToEscape, "\n", "");
			ReturnValue = System.Text.RegularExpressions.Regex.Replace(ReturnValue, "\r", "");

			return ReturnValue;
		}
		#endregion

		#region Private Functions
		#endregion
	}

	public struct DBResult : IDisposable
	{
		private int CurrentRow;
		private MySqlDataReader ClosedReader;

		private bool Disposed;

		public ArrayList Rows;

		public DBResult(MySqlDataReader Reader)
		{
			this.CurrentRow = 0;
			this.Disposed = false;
			Rows = new ArrayList();
			Hashtable Temp = new Hashtable();

			int i = 0;

			if (Reader.HasRows)
			{
				while (Reader.Read())
				{
					for (i = 0; i < Reader.FieldCount; i++)
					{
						Temp.Add(i, Reader.GetValue(i));
					}
					this.Rows.Add(Temp);
					Temp = new Hashtable();
				}
			}
			Reader.Close();
			this.ClosedReader = Reader;
		}

		public Hashtable FetchRow()
		{
			if (this.Disposed) throw new ObjectDisposedException("DBResult");

			return (Hashtable) this.Rows[CurrentRow++];
		}

		public int GetNumRows()
		{
			if (this.Disposed) throw new ObjectDisposedException("DBResult");

			return this.Rows.Count;
		}
		public int GetAffectedRows()
		{
			if (this.Disposed) throw new ObjectDisposedException("DBResult");

			if (this.ClosedReader == null) { return 0; }
			return this.ClosedReader.RecordsAffected;
		}

		public bool RowsLeft
		{
			get
			{
				if (this.Disposed) throw new ObjectDisposedException("DBResult");

				if (CurrentRow >= GetNumRows()) return false;
				else return true;
			}
		}

		#region IDisposable Members
		void IDisposable.Dispose()
		{
			this.ClosedReader.Dispose();
			this.CurrentRow = -1;
			this.Rows = null;

			this.Disposed = true;
		}
		#endregion
	}
}
