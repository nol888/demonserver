namespace DemonClient
{
	partial class Main
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = new System.ComponentModel.Container();

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.wbDisplay = new System.Windows.Forms.WebBrowser();
			this.cbChatList = new System.Windows.Forms.ComboBox();
			this.txtChat = new System.Windows.Forms.TextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.tvUserTree = new System.Windows.Forms.TreeView();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.refreshChatlistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.chatroomsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// wbDisplay
			// 
			this.wbDisplay.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.wbDisplay.Location = new System.Drawing.Point(1, 54);
			this.wbDisplay.MinimumSize = new System.Drawing.Size(20, 20);
			this.wbDisplay.Name = "wbDisplay";
			this.wbDisplay.Size = new System.Drawing.Size(426, 322);
			this.wbDisplay.TabIndex = 0;
			this.wbDisplay.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			this.wbDisplay.WebBrowserShortcutsEnabled = false;
			// 
			// cbChatList
			// 
			this.cbChatList.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cbChatList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbChatList.FormattingEnabled = true;
			this.cbChatList.Location = new System.Drawing.Point(0, 27);
			this.cbChatList.Name = "cbChatList";
			this.cbChatList.Size = new System.Drawing.Size(571, 21);
			this.cbChatList.TabIndex = 1;
			this.cbChatList.SelectedIndexChanged += new System.EventHandler(this.cbChatList_SelectedIndexChanged);
			// 
			// txtChat
			// 
			this.txtChat.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtChat.Location = new System.Drawing.Point(1, 382);
			this.txtChat.Name = "txtChat";
			this.txtChat.Size = new System.Drawing.Size(483, 20);
			this.txtChat.TabIndex = 2;
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.Location = new System.Drawing.Point(491, 380);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(75, 23);
			this.btnSend.TabIndex = 3;
			this.btnSend.Text = "&Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// tvUserTree
			// 
			this.tvUserTree.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tvUserTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tvUserTree.Indent = 10;
			this.tvUserTree.Location = new System.Drawing.Point(433, 54);
			this.tvUserTree.Name = "tvUserTree";
			this.tvUserTree.ShowLines = false;
			this.tvUserTree.ShowPlusMinus = false;
			this.tvUserTree.ShowRootLines = false;
			this.tvUserTree.Size = new System.Drawing.Size(133, 322);
			this.tvUserTree.TabIndex = 4;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.chatroomsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(572, 24);
			this.menuStrip1.TabIndex = 5;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshChatlistToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// refreshChatlistToolStripMenuItem
			// 
			this.refreshChatlistToolStripMenuItem.Name = "refreshChatlistToolStripMenuItem";
			this.refreshChatlistToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			this.refreshChatlistToolStripMenuItem.Text = "&Refresh Chatlist";
			this.refreshChatlistToolStripMenuItem.Click += new System.EventHandler(this.refreshChatlistToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(153, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			this.exitToolStripMenuItem.Text = "&Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// chatroomsToolStripMenuItem
			// 
			this.chatroomsToolStripMenuItem.Name = "chatroomsToolStripMenuItem";
			this.chatroomsToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
			this.chatroomsToolStripMenuItem.Text = "&Chatrooms";
			// 
			// Main
			// 
			this.AcceptButton = this.btnSend;
			this.ClientSize = new System.Drawing.Size(572, 411);
			this.Controls.Add(this.tvUserTree);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.txtChat);
			this.Controls.Add(this.cbChatList);
			this.Controls.Add(this.wbDisplay);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Main";
			this.Text = "Demon Chat";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.TreeView tvUserTree;
		private System.Windows.Forms.TextBox txtChat;
		private System.Windows.Forms.WebBrowser wbDisplay;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.ComboBox cbChatList;
		private System.Windows.Forms.ToolStripMenuItem chatroomsToolStripMenuItem;
		private System.Windows.Forms.Timer queueTimer;
		private System.Windows.Forms.ToolStripMenuItem refreshChatlistToolStripMenuItem;
	}
}