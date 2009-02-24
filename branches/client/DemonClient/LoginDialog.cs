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
        private Button btnCancel;
        private Button btnCreate;
        private Button btnLogin;
        private byte[] buff = new byte[0x400];
        private CheckBox cbRemember;
        private IContainer components = null;
        private Label lblPassword;
        private Label lblStatus;
        private Label lblUsername;
        private StringBuilder sbText = new StringBuilder();
        private TextBox txtPassword;
        private TextBox txtUsername;

        public LoginDialog()
        {
            this.InitializeComponent();
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

        private void InitializeComponent()
        {
            this.btnLogin = new Button();
            this.btnCreate = new Button();
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.lblUsername = new Label();
            this.lblPassword = new Label();
            this.btnCancel = new Button();
            this.lblStatus = new Label();
            this.cbRemember = new CheckBox();
            base.SuspendLayout();
            this.btnLogin.Location = new Point(0x13, 0x63);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new Size(0x4b, 0x17);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Text = "&Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new EventHandler(this.btnLogin_Click);
            this.btnCreate.Location = new Point(100, 0x63);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new Size(0x4b, 0x17);
            this.btnCreate.TabIndex = 6;
            this.btnCreate.Text = "&Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new EventHandler(this.btnCreate_Click);
            this.txtUsername.Location = new Point(0x4d, 14);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new Size(180, 20);
            this.txtUsername.TabIndex = 1;
            this.txtUsername.TextChanged += new EventHandler(this.textBox1_TextChanged);
            this.txtPassword.Location = new Point(0x4d, 40);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new Size(180, 20);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.TextChanged += new EventHandler(this.txtPassword_TextChanged);
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new Point(0x10, 0x15);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new Size(0x37, 13);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "Username";
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new Point(0x10, 0x2e);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new Size(0x35, 13);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password";
            this.btnCancel.Location = new Point(0xb6, 0x63);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4b, 0x17);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new Point(0x10, 0x43);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(0, 13);
            this.lblStatus.TabIndex = 8;
            this.cbRemember.AutoSize = true;
            this.cbRemember.Location = new Point(0xa3, 0x52);
            this.cbRemember.Name = "cbRemember";
            this.cbRemember.Size = new Size(0x5e, 0x11);
            this.cbRemember.TabIndex = 4;
            this.cbRemember.Text = "Remember me";
            this.cbRemember.UseVisualStyleBackColor = true;
            this.cbRemember.CheckedChanged += new EventHandler(this.cbRemember_CheckedChanged);
            base.AcceptButton = this.btnLogin;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x11c, 0x80);
            base.Controls.Add(this.cbRemember);
            base.Controls.Add(this.lblStatus);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.lblPassword);
            base.Controls.Add(this.lblUsername);
            base.Controls.Add(this.txtPassword);
            base.Controls.Add(this.txtUsername);
            base.Controls.Add(this.btnCreate);
            base.Controls.Add(this.btnLogin);
            base.Name = "LoginDialog";
            this.Text = "LoginDialog";
            base.Load += new EventHandler(this.LoginDialog_Load);
            base.ResumeLayout(false);
            base.PerformLayout();
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

