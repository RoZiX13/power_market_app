using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace power_market_app
{
    internal static class Program
    { 
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (UserIsAuth())
            {
                Application.Run(new Power_Market());
            }
            else
            {
                Application.Run(new Auth());
            }
            
        }
        private static bool UserIsAuth()
        {
            string token = GetTokenSecurely();

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            return ValidateToken(token);
        }

        private static bool ValidateToken(string token)
        {
            using (var client = new HttpClient())
            {
                // Создаем объект JSON для отправки
                var content = new StringContent($"{{\"token\":\"{token}\"}}", Encoding.UTF8, "application/json");

                // Отправляем POST-запрос и ждем выполнения
                HttpResponseMessage response = client.PostAsync("http://localhost:8080/auth", content).Result;
                // Проводим проверку успешности запроса
                return response.IsSuccessStatusCode;
            }
        }
        private static string GetTokenSecurely()
        {
            string tempDir = Path.GetTempPath();
            // Определяем временный каталог
            string filePath = Path.Combine(tempDir, "token.dat");

            // Проверяем, существует ли файл
            if (!File.Exists(filePath))
            {
                return null;
            }

            // Читаем зашифрованный токен из файла
            byte[] encryptedToken = File.ReadAllBytes(filePath);

            // Расшифровываем токен
            byte[] decryptedToken = ProtectedData.Unprotect(
                encryptedToken,
                null,
                DataProtectionScope.CurrentUser);

            // Возвращаем токен в виде строки
            return Encoding.UTF8.GetString(decryptedToken);
        }

        public static void OpenForm(Form open, Form close)
        { 
            open.Show();
            close.Hide();
        }

        public static void SetPlaceholderBehavioForHiden(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            textBox.Enter += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                    textBox.UseSystemPasswordChar = true;
                }
            };

            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.UseSystemPasswordChar = false;
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }

        public static void SetPlaceholderBehavior(TextBox textBox, string placeholderText)
        {
            textBox.Enter += (sender, e) =>
            {
                if (textBox.Text == placeholderText)
                {
                    textBox.Text = "";
                    textBox.ForeColor = SystemColors.WindowText;
                }
            };

            textBox.Leave += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholderText;
                    textBox.ForeColor = SystemColors.GrayText;
                }
            };

            textBox.Text = placeholderText;
            textBox.ForeColor = SystemColors.GrayText;
        }
    }

}
