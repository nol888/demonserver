namespace DemonClient
{
    using dAmnSharp;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    public class Main : Form
    {
        private Button btnSend;
        private ComboBox cbChatList;
        private ToolStripMenuItem chatroomsToolStripMenuItem;
        private IContainer components = null;
        private dAmn dAmnConnection = new dAmn();
        private Queue<PendingHTMLWrite> docQueue = new Queue<PendingHTMLWrite>();
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem;
        private states lastState = states.DISCONNECTED;
        private Dictionary<string, Dictionary<string, Userinfo>> members = new Dictionary<string, Dictionary<string, Userinfo>>();
        private MenuStrip menuStrip1;
        private Dictionary<string, Dictionary<string, privclass>> privclassorders = new Dictionary<string, Dictionary<string, privclass>>();
        private Timer queueTimer;
        private ToolStripMenuItem refreshChatlistToolStripMenuItem;
        private List<RoomInfo> Rooms = new List<RoomInfo>();
        private ToolStripSeparator toolStripSeparator1;
        private TreeView tvUserTree;
        private TextBox txtChat;
        private WebBrowser wbDisplay;

        public Main()
        {
			this.components = new Container();

            this.InitializeComponent();
            this.dAmnConnection.server = Program.Server;
            this.dAmnConnection.port = 0xf3c;
            this.dAmnConnection.SelfJoin += new dAmn.dlgJP(this.dAmnConnection_SelfJoin);
            this.dAmnConnection.SelfPart += new dAmn.dlgJP(this.dAmnConnection_SelfPart);
            this.dAmnConnection.PropertyUpdate += new dAmn.dlgPropUpdate(this.dAmnConnection_PropertyUpdate);
            this.dAmnConnection.Privchg += new dAmn.dlgPrivchg(this.dAmnConnection_Privchg);
            this.dAmnConnection.Kicked += new dAmn.dlgKicked(this.dAmnConnection_Kicked);
            this.dAmnConnection.Kick += new dAmn.dlgKicked(this.dAmnConnection_Kick);
            this.dAmnConnection.Error += new dAmn.dlgError(this.dAmnConnection_Error);
            this.dAmnConnection.Message += new dAmn.dlgMessage(this.dAmnConnection_Message);
            this.dAmnConnection.StateChange += new dAmn.dlgState(this.dAmnConnection_StateChange);
            this.dAmnConnection.Join += new dAmn.dlgJoinPart(this.dAmnConnection_Join);
            this.dAmnConnection.Part += new dAmn.dlgJoinPart(this.dAmnConnection_Part);
            this.dAmnConnection.RawPacket += new dAmn.dlgPacket(this.dAmnConnection_RawPacket);
            this.wbDisplay.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.wbDisplay_DocumentCompleted);
            this.WriteHTML("system:system", "Demon Client startup complete.");
            this.addToChatList("system:system");
            this.cbChatList.SelectedIndex = 0;
        }

        private void addToChatList(string channel)
        {
            if (channel.StartsWith("system:"))
            {
                channel = channel.Split(new char[] { ':' })[1];
            }
            else
            {
                channel = this.dAmnConnection.formatNamespace(channel);
            }
            if (!this.cbChatList.Items.Contains(channel))
            {
                this.cbChatList.Items.Add(channel);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string text = this.txtChat.Text;
            this.txtChat.Clear();
            this.txtChat.Focus();
            string room = this.parseNS((string) this.cbChatList.SelectedItem);
            if (text.StartsWith("/"))
            {
                string str3 = text.Substring(1);
                int index = str3.IndexOf(' ');
                if (index >= 0)
                {
                    str3 = str3.Substring(0, index);
                    if (text.Length > (index + 2))
                    {
                        text = text.Substring(index + 2);
                    }
                    else
                    {
                        text = "";
                    }
                }
                else
                {
                    text = "";
                }
                switch (str3.ToLower().Trim())
                {
                    case "join":
                        this.dAmnConnection.join(text);
                        return;

                    case "part":
                        this.dAmnConnection.part(text);
                        return;

                    case "admin":
                    case "kick":
                    case "kill":
                    case "title":
                    case "topic":
                        this.WriteHTML(room, "<b>That command is not currently implemented. Sorry.</b>");
                        return;

                    case "raw":
                        this.dAmnConnection.raw(text);
                        return;
                }
                this.WriteHTML(room, string.Format("<b>{0} is not a valid command.</b>", str3));
            }
            else
            {
                this.dAmnConnection.say(room, text.Replace(@"\", @"\\"));
            }
        }

        private void cbChatList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.hideAll();
                string selectedItem = (string) this.cbChatList.SelectedItem;
                selectedItem = (selectedItem.StartsWith("#") || selectedItem.StartsWith("@")) ? this.dAmnConnection.parseNamespace(selectedItem) : ("system:" + selectedItem);
                this.makePaneVisible(selectedItem);
                this.DoMemberUpdate(selectedItem);
            }
            catch
            {
            }
        }

        private void Chatroom_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem) sender;
            this.dAmnConnection.join(item.Text);
        }

        private void createChatPane(string id)
        {
            HtmlElement newElement = this.wbDisplay.Document.CreateElement("div");
            newElement.Id = id;
            newElement.Style = "display:none";
            this.wbDisplay.Document.Body.AppendChild(newElement);
        }

        private HtmlElement CreateElement(string tagName)
        {
            return this.wbDisplay.Document.CreateElement(tagName);
        }

        private void dAmnConnection_Error(string cmd, string param, string e)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgError(this.dAmnConnection_Error), new object[] { cmd, param, e });
            }
            else
            {
                string format = "{0} error ({1}): {2}";
                this.WriteHTML((((string) this.cbChatList.SelectedItem).StartsWith("@") || ((string) this.cbChatList.SelectedItem).StartsWith("#")) ? this.dAmnConnection.parseNamespace((string) this.cbChatList.SelectedItem) : ("system:" + ((string) this.cbChatList.SelectedItem)), string.Format(format, cmd, param, e));
            }
        }

        private void dAmnConnection_Join(string room, string user, string reason, bool hidden)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgJoinPart(this.dAmnConnection_Join), new object[] { room, user, reason, hidden });
            }
            else
            {
                if (!hidden)
                {
                    this.WriteHTML(room, string.Format("<b>** {0} has joined</b>", user), "font-style: bold");
                }
                this.members[room] = new Dictionary<string, Userinfo>(this.dAmnConnection[room].UserDict);
                this.DoMemberUpdate(room);
            }
        }

        private void dAmnConnection_Kick(string room, string who, string by, string reason)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgKicked(this.dAmnConnection_Kick), new object[] { room, who, by, reason });
            }
            else
            {
                this.WriteHTML(room, string.Format("<b>** {0} was kicked by {1} {2} *", who, by, reason), "font-style: bold");
            }
        }

        private void dAmnConnection_Kicked(string room, string who, string by, string reason)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgKicked(this.dAmnConnection_Kicked), new object[] { room, who, by, reason });
            }
            else
            {
                this.WriteHTML(room, string.Format("<b>*** You have been kicked by {0} {1} *", by, reason), "font-style: bold");
            }
        }

        private void dAmnConnection_Message(string room, string from, string message, bool action)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgMessage(this.dAmnConnection_Message), new object[] { room, from, message, action });
            }
            else
            {
                string str;
                if (!Regex.IsMatch(message, @"\b" + Regex.Escape(this.dAmnConnection.username) + @"\b", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                {
                    str = "<table border='0' cellpadding='0' cellspacing='0' style='font-size:12px'><tr>";
                }
                else
                {
                    str = "<table border='0' cellpadding='0' cellspacing='0' style='font-size:12px; background-color:#fafafa'><tr>";
                }
                str = str + (action ? "<td><b>*<i>&nbsp;{1}&nbsp;</i></b></td><td><i>{2}</i></td>" : "<td><b>&lt;{1}&gt;&nbsp;</b></td><td>{2}</td></tr></table>");
                this.WriteHTML(room, string.Format(str, this.dAmnConnection.formatNamespace(room), from, dAmn.tablumps.Parse(message)));
            }
        }

        private void dAmnConnection_Part(string room, string user, string reason, bool hidden)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgJoinPart(this.dAmnConnection_Part), new object[] { room, user, reason, hidden });
            }
            else
            {
                if (!hidden)
                {
                    this.WriteHTML(room, string.Format("<b>** {0} has left [ {1} ]</b>", user, (reason == "") ? "no reason" : reason), "font-style: bold");
                }
                this.members[room] = new Dictionary<string, Userinfo>(this.dAmnConnection[room].UserDict);
                this.DoMemberUpdate(room);
            }
        }

        private void dAmnConnection_Privchg(string room, string who, string by, string pc)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgPrivchg(this.dAmnConnection_Privchg), new object[] { room, who, by, pc });
            }
            else
            {
                this.WriteHTML(room, string.Format("<b>** {0} has been made a member of {1} by {2}*</b>", who, pc, by), "font-style: bold");
                this.members[room] = new Dictionary<string, Userinfo>(this.dAmnConnection[room].UserDict);
                this.DoMemberUpdate(room);
            }
        }

        private void dAmnConnection_PropertyUpdate(string property, string room)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgPropUpdate(this.dAmnConnection_PropertyUpdate), new object[] { property, room });
            }
            else
            {
                string str = property;
                if (str != null)
                {
                    if (!(str == "privclasses"))
                    {
                        if (str == "members")
                        {
                            this.members[room] = new Dictionary<string, Userinfo>(this.dAmnConnection[room].UserDict);
                            this.DoMemberUpdate(room);
                        }
                        else if (str == "title")
                        {
                            this.WriteHTML(room, string.Format("<b>** {0} changed by {1} on {2} *</b>", property, this.dAmnConnection[room].Title.SetBy, this.dAmnConnection[room].Title.Local.ToString()));
                        }
                        else if (str == "topic")
                        {
                            this.WriteHTML(room, string.Format("<b>** {0} changed by {1} on {2} *</b>", property, this.dAmnConnection[room].Topic.SetBy, this.dAmnConnection[room].Topic.Local.ToString()));
                        }
                    }
                    else
                    {
                        this.privclassorders[room] = new Dictionary<string, privclass>(this.dAmnConnection[room].PrivclassByName);
                    }
                }
            }
        }

        private void dAmnConnection_RawPacket(dAmnPacket p)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgPacket(this.dAmnConnection_RawPacket), new object[] { p });
            }
            else
            {
                string cmd = p.cmd;
                if (((cmd != null) && (cmd == "property")) && ((p.param == "server:info") && (p.args["p"] == "rooms")))
                {
                    lock (this.Rooms)
                    {
                        this.Rooms = new List<RoomInfo>();
                        while (p.body != "")
                        {
                            RoomInfo info;
                            p = dAmnPacket.parse(p.body);
                            info.fullname = p.param;
                            info.ownedby = p.args["owner"];
                            this.Rooms.Add(info);
                        }
                    }
                    this.UpdateRoomMenu();
                }
            }
        }

        private void dAmnConnection_SelfJoin(string room, string r)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgJP(this.dAmnConnection_SelfJoin), new object[] { room, r });
            }
            else
            {
                if (!this.hasChatPane(room))
                {
                    this.createChatPane(room);
                    this.cbChatList_SelectedIndexChanged(null, null);
                }
                this.addToChatList(room);
                this.cbChatList.SelectedIndex = this.cbChatList.Items.IndexOf(this.dAmnConnection.formatNamespace(room));
                this.WriteHTML(room, string.Format("<b>*** You have joined {0} *</b>", this.dAmnConnection.formatNamespace(room)));
            }
        }

        private void dAmnConnection_SelfPart(string room, string r)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new dAmn.dlgJP(this.dAmnConnection_SelfPart), new object[] { room, r });
            }
            else
            {
                this.delFromChatList(room);
                this.cbChatList_SelectedIndexChanged(null, null);
            }
        }

        private void dAmnConnection_StateChange(states State)
        {
            if (this.lastState != State)
            {
                if (base.InvokeRequired)
                {
                    base.Invoke(new dlgSC(this.dAmnConnection_StateChange), new object[] { State });
                }
                else
                {
                    this.lastState = State;
                    switch (State)
                    {
                        case states.DISCONNECTED:
                            this.WriteHTML("system:system", "Disconnected: " + this.dAmnConnection.LastError);
                            if (MessageBox.Show("You have been Disconnected, Would you like to re-connect?", "Disconnected", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                this.dAmnConnection.Connect();
                            }
                            return;

                        case states.DISCONNECTING:
                        case states.FETCHINGCOOKIE:
                            return;

                        case states.CONNECTING:
                            this.WriteHTML("system:system", "Connecting...");
                            return;

                        case states.CONNECTED:
                            this.WriteHTML("system:system", "Connected to " + this.dAmnConnection.server + "<br>Handshaking");
                            return;

                        case states.HANDSHAKING:
                            this.WriteHTML("system:system", "Handshaking...");
                            return;

                        case states.AUTHENTICATING:
                            this.WriteHTML("system:system", "Logging in...");
                            return;

                        case states.LOGGEDIN:
                            this.WriteHTML("system:system", "Logged in successfully as: " + this.dAmnConnection.username);
                            this.dAmnConnection.join("chat:servermessages");
                            this.dAmnConnection.raw("get server:info\np=rooms\n\n");
                            return;
                    }
                }
            }
        }

        private void delFromChatList(string r)
        {
            r = this.dAmnConnection.formatNamespace(r);
            if (this.cbChatList.Items.Contains(r))
            {
                this.cbChatList.Items.Remove(r);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DoMemberUpdate(string room)
        {
            if (room.StartsWith("system:"))
            {
                this.tvUserTree.Nodes.Clear();
            }
            else
            {
                string str = this.dAmnConnection.formatNamespace(room);
                if ((this.cbChatList.SelectedItem.ToString().ToLower().Trim() == str.ToLower().Trim()) && this.members.ContainsKey(room))
                {
                    this.tvUserTree.Nodes.Clear();
                    Dictionary<string, Userinfo> dictionary = new Dictionary<string, Userinfo>(this.members[room]);
                    Dictionary<string, privclass> dictionary2 = new Dictionary<string, privclass>(this.privclassorders[room]);
                    Dictionary<int, TreeNode> dictionary3 = new Dictionary<int, TreeNode>();
                    Dictionary<string, int> dictionary4 = new Dictionary<string, int>();
                    foreach (KeyValuePair<string, privclass> pair in dictionary2)
                    {
                        dictionary4[pair.Value.Name.ToLower().Trim()] = pair.Value.Order;
                        dictionary3[pair.Value.Order] = new TreeNode(pair.Value.Name);
                    }
                    foreach (KeyValuePair<string, Userinfo> pair2 in dictionary)
                    {
                        TreeNode node = new TreeNode(pair2.Value.properties["symbol"] + pair2.Value.username);
                        dictionary3[dictionary4[pair2.Value.properties["pc"].ToLower().Trim()]].Nodes.Add(node);
                        node.ToolTipText = string.Format("* {0}\n* {1}\nGPC: {2}", pair2.Value.properties["realname"], pair2.Value.properties["typename"], pair2.Value.properties["gpc"]);
                    }
                    for (int i = 0x63; i > 0; i--)
                    {
                        if (dictionary3.ContainsKey(i) && (dictionary3[i].Nodes.Count > 0))
                        {
                            this.tvUserTree.Nodes.Add(dictionary3[i]);
                        }
                    }
                    this.tvUserTree.ExpandAll();
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoginDialog dialog = new LoginDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.dAmnConnection.username = dialog.username;
                this.dAmnConnection.password = dialog.pk;
                this.dAmnConnection.Connect();
            }
            else
            {
                Application.Exit();
            }
        }

        private HtmlElement getChatPane(string id)
        {
            if (!this.hasChatPane(id))
            {
                this.createChatPane(id);
            }
            return this.wbDisplay.Document.GetElementById(id);
        }

        private bool hasChatPane(string id)
        {
            try
            {
                if (this.wbDisplay.Document.GetElementById(id) != null)
                {
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        private void hideAll()
        {
            foreach (HtmlElement element in this.wbDisplay.Document.Body.Children)
            {
                element.Style = "display:none";
            }
        }

        private void InitializeComponent()
        {
            this.wbDisplay = new WebBrowser();
            this.cbChatList = new ComboBox();
            this.txtChat = new TextBox();
            this.btnSend = new Button();
            this.queueTimer = new Timer(this.components);
            this.tvUserTree = new TreeView();
            this.menuStrip1 = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.chatroomsToolStripMenuItem = new ToolStripMenuItem();
            this.refreshChatlistToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.menuStrip1.SuspendLayout();
            base.SuspendLayout();
            this.wbDisplay.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.wbDisplay.Location = new Point(1, 0x36);
            this.wbDisplay.MinimumSize = new Size(20, 20);
            this.wbDisplay.Name = "wbDisplay";
            this.wbDisplay.Size = new Size(0x1aa, 0x142);
            this.wbDisplay.TabIndex = 0;
            this.wbDisplay.Url = new Uri("about:blank", UriKind.Absolute);
            this.wbDisplay.WebBrowserShortcutsEnabled = false;
            this.cbChatList.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.cbChatList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbChatList.FormattingEnabled = true;
            this.cbChatList.Location = new Point(0, 0x1b);
            this.cbChatList.Name = "cbChatList";
            this.cbChatList.Size = new Size(0x23b, 0x15);
            this.cbChatList.TabIndex = 1;
            this.cbChatList.SelectedIndexChanged += new EventHandler(this.cbChatList_SelectedIndexChanged);
            this.txtChat.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this.txtChat.Location = new Point(1, 0x17e);
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new Size(0x1e3, 20);
            this.txtChat.TabIndex = 2;
            this.btnSend.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnSend.Location = new Point(0x1eb, 0x17a);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new Size(0x4b, 0x17);
            this.btnSend.TabIndex = 3;
            this.btnSend.Text = "&Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new EventHandler(this.btnSend_Click);
            this.queueTimer.Interval = 250;
            this.queueTimer.Tick += new EventHandler(this.queueTimer_Tick);
            this.tvUserTree.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            this.tvUserTree.BorderStyle = BorderStyle.None;
            this.tvUserTree.Indent = 10;
            this.tvUserTree.Location = new Point(0x1b1, 0x36);
            this.tvUserTree.Name = "tvUserTree";
            this.tvUserTree.ShowLines = false;
            this.tvUserTree.ShowPlusMinus = false;
            this.tvUserTree.ShowRootLines = false;
            this.tvUserTree.Size = new Size(0x85, 0x142);
            this.tvUserTree.TabIndex = 4;
            this.menuStrip1.Items.AddRange(new ToolStripItem[] { this.fileToolStripMenuItem, this.chatroomsToolStripMenuItem });
            this.menuStrip1.Location = new Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new Size(0x23c, 0x18);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.refreshChatlistToolStripMenuItem, this.toolStripSeparator1, this.exitToolStripMenuItem });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(0x25, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new Size(0x98, 0x16);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new EventHandler(this.exitToolStripMenuItem_Click);
            this.chatroomsToolStripMenuItem.Name = "chatroomsToolStripMenuItem";
            this.chatroomsToolStripMenuItem.Size = new Size(0x4e, 20);
            this.chatroomsToolStripMenuItem.Text = "&Chatrooms";
            this.refreshChatlistToolStripMenuItem.Name = "refreshChatlistToolStripMenuItem";
            this.refreshChatlistToolStripMenuItem.Size = new Size(0x9c, 0x16);
            this.refreshChatlistToolStripMenuItem.Text = "&Refresh Chatlist";
            this.refreshChatlistToolStripMenuItem.Click += new EventHandler(this.refreshChatlistToolStripMenuItem_Click);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new Size(0x99, 6);
            base.AcceptButton = this.btnSend;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.ClientSize = new Size(0x23c, 0x19b);
            base.Controls.Add(this.tvUserTree);
            base.Controls.Add(this.btnSend);
            base.Controls.Add(this.txtChat);
            base.Controls.Add(this.cbChatList);
            base.Controls.Add(this.wbDisplay);
            base.Controls.Add(this.menuStrip1);
            base.MainMenuStrip = this.menuStrip1;
            base.Name = "Main";
            this.Text = "Demon Chat";
            base.Load += new EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void makePaneVisible(string id)
        {
            HtmlElement elementById = this.wbDisplay.Document.GetElementById(id);
            if (elementById != null)
            {
                elementById.Style = "display:inline";
            }
        }

        private string parseNS(string paneId)
        {
            return ((paneId.StartsWith("#") || paneId.StartsWith("@")) ? this.dAmnConnection.parseNamespace(paneId) : ("system:" + paneId));
        }

        private void ProcessQueue()
        {
            lock (this.docQueue)
            {
                while (this.docQueue.Count > 0)
                {
                    PendingHTMLWrite write = this.docQueue.Peek();
                    try
                    {
                        if (!this.WriteHTML(write.room, write.html, write.style, true))
                        {
                            this.queueTimer.Enabled = true;
                            break;
                        }
                        this.docQueue.Dequeue();
                    }
                    catch
                    {
                        this.queueTimer.Enabled = true;
                        break;
                    }
                }
            }
        }

        private void queueTimer_Tick(object sender, EventArgs e)
        {
            this.queueTimer.Enabled = false;
            this.ProcessQueue();
        }

        private void refreshChatlistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dAmnConnection.raw("get server:info\np=rooms\n\n");
        }

        private void UpdateRoomMenu()
        {
            lock (this.Rooms)
            {
                this.chatroomsToolStripMenuItem.DropDownItems.Clear();
                for (int i = 0; i < this.Rooms.Count; i++)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem();
                    item.Text = this.dAmnConnection.formatNamespace(this.Rooms[i].fullname);
                    item.ToolTipText = "Created by: " + this.Rooms[i].ownedby;
                    item.Click += new EventHandler(this.Chatroom_Click);
                    this.chatroomsToolStripMenuItem.DropDownItems.Add(item);
                }
            }
        }

        private void wbDisplay_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.wbDisplay.Document.Body.Style = "font-size:12px; background-color: #dedede";
            this.ProcessQueue();
            this.hideAll();
            this.makePaneVisible("system:system");
        }

        private void WriteHTML(string room, string html)
        {
            this.WriteHTML(room, html, "");
        }

        private bool WriteHTML(string room, string html, bool rewrite)
        {
            return this.WriteHTML(room, html, "", rewrite);
        }

        private void WriteHTML(string room, string html, string style)
        {
            this.WriteHTML(room, html, style, false);
        }

        private bool WriteHTML(string room, string html, string style, bool rewrite)
        {
            PendingHTMLWrite write;
            Queue<PendingHTMLWrite> queue;
            if (base.InvokeRequired)
            {
                return false;
            }
            try
            {
                if (this.wbDisplay.ReadyState == WebBrowserReadyState.Complete)
                {
                    HtmlElement newElement = this.CreateElement("div");
                    newElement.SetAttribute("class", "default-chat main-text");
                    newElement.InnerHtml = html;
                    newElement.Style = style;
                    this.getChatPane(room).AppendChild(newElement);
                    newElement.ScrollIntoView(true);
                    return true;
                }
                if (!rewrite)
                {
                    write.html = html;
                    write.room = room;
                    write.style = style;
                    lock ((queue = this.docQueue))
                    {
                        this.docQueue.Enqueue(write);
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                if (!rewrite)
                {
                    write.html = html;
                    write.room = room;
                    write.style = style;
                    lock ((queue = this.docQueue))
                    {
                        this.docQueue.Enqueue(write);
                    }
                    return true;
                }
                return false;
            }
        }

        private delegate void dlgSC(states state);

        [StructLayout(LayoutKind.Sequential)]
        private struct PendingHTMLWrite
        {
            public string room;
            public string html;
            public string style;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RoomInfo
        {
            public string fullname;
            public string ownedby;
        }
    }
}

