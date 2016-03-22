using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Windows;

namespace RaumKasse.viewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        private User dummyUser = new User
        {
            Name = "Benutzer wählen!",
            Balance = 0,
            Points = 0,
            Image = "images/empty.jpg"
        };

        private Drink dummyDrink = new Drink
        {
            Name = "Getränk wählen!",
            Price = 0,
            Points = 0,
            Image = "images/empty.jpg"
        };

        public ObservableCollection<User> UserList { get; set; }
        public ObservableCollection<Drink> DrinkList { get; set; }
        public ObservableCollection<ILoggable> LogList { get; set; }

        public ObservableCollection<User> UserRankData { get; set; }
        public ObservableCollection<User> UserDeptData { get; set; }
        public ObservableCollection<Drink> DrinkRankData { get; set; }

        private User _selectedUser = null;
        public User SelectedUser
        {
            get { return this._selectedUser; }
            set
            {
                this._selectedUser = value;
                NotifyPropertyChanged("SelectedUser");
            }
        }

        private Drink _selectedDrink = null;
        public Drink SelectedDrink
        {
            get { return this._selectedDrink; }
            set
            {
                this._selectedDrink = value;
                NotifyPropertyChanged("SelectedDrink");
            }
        }

        public CheckoutDataContext db { get; internal set; }
        private CheckoutDataContext createDataContext()
        {
            return new CheckoutDataContext(Path.Combine(new String[] { Directory.GetCurrentDirectory(), "Data.sdf" }));
        }

        public MainViewModel()
        {
            // Create Backup
            try
            {
                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                var user = WindowsIdentity.GetCurrent();
                var groupNames = from id in user.Groups
                                 select id.Translate(typeof(NTAccount)).Value;
                foreach (var s in groupNames)
                {
                    Console.WriteLine(s);
                }
                var dbPath = Path.Combine(new String[] { Directory.GetCurrentDirectory(), "Data.sdf" });
                var dir = Path.Combine(new String[] { Directory.GetCurrentDirectory(), "AutoBackup" });
                if (!Directory.Exists(dir.ToString()))
                    Directory.CreateDirectory(dir.ToString());
                var filename = Path.GetRandomFileName();
                var dstPath = Path.Combine(new String[] { Directory.GetCurrentDirectory(), "AutoBackup", filename });

                File.Copy(dbPath.ToString(), dstPath.ToString(), false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // Create the data context, specify the database file location
            this.db = createDataContext();

            // Create the database after checking it does not exist
            if (!db.DatabaseExists())
            {
                db.CreateDatabase();
            }

            // check if reset has to be done
            var d = new DateTime(DateTime.Now.Year, 1, 1, 6, 0, 0);
            var toReset = (from u in db.Users where u.EntryDate < d select u).ToList<User>();
            if (toReset.Count != 0)
            {
                MessageBox.Show("Neues Spiel, neues Glück! Erreichte Punkte werden für das neue Jahr zurückgesetzt...", "Prost Neujahr!", MessageBoxButton.OK, MessageBoxImage.Information);
                foreach (var user in toReset)
                {
                    user.Points = 0;
                    user.EntryDate = DateTime.Now;
                }
                db.SubmitChanges();
            }          

            SelectedUser = dummyUser;
            SelectedDrink = dummyDrink;
        }

        public void loadDrinks()
        {
            loadDrinks(String.Empty);
        }

        public void loadDrinks(string search)
        {
            IQueryable<Drink> temp = null;
            if (search.Equals(String.Empty))
                temp = from c in db.Drinks orderby c.Name select c;
            else
                temp = from c in db.Drinks orderby c.Name where c.Name.Contains(search) select c;
            DrinkList = ToObservableCollection<Drink>(temp);

            NotifyPropertyChanged("DrinkList");
        }

        public void loadUsers()
        {
            loadUsers(String.Empty);
        }

        public void loadUsers(string search)
        {
            IQueryable<User> temp = null;
            if (search.Equals(String.Empty))
                temp = from c in db.Users orderby c.Name select c;
            else
                temp = from c in db.Users orderby c.Name where c.Name.Contains(search) select c;
            UserList = ToObservableCollection<User>(temp);

            NotifyPropertyChanged("UserList");
        }

        public void loadLog()
        {
            var refDate = DateTime.Now.AddDays(-14);
            var purchases = ToObservableCollection<ILoggable>(from p in db.Purchases where p.PurchaseDate >= refDate orderby p.PurchaseDate descending select p);
            var payments = ToObservableCollection<ILoggable>(from p in db.Payments where p.PaymentDate >= refDate orderby p.PaymentDate descending select p);

            var concat = purchases.Concat(payments);
            var ordered = concat.OrderByDescending(o => o.Date).ToList();

            LogList = new ObservableCollection<ILoggable>();
            foreach (var item in ordered)
            {
                LogList.Add(item);
            }
            NotifyPropertyChanged("LogList");

            loadChartData();
        }

        private void loadChartData()
        {
            var refDate = DateTime.Now.AddMonths(-1);

            // load user chart data
            UserRankData = ToObservableCollection<User>((from u in db.Users orderby u.Points descending select u).Take(10));
            NotifyPropertyChanged("UserRankData");

            // load user dept data
            UserDeptData = ToObservableCollection<User>((from u in db.Users where u.Balance < 0 orderby u.Balance ascending select u).Take(10));
            NotifyPropertyChanged("UserDeptData");

            // gather drink rank data
            var tempDrink =
                (
                    from p in db.Purchases
                    where p.PurchaseDate >= refDate
                    group p by new
                    {
                        p.DrinkID,
                        p.Drink.Name
                    } into g
                    select new
                    {
                        ID = g.Key.DrinkID,
                        Name = g.Key.Name,
                        Points = g.Count()
                    }
                ).OrderByDescending(o => o.Points).Take(5);
            tempDrink.ToList();
            DrinkRankData = new ObservableCollection<Drink>();
            foreach (var item in tempDrink)
            {
                DrinkRankData.Add(new Drink
                {
                    ID = item.ID,
                    Name = item.Name,
                    Points = item.Points
                });
            }
            NotifyPropertyChanged("DrinkRankData");
        }

        public void addUser()
        {
            Window addUserWindow = new AddUserWindow(null, db);
            addUserWindow.ShowDialog();
            loadUsers();
        }

        public void editUser()
        {
            if (SelectedUser != null && SelectedUser != dummyUser)
            {
                Window addUserWindow = new AddUserWindow(SelectedUser, db);
                addUserWindow.ShowDialog();
                loadUsers();
            }
            else
                MessageBox.Show("Du musst einen Benutzer wählen!", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        internal void deleteUser()
        {
            try
            {
                deleteUserWithPermission();
            }
            catch (SecurityException)
            {
                MessageBox.Show("Du benötigst Admin-Rechte für diese Aktion", "Nicht genügend Rechte.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "VORDEFINIERT\\Administratoren")]
        public void deleteUserWithPermission()
        {
            // TODO: ask --> delete all dependencies (on delete ... cascade)
            if (SelectedUser != null && SelectedUser != dummyUser)
            {
                MessageBoxResult res = MessageBox.Show("Soll der Benutzer wirklich gelöscht werden?",
                        "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (res != MessageBoxResult.Yes)
                    return;
                
                try
                {
                    db.Purchases.DeleteAllOnSubmit<userActionBuy>(from p in db.Purchases where p.User == SelectedUser select p);
                    db.Payments.DeleteAllOnSubmit<userActionPay>(from p in db.Payments where p.User == SelectedUser select p);
                    db.Users.DeleteOnSubmit(SelectedUser);
                    db.SubmitChanges();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Der Benutzer konnte nicht gelöscht werden, da er im Log verwendet wird. Sie müssen zuerst alle Logeinträge für diesen Nutzer entfernen. ("
                        + e.Message + ")",
                        "Löschen", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                SelectedUser = dummyUser;
                loadUsers();
                loadLog();
            }
            else
                MessageBox.Show("Du musst einen Benutzer wählen!", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void addDrink()
        {
            Window addDrinkWindow = new AddDrinkWindow(null, db);
            addDrinkWindow.ShowDialog();
            loadDrinks();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<T> ToObservableCollection<T>(IEnumerable<T> source)
        {
            var collection = new ObservableCollection<T>();
            foreach (var contact in source)
                collection.Add(contact);
            return collection;
        }

        internal void editDrink()
        {
            if (SelectedDrink != null && SelectedDrink != dummyDrink)
            {
                Window addDrinkWindow = new AddDrinkWindow(SelectedDrink, db);
                addDrinkWindow.ShowDialog();
                loadDrinks();
            }
            else
                MessageBox.Show("Du musst ein Getränk wählen!", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        internal void deleteDrink()
        {
            try
            {
                deleteDrinkWithPermission();
            }
            catch (SecurityException)
            {
                MessageBox.Show("Du benötigst Admin-Rechte für diese Aktion", "Nicht genügend Rechte.", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "VORDEFINIERT\\Administratoren")]
        internal void deleteDrinkWithPermission()
        {
            // TODO: ask --> delete all dependencies (on delete ... cascade)
            if (SelectedDrink != null && SelectedDrink != dummyDrink)
            {
                MessageBoxResult res = MessageBox.Show("Soll das Getränk wirklich gelöscht werden?",
                        "Löschen", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (res != MessageBoxResult.Yes)
                    return;
                
                try
                {
                    db.Purchases.DeleteAllOnSubmit<userActionBuy>(from p in db.Purchases where p.Drink == SelectedDrink select p);
                    db.Drinks.DeleteOnSubmit(SelectedDrink);
                    db.SubmitChanges();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Das Getränk konnte nicht gelöscht werden, da es im Log verwendet wird. Sie müssen zuerst alle Logeinträge für dieses Getränk entfernen. ("
                        + e.Message + ")",
                        "Löschen", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                SelectedDrink = dummyDrink;
                loadDrinks();
                loadLog();
            }
            else
                MessageBox.Show("Du musst ein Getränk wählen!", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        internal void addPurchase()
        {
            if (SelectedDrink == null || SelectedUser == null || SelectedDrink == dummyDrink || SelectedUser == dummyUser)
            {
                MessageBox.Show("Du musst einen Benutzer und ein Getränk wählen!", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var user = (from c in db.Users where c.ID == SelectedUser.ID select c).Single();

            var isTrainingBeer = SelectedDrink.Points > 0 
                && (DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Tuesday) 
                && (DateTime.Now.Hour >= 16 && DateTime.Now.Hour <= 19);
            if (isTrainingBeer)
            {
                MessageBox.Show("Trainingsbier... Gibt keine Punkte!", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (user.Balance > -25 && !isTrainingBeer)
            {
                user.Points += SelectedDrink.Points;
            }

            if (user.Balance > -25 && user.Balance - SelectedDrink.Price <= -25)
            {
                MessageBox.Show("Du hast >= 25€ Schulden, daher erhälst du keine Punkte! Denk ans Bezahlen...", "Punktesperre erreicht!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            user.Balance -= SelectedDrink.Price;
            SelectedUser = user;
            db.SubmitChanges();

            db.Purchases.InsertOnSubmit(new userActionBuy
            {
                DrinkID = SelectedDrink.ID,
                UserID = SelectedUser.ID,
                PurchaseDate = DateTime.Now,
                IsProcessed = false
            });
            db.SubmitChanges();
            loadUsers();
            loadLog();
        }

        internal void addPayment(double amount)
        {
            if (SelectedUser == null || SelectedUser == dummyUser)
            {
                MessageBox.Show("Du musst einen Benutzer wählen!", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var user = (from c in db.Users where c.ID == SelectedUser.ID select c).Single();
            user.Balance += amount;
            SelectedUser = user;
            db.SubmitChanges();

            db.Payments.InsertOnSubmit(new userActionPay
            {
                UserID = SelectedUser.ID,
                Sum = amount,
                IsProcessed = false,
                PaymentDate = DateTime.Now
            });
            db.SubmitChanges();
            loadUsers();
            loadLog();
        }

        internal void deleteUserAction(ILoggable entry)
        {
            if (entry == null)
            {
                MessageBox.Show("Du musst im Log eine Aktion auswählen!", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult res = MessageBox.Show("Soll die Aktion wirklich rückgängig gemacht werden?",
                    "Rückgängig", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (res != MessageBoxResult.Yes)
                return;

            switch (entry.Type)
            {
                case LogType.Purchase:
                    var buy = (userActionBuy)entry;
                    var buyUser = (from c in db.Users where c.ID == buy.UserID select c).Single();
                    buyUser.Points -= buy.Drink.Points;
                    buyUser.Balance += buy.Drink.Price;
                    SelectedUser = buyUser;
                    db.SubmitChanges();
                    db.Purchases.DeleteOnSubmit(buy);
                    break;
                case LogType.Payment:
                    var pay = (userActionPay)entry;
                    var payUser = (from c in db.Users where c.ID == pay.UserID select c).Single();
                    payUser.Balance -= pay.Sum;
                    SelectedUser = payUser;
                    db.SubmitChanges();
                    db.Payments.DeleteOnSubmit(pay);
                    break;
            }
            db.SubmitChanges();
            loadUsers();
            loadLog();
        }

        internal bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
