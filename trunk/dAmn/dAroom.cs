namespace dAmnSharp
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class dAroom
    {
        private dAmn _parent;
        private Dictionary<int, privclass> _pci = new Dictionary<int, privclass>();
        private Dictionary<string, privclass> _pcs = new Dictionary<string, privclass>();
        private string _room;
        private dAtopic _title = new dAtopic();
        private dAtopic _topic = new dAtopic();
        private Dictionary<string, Userinfo> _users = new Dictionary<string, Userinfo>();

        public dAroom(string ns, dAmn p)
        {
            this.name = ns;
            this.parent = p;
        }

        public void processPacket(dAmnPacket p)
        {
            string str2 = p.cmd.ToLower();
            if (str2 != null)
            {
                Userinfo userinfo;
                if (!(str2 == "property"))
                {
                    if (str2 == "recv")
                    {
                        dAmnPacket packet2 = dAmnPacket.parse(p.body);
                        str2 = packet2.cmd.ToLower().Trim();
                        if (str2 != null)
                        {
                            if (!(str2 == "join"))
                            {
                                if (str2 == "part")
                                {
                                    if (this._users.ContainsKey(packet2.param.ToLower().Trim()))
                                    {
                                        userinfo = this[packet2.param];
                                        userinfo.Count--;
                                        if (userinfo.Count <= 0)
                                        {
                                            this._users.Remove(packet2.param.ToLower().Trim());
                                        }
                                        else
                                        {
                                            this[packet2.param] = userinfo;
                                        }
                                    }
                                }
                                else if (str2 == "kicked")
                                {
                                    this._users.Remove(packet2.param.ToLower().Trim());
                                }
                                else if ((str2 == "privchg") && this._users.ContainsKey(packet2.param.ToLower().Trim()))
                                {
                                    userinfo = this[packet2.param];
                                    userinfo.properties["pc"] = packet2.args["pc"];
                                    this[packet2.param] = userinfo;
                                }
                            }
                            else
                            {
                                userinfo = new Userinfo();
                                userinfo.username = packet2.param;
                                dAmnArgs args = dAmnArgs.getArgsNData(packet2.body);
                                userinfo.properties = args.args;
                                if (this._users.ContainsKey(userinfo.username.ToLower().Trim()))
                                {
                                    Userinfo userinfo2 = this[userinfo.username];
                                    userinfo.Count = 1 + userinfo2.Count;
                                }
                                this[userinfo.username] = userinfo;
                            }
                        }
                    }
                }
                else
                {
                    str2 = p.args["p"].ToLower();
                    if (str2 != null)
                    {
                        if (!(str2 == "members"))
                        {
                            if (str2 == "privclasses")
                            {
                                string[] strArray = p.body.Split(new char[] { '\n' });
                                List<privclass> list = new List<privclass>();
                                this._pci.Clear();
                                this._pcs.Clear();
                                for (int i = 0; i < strArray.Length; i++)
                                {
                                    if (strArray[i] != "")
                                    {
                                        string[] strArray2 = strArray[i].Split(new char[] { ':' });
                                        if (strArray2.Length == 2)
                                        {
                                            int num2;
                                            privclass privclass = new privclass();
                                            privclass.Name = strArray2[1];
                                            int.TryParse(strArray2[0], out num2);
                                            privclass.Order = num2;
                                            this._pci[privclass.Order] = privclass;
                                            this._pcs[privclass.Name] = privclass;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                dAtopic atopic;
                                if (str2 == "title")
                                {
                                    atopic = new dAtopic();
                                    atopic.value = p.body;
                                    atopic.SetBy = p.args["by"];
                                    atopic.Timestamp = p.args["ts"];
                                    this._title = atopic;
                                }
                                else if (str2 == "topic")
                                {
                                    atopic = new dAtopic();
                                    atopic.value = p.body;
                                    atopic.SetBy = p.args["by"];
                                    atopic.Timestamp = p.args["ts"];
                                    this._topic = atopic;
                                }
                            }
                        }
                        else
                        {
                            string body = p.body;
                            Dictionary<string, Userinfo> dictionary = new Dictionary<string, Userinfo>();
                            while (body.Trim() != "")
                            {
                                dAmnPacket packet = dAmnPacket.parse(body);
                                userinfo = new Userinfo();
                                body = packet.body;
                                userinfo.username = packet.param;
                                userinfo.properties = packet.args;
                                userinfo.Count = 1;
                                if (dictionary.ContainsKey(userinfo.username.ToLower().Trim()))
                                {
                                    userinfo.Count += dictionary[userinfo.username.ToLower().Trim()].Count;
                                }
                                dictionary[userinfo.username.ToLower().Trim()] = userinfo;
                            }
                            this._users = dictionary;
                        }
                    }
                }
            }
        }

        public void say(string message)
        {
            this.parent.say(this.name, message);
        }

        public Userinfo this[string username]
        {
            get
            {
                return this._users[username.ToLower().Trim()];
            }
            private set
            {
                this._users[username.ToLower().Trim()] = value;
            }
        }

        public string name
        {
            get
            {
                return this._room;
            }
            private set
            {
                this._room = value;
            }
        }

        private dAmn parent
        {
            get
            {
                return this._parent;
            }
            set
            {
                this._parent = value;
            }
        }

        public Dictionary<string, privclass> PrivclassByName
        {
            get
            {
                return this._pcs;
            }
        }

        public Dictionary<int, privclass> PrivclassByOrder
        {
            get
            {
                return this._pci;
            }
        }

        public dAtopic Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
            }
        }

        public dAtopic Topic
        {
            get
            {
                return this._topic;
            }
            private set
            {
                this._topic = value;
            }
        }

        public Dictionary<string, Userinfo> UserDict
        {
            get
            {
                return this._users;
            }
        }
    }
}

