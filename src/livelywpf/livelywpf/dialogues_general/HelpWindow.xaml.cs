﻿using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace livelywpf
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public HelpWindow()
        {
            InitializeComponent();

            mePlayer.LoadedBehavior = MediaState.Manual;
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\docs\\help_vid_1.mp4");
            if (System.IO.File.Exists(path))
            {
                mePlayer.Source = new Uri(path);
                mePlayer.Stretch = Stretch.UniformToFill; 
                mePlayer.MediaEnded += MePlayer_MediaEnded;
                //mePlayer.MediaOpened += MePlayer_MediaOpened; 
                mePlayer.MediaFailed += MePlayer_MediaFailed;
                mePlayer.Volume = 0;

                mePlayer.Play();
            }
        }

        private void MePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {

        }

        //license file hyperlink
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void MePlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Logger.Error("MediaFoundation Playback Failure:-" + e.ToString());
        }
        
        public void MutePlayer(bool isMute)
        {
            if (isMute)
                mePlayer.Volume = 0;
            else
                mePlayer.Volume = 1;
        }

        public void PausePlayer()
        {
            mePlayer.Pause();
        }

        public void PlayMedia()
        {
            mePlayer.Play();
        }

        public void StopPlayer()
        {
            mePlayer.Stop();
        }

        private void MePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            mePlayer.Position = TimeSpan.Zero;//new TimeSpan(0, 0, 0, 1);
            mePlayer.Play();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mePlayer.MediaOpened -= MePlayer_MediaOpened;
            mePlayer.MediaEnded -= MePlayer_MediaEnded;
            mePlayer.MediaFailed -= MePlayer_MediaFailed;

            mePlayer.Stop();
            mePlayer.Source = null;
            mePlayer.Close();
        }
    }
}
