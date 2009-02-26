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
using System.ComponentModel;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

using dAmnSharp;

using DemonClient.Properties;

namespace DemonClient
{
    public partial class LoginDialog : Form
    {
        private string _pk;
		private byte[] buff = new byte[0x400];
        private StringBuilder sbText = new StringBuilder();

		public string username
		{
			get
			{
				return this.txtUsername.Text;
			}
		}
		public string pk
		{
			get	{ return this._pk; }
			private set { this._pk = value; }
		}
		
        public LoginDialog()
        {
            InitializeComponent();

			// Load settings.
            this.cbRemember.Checked = Settings.Default.remember;
            this.txtUsername.Text = Settings.Default.username;
            this.txtPassword.Text = Settings.Default.password;
        }

		private void LoginDialog_Load(object sender, EventArgs e)
		{
			this.doValidate();
		}

		private void btnCreate_Click(object sender, EventArgs e)
		{
			this.SetEnabled(false);

			try
			{
				if (this.Create(this.txtUsername.Text, this.txtPassword.Text))
				{
					this.lblStatus.Text = "User " + this.txtUsername.Text + " created ok!";
				}
				else
				{
					this.lblStatus.Text = "User " + this.txtUsername.Text + " already exists!";
				}
			}
			catch
			{
				this.lblStatus.Text = "Internal Server Error";
			}

			this.SetEnabled(true);
		}
        private void btnCancel_Click(object sender, EventArgs e)
        {
			// Fail result.
            this.DialogResult = DialogResult.Cancel;
			this.Hide();
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string pkStr = "";

            try
            {
				pkStr = this.Login(this.txtUsername.Text, this.txtPassword.Text);
				if (pkStr != "")
                {
					this.pk = pkStr;

					// Save settings.
                    if (this.cbRemember.Checked)
                    {
                        Settings.Default.password = this.txtPassword.Text;
                        Settings.Default.username = this.txtUsername.Text;
                        Settings.Default.remember = true;
                    }
                    else
                    {
                        Settings.Default.Reset();
                    }

                    Settings.Default.Save();

					// Win result.
                    this.DialogResult = DialogResult.OK;
					this.Hide();
                }
                else
                {
                    this.lblStatus.Text = "Login failed.";
                }
            }
            catch
            {
                this.lblStatus.Text = "Login failed, Unable to communicate with authentication server.";
            }
        }

        private bool Create(string username, string password)
        {
            this.sbText = new StringBuilder();

			// Connect to auth port.
            TcpClient client = new TcpClient();
            client.Connect(Program.Server, 3901);

			// Create packet.
            string s = "create " + username.Trim() + "\npassword=" + password.Trim() + "\n\n\0";

			// Write to stream.
            byte[] bytes = new ASCIIEncoding().GetBytes(s);
            client.GetStream().Write(bytes, 0, bytes.Length);

			// Read response until \0.
            int count = client.GetStream().Read(this.buff, 0, 0x400);
            string data = this.incBuildString(this.buff, 0, count);
            while (data == "")
            {
                count = client.GetStream().Read(this.buff, 0, 0x400);
                data = this.incBuildString(this.buff, 0, count);
            }

			// Return TRUE if we returned ok.
            return (dAmnPacket.parse(data).args["e"] == "ok");
        }
		private string Login(string username, string password)
		{
			this.sbText = new StringBuilder();

			// Connect to auth port.
			TcpClient client = new TcpClient();
			client.Connect(Program.Server, 3901);

			// Create packet.
			string s = "login " + username.Trim() + "\npassword=" + password.Trim() + "\n\n\0";

			// Write to stream.
			byte[] bytes = new ASCIIEncoding().GetBytes(s);
			client.GetStream().Write(bytes, 0, bytes.Length);

			// Read response until \0.
			int count = client.GetStream().Read(this.buff, 0, 0x400);
			string data = this.incBuildString(this.buff, 0, count);
			while (data == "")
			{
				count = client.GetStream().Read(this.buff, 0, 0x400);
				data = this.incBuildString(this.buff, 0, count);
			}

			// Return the authtoken if we can.  Blank otherwise.
			dAmnPacket packet = dAmnPacket.parse(data);
			if (packet.args["e"] == "ok")
			{
				return packet.args["pk"];
			}

			return "";
		}

		private string incBuildString(byte[] data, int offset, int count)
		{
			// Builds a string up to count characters, breaking at the first \0 character.

			int index = 0;
			for (index = offset; index < (offset + count); index++)
			{
				if (data[index] == 0)
				{
					return this.sbText.ToString();
				}
				if (this.sbText.Length > 0x2710)
				{
					throw new InvalidOperationException("Message too big.");
				}
				this.sbText.Append((char) data[index]);
			}

			// Return empty if we couldn't build.
			return "";
		}

		private void doValidate()
		{
			// Check if we're valid.
			if ((this.txtUsername.Text.Trim().Length > 2) && (this.txtPassword.Text.Trim().Length > 5))
			{
				this.SetEnabled(true);
			}
			else
			{
				this.SetEnabled(false);
			}
		}

        private void SetEnabled(bool p)
        {
            this.btnLogin.Enabled = p;
            this.btnCreate.Enabled = p;
        }
        private void SettxtEnabled(bool p)
        {
            this.txtUsername.Enabled = p;
            this.txtPassword.Enabled = p;
        }

		private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            this.doValidate();
        }
        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            this.doValidate();
        }
    }
}

