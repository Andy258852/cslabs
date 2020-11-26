using Flurl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Configurator;

namespace Service1
{
    public partial class Service1 : ServiceBase
    {
        static string sourceDirectory;
        static string targetDirectory;
        static string archiveDirectory;
        static bool EnableSsl;
        static int Timeout;
        static int RefreshTime;
        static bool KeepAlive;
        static int WatcherRefreshTime;
        static NetworkCredential credentials;

        Thread serverThread = new Thread(ServerFunc);
        Thread localThread = new Thread(LocalFunc);

        public Service1()
        {
            InitializeComponent();

            try
            {
                OptionManager.LogEnabled = true;
                OptionManager.ValidatingWithSchema = true;
                OptionManager.XmlIsHigher = true;
                OptionManager.ChangeDir(Environment.CurrentDirectory);

                var ConfigurationSet = OptionManager.GetOptions(
                    new Option("TargetDirectory", false, typeof(string), "C:\\"),
                    new Option("ArchiveDirectory", false, typeof(string), "C:\\"),
                    new Option("SourceDirectory", true, typeof(string)),
                    new Option("EnableSsl", false, typeof(bool), true),
                    new Option("Timeout", false, typeof(int), -1),
                    new Option("RefreshTime", false, typeof(int), 5000),
                    new Option("KeepAlive", false, typeof(bool), false),
                    new Option("WatcherRefreshTime", false, typeof(int), 30000),
                    new Option("login", true, typeof(string)),
                    new Option("password", true, typeof(string))
                    );

                targetDirectory = (string)ConfigurationSet[0];
                archiveDirectory = (string)ConfigurationSet[1];
                sourceDirectory = (string)ConfigurationSet[2];
                EnableSsl = (bool)ConfigurationSet[3];
                Timeout = (int)ConfigurationSet[4];
                RefreshTime = (int)ConfigurationSet[5];
                KeepAlive = (bool)ConfigurationSet[6];
                WatcherRefreshTime = (int)ConfigurationSet[7];

                credentials = new NetworkCredential((string)ConfigurationSet[8], (string)ConfigurationSet[9]);
            }
            catch(Exception ex)
            {
                OptionManager.Log(ex.Message);
            }

            CheckPath(archiveDirectory);
            CheckPath(targetDirectory);

        }

        static void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        protected override void OnStart(string[] args)
        {
            serverThread.Start();
            localThread.Start();
        }

        protected override void OnStop()
        {
            serverThread.Abort();
            localThread.Abort();
        }

