namespace dAmnSharp
{
    using System;

    public class privclass
    {
        private string _name;
        private int _order;

        public override string ToString()
        {
            return this.ToString(0);
        }

        public string ToString(int mode)
        {
            switch (mode)
            {
                case 1:
                    return this.Order.ToString();

                case 2:
                    return this.Name;
            }
            return (this.Order.ToString() + ":" + this.Name);
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            internal set
            {
                this._name = value;
            }
        }

        public int Order
        {
            get
            {
                return this._order;
            }
            internal set
            {
                this._order = value;
            }
        }
    }
}

