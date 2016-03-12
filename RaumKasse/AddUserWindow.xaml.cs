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
    /// Interaction logic for AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        AddUserViewModel viewModel;

        public AddUserWindow(User user, CheckoutDataContext context)
        {
            InitializeComponent();

            viewModel = new AddUserViewModel(user, context);
            this.DataContext = viewModel;

            ImgAvatar.MouseDown += ImgAvatar_MouseDown;
            BtnAddUser.Click += BtnAddUser_Click;
            BtnCancel.Click += BtnCancel_Click;
        }

        void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Cancel();
        }

        void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddUser();
            this.Close();
        }

        void ImgAvatar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JPEG Dateien|*.jpeg;*.jpg|PNG Dateien|*.png|GIF Dateien|*.gif|Alle Formate|*.*";

            if ((bool)dialog.ShowDialog())
            {
                viewModel.User.Image = dialog.FileName;
            }
            
        }
    }
}
