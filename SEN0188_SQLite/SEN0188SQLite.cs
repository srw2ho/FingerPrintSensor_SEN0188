using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
//using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace SEN0188_SQLite
{
    [Windows.UI.Xaml.Data.Bindable]
    public sealed class SEN0188SQLite
    {
        string m_DBConnectName = "Filename = sqliteSample.db";


        string m_DBDataBasePath;
        string m_DBDataBaseName;
        ObservableCollection<DBDataSet> m_DataSets;
      //  IDictionary<int, DBDataSet> m_DataMap;


        public SEN0188SQLite()
        {


            m_DBConnectName = "Filename = sqliteSample.db";

            m_DBDataBaseName = "sqliteSample.db";
            m_DBDataBasePath = "";
            m_DataSets = new ObservableCollection<DBDataSet>();

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

        public IList<DBDataSet> DataSets
        {
            get { return m_DataSets; }

        }

        public int getFreeFingerId()
        {
            if (m_DataSets.Count == 0) return 0;
            bool bfound = false;
            for (int j = 0; j < 1000; j++)
            {
                bfound = false;
                for (int i = 0; i < m_DataSets.Count; i++)
                {
                    if (m_DataSets[i].FingerID == j)
                    {
                        bfound = true;
                        break;
                    }
                }

                if (!bfound)
                {
                    return j;
                }

            }


            return -1;
        }

        public DBDataSet getDatabyId(int Id)
        {

            for (int i = 0; i < m_DataSets.Count; i++)
            {
                if (m_DataSets[i].FingerID == Id)
                {
                    return m_DataSets[i];
                }
            }



            return null;
        }

        public async Task<bool> GetDataSetsAsync()
        {
            var auto = await Task.Run(() => GetDataSets());

            return auto; 

        }


        public bool GetDataSets()
        {

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    m_DataSets.Clear();
                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("SELECT * from FingerIdTbl", db);


                    query = selectCommand.ExecuteReader();
                    while (query.Read())
                    {
                        DBDataSet fingerSet = new DBDataSet();
                        if (query.FieldCount > 0)
                            fingerSet.FingerID = query.GetInt32(0);



                        if (query.FieldCount > 1)
                        {
                            byte[] buffer = new byte[32];
                            query.GetBytes(1, 0, buffer, 0, buffer.Length);
                            fingerSet.SensorId = buffer;
                        }
     
                        if (query.FieldCount > 2)
                            fingerSet.SecondName = query.GetString(2);

                        if (query.FieldCount > 3)
                            fingerSet.FirstName = query.GetString(3);

                        if (query.FieldCount > 4)
                        {
                            byte[] buffer = new byte[512];
                            query.GetBytes(4, 0, buffer, 0, buffer.Length);
                            fingerSet.FingerTemplate = buffer;
                        }

                        if (query.FieldCount > 5)
                            fingerSet.AccessRights = (ulong)query.GetInt64(5);
                       
                        if (query.FieldCount>6)
                        {
                            fingerSet.MatchScore = query.GetInt32(6);
    
                        }

                        if (query.FieldCount > 7)
                        {
                            long timeTicks;
                            timeTicks = query.GetInt64(7);
                            fingerSet.CreationTime = new DateTime(timeTicks);
                        }

                        if (query.FieldCount > 8)
                        {
                            fingerSet.Info = query.GetString(8);
                        }


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


        public bool InitializeDatabase()
        {

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {

                try
                {
                    db.Open();
                    String tableCommand = "CREATE TABLE IF NOT " +
     "EXISTS FingerIdTbl (FingerID INTEGER PRIMARY KEY, " +
    "SensorId BLOB, " +
    "SecondName NVARCHAR(520) NOT NULL, " +
    "FirstName NVARCHAR(520) NOT NULL, " +
    "FingerTemplate BLOB, " +
    "AccessRights UNSIGNED BIG INT NOT NULL, " +
    "MatchScore INTEGER NOT NULL, " +
     "CreationTime UNSIGNED BIG INT NOT NULL, " +
     "Info NVARCHAR(520) NOT NULL)";


                    SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                    createTable.ExecuteReader();
                    return true;
                }
                catch (SqliteException )
                {
                    return false;
                    //Do nothing
                }
            }

        }



        public bool DelDataSetByFingerId(int fingerId)
        {
         
            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("DELETE from FingerIdTbl WHERE FingerID = @FingerId;", db);
                    selectCommand.Parameters.AddWithValue("@FingerId", fingerId);
                    query = selectCommand.ExecuteReader();
                
                    db.Close();
                    return true;
                }
                catch (SqliteException )
                {
                    //Handle error
                    db.Close();
                    return false;
                }


            }

        }
        public bool DelallDataSets()
        {
 
            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("DELETE from FingerIdTbl;", db);
                    query = selectCommand.ExecuteReader();
                    db.Close();
                    return true;
                }
                catch (SqliteException )
                {
                    //Handle error
                    db.Close();
                    return false;
                }


            }

        }


        public bool GetDataSetsByName(string firstname, string secondName, ObservableCollection<DBDataSet> DataSets)
        {
     
            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {

                try
                {
                    bool ret = false;
                    DataSets.Clear();

                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("SELECT * from FingerIdTbl WHERE FirstName = @FirstName AND SecondName  = @SecondName", db);
                    selectCommand.Parameters.AddWithValue("@FirstName", firstname);
                    selectCommand.Parameters.AddWithValue("@SecondName", secondName);
                    query = selectCommand.ExecuteReader();
                    while (query.Read())
                    {

                        DBDataSet fingerSet = new DBDataSet();
                        if (query.FieldCount > 0)
                            fingerSet.FingerID = query.GetInt32(0);

                        if (query.FieldCount > 1)
                        {
                            byte[] buffer = new byte[32];
                            query.GetBytes(1, 0, buffer, 0, buffer.Length);
                            fingerSet.SensorId = buffer;
                        }


                        if (query.FieldCount > 2)
                            fingerSet.SecondName = query.GetString(2);

                        if (query.FieldCount > 3)
                            fingerSet.FirstName = query.GetString(3);

                        if (query.FieldCount > 4)
                        {
                            byte[] buffer = new byte[512];
                            query.GetBytes(4, 0, buffer, 0, buffer.Length);
                            fingerSet.FingerTemplate = buffer;
                        }

                        if (query.FieldCount > 5)
                            fingerSet.AccessRights = (ulong)query.GetInt64(5);

                        if (query.FieldCount > 6)
                        {
                            fingerSet.MatchScore = query.GetInt32(6);

                        }
                        if (query.FieldCount > 7)
                        {
                            long timeTicks;
                            timeTicks = query.GetInt64(7);
                            fingerSet.CreationTime = new DateTime(timeTicks);
                        }

                        if (query.FieldCount > 8)
                        {
                            fingerSet.Info = query.GetString(8);
                        }

                        DataSets.Add(fingerSet);

                        ret = true;
         
                    }
                    db.Close();
                    return true;
                }
                catch (SqliteException )
                {
                    //Handle error
                    db.Close();
                    return false;
                }


            }

        }

        public bool GetDataSetByFingerId(int fingerId, DBDataSet fingerSet)
        {
            bool ret = false;
            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    SqliteDataReader query;
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("SELECT * from FingerIdTbl WHERE FingerID = @FingerId", db);
                    selectCommand.Parameters.AddWithValue("@FingerId", fingerId);
                    query = selectCommand.ExecuteReader();
                    while (query.Read())
                    {
       
                        if (query.FieldCount > 0)
                            fingerSet.FingerID = query.GetInt32(0);

                        if (query.FieldCount > 1)
                        {
                            byte[] buffer = new byte[32];
                            query.GetBytes(1, 0, buffer, 0, buffer.Length);
                            fingerSet.SensorId = buffer;
                        }

                    
                        if (query.FieldCount > 2)
                            fingerSet.SecondName = query.GetString(2);

                        if (query.FieldCount > 3)
                            fingerSet.FirstName = query.GetString(3);

                        if (query.FieldCount > 4)
                        {
                            byte[] buffer = new byte[512];
                            query.GetBytes(4, 0, buffer, 0, buffer.Length);
                            fingerSet.FingerTemplate = buffer;
                        }

                        if (query.FieldCount > 5)
                            fingerSet.AccessRights = (ulong)query.GetInt64(5);

                        if (query.FieldCount > 6)
                        {
                            fingerSet.MatchScore = query.GetInt32(6);

                        }
                        if (query.FieldCount > 7)
                        {
                            long timeTicks;
                            timeTicks = query.GetInt64(7);
                            fingerSet.CreationTime = new DateTime(timeTicks);
                        }

                        if (query.FieldCount > 8)
                        {
                            fingerSet.Info = query.GetString(8);
                        }


                        ret = true;
                        break;
                    }
                    db.Close();
                    return true;
                }
                catch (SqliteException )
                {
                    //Handle error
                    db.Close();
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
                    insertCommand.CommandText = "DROP TABLE FingerIdTbl;";

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


        public bool InsertDataSet(DBDataSet dataSet)
        {
            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {
                try
                {
                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "INSERT OR IGNORE INTO FingerIdTbl VALUES (@FingerID, @SensorId,  @SecondName, @FirstName, @FingerTemplate, @AccessRights, @MatchScore, @CreationTime, @Info);";
                    insertCommand.Parameters.AddWithValue("@FingerID", dataSet.FingerID);
                    insertCommand.Parameters.AddWithValue("@SensorId", dataSet.SensorId);

                    insertCommand.Parameters.AddWithValue("@SecondName", dataSet.SecondName);
                    insertCommand.Parameters.AddWithValue("@FirstName", dataSet.FirstName);


                    insertCommand.Parameters.AddWithValue("@FingerTemplate", dataSet.FingerTemplate);

                    insertCommand.Parameters.AddWithValue("@AccessRights", dataSet.AccessRights);

                    insertCommand.Parameters.AddWithValue("@MatchScore", dataSet.MatchScore);

                    dataSet.CreationTime = DateTime.Now;

                    insertCommand.Parameters.AddWithValue("@CreationTime", dataSet.CreationTime.Ticks);

                    insertCommand.Parameters.AddWithValue("@Info", dataSet.Info);

                    insertCommand.ExecuteReader();
                    db.Close();
                    return true;


                }
                catch (SqliteException )
                {
                    db.Close();
                    //Handle error
                    return false;
                }



            }

        }

        public bool UpdateDataSet(DBDataSet dataSet)
        {

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {


                try
                {

                    db.Open();
                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "UPDATE FingerIdTbl SET SecondName = @SecondName, FirstName = @FirstName, SensorId = @SensorId, AccessRights = @AccessRights, MatchScore = @MatchScore, CreationTime = @CreationTime, Info = @Info WHERE FingerID = @FingerID;";
                    insertCommand.Parameters.AddWithValue("@FingerID", dataSet.FingerID);
                    insertCommand.Parameters.AddWithValue("@AccessRights", dataSet.AccessRights);
                    insertCommand.Parameters.AddWithValue("@FirstName", dataSet.FirstName);
                    insertCommand.Parameters.AddWithValue("@SecondName", dataSet.SecondName);

                    insertCommand.Parameters.AddWithValue("@MatchScore", dataSet.MatchScore);
                    insertCommand.Parameters.AddWithValue("@SensorId", dataSet.SensorId);

                    dataSet.CreationTime = DateTime.Now;
                    insertCommand.Parameters.AddWithValue("@CreationTime", dataSet.CreationTime.Ticks);

                    insertCommand.Parameters.AddWithValue("@Info", dataSet.Info);
                    insertCommand.ExecuteReader();
                    db.Close();
                    return true;
                }
                catch (SqliteException e)
                {
                    db.Close();
                    //Do nothing
                    return false;
                }


            }

        }

        public bool UpdateAccessRightsDataSet(DBDataSet dataSet)
        {

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {

                try
                {

                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "UPDATE FingerIdTbl SET AccessRights = @AccessRights WHERE FingerID = @FingerID;";
                    insertCommand.Parameters.AddWithValue("@FingerID", dataSet.FingerID);

                    insertCommand.Parameters.AddWithValue("@AccessRights", dataSet.AccessRights);

                    insertCommand.ExecuteReader();
                    db.Close();
                    return true;
                }
                catch (SqliteException e)
                {
                    db.Close();
                    //Do nothing
                    return false;
                }


            }

        }
        public bool UpdateMatchScoreDataSet(DBDataSet dataSet)
        {

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {

                try
                {

                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "UPDATE FingerIdTbl SET MatchScore = @MatchScore WHERE FingerID = @FingerID;";
                    insertCommand.Parameters.AddWithValue("@FingerID", dataSet.FingerID);
                    insertCommand.Parameters.AddWithValue("@MatchScore", dataSet.MatchScore);

                    insertCommand.ExecuteReader();
                    db.Close();
                    return true;
                }
                catch (SqliteException e)
                {
                    db.Close();
                    //Do nothing
                    return false;
                }


            }

        }
        public bool UpdateFingerTemplateDataSet(DBDataSet dataSet)
        {

            using (SqliteConnection db = new SqliteConnection(m_DBConnectName))
            {

                try
                {

                    db.Open();

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = "UPDATE FingerIdTbl SET FingerTemplate = @FingerTemplate, SensorId = @SensorId WHERE FingerID = @FingerID;";
                    insertCommand.Parameters.AddWithValue("@FingerID", dataSet.FingerID);
                    insertCommand.Parameters.AddWithValue("@FingerTemplate", dataSet.FingerTemplate);
                    insertCommand.Parameters.AddWithValue("@SensorId", dataSet.SensorId);

                    insertCommand.ExecuteReader();
                    db.Close();
                    return true;
                }
                catch (SqliteException e)
                {
                    db.Close();
                    //Do nothing
                    return false;
                }


            }

        }

    }

}
