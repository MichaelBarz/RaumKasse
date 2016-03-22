using System.Data.Linq;

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
