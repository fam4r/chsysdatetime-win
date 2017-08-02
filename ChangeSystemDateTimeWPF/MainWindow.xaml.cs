using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Security.Principal; // Per Classi di verifica account Administrator
using System.Net;
using System.Globalization;

//using System.Collections.Generic;
//using System.Linq;
//using System.Windows.Input;
//using System.Text;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace ChangeSystemDateTimeWPF
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckForAdministrator();
            AllToDefaults();
        }

        public struct SystemTime
        {
            public ushort Year, Month, DayOfWeek, Day, Hour, Minute, Second, Millisecond;
        };

        [DllImport("kernel32.dll", EntryPoint = "GetSystemTime", SetLastError = true)]
        public extern static void Win32GetSystemTime(ref SystemTime sysTime);

        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        public extern static bool Win32SetSystemTime(ref SystemTime sysTime);

        DateTime dt;
        DateTime prevDt = DateTime.Now;
        SystemTime updatedTime;

        string result;
        bool defValues;
        const int yearDef = 2000, monthDef = 1, dayDef = 1, hourDef = 10, minuteDef = 0, secondDef = 0;
        int year, month, day, hour, minute, second;
        
        private void btnSetOldDate_Click(object sender, RoutedEventArgs e)
        {
            lblOutput.Content = "";

            if (!defValues)
            {
                if (!ParseTextBoxes())
                {
                    ShowMessage("Errore! I valori devono essere numeri", true);
                    return;
                }
            }

            try
            {
                dt = new DateTime(year, month, day, hour, minute, second);
            }
            catch
            {
                ShowMessage("Errore! La data non è corretta", true);
                return;
            }


            if (!ChangeTime(dt, out result))
            {
                ShowMessage(result.ToString(), true);
                return;
            }
            else
            {
                ShowMessage("Data impostata: " + dt.ToString(), false);
            }
        }

        private void btnSetNowDate_Click(object sender, RoutedEventArgs e)
        {
            lblOutput.Content = "";

            try
            {
                using (var response = WebRequest.Create("http://www.google.com").GetResponse())
                {
                    dt = DateTime.ParseExact(response.Headers["date"],
                         "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                         CultureInfo.InvariantCulture.DateTimeFormat,
                         DateTimeStyles.AssumeLocal);
                }

            }
            catch (WebException)
            {
                ShowMessage("Controllare la connessione ad Internet", true);
                // Se non riesce a prendere l'ora da Internet, recupera l'ultimo orario memorizzato.
                dt = prevDt;
                return;
            }

            if (!ChangeTime(dt, out result))
            {
                ShowMessage(result.ToString(), true);
                return;
            }
            else
            {
                ShowMessage("Data impostata: " + GetSystemTime(updatedTime) , false);
            }
        }

        private bool ChangeTime(DateTime dt, out string ex)
        {
            try
            {
                updatedTime = new SystemTime();
                updatedTime.Year = (ushort)dt.Year;
                updatedTime.Month = (ushort)dt.Month;
                updatedTime.Day = (ushort)dt.Day;
                updatedTime.Hour = (ushort)dt.Hour;
                updatedTime.Minute = (ushort)dt.Minute;
                updatedTime.Second = (ushort)dt.Second;
                // Call the unmanaged function that sets the new date and time instantly
                Win32SetSystemTime(ref updatedTime);
                ex = "";
                return true;
            }
            catch (Exception exc)
            {
                ex = exc.Message;
                return false;
            }
        }

        // Why? ????
        private string GetSystemTime(SystemTime st)
        {
            return st.Day + "/" + st.Month + "/" + st.Year + " " + (st.Hour+2) + ":" + st.Minute + ":" + st.Second;
        }

        private void CheckForAdministrator()
        {
            // Verifica che l'utente attuale appartenga a un gruppo di utenti di Windows.
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            // Se admin
            if (principal.IsInRole(WindowsBuiltInRole.Administrator)) // Controlla che l'utente attuale sia un Administrator.
            {
                btnSetOldDate.IsEnabled = true;
            }
            else
            {
                btnSetOldDate.IsEnabled = false;
            }
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Campi reimpostati", false);
            AllToDefaults();
        }

        private bool ParseTextBoxes()
        {
            DateTime dtMin = DateTime.MinValue;
            DateTime dtMax = DateTime.MaxValue;

            if (int.TryParse(txtYear.Text, out year)        &&
                int.TryParse(txtMonth.Text, out month)      &&
                int.TryParse(txtDay.Text, out day)          &&
                int.TryParse(txtHour.Text, out hour)        &&
                int.TryParse(txtMinute.Text, out minute)    &&
                int.TryParse(txtSecond.Text, out second))
            {
                // Questo if si può accorpare a quello sopra?
                if ((year >= dtMin.Year && year <= dtMax.Year) &&
                   (month >= dtMin.Month && month <= dtMax.Month) &&
                   (day >= dtMin.Day && day <= dtMax.Day) &&
                   (hour >= dtMin.Hour && hour <= dtMax.Hour) &&
                   (minute >= dtMin.Minute && minute <= dtMax.Minute) &&
                   (second >= dtMin.Second && second <= dtMax.Second))
                {
                    return true;
                }
                else return false;
            }
            else return false;

        }

        private void SetDefaultValues()
        {
            year = yearDef;
            month = monthDef;
            day = dayDef;
            hour = hourDef;
            minute = minuteDef;
            second = secondDef;
            defValues = true;
        }

        private void AllToDefaults()
        {
            // Values
            SetDefaultValues();

            // TextBoxes
            txtYear.Text = yearDef.ToString();
            txtMonth.Text = monthDef.ToString();
            txtDay.Text = dayDef.ToString();
            txtHour.Text = hourDef.ToString();
            txtMinute.Text = minuteDef.ToString();
            txtSecond.Text = secondDef.ToString();
        }

        private void ShowMessage(string Message, bool IsError)
        {
            lblOutput.Foreground = IsError ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Black;
            lblOutput.Content = Message;
        }

        // Se il valore è stato cambiato devo parsarlo
        // Usare if ternario?
        private void txtYear_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtYear.Text == yearDef.ToString())
                defValues = true;
            else defValues = false;
        }

        private void txtMonth_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtMonth.Text == monthDef.ToString())
                defValues = true;
            else defValues = false;
        }

        private void txtDay_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtDay.Text == dayDef.ToString())
                defValues = true;
            else defValues = false;
        }

        private void txtHour_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtHour.Text == hourDef.ToString())
                defValues = true;
            else defValues = false;
        }

        private void txtMinute_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtMinute.Text == minuteDef.ToString())
                defValues = true;
            else defValues = false;
        }

        private void txtSecond_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            defValues = txtSecond.Text == secondDef.ToString() ? true : false;
        }

    }
}
