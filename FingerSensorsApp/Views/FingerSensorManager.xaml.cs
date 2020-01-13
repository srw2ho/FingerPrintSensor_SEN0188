using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using SEN0188_SQLite;
using FingerSensorsApp.Models;
using FingerPrintSensor_SEN0188;
using Windows.Foundation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using FingerSensorsApp.Helpers;
using System.Text;
using FingerSensorsApp.Services;
using System.Threading.Tasks;

namespace FingerSensorsApp.Views
{
    public sealed partial class FingerSensorManager : Page, INotifyPropertyChanged
    {
        System.Collections.Generic.IList<SEN0188_SQLite.DBDataSet> m_DataSets;
        Models.StationEnvironment m_Environment;
        SEN0188SQLite m_SEN0188SQLite;
        Connector_SEN0188 m_Connector_SEN0188;
        SerDevice m_serDev;
        Windows.Foundation.Collections.PropertySet m_Sensoroutputconfigoptions;
        Windows.Foundation.Collections.PropertySet m_Sensorinputconfigoptions;

        System.Collections.Generic.IList<uint> m_FilledFingerLib;

     //  System.Collections.Generic.IList<byte> m_SensorId;



        public FingerSensorManager()
        {
            InitializeComponent();
            m_Environment = null;
            m_DataSets = null;
            m_SEN0188SQLite = null;
            m_Sensoroutputconfigoptions = null;
            m_Sensorinputconfigoptions = null;
            m_serDev = null;
            m_FilledFingerLib = new ObservableCollection<uint>();


        }

        public event PropertyChangedEventHandler PropertyChanged;



        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        public System.Collections.Generic.IList<DBDataSet> SensorDataSets
        {
            get { return m_DataSets; }

        }




        public System.Collections.Generic.IList<uint> FilledFingerLib
        {
            get { return m_FilledFingerLib; }

        }

        public Models.StationEnvironment Environment
        {
            get { return m_Environment; }

        }


        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /*
        async private void SensorStopConnector()
        {
          //  if (m_Connector_SEN0188.ProcessingPackagesStarted)
            {
                await m_Connector_SEN0188.stopProcessingPackagesAsync();

            }


        }

   
        async private void SensorStartConnector()
        {
            if (m_serDev != null)
            {
             //   if (!m_Connector_SEN0188.ProcessingPackagesStarted)
                {
                    TimeSpan _timeOut = TimeSpan.FromMilliseconds(1000);
                    m_serDev.BaudRate = (uint)57600;
                    m_serDev.WriteTimeout = _timeOut;
                    m_serDev.ReadTimeout = _timeOut;
                    m_serDev.Parity = Windows.Devices.SerialCommunication.SerialParity.None;
                    m_serDev.StopBits = Windows.Devices.SerialCommunication.SerialStopBitCount.One;
                    m_serDev.DataBits = 8;
                    m_serDev.Handshake = Windows.Devices.SerialCommunication.SerialHandshake.None;


                    m_Sensorinputconfigoptions["UpdateState"] = PropertyValue.CreateInt32(1);
        
                    await m_Connector_SEN0188.startProcessingPackagesAsync(m_serDev, m_Sensorinputconfigoptions, m_Sensoroutputconfigoptions);
                }


            }

        }
        */

        protected override void OnNavigatingFrom(Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            m_Connector_SEN0188.NotifyChangeState -= Connector_SEN0188_NotifyChangeState;

        }




