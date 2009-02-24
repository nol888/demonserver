namespace dAmnSharp
{
    using System;
    using System.Collections.Generic;

    public class Userinfo
    {
        private int _count;
        private string _username;
        private Dictionary<string, string> u_prop;

        public int Count
        {
            get
            {
                return this._count;
            }
            set
            {
                this._count = value;
            }
        }

        public Dictionary<string, string> properties
        {
            get
            {
                return this.u_prop;
            }
            set
            {
                this.u_prop = value;
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
    }
}

