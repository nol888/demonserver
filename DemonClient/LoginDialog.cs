namespace DemonClient
{
    using dAmnSharp;
    using DemonClient.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Net.Sockets;
    using System.Text;
    using System.Windows.Forms;

    public class LoginDialog : Form
    {
        private string _pk;
		private byte[] buff = new byte[0x400];
        private StringBuilder sbText = new StringBuilder();

        public LoginDialog()
        {
            InitializeComponent();

            this.cbRemember.Checked = Settings.Default.remember;
            this.txtUsername.Text = Settings.Default.username;
            this.txtPassword.Text = Settings.Default.password;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Hide();
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string str = "";
            try
            {
                str = this.Login(this.txtUsername.Text, this.txtPassword.Text);
                if (str != "")
                {
                    this.pk = str;
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
                    base.DialogResult = DialogResult.OK;
                    base.Hide();
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

        private void cbRemember_CheckedChanged(object sender, EventArgs e)
        {
        }

        private bool Create(string username, string password)
        {
            this.sbText = new StringBuilder();
            TcpClient client = new TcpClient();
            client.Connect(Program.Server, 0xf3d);
            dAmnPacket packet = new dAmnPacket();
            string s = "create " + username.Trim() + "\npassword=" + password.Trim() + "\n\n\0";
            byte[] bytes = new ASCIIEncoding().GetBytes(s);
            client.GetStream().Write(bytes, 0, bytes.Length);
            int count = client.GetStream().Read(this.buff, 0, 0x400);
            string data = this.incBuildString(this.buff, 0, count);
            while (data == "")
            {
                count = client.GetStream().Read(this.buff, 0, 0x400);
                data = this.incBuildString(this.buff, 0, count);
            }
            return (dAmnPacket.parse(data).args["e"] == "ok");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void doValidate()
        {
            if (this.txtUsername.Text.Length <= 2)
            {
                this.SetEnabled(false);
            }
            else if (this.txtPassword.Text.Length <= 5)
            {
                this.SetEnabled(false);
            }
            else
            {
                this.SetEnabled(true);
            }
        }

        private string incBuildString(byte[] data, int offset, int count)
        {
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
            return "";
        }

        

        private string Login(string username, string password)
        {
            this.sbText = new StringBuilder();
            TcpClient client = new TcpClient();
            client.Connect(Program.Server, 0xf3d);
            string s = "login " + username.Trim() + "\npassword=" + password.Trim() + "\n\n\0";
            byte[] bytes = new ASCIIEncoding().GetBytes(s);
            client.GetStream().Write(bytes, 0, bytes.Length);
            int count = client.GetStream().Read(this.buff, 0, 0x400);
            string data = this.incBuildString(this.buff, 0, count);
            while (data == "")
            {
                count = client.GetStream().Read(this.buff, 0, 0x400);
                data = this.incBuildString(this.buff, 0, count);
            }
            dAmnPacket packet = dAmnPacket.parse(data);
            if (packet.args["e"] == "ok")
            {
                return packet.args["pk"];
            }
            return "";
        }

        private void LoginDialog_Load(object sender, EventArgs e)
        {
            this.doValidate();
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.doValidate();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            this.doValidate();
        }

        public string pk
        {
            get
            {
                return this._pk;
            }
            private set
            {
                this._pk = value;
            }
        }

        public string username
        {
            get
            {
                return this.txtUsername.Text;
            }
        }
    }
}