        protected override  void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.Parameter is FingerSensorsApp.App)
            {

                FingerSensorsApp.App AppEnvironment = e.Parameter as FingerSensorsApp.App;


                if (AppEnvironment != null)
                {
                    m_Environment = AppEnvironment.Environment;
                    m_DataSets = m_Environment.SEN0188SQLite.DataSets;
                    m_SEN0188SQLite = m_Environment.SEN0188SQLite;
                    m_Connector_SEN0188 = m_Environment.SensorConnector;
                    m_Sensoroutputconfigoptions = m_Environment.SensorOutPutServiceConnectorConfig;
                    m_Sensorinputconfigoptions = m_Environment.SensorInputServiceConnectorConfig;
                    m_SEN0188SQLite.GetDataSets();
     
                    m_serDev = m_Environment.SerDev;
                    SensorIDValue.Text = "not set";
                    m_Connector_SEN0188.NotifyChangeState += Connector_SEN0188_NotifyChangeState;

                }
            }
            base.OnNavigatedTo(e);
        }

        async private void UpdateDataBaseData()
        {
         
            var a = await m_SEN0188SQLite.GetDataSetsAsync();


        }


        void ButtonComWithSensor(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

            AppBarButton buttton = sender as AppBarButton;
            if (buttton != null)
            {
                if (buttton.Name == "InitSensor")
                {
                    SensorCMDs.InitSensor(m_Sensorinputconfigoptions);

                    m_Environment.SensorInitialized = false;
                    SensorIDValue.Text = "";



                }
                else if (buttton.Name == "RegisterFingerId")
                {
                    if (DataSets.SelectedItem != null)
                    {
                        DBDataSet dataSet = DataSets.SelectedItem as DBDataSet;
                        if (dataSet != null)
                        {
                            SensorCMDs.RegisterFingerId(m_Sensorinputconfigoptions, (UInt16)dataSet.FingerID);
                            DataSets.SelectedItem = null;
                        }


                    }



                }
                else if (buttton.Name == "VerifyFingerId")
                {
                    SensorCMDs.VerifyFingerId(m_Sensorinputconfigoptions);
                    DataSets.SelectedItem = null;
                }
                else if (buttton.Name == "DeleteallFingerIs")
                {
                    SensorCMDs.DeleteFingerId(m_Sensorinputconfigoptions, (UInt16)10000);


                }
                else if (buttton.Name == "DownloadFingerId")
                {
                    if (DataSets.SelectedItem != null)
                    {
                        DBDataSet dataSet = DataSets.SelectedItem as DBDataSet;
                        if (dataSet != null)
                        {
                            SensorCMDs.DownloadFingerId(m_Sensorinputconfigoptions, (UInt16)dataSet.FingerID, dataSet.FingerTemplate);

                        }
                        DataSets.SelectedItem = null;
                    }
                }
                else if (buttton.Name == "DownloadallFingerIds")
                {
                    SensorCMDs.DeleteFingerId(m_Sensorinputconfigoptions, 10000); // delete complete FingerLib into Sensor
                    for (int i = 0; i < this.m_DataSets.Count; i++)
                    {
                        DBDataSet dataSet = m_DataSets[i];
                        SensorCMDs.DownloadFingerId(m_Sensorinputconfigoptions, (UInt16)dataSet.FingerID, dataSet.FingerTemplate);
                    }
                    DataSets.SelectedItem = null;
                }
                else if (buttton.Name == "SetSensorID")
                {
                    
                    byte[] SensorId  = Encoding.ASCII.GetBytes(SensorIDValue.Text);
                    SensorCMDs.SetSensorID(m_Sensorinputconfigoptions, SensorId); // Set Sensor ID

                    SensorIDValue.Text = "";
                    DataSets.SelectedItem = null;
                }

                

                SensorCmdState.Text = "...";
            }


        }




        void stopRecording_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            m_Environment.SensorStopConnector();
       //     SensorStopConnector();
        }


        void startRecording_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            m_Environment.SensorStartConnector();

        }

        private void DataSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DBDataSet dataSet = DataSets.SelectedItem as DBDataSet;

            if (dataSet != null)
            {


            }
        }

        public void ButtonDeleteTemplate(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

            if (DataSets.SelectedItem == null) return;

            DBDataSet dataSet = DataSets.SelectedItem as DBDataSet;

            if (dataSet != null)
            {
                m_SEN0188SQLite.DelDataSetByFingerId(dataSet.FingerID);
                if (m_Environment.SensorInitialized)
                {
                    SensorCMDs.DeleteFingerId(m_Sensorinputconfigoptions, (UInt16)dataSet.FingerID);
                }

                m_SEN0188SQLite.GetDataSets();

            }

        }


        public async void ButtonEditTemplate(object sender, RoutedEventArgs e)
        {
            if (DataSets.SelectedItem == null) return;

            DBDataSet dataSet = DataSets.SelectedItem as DBDataSet;

            if (dataSet != null)
            {
                m_SEN0188SQLite.UpdateDataSet(dataSet);
                //    m_SEN0188SQLite.GetDataSets();
                m_Environment.IsAuthorized = m_Environment.IsLoggedIn && await m_Environment.checkAuthorizationAsync(m_Environment.User);
            }


        }




        public void ButtoninitDatabase(object sender, RoutedEventArgs e)
        {
            m_SEN0188SQLite.dropTable();
            m_SEN0188SQLite.InitializeDatabase();
            m_SEN0188SQLite.GetDataSets();
        }


        public void ButtondelFingerTable(object sender, RoutedEventArgs e)
        {

            m_SEN0188SQLite.DelallDataSets();
            m_SEN0188SQLite.GetDataSets();
            if (m_Environment.SensorInitialized)
            {
                SensorCMDs.DeleteFingerId(this.m_Sensorinputconfigoptions, 10000); // Delete complete Finger Template Library into Sensor
            }


        }

        public async void ButtonAddLoggedInUserTemplate(object sender, RoutedEventArgs e)
        {
            if (m_Environment.User == null) return;

            DBDataSet dataSet = new DBDataSet();
            dataSet.AccessRights = 0x01; // Master Access
            dataSet.FingerID = m_SEN0188SQLite.getFreeFingerId();
            dataSet.FirstName = m_Environment.User.GivenName;
            dataSet.SecondName = m_Environment.User.Surname;
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            dataSet.SensorId = enc.GetBytes(SensorIDValue.Text);
            m_SEN0188SQLite.InsertDataSet(dataSet);
            m_SEN0188SQLite.GetDataSets();
            DataSets.SelectedItem = m_SEN0188SQLite.getDatabyId(dataSet.FingerID);
            m_Environment.IsAuthorized = m_Environment.IsLoggedIn && await m_Environment.checkAuthorizationAsync(m_Environment.User);


        }

        public void ButtonAddTemplate(object sender, RoutedEventArgs e)
        {

            DBDataSet dataSet = new DBDataSet();
            dataSet.AccessRights = 0;
            dataSet.FingerID = m_SEN0188SQLite.getFreeFingerId();
            dataSet.FirstName = "John";
            dataSet.SecondName = "Doe";
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            dataSet.SensorId = enc.GetBytes(SensorIDValue.Text);
            m_SEN0188SQLite.InsertDataSet(dataSet);
            m_SEN0188SQLite.GetDataSets();
            DataSets.SelectedItem = m_SEN0188SQLite.getDatabyId(dataSet.FingerID);



        }





        void Update_SEN0188_NotifyChangeState(IPropertySet Outputpropertys)
        {
            bool doactFilledFingerLib = false;
            bool doactFilledSensorId = false;
            Object Valout;
            Int16 state = -1;
            if (Outputpropertys.TryGetValue("FingerPrint.CMDState", out Valout))
            {
                if (Valout != null)
                {
                    state = (Int16)Valout;
                }


            }

            if (Outputpropertys.TryGetValue("FingerPrint.CMD", out Valout))
            {
                if (Valout != null)
                {
                    Int32 CMD = (Int32)Valout;

                    if (CMD == Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerSensorInitialize"))
                    {
                        if (state == 0)
                        {
                            doactFilledFingerLib = true;
                            doactFilledSensorId = true;
                            m_Environment.SensorInitialized = true;
                        }



                    }
                    else if (CMD == Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerRegistration") || CMD == Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerAutoRegistration"))
                    {
                        if (Outputpropertys.TryGetValue("FingerPrint.RegististrationState", out Valout))
                        {
                            if (Valout != null)
                            {
                                Int32 regState = (Int32)Valout;
                                if (regState >= 5)
                                {
                                    if (Outputpropertys.TryGetValue("FingerPrint.Registration_FingerID", out Valout))
                                    {
                                        UInt16 PageId = (UInt16)Valout;
                                        if (state == 0)
                                        {
                                            if (Outputpropertys.TryGetValue("FingerPrint.CHARUpLoad", out Valout))
                                            {
                                                if (Valout != null)
                                                {
                                                    byte[] array = Valout as byte[];
                                                    if (array != null)
                                                    {
                                                        DBDataSet dataset = m_SEN0188SQLite.getDatabyId(PageId);
                                                        if (dataset != null)
                                                        {
                                                            dataset.FingerTemplate = array;

                                                            if (SensorIDValue.Text.Length > 0)
                                                            {
                                                                dataset.SensorId = System.Text.Encoding.UTF8.GetBytes(SensorIDValue.Text);
                                                            }


                                                            this.m_SEN0188SQLite.UpdateFingerTemplateDataSet(dataset);
                                                            DataSets.SelectedItem = dataset;
                                                            doactFilledFingerLib = true;
                                                        }

                                                    }
                                                }

                                            }

                                        }
                                    }

                                }
                            }

                        }


                    }

                    else if (CMD == Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerDeleteId"))
                    {
                        if (state == 0)
                        {
                            doactFilledFingerLib = true;
                        }

                    }
                    else if (CMD == Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerAutoVerifiying"))
                    {
                        if (state == 0)
                        {
                            if (Outputpropertys.TryGetValue("FingerPrint.FingerID", out Valout))
                            {
                                if (Valout != null)
                                {
                                    UInt16 fingerId = (UInt16)Valout;
                                    if (Outputpropertys.TryGetValue("FingerPrint.Search_MatchScore", out Valout))
                                    {
                                        UInt16 MatchScore = (UInt16)Valout;
                                        DBDataSet dataset = m_SEN0188SQLite.getDatabyId(fingerId);
                                        if (dataset != null)
                                        {
                                            if (MatchScore> dataset.MatchScore)
                                            {
                                                dataset.MatchScore = MatchScore;
                                                this.m_SEN0188SQLite.UpdateMatchScoreDataSet(dataset);
                                            }
                                            DataSets.SelectedItem = dataset;
                                        }
                                        else DataSets.SelectedItem = null;

                                    }
                                }


                            }
                        }


                    }
                    else if (CMD == Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerCharDownLoad"))
                    {
                        if (state == 0)
                        { // dowwload succesfull
                            if (Outputpropertys.TryGetValue("FingerPrint.Registration_FingerID", out Valout))
                            {
                                UInt16 FingerId = (UInt16)Valout;
                                if (state == 0) {
                                    DBDataSet dataset = m_SEN0188SQLite.getDatabyId(FingerId);
                                    if (dataset != null)
                                    {
                                        DataSets.SelectedItem = dataset;
                                        doactFilledFingerLib = true;
                                    }
                                    else DataSets.SelectedItem = null;
                                }
                            }
            
                        }
                    }
                    else if (CMD == Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerWriteSensorID"))// write Sensor-ID
                    {
                        if (state == 0)
                        { // dowwload succesfull
                            doactFilledSensorId = true;

                        }
                    }

                }

            }



            if (doactFilledSensorId)
            {
             //   m_SensorId.Clear();
                if (Outputpropertys.TryGetValue("FingerPrint.SensorID", out Valout))
                {
                    if (Valout != null)
                    {

                        byte[] array = Valout as byte[];
                        if (array != null)
                        {
                            /*
                            for (uint i = 0; i < array.Length; i++)
                            {
                                m_SensorId.Add(array[i]);
                            }
                            */
                            SensorIDValue.Text = System.Text.Encoding.UTF8.GetString(array, 0, array.Length);
                        }
                    }
                }
            }

            if (doactFilledFingerLib )
            {

                m_FilledFingerLib.Clear();
                if ( Outputpropertys.TryGetValue("FingerPrint.FilledFingerLib", out Valout))
                {
               
                    if (Valout != null)
                    {
                        UInt32[] array = Valout as UInt32[];
                        if (array != null)
                        {
                            for (uint i = 0; i < array.Length; i++)
                            {
                                m_FilledFingerLib.Add(array[i]);
                            }

                        }
                    }
                }

            }

            if (Outputpropertys.TryGetValue("FingerPrint.CMDTextState", out Valout))
            {
                String cmd = Valout as String;
                if (cmd != null)
                {
                    SensorCmdState.Text = cmd;

                }
            }


        }

        async private void Connector_SEN0188_NotifyChangeState(object sender, IPropertySet args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,  () =>
            { // your code should be here
                Update_SEN0188_NotifyChangeState(args);

            });
        }
        /*
        async private void Connector_SEN0188_Failed(object sender, string args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            { // your code should be here
                SensorInitialized = false;
                InitSensor.IsEnabled = false;
               // startRecording.IsEnabled = true;
               // stopRecording.IsEnabled = false;
                SensorStopConnector();

                var msg = "Serial Device: " + args;
                var messageDialog = new MessageDialog(msg);
                await messageDialog.ShowAsync();
            });

            //  throw new NotImplementedException();
        }
        async private void Connector_SEN0188_stopStreaming(object sender, string args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,  () =>
            { // your code should be here
                InitSensor.IsEnabled = false;
                m_Environment.SensorInitialized = false;
            //    startRecording.IsEnabled = true;
            //    stopRecording.IsEnabled = false;
                SensorStopConnector();

            });
        }

        async private void Connector_SEN0188_startStreaming(object sender, SerDevice args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,  () =>
            { // your code should be here
                InitSensor.IsEnabled = true;
            //    startRecording.IsEnabled = false;
            //    stopRecording.IsEnabled = true;

            });
        }
        */


    }


}
