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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _10_ResoutanOrders.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        private Users _selectedUser;
        public AdminPage()
        {
            InitializeComponent();
            LoadUsers();
        }
        private void LoadUsers()
        {
            using (var db = new RestouranOrders_10Entities())
            {
                DgUsers.ItemsSource = db.Users
                    .Include("Roles")
                    .ToList();
            }
        }
        private void ClearForm()
        {
            TxtLogin.Text = "";
            TxtPassword.Password = "";
            CmbRole.SelectedIndex = 1; // Пользователь по умолчанию
            _selectedUser = null;
            BtnAdd.IsEnabled = true;
            BtnUpdate.IsEnabled = true;
        }

        private void DgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = DgUsers.SelectedItem as Users;

            if (_selectedUser != null)
            {
                TxtLogin.Text = _selectedUser.Login;
                TxtPassword.Password = _selectedUser.Password;

               
                foreach (ComboBoxItem item in CmbRole.Items)
                {
                    if (item.Tag.ToString() == _selectedUser.Roles.ToString())
                    {
                        CmbRole.SelectedItem = item;
                        break;
                    }
                }

                BtnAdd.IsEnabled = false;
            }
        }

               private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация входных данных
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) || string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                MessageBox.Show("Заполните логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbRole.SelectedItem == null)
            {
                MessageBox.Show("Выберите роль для пользователя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Получение ID выбранной роли из Tag элемента ComboBox
                var selectedItem = CmbRole.SelectedItem as ComboBoxItem;
                if (!int.TryParse(selectedItem.Tag.ToString(), out int roleId))
                {
                    MessageBox.Show("Ошибка при определении роли!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 3. Создание и сохранение нового пользователя
                using (var db = new RestouranOrders_10Entities())
                {
                    // Проверка на уникальность логина
                    if (db.Users.Any(u => u.Login == TxtLogin.Text.Trim()))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Users newUser = new Users
                    {
                        Login = TxtLogin.Text.Trim(),
                        Password = TxtPassword.Password.Trim(), // Внимание: в реальных проектах пароль нужно хешировать!
                       IdRoles = roleId // Замените на имя вашего внешнего ключа в таблице Users (например, RoleID или IdRole)
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();
                }

                // 4. Обновление UI
                MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    



        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password;
            int IdRoles = int.Parse(((ComboBoxItem)CmbRole.SelectedItem).Tag.ToString());

            using (var db = new RestouranOrders_10Entities())
            {
                var user = db.Users.Find(_selectedUser.IdUsers);

                if (user != null)
                {
                    // Проверка на уникальность логина (если меняем)
                    if (user.Login != login && db.Users.Any(u => u.Login == login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    user.Login = login;
                    user.Password = password;
                    user.IdRoles = IdRoles;

                    db.SaveChanges();
                }
            }

            MessageBox.Show("Данные пользователя обновлены", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            LoadUsers();
            ClearForm();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnUnblock_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            int userId = (int)btn.Tag;

            using (var db = new RestouranOrders_10Entities())
            {
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    user.IsBlocked = false;
                    user.Attempts = 0;
                    db.SaveChanges();
                }
            }

            LoadUsers(); 

            MessageBox.Show("Пользователь разблокирован", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnNazad_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Navigation());
        }
    }
}
