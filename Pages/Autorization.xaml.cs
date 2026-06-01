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
    /// Логика взаимодействия для Autorization.xaml
    /// </summary>
    public partial class Autorization : Page
    {
        private int _localAttemptCount = 0;
        public Autorization()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

           
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Заполните логин и пароль";
                return;
            }

            
            var captchaWindow = new Captha();
            captchaWindow.Owner = Window.GetWindow(this);

            bool? result = captchaWindow.ShowDialog();

           
            if (captchaWindow.IsCaptchaFailed)
            {
                // Блокируем пользователя
                using (var db = new RestouranOrders_10Entities())
                {
                    var user = db.Users.FirstOrDefault(u => u.Login == login);
                    if (user != null && user.IdRoles != 1)
                    {
                        user.IsBlocked = true;
                        user.Attempts = 3;
                        db.SaveChanges();
                        ErrorTextBlock.Text = "Вы заблокированы (3 ошибки капчи). Обратитесь к администратору";
                        return;
                    }
                }
                ErrorTextBlock.Text = "Слишком много ошибок капчи";
                return;
            }

            if (!captchaWindow.IsCaptchaPassed)
            {
                ErrorTextBlock.Text = "Необходимо пройти капчу";
                return;
            }

            
            using (var db = new RestouranOrders_10Entities())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);

                if (user == null)
                {
                    ErrorTextBlock.Text = "Неверный логин или пароль";
                    return;
                }

                if (user.IsBlocked)
                {
                    ErrorTextBlock.Text = "Вы заблокированы. Обратитесь к администратору";
                    return;
                }

                if (user.Password == password)
                {
                   
                    if (user.IdRoles != 1)
                    {
                        user.Attempts = 0;
                    }
                    db.SaveChanges();

                    MessageBox.Show("Вы успешно авторизовались", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    if (user.IdRoles == 1)
                        NavigationService.Navigate(new AdminPage());
                    else
                        NavigationService.Navigate(new Navigation());
                }
                else
                {
                    
                    if (user.IdRoles != 1)
                    {
                        _localAttemptCount++;
                        int newAttempts = user.Attempts + 1;
                        user.Attempts = newAttempts;

                        if (newAttempts >= 3 || _localAttemptCount >= 3)
                        {
                            user.IsBlocked = true;
                            db.SaveChanges();
                            ErrorTextBlock.Text = "Вы заблокированы (3 ошибки пароля). Обратитесь к администратору";
                            return;
                        }
                    }

                    db.SaveChanges();
                    ErrorTextBlock.Text = user.IdRoles == 1
                        ? "Неверный пароль администратора"
                        : $"Неверный логин или пароль. Осталось попыток: {3 - user.Attempts}";
                }
            }
        }
    }
}
    
