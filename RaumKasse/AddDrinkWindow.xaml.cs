using Microsoft.Win32;
using RaumKasse.viewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RaumKasse
{
    /// <summary>
    /// Interaction logic for AddDrinkWindow.xaml
    /// </summary>
    public partial class AddDrinkWindow : Window
    {
        AddDrinkViewModel viewModel;

        public AddDrinkWindow(Drink drink, CheckoutDataContext context)
        {
            InitializeComponent();

            viewModel = new AddDrinkViewModel(drink, context);
            this.DataContext = viewModel;

            ImgLogo.MouseDown += ImgLogo_MouseDown;
            BtnAddDrink.Click += BtnAddDrink_Click;
            BtnCancel.Click += BtnCancel_Click;
        }

        void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
        }

        void BtnAddDrink_Click(object sender, RoutedEventArgs e)
        {     
            viewModel.AddDrink();
            this.Close();
        }

        void ImgLogo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JPEG Dateien|*.jpeg;*.jpg|PNG Dateien|*.png|GIF Dateien|*.gif|Alle Formate|*.*";

            if ((bool)dialog.ShowDialog())
            {
                viewModel.Drink.Image = dialog.FileName;
            }
        }
    }
}
