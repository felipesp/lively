﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace livelywpf
{
    public static partial class SaveData
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns data stored in class object file.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string PropertyList(this object obj)
        {
            try
            {
                var props = obj.GetType().GetProperties();
                var sb = new StringBuilder();
                foreach (var p in props)
                {
                    sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
                }
                return sb.ToString();
            }
            catch
            {
                return "Failed to retrive properties of config file.";
            }
        }

        #region apprule
        public enum AppRulesEnum
        {
            [Description("Pause")]
            pause,
            [Description("Ignore")]
            ignore,
            [Description("Kill(Free Memory)")]
            kill
        }

        [Serializable]
        public class ApplicationRules : INotifyPropertyChanged
        {
            private string appName;
            private AppRulesEnum rule;
            public string LocalisedRule { get; set; }
            public string AppName
            {
                get
                {
                    return appName;
                }
                set
                {
                    appName = value;
                    OnPropertyChanged("AppName");
                }
            }
            public AppRulesEnum Rule
            {
                get
                {
                    return rule;
                }
                set
                {
                    rule = value;
                    if(value == AppRulesEnum.pause)
                    {
                        LocalisedRule = Properties.Resources.cmbBoxPause;
                    }
                    else if(value == AppRulesEnum.ignore)
                    {
                        LocalisedRule = Properties.Resources.txtIgnore;
                    }
                    else
                    {
                        LocalisedRule = Properties.Resources.cmbBoxKill;
                    }
                    OnPropertyChanged("Rule");
                    OnPropertyChanged("LocalisedRule");
                }
            }
            public ApplicationRules()
            {
                AppName = null;
                Rule = AppRulesEnum.ignore;
               
            }

            private void OnPropertyChanged(string property)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
        public static List<ApplicationRules> appRules = new List<ApplicationRules>();
        public class ApplicationRulesList
        {
            public string AppVersion { get; set; }
            public List<ApplicationRules> App { get; set; }
            public ApplicationRulesList()
            {
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static void SaveApplicationRules()
        {
            ApplicationRulesList tmp = new ApplicationRulesList
            {
                App = appRules
            };

            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            //serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Include;
            /*
            if (String.IsNullOrWhiteSpace(tmp.AppVersion))
                tmp.AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            */
            try
            {
                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\application_rules.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, tmp);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        public static void LoadApplicationRules()
        {
            //default rules.
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\application_rules.json"))
            {
                appRules.Add(new ApplicationRules { AppName = "Photoshop", Rule = AppRulesEnum.pause });
                appRules.Add(new ApplicationRules { AppName = "Discord", Rule = AppRulesEnum.ignore });
                SaveApplicationRules();
                return;
            }

            try
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\application_rules.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ApplicationRulesList tmp = (ApplicationRulesList)serializer.Deserialize(file, typeof(ApplicationRulesList));
                    appRules = tmp.App;
                }

            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        #endregion apprule

        #region livelyinfo

        [Serializable]
        public class LivelyInfo //wallpaper metadata
        {
            public string AppVersion { get; set; }
            public string Title { get; set; }
            public string Thumbnail { get; set; }
            public string Preview { get; set; } //preview clip
            public string Desc { get; set; }
            public string Author { get; set; }
            public string License { get; set; }
            public string Contact { get; set; }
            public SetupDesktop.WallpaperType Type { get; set; }
            public string FileName { get; set; }
            public string Arguments { get; set; } //start commandline args
            public LivelyInfo()
            {
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Title = null;
                Thumbnail = null;
                Preview = null;
                Type = SetupDesktop.WallpaperType.web_audio;
                FileName = null;
                Desc = null;
                Author = null;
                License = null;
                Contact = null;
                Arguments = null;
            }

            public LivelyInfo(LivelyInfo info)
            {
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Title = info.Title;
                Thumbnail = info.Thumbnail;
                Preview = info.Preview;
                Type = info.Type;
                FileName = info.FileName;
                Desc = info.Desc;
                Author = info.Author;
                Contact = info.Contact;
                License = info.License;
                Arguments = info.Arguments;
            }
        }

        public static LivelyInfo info = new LivelyInfo();
        public static void SaveWallpaperMetaData(LivelyInfo info, string path) //used to create livelyinfo.json file
        {
            //return;
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
            /*
            if (String.IsNullOrWhiteSpace(info.AppVersion))
                info.AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            */
            try
            {
                using (StreamWriter sw = new StreamWriter(path + "\\LivelyInfo.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, info);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Load Livelinfo.json file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true - success, false - failure</returns>
        public static bool LoadWallpaperMetaData(string path)
        {
            try
            {
                using (StreamReader file = File.OpenText(path + "\\LivelyInfo.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    info = (LivelyInfo)serializer.Deserialize(file, typeof(LivelyInfo));
                }
                return true;
            }
            catch (IOException e1)
            {
                Logger.Error("Error trying to read livelyinfo file from disc:- " + path + "\n" + e1.ToString() );
                return false;
            }
            catch(Exception e2)
            {
                Logger.Error("Corrupted livelinfo file, skipping wallpaper:- " + path + "\n" + e2.ToString());
                return false;
            }
        }
        #endregion

        #region monitor_layout

        [Serializable]
        public class WallpaperLayout
        {
            public string DeviceName { get; set; }
            public SetupDesktop.WallpaperType Type { get; set; } //unsure
            public string FilePath { get; set; }
            public string Arguments { get; set; }

            public WallpaperLayout()
            {
                //displayID = 1;
                Type = SetupDesktop.WallpaperType.video;
                FilePath = null;
                DeviceName = null;
                Arguments = "";
            }
        }

        public class WallpaperLayoutList
        {
            public string AppVersion { get; set; }
            public List<WallpaperLayout> Layouts { get; set; }

            public WallpaperLayoutList()
            {
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        //public static List<WallpaperLayout> wallpapers = new List<WallpaperLayout>();

        public static void SaveWallpaperLayout()
        {
            WallpaperLayoutList tmp = new WallpaperLayoutList
            {
                Layouts = SetupDesktop.wallpapers
            };

            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                //serializer.Converters.Add(new JavaScriptDateTimeConverter());
                NullValueHandling = NullValueHandling.Include
            };
            /*
            if (String.IsNullOrWhiteSpace(tmp.AppVersion))
                tmp.AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
             */
            try
            {
                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\lively_layout.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, tmp);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e.ToString());
            }
        }


        public static void LoadWallpaperLayout()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\lively_layout.json"))
            {
                //SaveWallpaperLayout()
                return;
            }

            try
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\lively_layout.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    WallpaperLayoutList tmp = (WallpaperLayoutList)serializer.Deserialize(file, typeof(WallpaperLayoutList));
                    SetupDesktop.wallpapers = tmp.Layouts;
                }

            }
            catch (Exception e)
            {
                SetupDesktop.wallpapers.Clear();
                Logger.Error(e.ToString());
            }
        }
        #endregion

        #region configuration_file

        public enum VideoPlayer
        {
            windowsmp, //0
            mediakit, //1
        }

        public enum GIFPlayer
        {
            xaml,
            mediakit,
            cef
        }

        public enum DisplayPauseEnum
        {
            perdisplay,
            all
        }

        public enum ProcessMonitorAlgorithm
        {
            foreground,
            all
        }

        public enum WallpaperArrangement
        {
            [Description("Per Display")]
            per,
            [Description("Span Across All Display(s)")]
            span
        }

        public enum WallpaperRenderingMode
        {
            [Description("Behind desktop icons")]
            behind_icons,
            [Description("Make the window bottom-most, infront of icons")]
            bottom_most
        }

        [Serializable]
        public class SupportedLanguages
        {
            public string Language { get; set; }
            public string[] Codes { get; set; }

            public SupportedLanguages(string language, string[] codes)
            {
                this.Language = language;
                this.Codes = codes;
            }
        }
        //codes: https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
        public static SupportedLanguages[] supportedLanguages = new SupportedLanguages[] {
                                    new SupportedLanguages("English(en-US)", new string[]{"en-US"}), //technically not US english, sue me..
                                    new SupportedLanguages("中文(zh-CN)", new string[]{"zh", "zh-Hans","zh-CN","zh-SG"}), //are they same?
                                    new SupportedLanguages("日本人(ja-JP)", new string[]{"ja", "ja-JP"}),
                                    new SupportedLanguages("русский(ru)", new string[]{"ru", "ru-BY", "ru-KZ", "ru-KG", "ru-MD", "ru-RU","ru-UA"}), //are they same?
                                    new SupportedLanguages("हिन्दी(hi-IN)", new string[]{"hi", "hi-IN"})                                 
                                    };
        [Serializable]
        public class ConfigFile
        {
            private string language;
            public string AppVersion { get; set; }
            public string Language 
            { 
                get { 
                        return language;
                    }

                set 
                {
                    if (value == null)
                    {
                        value = "en-US";
                    }
                    bool detectedLang = false;
                    //todo: make this more elegant? 
                    foreach (var item in supportedLanguages)
                    {
                        if(Array.Exists(item.Codes, x => x.Equals(value, StringComparison.OrdinalIgnoreCase) ))
                        {
                            language = value;
                            detectedLang = true;
                            break;
                        }
                    }

                    if(!detectedLang)
                        language = "en-US";
                } 
            }

            public bool Startup { get; set; }
            public bool AppTransparency { get; set; }
            public bool IsFirstRun { get; set; }
            public AppRulesEnum AppFocusPause { get; set; }
            public AppRulesEnum AppFullscreenPause { get; set; }
            public DisplayPauseEnum DisplayPauseSettings { get; set; }
            public ProcessMonitorAlgorithm ProcessMonitorAlgorithm { get; set; }
            public bool MuteVideo { get; set; }
            public bool MuteCef { get; set; } //unused, need to get processid of subprocess of cef.
            public bool MuteCefAudioIn { get; set; }
            public bool MuteMic { get; set; }
            public bool LiveTile { get; set; }
            public System.Windows.Media.Stretch VideoScaler { get; set; }
            public System.Windows.Media.Stretch GifScaler { get; set; }

            public WallpaperArrangement WallpaperArrangement { get; set; } // 0 -per monitor, 1-span
            public bool DXVA { get; set; } //hw acceleration videoplayback, currently unused.
            public bool MouseHook { get; set; } //unused currently.
            public bool KeyHook { get; set; } //only bizhawk.
            public bool RunOnlyDesktop { get; set; } //run only when on desktop focus.
            public VideoPlayer VidPlayer { get; set; }
            public GIFPlayer GifPlayer { get; set; }

            public string DefaultURL { get; set; }
            public string BizHawkPath { get; set; }
            public string MPVPath { get; set; } //mpv external video player, unused
            public bool Ui120FPS { get; set; }
            public bool UiDisableHW { get; set; }

            /// <summary>
            /// Timer interval(in milliseconds), used to monitor running apps to determine pause/play of wp's.
            /// </summary>
            public int ProcessTimerInterval { get; set; }
            public WallpaperRenderingMode WallpaperRendering { get; set; }
            /// <summary>
            /// Timeout for application wallpaper startup (in milliseconds), lively will kill wp if gui is not ready within this timeframe.
            /// </summary>
            public int WallpaperWaitTime { get; set; }

            // warning user of risk, count.
            public int WarningUnity { get; set; }
            public int WarningGodot { get; set; }
            public int WarningURL { get; set; }
            public int WarningApp { get; set; }
            public bool SafeShutdown { get; set; }

            public bool IsRestart { get; set; }
            public bool InstallUpdate { get; set; } //future use.

            //default values
            public ConfigFile()
            {
                DefaultURL = "https://www.shadertoy.com/view/MsKcRh";
                MuteCef = false;
                MuteMic = false;
                MuteCefAudioIn = false;
                MuteVideo = false;
                ProcessMonitorAlgorithm = ProcessMonitorAlgorithm.foreground;
                WallpaperArrangement = WallpaperArrangement.per;
                AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Startup = false;
                IsFirstRun = true;
                AppFocusPause = AppRulesEnum.ignore;
                AppFullscreenPause = AppRulesEnum.pause;
                DXVA = true;
                MouseHook = true;
                KeyHook = false;
                BizHawkPath = null;
                //WallpaperType = SetupDesktop.WallpaperType.app;
                VidPlayer = VideoPlayer.windowsmp;
                //CurrWallpaperPath = null;
                MPVPath = null;
                RunOnlyDesktop = false;
                AppTransparency = false;
                GifPlayer = GIFPlayer.xaml;
                Ui120FPS = false;
                UiDisableHW = false;
                WallpaperRendering = WallpaperRenderingMode.behind_icons;
                WallpaperWaitTime = 30000; // 30sec
                ProcessTimerInterval = 500; //reduce to 250 for quicker response.
                Language = CultureInfo.CurrentCulture.Name;//"en"; 

                //media scaling
                VideoScaler = System.Windows.Media.Stretch.UniformToFill; //3
                GifScaler = System.Windows.Media.Stretch.UniformToFill;

                WarningApp = 0;
                WarningUnity = 0;
                WarningGodot = 0;
                WarningURL = 0;

                SafeShutdown = true;
                IsRestart = false;
                InstallUpdate = false;
            }
        }
        public static ConfigFile config = new ConfigFile();
        /// <summary>
        /// Loads settigns file from disk if found, else creates settings files with default constructor values.
        /// </summary>
        public static void LoadConfig()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\lively_config.json"))
            {
                //writing default savefile to storage.
                SaveConfig();
                return;
            }

            try
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\lively_config.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    config = (ConfigFile)serializer.Deserialize(file, typeof(ConfigFile));
                }
                
                //config = JsonConvert.DeserializeObject<Config>(AppDomain.CurrentDomain.BaseDirectory + "\\lively_config.json");
            }
            catch(Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        public static void SaveConfig()
        {
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                //serializer.Converters.Add(new JavaScriptDateTimeConverter());
                NullValueHandling = NullValueHandling.Include
            };
            /*
            if (String.IsNullOrWhiteSpace(config.AppVersion))
                config.AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            */
            try
            {
                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\SaveData\\lively_config.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, config);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
        #endregion

        #region background_monitor_process

        [Serializable]
        public class RunningProgram
        {
            public string ProcessName { get; set; }
            public int Pid { get; set; }
            public RunningProgram()
            {
                Pid = 0;
                ProcessName = null;
            }
        }
        public static List<RunningProgram> runningPrograms = new List<RunningProgram>();
        
        public class RunningProgramsList
        {
            public List<RunningProgram> Item { get; set; }
        }
        

        public static void SaveRunningPrograms()
        {
            RunningProgramsList tmp = new RunningProgramsList
            {
                Item = runningPrograms
            };

            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,

                NullValueHandling = NullValueHandling.Include
            };

            try
            {
                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\lively_running_pgms.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, tmp);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        public static void LoadRunningPrograms()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\lively_running_pgms.json"))
            {
                return;
            }

            try
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "\\lively_running_pgms.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    RunningProgramsList tmp = (RunningProgramsList)serializer.Deserialize(file, typeof(RunningProgramsList));
                    runningPrograms = tmp.Item;
                }

            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        #endregion background_monitor_process

        #region library_tile
        public class TileData : INotifyPropertyChanged
        {
            private BitmapImage img;
            private string tilePreview;
            public BitmapImage Img
            {
                get
                {
                    /*
                    if (img == null)
                        return DependencyProperty.UnsetValue;
                    */
                    return img;
                }
                set
                {
                    img = value;
                    OnPropertyChanged("Img");
                }
            }
            public string TilePreview
            {
                get
                {
                    /*
                    if (string.IsNullOrEmpty(tilePreview))
                        return DependencyProperty.UnsetValue;
                    */
                    return tilePreview;
                }
                set
                {
                    tilePreview = value;
                    OnPropertyChanged("TilePreview");
                }
            }

            public Uri UriContact { get; set; }
            public string Type { get; set; }
            public SaveData.LivelyInfo LivelyInfo { get; set; }

            public TileData(SaveData.LivelyInfo info)
            {
                TilePreview = null; //otherwise everything gets loaded at once!
                if (!SaveData.config.LiveTile)
                {
                    //Img = LoadImage(info.Thumbnail);
                    Img = LoadConvertImage(info.Thumbnail);
                }
                else
                {
                    Img = null;
                }
                UriContact = GetUri(info.Contact, "https");
                Type = LibraryInfoTypeText(info);
                LivelyInfo = info;
            }

            private void OnPropertyChanged(string property)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
            public event PropertyChangedEventHandler PropertyChanged;

            private string LibraryInfoTypeText(SaveData.LivelyInfo info)
            {
                if (info.Type == SetupDesktop.WallpaperType.video)
                {
                    var dimension = MainWindow.GetVideoSize(info.FileName);
                    //return info.Type.ToString().ToUpper() + ", " + dimension.Width + "x" + dimension.Height;
                    return Properties.Resources.txtLivelyWallpaperTypeVideo + ", " + dimension.Width + "x" + dimension.Height;
                }
                else if(info.Type == SetupDesktop.WallpaperType.app)
                {
                    return Properties.Resources.txtLivelyWallpaperTypeApp;
                    //return info.Type.ToString().ToUpper();
                }
                else if(info.Type == SetupDesktop.WallpaperType.godot)
                {
                    return Properties.Resources.txtLivelyWallpaperTypeGodot;
                }
                else if (info.Type == SetupDesktop.WallpaperType.unity)
                {
                    return Properties.Resources.txtLivelyWallpaperTypeUnity;
                }
                else if (info.Type == SetupDesktop.WallpaperType.unity_audio)
                {
                    return Properties.Resources.txtLivelyWallpaperTypeUnityAudio;
                }
                else if (info.Type == SetupDesktop.WallpaperType.web)
                {
                    return Properties.Resources.txtLivelyWallpaperTypeWeb;
                }
                else if (info.Type == SetupDesktop.WallpaperType.web_audio)
                {
                    return Properties.Resources.txtLivelyWallpaperTypeWebAudio;
                }
                else if (info.Type == SetupDesktop.WallpaperType.gif)
                {
                    return Properties.Resources.txtLivelyWallpaperTypeGIF;
                }
                else
                {
                    return info.Type.ToString();
                }
            }

            public BitmapImage LoadImage(string filename)
            {
                //return new BitmapImage(new Uri(filename)); // file gets locked, can't delete folder.
                try
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.StreamSource = stream;
                        bmp.CacheOption = BitmapCacheOption.OnLoad;//allow deletion of file on disk.
                        bmp.EndInit();
                        return bmp;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            /// <summary>
            /// Reduces imagesize to 100x100
            /// </summary>
            private BitmapImage LoadConvertImage(string filename)
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.DecodePixelWidth = 100;
                bi.CacheOption = BitmapCacheOption.OnLoad; //allow deletion of file on disk.
                bi.UriSource = new Uri(filename);
                bi.EndInit();
                return bi;
            }

            private Uri GetUri(string s, string scheme)
            {
                try
                {
                    return new UriBuilder(s)
                    {
                        Scheme = scheme,
                        Port = -1,
                    }.Uri;
                }
                catch (ArgumentNullException)
                {
                    return null;
                }
                catch (UriFormatException)
                {
                    return null;
                }
            }
        }

        #endregion

        #region displayID_dialogue_data
        public class DisplayListBox : INotifyPropertyChanged
        {
            private string displayDevice;
            private string fileName;
            private string filePath;
            public string DisplayDevice
            {
                get
                {
                    return displayDevice;
                }
                set
                {
                    displayDevice = value;
                    OnPropertyChanged("DisplayID");
                }
            }
            public string FileName
            {
                get
                {
                    return fileName;
                }
                set
                {
                    fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
            public string FilePath
            {
                get
                {
                    return filePath;
                }
                set
                {
                    filePath = value;
                    OnPropertyChanged("FilePath");
                }
            }

            public DisplayListBox(string scr, string filePath)
            {
                DisplayDevice = scr;
                FilePath = filePath;
                if (filePath != null)
                {
                    try
                    {
                        FileName = System.IO.Path.GetFileName(filePath);
                    }
                    catch (ArgumentException)
                    {
                        FileName = "Error";
                    }

                    if (String.IsNullOrWhiteSpace(FileName))
                    {
                        FileName = filePath;
                    }
                }
            }
            private void OnPropertyChanged(string property)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }
        #endregion

    }
}
