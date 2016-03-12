using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaumKasse.viewModels
{
    class AddDrinkViewModel : INotifyPropertyChanged
    {
        private Drink drink;
        public Drink Drink
        {
            get 
            {
                if (this.drink.Image == null || !File.Exists(this.drink.Image))
                    this.drink.Image = "images/empty.jpg";
                return this.drink; 
            }
            set
            {
                this.drink = value;
                NotifyPropertyChanged("Drink");
            }
        }

        private CheckoutDataContext db;

        public AddDrinkViewModel(Drink drink, CheckoutDataContext context)
        {
            this.db = context;

            if (drink == null)
            {
                this.drink = new Drink
                {
                    Name = "Name",
                    Price = 0,
                    Image = "images/empty.jpg",
                    Points = 0
                };
            }
            else
            {
                this.drink = drink;
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

        internal void AddDrink()
        {
            // TODO as validation rule
            // check name
            if (drink.Name.Equals(String.Empty) || drink.Name.Equals("Name"))
            {
                MessageBox.Show("Du hast den Namen vergessen...", "Ooops!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // copy image
            string ext = Path.GetExtension(Drink.Image);
            string source = Drink.Image;
            string target = Path.Combine(new string[] 
            { 
                Directory.GetCurrentDirectory(), 
                "images", 
                Path.GetFileName(source) 
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
                    drink.Image = target;
                }
                // TODO check if I need to set Image null
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //insert data
            if (drink.ID == 0)
                db.Drinks.InsertOnSubmit(drink);

            // commit changes
            db.SubmitChanges();
        }

        internal void Cancel()
        {
            if (drink.ID != 0)
            {
                var original = db.Drinks.GetOriginalEntityState(drink);
                drink.Name = original.Name;
                drink.Image = original.Image;
                drink.Points = original.Points;
                drink.Price = original.Price;
                db.SubmitChanges();
            }
        } 
    }
}
