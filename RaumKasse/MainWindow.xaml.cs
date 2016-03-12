using RaumKasse.viewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaumKasse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainViewModel();
            viewModel.loadUsers();
            viewModel.loadDrinks();
            viewModel.loadLog();
            this.DataContext = viewModel;

            UserListBox.SelectionChanged += UserListBox_SelectionChanged;
            DrinkListBox.SelectionChanged += DrinkListBox_SelectionChanged;

            UserSearch.GotKeyboardFocus += UserSearch_GotKeyboardFocus;
            UserSearch.TextChanged += UserSearch_TextChanged;
            DrinkSearch.GotKeyboardFocus += DrinkSearch_GotKeyboardFocus;
            DrinkSearch.TextChanged += DrinkSearch_TextChanged;

            BtnAddUser.Click += BtnAddUser_Click;
            BtnEditUser.Click += BtnEditUser_Click;
            BtnDelUser.Click += BtnDelUser_Click;

            BtnAddDrink.Click += BtnAddDrink_Click;
            BtnEditDrink.Click += BtnEditDrink_Click;
            BtnDelDrink.Click += BtnDelDrink_Click;

            BtnDelUserAction.Click += BtnDelUserAction_Click;

            ((INotifyCollectionChanged)LogListBox.Items).CollectionChanged += LogListBox_CollectionChanged;
        }

        void BtnDelUserAction_Click(object sender, RoutedEventArgs e)
        {
            viewModel.deleteUserAction(LogListBox.SelectedItem != null ? (ILoggable)LogListBox.SelectedItem : null);
        }

        void LogListBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
            LogListBox.ScrollIntoView(LogListBox.SelectedItem);
        }

        void DrinkSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel.loadDrinks(DrinkSearch.Text);
        }

        void UserSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel.loadUsers(UserSearch.Text);
        }

        void BtnDelDrink_Click(object sender, RoutedEventArgs e)
        {
            viewModel.deleteDrink();
        }

        void BtnEditDrink_Click(object sender, RoutedEventArgs e)
        {
            viewModel.editDrink();
        }

        void BtnAddDrink_Click(object sender, RoutedEventArgs e)
        {
            viewModel.addDrink();
        }

        void BtnDelUser_Click(object sender, RoutedEventArgs e)
        {
            viewModel.deleteUser();
        }

        void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            viewModel.editUser();
        }

        void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            viewModel.addUser();
        }

        void DrinkSearch_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DrinkSearch.Clear();
        }

        void UserSearch_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UserSearch.Clear();
        }

        void DrinkListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DrinkListBox.SelectedItem == null)
                return;

            viewModel.SelectedDrink = (Drink)DrinkListBox.SelectedItem;
            Debug.WriteLine("selected drink: " + viewModel.SelectedDrink.Name);
        }

        void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserListBox.SelectedItem == null)
                return;

            viewModel.SelectedUser = (User)UserListBox.SelectedItem;
            Debug.WriteLine("selected user: " + viewModel.SelectedUser.Name);
        }

        private void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            double amount = 0;
            if (Double.TryParse(TxtPay.Text, out amount))
            {
                viewModel.addPayment(amount);
            }

            TxtPay.Clear();
        }

        private void BtnBuy_Click(object sender, RoutedEventArgs e)
        {
            viewModel.addPurchase();
        }
    }
}
