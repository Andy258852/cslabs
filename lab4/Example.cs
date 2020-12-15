using System;
using System.Threading;
using DataManagerLibrary;
using Configurator;

namespace TestProject
{
    class Example
    {
        string SourceDirectory { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        int Frequency { get; set; }
        string SqlServerName { get; set; }
        string DbName { get; set; }

        public Example()
        {

        }

        public void Start()
        {
            try
            {
                OptionManager.LogEnabled = true;
                OptionManager.ValidatingWithSchema = true;
                OptionManager.ChangeDir(Environment.CurrentDirectory);

                var ConfigurationSet = OptionManager.GetOptions(
                    new Option("sourceDirectory", true, typeof(string)),
                    new Option("login", true, typeof(string)),
                    new Option("password", true, typeof(string)),
                    new Option("frequency", false, typeof(int)),
                    new Option("sqlservername", true, typeof(string)),
                    new Option("dbname", true, typeof(string))
                    );

                SourceDirectory = (string)ConfigurationSet[0];
                Login = (string)ConfigurationSet[1];
                Password = (string)ConfigurationSet[2];
                Frequency = (int)ConfigurationSet[3];
                SqlServerName = (string)ConfigurationSet[4];
            }
            catch (Exception ex)
            {
                OptionManager.Log(ex.Message);
                return;
            }

            new Thread(Call).Start();
            Console.ReadKey();
        }

        private void Call()
        {
            DataManager manager = new DataManager(SourceDirectory, Login, Password, SqlServerName, DbName, true, DateTime.Now);
            while (true)
            {
                manager.SendInfo();
                Thread.Sleep(Frequency);
            }
        }
    }
}
