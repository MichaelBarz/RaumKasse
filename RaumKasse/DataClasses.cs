
using System;
using System.Runtime.Serialization;
using System.Windows.Media;
namespace RaumKasse
{
    interface ILoggable
    {
        LogType Type { get; }
        DateTime Date { get; }
        string Message { get; }
    }

    public enum LogType
    {
        Purchase,
        Payment
    }

    partial class userActionBuy : ILoggable
    {
        public LogType Type
        {
            get { return LogType.Purchase; }
        }
        
        public DateTime Date
        {
            get { return this.PurchaseDate; }
        }

        public string Message
        {
            get { return this.User.Name + " hat " + this.Drink.Name + " gekauft." ; }
        }
    }

    partial class userActionPay : ILoggable
    {
        public LogType Type
        {
            get { return LogType.Payment; }
        }

        public DateTime Date
        {
            get { return this.PaymentDate; }
        }

        public string Message
        {
            get { return this.User.Name + " hat " + this.Sum + " € eingezahlt."; }
        }
    }

    interface IListable
    {
        string Name { get; }
        string Amount { get; }
        string Rank { get; }
        string Image { get; }
        Brush Foreground { get; }
    }

    partial class Drink : IListable, ICloneable
    {
        public string Amount
        {
            get { return String.Format("{0:0.##}€", _Balance); }
        }

        public object Clone()
        {
            var d = new Drink();
            d.ID = this.ID;
            d.Image = this.Image != null ? String.Copy(this.Image) : null;
            d.Name = this.Name != null ? String.Copy(this.Name) : null;
            d.Price = this.Price;
            d.Points = this.Points;

            return d;
        }

        public string Rank
        {
            get { return _Points.ToString() + " Punkte"; }
        }


        public Brush Foreground
        {
            get { return new SolidColorBrush(Colors.Black); }
        }
    }

    partial class User : IListable, ICloneable
    {
        public string Amount
        {
            get
            {
                if (_Balance <= -25)
                {
                    return String.Format("{0:0.##}€ (Punktesperre erreicht!)", _Balance);
                }

                return String.Format("{0:0.##}€", _Balance);
            }
        }

        public object Clone()
        {
            var u = new User();
            u.ID = this.ID;
            u.Image = this.Image != null ? String.Copy(this.Image) : null;
            u.Name = this.Name != null ? String.Copy(this.Name) : null;
            u.Balance = this.Balance;
            u.EntryDate = this.EntryDate;
            u.Points = this.Points;

            return u;
        }

        public string Rank
        {
            get 
            {
                return _Points.ToString() + " Punkte"; 
            }
        }

        public Brush Foreground
        {
            get 
            {
                if (Balance >= 0) return new SolidColorBrush(Colors.Green);
                if (Balance < 0 && Balance > -25) return new SolidColorBrush(Colors.DarkOrange);

                return new SolidColorBrush(Colors.Firebrick);
            }
        }
    }
}
