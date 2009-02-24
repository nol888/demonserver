namespace dAmnSharp
{
    using System;
    using System.Collections.Generic;

    public class dAmnArgs
    {
        private Dictionary<string, string> _args;
        private string _body;

        public static dAmnArgs getArgsNData(string data)
        {
            dAmnArgs args = new dAmnArgs();
            args.args = new Dictionary<string, string>();
            args.body = null;
            while (true)
            {
                if ((data.Length == 0) || (data[0] == '\n'))
                {
                    break;
                }
                int index = data.IndexOf('\n');
                int length = data.IndexOf('=');
                if (length > index)
                {
                    break;
                }
                args.args[data.Substring(0, length)] = data.Substring(length + 1, index - (length + 1));
                data = data.Substring(index + 1);
            }
            if ((data != null) && (data.Length > 0))
            {
                args.body = data.Substring(1);
                return args;
            }
            args.body = "";
            return args;
        }

        public Dictionary<string, string> args
        {
            get
            {
                return this._args;
            }
            set
            {
                this._args = value;
            }
        }

        public string body
        {
            get
            {
                return this._body;
            }
            set
            {
                this._body = value;
            }
        }
    }
}

