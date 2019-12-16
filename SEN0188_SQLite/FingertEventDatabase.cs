using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEN0188_SQLite
{
    [Windows.UI.Xaml.Data.Bindable]
    public sealed class FingertEventDatabase
    {
        string m_DBConnectName = "Filename = sqliteSample.db";


        string m_DBDataBasePath;
        string m_DBDataBaseName;

        public FingertEventDatabase()
        {


            m_DBConnectName = "Filename = sqliteFingerEvent.db";

            m_DBDataBaseName = "sqliteFingerEvent.db";
            m_DBDataBasePath = "";



        }

        public String DBDataBaseName
        {
            get { return m_DBDataBaseName; }
            set
            {
                m_DBDataBaseName = value;
            }

        }
        public String DBDataBasePath
        {
            get { return m_DBDataBasePath; }
            set
            {
                m_DBDataBasePath = value;
                m_DBConnectName = String.Format("Filename = {0}", m_DBDataBasePath);
            }

        }

        public bool InsertFingerEvent(FingerEvent dataSet)
        {
            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "INSERT OR IGNORE INTO FingerIdEventsTbl VALUES (NULL, @FingerID, @SensorId,  @SecondName, @FirstName, @EventType, @MatchScore, @SensorState, @SensorTxtState, @EventTime);";
               
                    insertCommand.Parameters.AddWithValue("@FingerID", dataSet.FingerID);

                    insertCommand.Parameters.AddWithValue("@SensorId", dataSet.SensorId);

                    insertCommand.Parameters.AddWithValue("@SecondName", dataSet.SecondName);

                    insertCommand.Parameters.AddWithValue("@FirstName", dataSet.FirstName);

                    insertCommand.Parameters.AddWithValue("@EventType", dataSet.EventType);

                    insertCommand.Parameters.AddWithValue("@MatchScore", dataSet.MatchScore);

                    insertCommand.Parameters.AddWithValue("@SensorState", dataSet.SensorState);

                    insertCommand.Parameters.AddWithValue("@SensorTxtState", dataSet.SensorTxtState);

                    dataSet.EventTime = DateTime.Now;
                    insertCommand.Parameters.AddWithValue("@EventTime", dataSet.EventTime.Ticks);

                    insertCommand.ExecuteReader();
                    db.Close();
                    return true;


                }
                catch (SqliteException ex)
                {
                    db.Close();
                    //Handle error
                    return false;
                }



            }

        }

        public bool dropTable()
        {
            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "DROP TABLE FingerIdEventsTbl;";

                    insertCommand.ExecuteReader();
                    db.Close();
                    return true;


                }
                catch (SqliteException )
                {
                    db.Close();

                    return false;
                }



            }
        }

        public bool InitializeDatabase()
        {

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {

                try
                {
                    db.Open();
                    String tableCommand = "CREATE TABLE IF NOT " +
     "EXISTS FingerIdEventsTbl (ID INTEGER PRIMARY KEY, " +
    "FingerID INTEGER NOT NULL, " +
    "SensorId BLOB, " +
    "SecondName NVARCHAR(520) NOT NULL, " +
    "FirstName NVARCHAR(520) NOT NULL, " +
    "EventType NVARCHAR(50) NOT NULL, " +
    "MatchScore INTEGER NOT NULL, " +
    "SensorState INTEGER NOT NULL, " +
    "SensorTxtState NVARCHAR(520) NOT NULL, " +
    "EventTime UNSIGNED BIG INTEGER NOT NULL)";


                    SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                    createTable.ExecuteReader();
                    return true;
                }
                catch (SqliteException e)
                {
                    return false;
                    //Do nothing
                }
            }

        }


        private void SetQueryToEventData(SqliteDataReader query, FingerEvent fingerSet) // get all datas > dateTime 
        {
            if (query.FieldCount > 0)
                fingerSet.EventID = query.GetInt32(0);


            if (query.FieldCount > 1)
                fingerSet.FingerID = query.GetInt32(1);

            if (query.FieldCount > 2)
            {
                byte[] buffer = new byte[32];
                query.GetBytes(2, 0, buffer, 0, buffer.Length);
                fingerSet.SensorId = buffer;
            }

            if (query.FieldCount > 3)
            {
                fingerSet.SecondName = query.GetString(3);
            }

            if (query.FieldCount > 4)
                fingerSet.FirstName = query.GetString(4);

            if (query.FieldCount > 5)
                fingerSet.EventType = query.GetString(5);

            if (query.FieldCount > 6)
            {
                fingerSet.MatchScore = query.GetInt32(6);

            }
            if (query.FieldCount > 7)
            {
                fingerSet.SensorState = query.GetInt32(7);
            }
            if (query.FieldCount > 8)
            {
                fingerSet.SensorTxtState = query.GetString(8);
            }

            if (query.FieldCount > 9)
            {
                long timeTicks;
                timeTicks = query.GetInt64(9);
                fingerSet.EventTime = new DateTime(timeTicks);
            }
        }

        public bool GetDataSetsGreaterThanDateTime(DateTime dateTime, ObservableCollection<FingerEvent> m_DataSets) // get all datas > dateTime 
        {
          //  TimeSpan sp = -TimeSpan.FromDays(14);
          //  dateTime.AddTicks(sp.Ticks);

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    m_DataSets.Clear();
                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("SELECT * from FingerIdEventsTbl WHERE EventTime >= @EventTime Order By EventTime DESC ;", db);
                    selectCommand.Parameters.AddWithValue("@EventTime", dateTime.Ticks);

                    query = selectCommand.ExecuteReader();
                    while (query.Read())
                    {

                        FingerEvent fingerSet = new FingerEvent();
                        SetQueryToEventData(query, fingerSet);
                        

                        m_DataSets.Add(fingerSet);

                    }
                    db.Close();
                    return (m_DataSets.Count > 0);
                }
                catch (SqliteException )
                {
                    //Handle error
                    db.Close();
                    return false;
                }


            }

        }


        public bool GetDataSetsGreaterThanDateTimeByNotIdenSensorState(DateTime dateTime, ObservableCollection<FingerEvent> m_DataSets, int SensorState) // get all datas > dateTime 
        {
            //  TimeSpan sp = -TimeSpan.FromDays(14);
            //  dateTime.AddTicks(sp.Ticks);

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    m_DataSets.Clear();
                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("SELECT * from FingerIdEventsTbl WHERE EventTime >= @EventTime AND SensorState != @SensorState Order By EventTime DESC ;", db);
                    selectCommand.Parameters.AddWithValue("@EventTime", dateTime.Ticks);
                    selectCommand.Parameters.AddWithValue("@SensorState", SensorState);
                    query = selectCommand.ExecuteReader();
                    while (query.Read())
                    {

                        FingerEvent fingerSet = new FingerEvent();
                        SetQueryToEventData(query, fingerSet);
                        /*
                        if (query.FieldCount > 0)
                            fingerSet.EventID = query.GetInt32(0);
                        

                        if (query.FieldCount > 1)
                            fingerSet.FingerID = query.GetInt32(1);

                        if (query.FieldCount > 2)
                        {
                            byte[] buffer = new byte[32];
                            query.GetBytes(2, 0, buffer, 0, buffer.Length);
                            fingerSet.SensorId = buffer;
                        }
       
                        if (query.FieldCount > 3)
                        {
                            fingerSet.SecondName = query.GetString(3);
                        }

                        if (query.FieldCount > 4)
                            fingerSet.FirstName = query.GetString(4);

                        if (query.FieldCount > 5)
                            fingerSet.EventType = query.GetString(5);

                        if (query.FieldCount > 6)
                        {
                            fingerSet.MatchScore = query.GetInt32(6);

                        }
                        if (query.FieldCount > 7)
                        {
                           fingerSet.SensorState = query.GetInt32(7);
                        }
                        if (query.FieldCount > 8)
                        {
                            fingerSet.SensorTxtState = query.GetString(8);
                        }

                        if (query.FieldCount > 9)
                        {
                            long timeTicks;
                            timeTicks = query.GetInt64(9);
                            fingerSet.EventTime = new DateTime(timeTicks);
                        }
                        */

                        m_DataSets.Add(fingerSet);

                    }
                    db.Close();
                    return (m_DataSets.Count > 0);
                }
                catch (SqliteException)
                {
                    //Handle error
                    db.Close();
                    return false;
                }


            }

        }


        public bool GetDataSetsGreaterThanDateTimeByIdenSensorState(DateTime dateTime, ObservableCollection<FingerEvent> m_DataSets, int SensorState) // get all datas > dateTime 
        {
            //  TimeSpan sp = -TimeSpan.FromDays(14);
            //  dateTime.AddTicks(sp.Ticks);

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    m_DataSets.Clear();
                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("SELECT * from FingerIdEventsTbl WHERE EventTime >= @EventTime AND SensorState = @SensorState Order By EventTime DESC ;", db);
                    selectCommand.Parameters.AddWithValue("@EventTime", dateTime.Ticks);
                    selectCommand.Parameters.AddWithValue("@SensorState", SensorState);
                    query = selectCommand.ExecuteReader();
                    while (query.Read())
                    {

                        FingerEvent fingerSet = new FingerEvent();
                        SetQueryToEventData(query, fingerSet);
                       
                        m_DataSets.Add(fingerSet);

                    }
                    db.Close();
                    return (m_DataSets.Count > 0);
                }
                catch (SqliteException ex)
                {
                    //Handle error
                    db.Close();
                    return false;
                }


            }

        }

        public bool deleteDataSetsLesserThanDateTime(DateTime dateTime) // delete all datas < dateTime 
        {
            //  TimeSpan sp = -TimeSpan.FromDays(14);
            //  dateTime.AddTicks(sp.Ticks);

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("DELETE from FingerIdEventsTbl WHERE EventTime <= @EventTime;", db);
                    selectCommand.Parameters.AddWithValue("@EventTime", dateTime.Ticks);
                    query = selectCommand.ExecuteReader();
                    db.Close();
                    return (true);
                }
                catch (SqliteException )
                {
                    //Handle error
                    db.Close();
                    return false;
                }


            }

        }

    }
}