        static void LocalFunc()
        {
            while (true)
            {
                FileRecovery();
            }
        }
        static void ServerFunc()
        {
            while (true)
            {
                CheckDirectory(sourceDirectory, targetDirectory, archiveDirectory);
                Thread.Sleep(RefreshTime);
            }
        }
        static void CreateDirectory(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }
        static void CheckDirectory(string sourceDirectory, string targetDirectory, string archiveDirectory)
        {
            CreateDirectory(targetDirectory);
            CreateDirectory(archiveDirectory);

            List<MyLink> list = new List<MyLink>();

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(sourceDirectory);
                request.Credentials = credentials;
                request.KeepAlive = KeepAlive;
                request.EnableSsl = EnableSsl;
                request.Timeout = Timeout;
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            reader.ReadLine();
                            reader.ReadLine();
                            while (!reader.EndOfStream)
                            {
                                string temp = reader.ReadLine();
                                bool isFile = temp[0] == '-';
                                temp = temp.Substring(temp.LastIndexOf(' ') + 1);
                                list.Add(new MyLink(isFile, temp));
                            }
                        }
                    }
                }
            }
            catch { }
            while (list.Count != 0)
            {
                Console.WriteLine(list[0].Type + " " + list[0].Name);
                if (list[0].Type)
                {
                    if (!File.Exists(Path.Combine(archiveDirectory, list[0].Name) + ".gz"))
                        SaveFile(sourceDirectory, targetDirectory, list[0].Name, archiveDirectory);
                }
                else
                {
                    string temp = Path.Combine(targetDirectory, list[0].Name);
                    CreateDirectory(temp);
                    CheckDirectory(Url.Combine(sourceDirectory, list[0].Name), temp, archiveDirectory);
                }
                list.RemoveAt(0);
            }
        }

        static void SaveFile(string sourceDirectory, string targetDirectory, string filename, string archiveDirectory)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Url.Combine(sourceDirectory, filename));
                request.Credentials = credentials;
                request.KeepAlive = KeepAlive;
                request.EnableSsl = EnableSsl;
                request.Timeout = Timeout;
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                {
                    string temp = Path.Combine(targetDirectory, filename);
                    using (Stream fileStream = File.Create(temp))
                    {
                        ftpStream.CopyTo(fileStream);
                    }
                    DecryptFile(temp);
                    using (FileStream stream_1 = new FileStream(temp, FileMode.OpenOrCreate))
                    {
                        using (Stream stream_2 = File.Create(Path.Combine(archiveDirectory, filename + ".gz")))
                        {
                            using (GZipStream stream = new GZipStream(stream_2, CompressionMode.Compress))
                            {
                                stream_1.CopyTo(stream);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        static void DecryptFile(string path)
        {
            string temp = SimpleCrypto.Decrypt(File.ReadAllText(path), 13);
            File.WriteAllLines(path, temp.Split(new char[] { '\n' }));
        }

        static void FileRecovery()
        {
            try
            {
                CreateDirectory(targetDirectory);
                CreateDirectory(archiveDirectory);
                using (FileSystemWatcher watcher = new FileSystemWatcher())
                {
                    watcher.Path = targetDirectory;
                    watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                    watcher.IncludeSubdirectories = true;
                    watcher.Filter = "*.txt";
                    watcher.Deleted += OnDeleted;
                    watcher.Renamed += OnRenamed;
                    watcher.Changed += OnChanged;
                    watcher.EnableRaisingEvents = true;
                    Thread.Sleep(WatcherRefreshTime);
                }
            }
            catch { }
        }
        static List<string> Watched { get; set; } = new List<string>();
        static bool WatchedContains(string path)
        {
            return Watched.FindIndex(x => x == path) != -1;
        }
        static void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!WatchedContains(e.Name))
            {
                Watched.Add(e.Name);
                string archivePath = Path.Combine(archiveDirectory, Path.GetFileName(e.Name)) + ".gz";
                Console.WriteLine(archivePath);
                if (File.Exists(archivePath))
                {
                    using (FileStream sourceStream = new FileStream(archivePath, FileMode.OpenOrCreate))
                    {
                        using (FileStream targetStream = File.Create(e.FullPath))
                        {
                            using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                            {
                                decompressionStream.CopyTo(targetStream);
                            }
                        }
                    }
                }
                Watched.Remove(e.Name);
            }
        }
        static void OnDeleted(object source, FileSystemEventArgs e)
        {
            string archivePath = Path.Combine(archiveDirectory, Path.GetFileName(e.Name)) + ".gz";
            Console.WriteLine(archivePath);
            if (File.Exists(archivePath))
            {
                if (!WatchedContains(e.Name))
                {
                    Watched.Add(e.Name);
                    using (FileStream sourceStream = new FileStream(archivePath, FileMode.OpenOrCreate))
                    {
                        using (FileStream targetStream = File.Create(e.FullPath))
                        {
                            using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                            {
                                decompressionStream.CopyTo(targetStream);
                            }
                        }
                    }
                    Watched.Remove(e.Name);
                }
            }
        }
        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            if (!WatchedContains(e.OldName))
            {
                Watched.Add(e.OldName);
                string archivePath = Path.Combine(archiveDirectory, Path.GetFileName(e.OldName)) + ".gz";
                Console.WriteLine(archivePath);
                if (File.Exists(archivePath))
                {
                    using (FileStream sourceStream = new FileStream(archivePath, FileMode.OpenOrCreate))
                    {
                        using (FileStream targetStream = File.Create(e.OldFullPath))
                        {
                            using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                            {
                                decompressionStream.CopyTo(targetStream);
                            }
                        }
                    }
                }
                Watched.Remove(e.OldName);
            }
        }
    }
}
