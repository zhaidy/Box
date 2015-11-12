using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;

namespace Box
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        string _playerId = "";
        string _server = "";
        string server = "";
        string playerId = "";
        string tier = "";
        string rank = "";
        string points = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _playerId = txtPlayerId.Text;
            _server = "网通一";
            GetPlayerService.GetPlayerService getPlayer = new GetPlayerService.GetPlayerService();
            var result = getPlayer.GetPlayerProfile(_server, _playerId);
            imgPlayerIcon.Source = icon(result.icon);
            lblPlayerId.Content = result.player_id;
            lblLevel.Content = result.level;
            for (int i = 0; i < result.com_champ.Length; i++)
            {
                Image img = new Image();
                img.Height = 24;
                img.Width = 24;
                img.Source = icon(result.com_champ[i].icon);
                Grid.SetColumn(img, i);
                gComChamp.Children.Add(img);
            }
        }
       
        protected BitmapImage icon(string url)
        {
            var image = new BitmapImage();
            int BytesToRead = 100;

            WebRequest request = WebRequest.Create(new Uri(url, UriKind.Absolute));
            request.Timeout = -1;
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            BinaryReader reader = new BinaryReader(responseStream);
            MemoryStream memoryStream = new MemoryStream();

            byte[] bytebuffer = new byte[BytesToRead];
            int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

            while (bytesRead > 0)
            {
                memoryStream.Write(bytebuffer, 0, bytesRead);
                bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
            }

            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);

            image.StreamSource = memoryStream;
            image.EndInit();
            return image;
        }
       
    }
}
