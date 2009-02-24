namespace dAmnSharp
{
    using System;
    using System.Collections.Generic;

    public class dAmnPacket
    {
        private string _cmd = "";
        private string _param = "";
        private dAmnArgs argsAndBody;

        public static dAmnPacket parse(string data)
        {
            dAmnPacket packet2;
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            string str = data;
            try
            {
                dAmnPacket packet = new dAmnPacket();
                int index = data.IndexOf('\n');
                if (index < 0)
                {
                    throw new Exception("Parser error, No line break.");
                }
                string str2 = data.Substring(0, index);
                packet.cmd = str2.Split(new char[] { ' ' })[0];
                int num2 = str2.IndexOf(' ');
                if (num2 > 0)
                {
                    packet.param = str2.Substring(num2 + 1);
                }
                packet.argsAndBody = dAmnArgs.getArgsNData(data.Substring(index + 1));
                packet2 = packet;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return packet2;
        }

        public Dictionary<string, string> args
        {
            get
            {
                return this.argsAndBody.args;
            }
            set
            {
                this.argsAndBody.args = value;
            }
        }

        public string body
        {
            get
            {
                return this.argsAndBody.body;
            }
            set
            {
                this.argsAndBody.body = value;
            }
        }

        public string cmd
        {
            get
            {
                return this._cmd;
            }
            set
            {
                this._cmd = value;
            }
        }

        public string param
        {
            get
            {
                return this._param;
            }
            set
            {
                this._param = value;
            }
        }
    }
}

