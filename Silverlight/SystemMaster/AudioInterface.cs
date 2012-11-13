using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SystemMaster
{
    public class AudioInterface
    {
        public static string BASE_AUDIO_URL = "audio/";//"http://masteredsoftware.com/";
        public static string URL_START_MENU_TRACK = "menu-start.wma";
        //static string IN_GAME_TRACK_FULL = "in-game-full.mp3";
        public static string[] IN_GAME_TRACKS = {"in-game1.wma", "in-game2.wma", "in-game3.wma", "in-game4.wma"};
        public static string URL_GAME_OVER_TRACK = "game-over.wma";

        private MediaElement mediaStart = null;

        private MediaElement mediaInGame1 = null;
        private MediaElement mediaInGame2 = null;
        private MediaElement mediaInGame3 = null;
        private MediaElement mediaInGame4 = null;
        private MediaElement mediaEnd = null;

        private AudioPhase phase = AudioPhase.StartMenu;

        private int currentInGameTrack = 0;

        private DispatcherTimer fadeoutTimer = null;
        private MediaElement fadingPlayer = null;

        private double volume = 1.0;
        public double Volume
        {
            get { return this.volume; }
            set
            {
                this.volume = value;

                this.mediaStart.Volume = value;
                this.mediaInGame1.Volume = value;
                this.mediaInGame2.Volume = value;
                this.mediaInGame3.Volume = value;
                this.mediaInGame4.Volume = value;
                this.mediaEnd.Volume = value;
            }
        }

        private bool muted = false;
        public bool Muted
        {
            get { return this.muted; }
            set 
            { 
                this.muted = value;

                this.mediaStart.IsMuted = value;
                this.mediaInGame1.IsMuted = value;
                this.mediaInGame2.IsMuted = value;
                this.mediaInGame3.IsMuted = value;
                this.mediaInGame4.IsMuted = value;
                this.mediaEnd.IsMuted = value;
            }
        }

        public AudioInterface(MediaElement start, MediaElement inGame1, MediaElement inGame2, MediaElement inGame3, MediaElement inGame4, MediaElement end)
        {
            this.mediaStart = start;
            this.mediaInGame1 = inGame1;
            this.mediaInGame2 = inGame2;
            this.mediaInGame3 = inGame3;
            this.mediaInGame4 = inGame4;
            this.mediaEnd = end;


            mediaStart.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(mediaStart_MediaFailed);
            mediaStart.MediaEnded += new RoutedEventHandler(mediaStart_MediaEnded);
            mediaStart.MediaOpened += new RoutedEventHandler(mediaStart_MediaOpened);
            mediaStart.DownloadProgressChanged += new RoutedEventHandler(mediaStart_DownloadProgressChanged);
            mediaStart.Source = new Uri(BASE_AUDIO_URL + URL_START_MENU_TRACK, UriKind.RelativeOrAbsolute);

            mediaInGame1.MediaEnded += new RoutedEventHandler(mediaInGame1_MediaEnded);
            mediaInGame1.DownloadProgressChanged += new RoutedEventHandler(mediaInGame1_DownloadProgressChanged);
            
            mediaInGame2.MediaEnded += new RoutedEventHandler(mediaInGame2_MediaEnded);
            mediaInGame2.DownloadProgressChanged += new RoutedEventHandler(mediaInGame2_DownloadProgressChanged);
            
            mediaInGame3.MediaEnded += new RoutedEventHandler(mediaInGame3_MediaEnded);
            mediaInGame3.DownloadProgressChanged += new RoutedEventHandler(mediaInGame3_DownloadProgressChanged);
            
            mediaInGame4.MediaEnded += new RoutedEventHandler(mediaInGame4_MediaEnded);
            mediaInGame4.DownloadProgressChanged += new RoutedEventHandler(mediaInGame4_DownloadProgressChanged);

            mediaEnd.MediaEnded += new RoutedEventHandler(mediaEnd_MediaEnded);        
            
            
            this.fadeoutTimer = new DispatcherTimer();
            this.fadeoutTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            this.fadeoutTimer.Tick += new EventHandler(fadeoutTimer_Tick);
            
        }

        void mediaStart_MediaOpened(object sender, RoutedEventArgs e)
        {
            //this.messageWindow("mediaStart_MediaOpened", "opened");
            mediaStart.Play();
        }

        void mediaStart_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            this.messageWindow("mediaStart_MediaFailed", e.ErrorException.ToString());
        }

        void mediaStart_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {
            if (this.mediaStart.DownloadProgress == 1)
            {
                //this.messageWindow("mediaStart_DownloadProgressChanged", "1");

                mediaInGame1.Source = new Uri(BASE_AUDIO_URL + IN_GAME_TRACKS[0], UriKind.RelativeOrAbsolute);
            }
        }

        void mediaInGame4_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {
            if (this.mediaInGame4.DownloadProgress == 1)
            {
                mediaEnd.Source = new Uri(BASE_AUDIO_URL + URL_GAME_OVER_TRACK, UriKind.RelativeOrAbsolute);
            }
        }

        void mediaInGame3_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {
            if (this.mediaInGame3.DownloadProgress == 1)
            {
                mediaInGame4.Source = new Uri(BASE_AUDIO_URL + IN_GAME_TRACKS[3], UriKind.RelativeOrAbsolute);
            }
        }

        void mediaInGame2_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {
            if (this.mediaInGame2.DownloadProgress == 1)
            {
                mediaInGame3.Source = new Uri(BASE_AUDIO_URL + IN_GAME_TRACKS[2], UriKind.RelativeOrAbsolute);
            }
        }

        void mediaInGame1_DownloadProgressChanged(object sender, RoutedEventArgs e)
        {
            if (this.mediaInGame1.DownloadProgress == 1)
            {
                mediaInGame2.Source = new Uri(BASE_AUDIO_URL + IN_GAME_TRACKS[1], UriKind.RelativeOrAbsolute);
            }
        }

        void mediaStart_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaStart.Stop();
            mediaStart.Position = TimeSpan.Zero;

            mediaEnded();
        }
        
        void mediaInGame1_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaInGame1.Stop();
            mediaInGame1.Position = TimeSpan.Zero;

            mediaEnded();
        }

        void mediaInGame2_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaInGame2.Stop();
            mediaInGame2.Position = TimeSpan.Zero;

            mediaEnded();
        }

        void mediaInGame3_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaInGame3.Stop();
            mediaInGame3.Position = TimeSpan.Zero;

            mediaEnded();
        }

        void mediaInGame4_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaInGame4.Stop();
            mediaInGame4.Position = TimeSpan.Zero;

            mediaEnded();
        }

        void mediaEnd_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaEnd.Stop();
            mediaEnd.Position = TimeSpan.Zero;

            mediaEnded();
        }

        private void mediaEnded()
        {
            if (phase == AudioPhase.InGame)
            {
                currentInGameTrack++;

                if (currentInGameTrack >= IN_GAME_TRACKS.Length)
                    currentInGameTrack = 0;
            }

            MediaElement meCurrent = getMediaElementForCurrentPhase();
            if (meCurrent != null)
            {
                //loop the track
                meCurrent.Play();
            }
        }

        private MediaElement getMediaElementForCurrentPhase()
        {
            MediaElement meRet = null;
            if (phase == AudioPhase.StartMenu)
            {
                meRet = this.mediaStart;
            }
            else if (phase == AudioPhase.GameOver)
            {
                meRet = this.mediaEnd;
            }
            else
            {
                switch (currentInGameTrack)
                {
                    case 0:
                        meRet = this.mediaInGame1;
                        break;
                    case 1:
                        meRet = this.mediaInGame2;
                        break;
                    case 2:
                        meRet = this.mediaInGame3;
                        break;
                    case 3:
                        meRet = this.mediaInGame4;
                        break;
                }
            }
            return meRet;
        }

        public void StartFadeOut()
        {
            this.fadingPlayer = getMediaElementForCurrentPhase();
            this.fadeoutTimer.Start();
        }

        void fadeoutTimer_Tick(object sender, EventArgs e)
        {

            if (fadingPlayer.Volume <= 0.1)
            {
                this.fadeoutTimer.Stop();

                this.fadingPlayer.Volume = 0;
                this.fadingPlayer.Stop();
                this.fadingPlayer.Position = TimeSpan.Zero;
                this.fadingPlayer.Volume = this.volume;
            }
            else
                fadingPlayer.Volume = fadingPlayer.Volume - 0.1;
        }
        /*
        void mediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            string problem = e.ToString();
            string errorMessage = "mediaPlayer_MediaFailed: " + e.ToString() + "\r\n" + e.ErrorException.ToString();
            //Console.Error.WriteLine(problem);
            System.Diagnostics.Debug.WriteLine(problem);
            //MessageBox.Show("mediaPlayer_MediaFailed: " + e.ErrorException.ToString());
        }
        */
        /*

        public void Stop(bool resetPosition)
        {
            this.playing = false;
            this.mediaPlayer.Stop();
            if(resetPosition)
                this.mediaPlayer.Position = TimeSpan.Zero;
        }
        */
        

        public void StartMenuFirst()
        {
            //this won't do the fade out, just play
            this.phase = AudioPhase.StartMenu;

            //playing is done initially when the media is opened
            //this.mediaStart.Play(); 
        }

        public void StartMenu()
        {
            this.StartFadeOut();

            this.Volume = 1.0;
            this.phase = AudioPhase.StartMenu;

            this.mediaStart.Position = TimeSpan.Zero;
            this.mediaStart.Play();

            
        }

        public void BeginGame()
        {
            this.StartFadeOut();

            this.phase = AudioPhase.InGame;
            this.currentInGameTrack = 0;

            this.mediaInGame1.Play();   
        }

        public void EndGame()
        {
            this.StartFadeOut();

            this.phase = AudioPhase.GameOver;

            this.mediaEnd.Play();
        }

        private void messageWindow(string title, string message)
        {
            ChildWindow cw = new ChildWindow();
            cw.Title = title;
            cw.Content = message;
            cw.Show();
        }

        enum AudioPhase
        {
            StartMenu,
            InGame,
            GameOver
        }
    }

}
