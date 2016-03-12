using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaumKasse.viewModels
{
    class AddUserViewModel : INotifyPropertyChanged
    {
        private User user;
        public User User
        {
            get 
            {
                if (this.user.Image == null || !File.Exists(this.user.Image))
                    this.user.Image = "images/empty.jpg";
                return this.user; 
            }
            set
            {
                this.user = value;
                NotifyPropertyChanged("User");
            }
        }

        private CheckoutDataContext db;

        public AddUserViewModel(User user, CheckoutDataContext context)
        {
            this.db = context;

            if (user == null)
            {
                this.user = new User
                {
                    Name = "Name",
                    Balance = 0,
                    Image = "images/empty.jpg",
                    EntryDate = DateTime.Now,
                    Points = 0
                };
            }
            else
            {
                this.user = user;
            }
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

        /// <summary>
        /// Add or updates a user
        /// </summary>
        internal void AddUser()
        {
            // TODO as validation rule
            // check name
            if (user.Name.Equals(String.Empty) || user.Name.Equals("Name") )
            {
                MessageBox.Show("Du hast deinen Namen vergessen...", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // copy image
            string ext = Path.GetExtension(User.Image);
            string source = User.Image;
            string target = Path.Combine(new string[] 
            { 
                Directory.GetCurrentDirectory(), 
                "images", 
                user.Name + ext  
            });

            // create directory if it does not exist
            if (!Directory.Exists(Path.GetDirectoryName(target)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target));
            }

            try
            {
                // Image can be empty
                if (File.Exists(source) && !source.Equals(target))
                {
                    // copy file, ask for overwrite if exists
                    if (File.Exists(target))
                    {
                        MessageBoxResult res = MessageBox.Show("Die Datei existiert bereits. Soll die Datei überschrieben werden?",
                        "Information", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (res != MessageBoxResult.Yes)
                            return;
                    }

                    File.Copy(source, target, true);
                    user.Image = target;
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //insert data
            if (user.ID == 0)
                db.Users.InsertOnSubmit(user);

            // commit changes
            db.SubmitChanges();
        }

        internal void Cancel()
        {
            if (user.ID != 0)
            {
                var original = db.Users.GetOriginalEntityState(user);
                user.Name = original.Name;
                user.Image = original.Image;
                db.SubmitChanges();
            }
        }
    }
}
