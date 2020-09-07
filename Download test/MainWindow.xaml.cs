using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
using System.Windows.Threading;

namespace Download_test
{
    /// <summary>
    /// All the back end code to download the file.
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebClient webClient;
        private bool wait;
        private DispatcherTimer timer;

        /*
         <Summary>
            On Startup, it configures the web client and the dispatchtimer
         </Summary>
        */
        public MainWindow()
        {
            InitializeComponent();
            webClient = new WebClient();
            webClient.DownloadProgressChanged += UpdateProgress;
            webClient.DownloadDataCompleted += CompleteDownload;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            pgbBar.Minimum = 0;
            pgbBar.Maximum = 100;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!CheckInternetStatus()) ThrowException(ErrorMessages.NoInternet);
        }

        /*
         <Summary>
            Starts the download process. Before starting, it checks if the url is valid.
         </Summary>
        */
        private void btnDowload_Click(object sender, RoutedEventArgs e)
        {
            Wait();
            if (pgbBar.Value > 0) pgbBar.Value = 0;
            var uri = new Uri(txtInput.Text);
            if (uri != null)
            {
                StartDownload(uri);
            }
            else
            {
                MessageBox.Show(ErrorMessages.InvalidUrl, "Url invalid", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*
         <Summary>
            Downloads the file. It throws an exception if there is en error.
         </Summary>
        */
        private void StartDownload(Uri uri)
        {
            string fileName = System.IO.Path.GetFileName(uri.AbsolutePath);
            string newPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}";
            try
            {
                if (CheckInternetStatus() == false) ThrowException(ErrorMessages.NoInternet);
                webClient.DownloadFileAsync(uri, newPath + "\\" + fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        /*
         <Summary>
            Checks if the host is connected to the internet
         </Summary>
        */
        public static bool CheckInternetStatus()
        {
            int desc;
            return InternetGetConnectedState(out desc, 0);
        }
        /*
         <Summary>
           Get called every time when an exception needs to be trown.
         </Summary>
        */
        private void ThrowException(string message)
        {
            throw new Exception(message);
        }
        /*
         <Summary>
            Displays the waiting message untill the web client connects and can reach the file.
         </Summary>
        */
        private async void Wait()
        {
            int dotcounter = 0;
            string process = "Starting process";
            timer.Start();
            timer.Tick += delegate
                {
                    if (dotcounter == 3)
                    {
                        dotcounter = 0;
                        process = process.Replace(".", "");
                    }
                    process += ".";
                    dotcounter++;
                    lblDownloadProgress.Content = process;
                };
        }

        /*
        <Summary>
           Gets called every time when the client recieves bytes.
           It updates the current the current download status.
        </Summary>
        */

        private void UpdateProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            if (timer.IsEnabled) timer.Stop();
            if (CheckInternetStatus() == false) ThrowException(ErrorMessages.DownloadInteruption);
            string stringBytesRecieved = FormatSize(e.BytesReceived);
            string stringTotalBytesToRecieve = FormatSize(e.TotalBytesToReceive);

            double doubleBytesRecieved = double.Parse(e.BytesReceived.ToString());
            double doubleTotalBytesToRecieve = double.Parse(e.TotalBytesToReceive.ToString());

            var percentage = doubleBytesRecieved / doubleTotalBytesToRecieve * 100;
            pgbBar.Value = percentage;
            lblDownloadProgress.Content = $"Progress: {stringBytesRecieved} /{stringTotalBytesToRecieve}";
            if (CheckInternetStatus() == false) ThrowException(ErrorMessages.DownloadInteruption);
        }

        static readonly string[] suffixes =
{ "Bytes", "KB", "MB", "GB", "TB", "PB" };

        /*
         <Summary>
            It Constantly divides the byteSize of the file that will be downloaded.
            Constantly divides it untill it's smaller than 1 increments the counter each time.
            Than it returns the correct size format based on the counter from the array above.
        </Summary>
         */
        public static string FormatSize(Int64 bytes)
        {
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }

        private void CompleteDownload(object sender, DownloadDataCompletedEventArgs e)
        {
            pgbBar.Value = 0;
            lblDownloadProgress.Content = "Download Succesfull!! 😊";
        }
    }
}
