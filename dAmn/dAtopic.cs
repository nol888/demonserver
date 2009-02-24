namespace dAmnSharp
{
    using System;

    public class dAtopic
    {
        private string _body = "";
        private string _by = "";
        private string _timestamp = "";

        public DateTime Local
        {
            get
            {
                return this.UTC.ToLocalTime();
            }
        }

        public string parsedvalue
        {
            get
            {
                return dAmn.tablumps.Parse(this._body);
            }
        }

        public string SetBy
        {
            get
            {
                return this._by;
            }
            set
            {
                this._by = value;
            }
        }

        public string Timestamp
        {
            get
            {
                return this._timestamp;
            }
            set
            {
                this._timestamp = value;
            }
        }

        public DateTime UTC
        {
            get
            {
                double num;
                double.TryParse(this._timestamp, out num);
                DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                return time.AddSeconds(num);
            }
        }

        public string value
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

