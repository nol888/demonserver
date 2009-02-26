using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace dAmnSharp
{
    public class dAmn
    {
        private string _lastError;
        private string _password;
        private int _port;
        private string _server;
        private double _sv;
        private string _username;
        private bool attached;
        private byte[] bData;
        private TcpClient dAmnSocket;
        private states myState;
        private object objLock;
        private object PacketLock;
        private Queue<dAmnPacket> PacketQueue;
        private bool processing;
        private object ProcessingLock;
        private object QueueLock;
        private bool recreated;
        private Dictionary<string, dAroom> rooms;
        private StringBuilder sbText;

        public event dlgAdmin_CreateUpdate Admin_Create;
        public event dlgAdmin_RenameMove Admin_Move;
        public event dlgAdmin_Remove Admin_Remove;
        public event dlgAdmin_RenameMove Admin_Rename;
        public event dlgAdmin_CreateUpdate Admin_Update;
        public event dlgError Error;
        public event dlgJoinPart Join;
        public event dlgKicked Kick;
        public event dlgKicked Kicked;
        public event dlgMessage Message;
        private event dlgPacket packet;
        public event dlgJoinPart Part;
        public event dlgPrivchg Privchg;
        public event dlgPropUpdate PropertyUpdate;
        public event dlgPacket RawPacket;
        public event dlgJP SelfJoin;
        public event dlgJP SelfPart;
        public event dlgState StateChange;

        public dAmn() : this("", "") { }

        public dAmn(string user, string pass)
        {
            this.attached = false;
            this.PacketQueue = new Queue<dAmnPacket>();
            this.QueueLock = new object();
            this.processing = false;
            this.ProcessingLock = new object();
            this.PacketLock = new object();
            this.rooms = new Dictionary<string, dAroom>();
            this._lastError = "";
			this._username = user;
			this._password = pass;
            this.bData = new byte[0x400];
            this.sbText = new StringBuilder();
            this.objLock = new object();
            this.myState = states.DISCONNECTED;
            this._server = "chat.deviantart.com";
            this._port = 3900;
            this.recreated = false;
            this.__init__();
        }

        private void __init__()
        {
            this.username = "";
            this.password = "";
            this.dAmnSocket = new TcpClient();
            if (!this.attached)
            {
                lock (this.objLock)
                {
                    if (!this.attached)
                    {
                        this.packet = (dlgPacket) Delegate.Combine(this.packet, new dlgPacket(this.dAmn_packet));
                        this.StateChange = (dlgState) Delegate.Combine(this.StateChange, new dlgState(this.dAmn_StateChange));
                        this.attached = true;
                    }
                }
            }
        }

        private void _send(string packet)
        {
            if ((this.myState != states.DISCONNECTED) && (this.myState != states.DISCONNECTING))
            {
                byte[] bytes = new ASCIIEncoding().GetBytes(packet);
                try
                {
                    this.dAmnSocket.GetStream().BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(this.endWrite), null);
                }
                catch (Exception exception)
                {
                    this._lastError = exception.Message;
                    this.StateChange(states.DISCONNECTED);
                }
            }
        }

        public void ban(string room, string username)
        {
            this.raw(string.Format("send {0}\n\nban {1}\n\n", this.parseNamespace(room), username));
        }

        public void Connect()
        {
            if ((this.username == "") || (this.password == ""))
            {
                throw new Exception("A username and password must be set before calling connect.");
            }
            if (this.password.Length == 0x20)
            {
                try
                {
                    this.dAmnSocket = new TcpClient();
                    this.dAmnSocket.BeginConnect(this._server, this._port, new AsyncCallback(this.endConnect), null);
                    this.setState(states.CONNECTING);
                }
                catch (Exception exception)
                {
                    if (this.recreated)
                    {
                        throw exception;
                    }
                    this.dAmnSocket = new TcpClient();
                }
            }
            else
            {
                string authtoken;
                this.setState(states.FETCHINGCOOKIE);
                try
                {
                    authtoken = dALogin.GetAuthtoken(this.username, this.password);
                }
                catch
                {
                    authtoken = "";
                }
                if (authtoken.Length == 0x20)
                {
                    this.password = authtoken;
                    this.Connect();
                }
                else
                {
                    this._lastError = "Failed to get authtoken from dA.";
                    this.setState(states.DISCONNECTED);
                }
            }
        }

        private void dAmn_Disconnect(string reason)
        {
            this.setState(states.DISCONNECTED);
            try
            {
                this.dAmnSocket.Close();
            }
            catch
            {
            }
            this.dAmnSocket = new TcpClient();
        }
        private void dAmn_packet(dAmnPacket p)
        {
            dAmnPacket packet;
            bool flag;
            string param;
            switch (p.cmd.ToLower())
            {
                case "damnserver":
                    double num;
                    double.TryParse(p.param, out num);
                    this.ServerVersion = num;
                    this.setState(states.AUTHENTICATING);
                    this.sendLogin();
                    return;

                case "login":
                    if (p.args["e"] != "ok")
                    {
                        this._lastError = p.args["e"];
                        this.setState(states.DISCONNECTED);
                        this.dAmnSocket.Close();
                        return;
                    }
                    this.setState(states.LOGGEDIN);
                    return;

                case "ping":
                    this.raw("pong\n");
                    return;

                case "disconnect":
                    this._lastError = "Disconnected: " + p.args["e"];
                    this.setState(states.DISCONNECTED);
                    this.dAmnSocket.Close();
                    return;

                case "kicked":
                    try
                    {
                        this.rooms[p.param.ToLower().Trim()] = null;
                    }
                    catch
                    {
                    }
                    this.Kicked(p.param, this.username, p.args["by"], p.body);
                    return;

                case "recv":
                    bool flag2;
                    if (this[p.param] != null)
                    {
                        this.rooms[p.param.ToLower().Trim()].processPacket(p);
                    }
                    packet = dAmnPacket.parse(p.body);
                    flag = false;
                    param = p.param;
                    switch (packet.cmd.ToLower())
                    {
                        case "action":
                            flag = true;
                            break;

                        case "join":
                            try
                            {
                                flag2 = false;
                                if (packet.args.ContainsKey("s") && (packet.args["s"] == "0"))
                                {
                                    flag2 = true;
                                }
                                this.Join(param, packet.param, "", flag2);
                            }
                            catch
                            {
                            }
                            return;

                        case "part":
                            try
                            {
                                flag2 = false;
                                if (packet.args.ContainsKey("s") && (packet.args["s"] == "0"))
                                {
                                    flag2 = true;
                                }
                                this.Part(param, packet.param, packet.args.ContainsKey("r") ? packet.args["r"] : "", flag2);
                            }
                            catch
                            {
                            }
                            return;

                        case "privchg":
                            try
                            {
                                this.Privchg(param, packet.param, packet.args["by"], packet.args["pc"]);
                            }
                            catch
                            {
                            }
                            return;

                        case "kicked":
                            try
                            {
                                this.Kick(param, packet.param, packet.args["by"], packet.body);
                            }
                            catch
                            {
                            }
                            return;

                        case "admin":
                            int num2;
                            switch (p.param.ToLower().Trim())
                            {
                                case "create":
                                    try
                                    {
                                        this.Admin_Create(param, packet.args["by"], packet.args["name"], packet.args["privs"]);
                                    }
                                    catch
                                    {
                                    }
                                    return;

                                case "update":
                                    try
                                    {
                                        this.Admin_Update(param, packet.args["by"], packet.args["name"], packet.args["privs"]);
                                    }
                                    catch
                                    {
                                    }
                                    return;

                                case "rename":
                                    try
                                    {
                                        num2 = 0;
                                        if (packet.args.ContainsKey("n"))
                                        {
                                            int.TryParse(packet.args["n"], out num2);
                                        }
                                        this.Admin_Rename(param, packet.args["by"], packet.args["prev"], packet.args["name"], num2);
                                    }
                                    catch
                                    {
                                    }
                                    return;

                                case "move":
                                    try
                                    {
                                        num2 = 0;
                                        if (packet.args.ContainsKey("n"))
                                        {
                                            int.TryParse(packet.args["n"], out num2);
                                        }
                                        this.Admin_Move(param, packet.args["by"], packet.args["prev"], packet.args["name"], num2);
                                    }
                                    catch
                                    {
                                    }
                                    return;

                                case "remove":
                                    try
                                    {
                                        num2 = 0;
                                        if (packet.args.ContainsKey("n"))
                                        {
                                            int.TryParse(packet.args["n"], out num2);
                                        }
                                        this.Admin_Remove(param, packet.args["by"], packet.args["name"], num2);
                                    }
                                    catch
                                    {
                                    }
                                    return;
                            }
                            return;
                    }
                    return;

                case "property":
                    if (this[p.param] != null)
                    {
                        lock (this.PacketLock)
                        {
                            this.rooms[p.param.ToLower().Trim()].processPacket(p);
                        }
                    }
                    try
                    {
                        this.PropertyUpdate(p.args["p"], p.param);
                    }
                    catch
                    {
                    }
                    return;

                case "join":
                    if (p.args["e"] != "ok")
                    {
                        try
                        {
                            this.Error(p.cmd, p.param, p.args["e"]);
                        }
                        catch
                        {
                        }
                        return;
                    }
                    if (this[p.param] == null)
                    {
                        this[p.param] = new dAroom(p.param, this);
                    }
                    try
                    {
                        this.SelfJoin(p.param, p.args.ContainsKey("r") ? p.args["r"] : "");
                    }
                    catch
                    {
                    }
                    return;

                case "part":
                    if (p.args["e"] != "ok")
                    {
                        try
                        {
                            this.Error(p.cmd, p.param, p.args["e"]);
                        }
                        catch
                        {
                        }
                        return;
                    }
                    this[p.param] = null;
                    try
                    {
                        this.SelfPart(p.param, p.args.ContainsKey("r") ? p.args["r"] : "");
                    }
                    catch
                    {
                    }
                    return;

                case "send":
                case "kick":
                case "get":
                case "set":
                case "kill":
                    try
                    {
                        this.Error(p.cmd, p.param, p.args["e"]);
                    }
                    catch
                    {
                    }
                    return;

                default:
                    return;
            }
            string from = packet.args["from"];
            string body = packet.body;
            try
            {
                this.Message(param, from, body, flag);
            }
            catch
            {
            }
        }
        private void dAmn_StateChange(states State)
        {
            this.internalStateHandler(State);
        }

        public void demote(string room, string username)
        {
            this.demote(room, username, "");
        }
        public void demote(string room, string username, string privclass)
        {
            this.raw(string.Format("send {0}\n\ndemote {1}\n\n{2}", this.parseNamespace(room), username, privclass));
        }

        public void disconnect()
        {
            this.raw("disconnect\n");
        }

        private void doRead(IAsyncResult ar)
        {
            int count = 0;
            try
            {
                count = this.dAmnSocket.GetStream().EndRead(ar);
                if (count < 1)
                {
                    this.setState(states.DISCONNECTED);
                    this._lastError = "Unknown Error (doRead(IAsyncResult))";
                }
                else
                {
                    this.incBuildString(this.bData, 0, count);
                    this.dAmnSocket.GetStream().BeginRead(this.bData, 0, 0x400, new AsyncCallback(this.doRead), null);
                }
            }
            catch (Exception exception)
            {
                this._lastError = exception.Message;
                this.setState(states.DISCONNECTED);
            }
        }

        private void endConnect(IAsyncResult ar)
        {
            try
            {
                this.dAmnSocket.EndConnect(ar);
            }
            catch (Exception exception)
            {
                this._lastError = exception.Message;
                this.setState(states.DISCONNECTED);
            }
            if (this.dAmnSocket.Connected)
            {
                this.setState(states.CONNECTED);
                this.dAmnSocket.GetStream().BeginRead(this.bData, 0, 0x400, new AsyncCallback(this.doRead), null);
            }
        }

        private void endInvoke(IAsyncResult ar)
        {
            try
            {
                this.StateChange.EndInvoke(ar);
            }
            catch
            {
            }
        }

        private void endWrite(IAsyncResult ar)
        {
            try
            {
                this.dAmnSocket.GetStream().EndWrite(ar);
            }
            catch (Exception exception)
            {
                this._lastError = exception.Message;
                this.StateChange(states.DISCONNECTED);
            }
        }

        public string formatNamespace(string input)
        {
            if (input.StartsWith("chat:"))
            {
                return input.Replace("chat:", "#");
            }
            if (input.StartsWith("pchat:"))
            {
                string[] strArray = input.Split(new char[] { ':' });
                for (int i = 1; i < strArray.Length; i++)
                {
                    if (strArray[i].ToLower().Trim() != this.username.ToLower().Trim())
                    {
                        return ("@" + strArray[i]);
                    }
                }
            }
            throw new ArgumentException("Incorrect format!", "input");
        }

        private void incBuildString(byte[] data, int offset, int count)
        {
            int index = 0;
            for (index = offset; index < (offset + count); index++)
            {
                if (data[index] == 0)
                {
                    try
                    {
                        this.Packet(this.sbText.ToString());
                    }
                    catch
                    {
                    }
                    this.sbText = new StringBuilder();
                }
                else
                {
                    this.sbText.Append((char) data[index]);
                }
            }
        }

        private void internalStateHandler(states state)
        {
            if (state == states.CONNECTED)
            {
                this.sendHandshake();
            }
        }

        public void join(string room)
        {
            string rawCommand = string.Format("join {0}\n", this.parseNamespace(room));
            this.raw(rawCommand);
        }

        public void kick(string room, string username)
        {
            this.kick(room, username, "");
        }

        public void kick(string room, string username, string reason)
        {
            room = this.parseNamespace(room);
            this.raw(string.Format("kick {0}\nu={1}\n\n{2}", room, username, reason));
        }

        private void Packet(string data)
        {
            try
            {
                lock (this.QueueLock)
                {
                    try
                    {
                        this.PacketQueue.Enqueue(dAmnPacket.parse(data));
                    }
                    catch
                    {
                    }
                }
                this.ProcessQueue(null);
            }
            catch
            {
            }
        }

        public string parseNamespace(string input)
        {
            if (input.StartsWith("chat:") || input.StartsWith("pchat:"))
            {
                return input;
            }
            if (input.StartsWith("#"))
            {
                input = input.Remove(0, 1);
                input = "chat:" + input;
                return input;
            }
            if (input.StartsWith("@"))
            {
                input = input.Remove(0, 1);
                if (string.Compare(input, this.username, true) > 0)
                {
                    input = this.username + ":" + input;
                }
                else
                {
                    input = input + ":" + this.username;
                }
                input = "pchat:" + input;
                return input;
            }
            return ("chat:" + input);
        }

        public void part(string room)
        {
            string rawCommand = string.Format("part {0}\n", this.parseNamespace(room));
            this.raw(rawCommand);
        }

        private void pong()
        {
            this.raw("pong\n");
        }

        private void ProcessQueue(object obj)
        {
            if (this.SafeSetProcessLock())
            {
                dAmnPacket packet;
                Queue<dAmnPacket> queue = new Queue<dAmnPacket>();
                try
                {
                    lock (this.QueueLock)
                    {
                        while (this.PacketQueue.Count > 0)
                        {
                            packet = this.PacketQueue.Dequeue();
                            try
                            {
                                this.packet(packet);
                            }
                            catch
                            {
                            }
                            queue.Enqueue(packet);
                        }
                    }
                }
                finally
                {
                    this.SafeUnsetProcessLock();
                }
                while (queue.Count > 0)
                {
                    try
                    {
                        packet = queue.Dequeue();
                        if (this.RawPacket != null)
                        {
                            this.RawPacket(packet);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void promote(string room, string username)
        {
            this.promote(room, username, "");
        }

        public void promote(string room, string username, string privclass)
        {
            this.raw(string.Format("send {0}\n\npromote {1}\n\n{2}", this.parseNamespace(room), username, privclass));
        }

        public void raw(string rawCommand)
        {
            this._send(Regex.Unescape(rawCommand) + "\0");
        }

        private bool SafeSetProcessLock()
        {
            lock (this.ProcessingLock)
            {
                if (!this.processing)
                {
                    this.processing = true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void SafeUnsetProcessLock()
        {
            lock (this.ProcessingLock)
            {
                this.processing = false;
            }
        }

        public void say(string room, string message)
        {
            string str = "msg";
            if (message.ToLower().StartsWith("/me "))
            {
                message = message.Remove(0, 4);
                str = "action";
            }
            else if (message.ToLower().StartsWith("/np "))
            {
                message = message.Remove(0, 4);
                str = "npmsg";
            }
            else
            {
                string[] strArray2;
                char[] chArray2;
                if (message.ToLower().StartsWith("/raw "))
                {
                    message = message.Remove(0, 5);
                    this.raw(message);
                    return;
                }
                if (message.ToLower().StartsWith("/kick "))
                {
                    message = message.Remove(0, 6);
                    char[] separator = new char[] { ' ' };
                    string[] strArray = message.Split(separator, 2);
                    this.kick(room, strArray[0], (strArray.Length == 2) ? strArray[1] : "");
                    return;
                }
                if (message.ToLower().StartsWith("/ban "))
                {
                    message = message.Remove(0, 5);
                    this.ban(room, message);
                    return;
                }
                if (message.ToLower().StartsWith("/unban "))
                {
                    message = message.Remove(0, 7);
                    this.unban(room, message);
                    return;
                }
                if (message.ToLower().StartsWith("/join "))
                {
                    message = message.Remove(0, 6);
                    chArray2 = new char[] { ' ', ',' };
                    strArray2 = message.Split(chArray2, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str2 in strArray2)
                    {
                        this.join(str2);
                    }
                    return;
                }
                if (message.ToLower().StartsWith("/part "))
                {
                    message = message.Remove(0, 6);
                    chArray2 = new char[] { ' ', ',' };
                    strArray2 = message.Split(chArray2, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray2.Length == 0)
                    {
                        this.part(room);
                    }
                    else
                    {
                        foreach (string str2 in strArray2)
                        {
                            this.part(str2);
                        }
                    }
                    return;
                }
                if (message.ToLower().StartsWith("/"))
                {
                    if (this.Error != null)
                    {
                        this.Error("cmd", this.parseNamespace(room), "invalid command");
                    }
                    return;
                }
            }
            string rawCommand = string.Format("send {0}\n\n{1} main\n\n{2}", this.parseNamespace(room), str, message);
            this.raw(rawCommand);
        }

        private void sendHandshake()
        {
            this.setState(states.HANDSHAKING);
            this.raw("dAmnClient 0.3\nagent=dAmnSharp 1.0 (By plaguethenet)\n");
        }

        private void sendLogin()
        {
            this.raw("login " + this.username + "\npk=" + this.password + "\n");
        }

        private void setState(states state)
        {
            this.myState = state;
            try
            {
                this.StateChange(state);
            }
            catch
            {
            }
        }

        public void unban(string room, string username)
        {
            this.raw(string.Format("send {0}\n\nunban {1}\n\n", this.parseNamespace(room), username));
        }

        public dAroom this[string key]
        {
            get
            {
                if (this.rooms.ContainsKey(key.ToLower().Trim()))
                {
                    return this.rooms[key.ToLower().Trim()];
                }
                return null;
            }
            private set
            {
                this.rooms[key.ToLower().Trim()] = value;
            }
        }

        public string LastError
        {
            get
            {
                return this._lastError;
            }
        }

        public string password
        {
            private get
            {
                return this._password;
            }
            set
            {
                this._password = value;
            }
        }

        public int port
        {
            get
            {
                return this._port;
            }
            set
            {
                this._port = value;
            }
        }

        public Dictionary<string, dAroom> roomCollection
        {
            get
            {
                return this.rooms;
            }
        }

        public string server
        {
            get
            {
                return this._server;
            }
            set
            {
                this._server = value;
            }
        }

        public double ServerVersion
        {
            get
            {
                return this._sv;
            }
            private set
            {
                this._sv = value;
            }
        }

        public states state
        {
            get
            {
                return this.myState;
            }
        }

        public string username
        {
            get
            {
                return this._username;
            }
            set
            {
                this._username = value;
            }
        }

        public delegate void dlgAdmin_CreateUpdate(string room, string by, string privclass, string privs);

        public delegate void dlgAdmin_Remove(string room, string by, string name, int n);

        public delegate void dlgAdmin_RenameMove(string room, string by, string prev, string name, int n);

        public delegate void dlgError(string cmd, string param, string e);

        public delegate void dlgJoinPart(string room, string user, string reason, bool hidden);

        public delegate void dlgJP(string room, string r);

        public delegate void dlgKicked(string room, string who, string by, string reason);

        public delegate void dlgMessage(string room, string from, string message, bool action);

        public delegate void dlgPacket(dAmnPacket p);

        public delegate void dlgPrivchg(string room, string who, string by, string pc);

        public delegate void dlgPropUpdate(string property, string room);

        public delegate void dlgState(states State);

        public class tablumps
        {
            public static string Parse(string input)
            {
                string[] strArray = new string[] { 
                    @"&emote\t([^\t]+)\t([0-9]+)\t([0-9]+)\t([^\t]+)\t([^\t]*)\t", @"&acro\t([^\t]+)\t([^&]*)&/acro\t", @"&abbr\t([^\t]+)\t([^&]*)&/abbr\t", @"&link\t([^\t]*)\t([^\t]*)\t&\t", @"&link\t([^\t]*)\t&\t", @"&a\t([^\t]+)\t([^\t]*)\t", @"&/a\t", @"&(iframe|embed)\t([^\t]*)\t([0-9]*)\t([0-9]*)\t", @"&/(iframe|embed)\t", @"&img\t([^\t]*)\t([0-9]*)\t([0-9]*)\t", @"&dev\t([^\t])\t([^\t]+)\t", @"&avatar\t([^\t]+)\t0\t", @"&abbr\t([^\t]+)\t&/abbr\t", @"&thumb\t([0-9]*)\t([^\t]*)\t([^\t]*)\t([0-9]*)x([0-9]*)\t([^\t]*)\t([^\t]*):([^\t]*).gif\t([^\t]*)\t", @"&thumb\t([0-9]*)\t([^\t]*)\t([^\t]*)\t([0-9]*)x([0-9]*)\t([^\t]*)\t([^\t]*):([^\t]*)\t([^\t]*)\t", @"&avatar\t([^\t])([^\t])([^\t]*)\t1\t", 
                    @"&avatar\t([^\t])([^\t])([^\t]*)\t2\t", @"&avatar\t([^\t])([^\t])([^\t]*)\t3\t", @"&br\t", @"&(b|i|s|u|sub|sup|ul|ol|li|p)\t", @"&/(b|i|s|u|sub|sup|ul|ol|li|p)\t", @"&bcode\t", @"&/bcode\t", @"&code\t", @"&/code\t"
                 };
                string[] strArray2 = new string[] { 
                    "$1", "<acronym title='$1'>$2</acronym>", "<abbr title='$1'>$2</abbr>", "<a href='$1'>$2</a>", "$1", "<a href='$1'>$2", "</a>", "", "", "<img src='$1' width='$2' height='$3'>", ":dev$2:", ":icon$1:", "", ":thumb$1:", ":thumb$1:", ":icon$1$2$3:", 
                    ":icon$1$2$3:", ":icon$1$2$3:", "<br>", "<$1>", "</$1>", "<pre><code>", "</code></pre>", "<code>", "</code>"
                 };
                for (int i = 0; i < strArray.Length; i++)
                {
                    input = Regex.Replace(input, strArray[i], strArray2[i], RegexOptions.Singleline | RegexOptions.IgnoreCase);
                }
                return input;
            }
        }
    }
}

