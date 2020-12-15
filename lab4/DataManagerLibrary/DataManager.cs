using System;
using DataSenderLibrary;
using SqlManagerLib;

namespace DataManagerLibrary
{
    public class DataManager
    {
        private DataSender dataSender;
        private SqlManager sqlManager;
        public DateTime LastTime { get; set; }

        public DataManager(string server, string login, string password, string sqlServer, string dbName, bool integratedSecurity, DateTime lastTime)
        {
            dataSender = new DataSender(server, login, password);
            sqlManager = new SqlManager(sqlServer, dbName, integratedSecurity);
            LastTime = lastTime;
        }

        public void SendInfo()
        {
            object[][] field = sqlManager.GetLastCommentInfo(LastTime);
            if (field is null)
                return;
            DateTime newTime = LastTime;
            for (int i = 0; i < field.Length; i++)
            {
                newTime = (DateTime)field[i][8];
                dataSender.Record(new Comment(field[i]), newTime);
            }
            LastTime = newTime;
        }
    }
}
