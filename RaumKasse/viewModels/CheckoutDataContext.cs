using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaumKasse.viewModels
{
    public class CheckoutDataContext : DataContext
    {
        public Table<User> Users;
        public Table<Drink> Drinks;
        public Table<userActionBuy> Purchases;
        public Table<userActionPay> Payments;
        //...

        public CheckoutDataContext(string connection) : base(connection) { }
    }
}
