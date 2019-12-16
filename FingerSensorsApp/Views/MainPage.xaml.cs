using FingerPrintSensor_SEN0188;
using FingerSensorsApp.Helpers;
using FingerSensorsApp.Models;
using GPIOServiceConnector;
using SEN0188_SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FingerSensorsApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
     //   GPIOOBank m_OutPuts;
     //   GPIOOBank m_Inputs;
     //   PropertySet m_GPIOOutPutServiceConnectorConfig;
     //   PropertySet m_GPIOInputServiceConnectorConfig;
     //   GPIOServiceConnector.GPIOConnector m_GPIOConnector;
     //   GPIOOInOutBanks m_Banks;
        private StationEnvironment m_Environment;


        SEN0188SQLite m_SEN0188SQLite;
        FingertEventDatabase m_FingertEventDatabase;
        Connector_SEN0188 m_Connector_SEN0188;
        SerDevice m_serDev;
        Windows.Foundation.Collections.PropertySet m_Sensoroutputconfigoptions;
        Windows.Foundation.Collections.PropertySet m_Sensorinputconfigoptions;

        bool m_SensorInitialized;
        bool m_SensorConnecorInitialized;
        bool m_GPIOConnecorInitialized;

       // string m_SensorID;
      //  System.Collections.Generic.IList<FingerEvent> m_DataSets;

        ObservableCollection<FingerEvent> m_DataSets;

        GPIOEnvironmentConnectors m_GPIOEnvironmentConnectors;

        ProcessorGPIOEvents m_ProcessorGPIOEvents;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
            //m_GPIOOutPutServiceConnectorConfig = null;
            //m_GPIOInputServiceConnectorConfig = null;
            //m_GPIOConnector = null;
            //m_Banks = null;

            m_SEN0188SQLite = null;
            m_FingertEventDatabase = null;
            m_Sensoroutputconfigoptions = null;
            m_Sensorinputconfigoptions = null;
            m_serDev = null;
            m_SensorInitialized = false;
            m_SensorConnecorInitialized = false;
            m_GPIOConnecorInitialized = false;
            m_DataSets = null;
            //m_SensorID = "not set";
            m_GPIOEnvironmentConnectors = null;
            m_ProcessorGPIOEvents = null;
            m_DataSets = new ObservableCollection<FingerEvent> ();


        }



        public StationEnvironment Environment
        {
            get { return m_Environment; }

        }


        public System.Collections.Generic.IList<GPIOEnvironmentConnector> GPIOEnvironmentConnectors
        {
            get { return m_GPIOEnvironmentConnectors.EnvironmentConnectors; }

        }

        public bool SensorInitialized
        {
            get { return m_SensorInitialized; }
            set
            {
                Set(ref m_SensorInitialized, value);
            }

        }

        public bool ConnecorsInitialized
        {
            get {
                return m_SensorConnecorInitialized && GPIOConnecorInitialized;
            }

        }

        public bool SensorConnecorInitialized
        {
            get { return m_SensorConnecorInitialized; }
            set
            {
                Set(ref m_SensorConnecorInitialized, value);
                OnPropertyChanged("ConnecorsInitialized");


            }

        }

        public bool GPIOConnecorInitialized
        {
            get {
                return m_GPIOConnecorInitialized;
        
            }
            set
            {
                Set(ref m_GPIOConnecorInitialized, value);
                OnPropertyChanged("ConnecorsInitialized");
            }

        }

        public IList<FingerEvent> FingerEventsDataSets
        {
            get { return m_DataSets; }

        }



        /*
        public GPIOOBank OutPuts
        {
            get { return m_OutPuts; }

        }
        public string VisibleConnectorName
        {
            get { return m_GPIOConnector.HostName; }

        }


        public GPIOOBank Inputs
        {
            get { return m_Inputs; }

        }

            */
        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private void StopConnectors()
        {

            m_Environment.SensorStopConnector();
            m_Environment.GPIOStopConnector();
        }

        private void StartConnectors()
        {

            m_Environment.SensorStartConnector();
            m_Environment.GPIOStartConnector();



        }

     

        private void ProcessPropertysFromGPIOConnector(IPropertySet propertys)
        {
            this.m_GPIOEnvironmentConnectors.ProcessPropertysFromGPIOConnector(propertys);
        }



        async private void GPIOConnector_ChangeGPIOs(object sender, IPropertySet args)
        {


            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            { // your code should be here

                ProcessPropertysFromGPIOConnector(args);
            });


        }


        async private void GPIOConnector_startStreaming(object sender, Windows.Networking.Sockets.StreamSocket args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,  () =>
            { // your code should be here

                GPIOConnector GPIOcon = sender as GPIOConnector;

                GPIOEnvironmentConnector con = this.m_GPIOEnvironmentConnectors.getGPIOOConnectorByGPIOConnector(GPIOcon);
                if (con != null)
                {
                    con.GPIOConnecorInitialized = true;
                }
                GPIOConnecorInitialized = m_GPIOEnvironmentConnectors.GPIOConnecorInitialized; 


            });
        }

        async private void GPIOConnector_stopStreaming(object sender, string args)
        {

            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                { // your code should be here

                    GPIOConnector GPIOcon = sender as GPIOConnector;

                    GPIOEnvironmentConnector con = this.m_GPIOEnvironmentConnectors.getGPIOOConnectorByGPIOConnector(GPIOcon);
                    if (con != null)
                    {
                        con.GPIOConnecorInitialized = false;
                    }

                    GPIOConnecorInitialized = m_GPIOEnvironmentConnectors.GPIOConnecorInitialized;

                    GPIOStopConnector();
                    if (args.Length > 0)
                    {
                        var messageDialog = new MessageDialog(args);
                        await messageDialog.ShowAsync();
                    }


                });
            }


        }

        async private void SensorStopConnector()
        {

            await m_Connector_SEN0188.stopProcessingPackagesAsync();

        }

        async private void SensorStartConnector()
        {
            if (m_serDev != null)
            {
                if (m_Environment.ConnectorSEN0188Enable)
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

        private void GPIOStopConnector()
        {
            this.m_GPIOEnvironmentConnectors.stopConnectors();

        }

        private void GPIOStartConnector()
        {

            this.m_GPIOEnvironmentConnectors.startConnectors();
 
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {

  

            m_ProcessorGPIOEvents.NotifyEvent -= ProcessorGPIOEvents_NotifyEvent;


            base.OnNavigatingFrom(e);

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is FingerSensorsApp.App)
            {

                FingerSensorsApp.App AppEnvironment = e.Parameter as FingerSensorsApp.App;


                if (AppEnvironment != null)
                {
                    m_Environment = AppEnvironment.Environment;
  
                    m_GPIOEnvironmentConnectors = m_Environment.GPIOEnvironmentConnectors;

                    m_SEN0188SQLite = m_Environment.SEN0188SQLite;
                    m_FingertEventDatabase = m_Environment.FingertEventDatabase;
                    //m_DataSets = m_FingertEventDatabase.DataSets;
                    m_Connector_SEN0188 = m_Environment.SensorConnector;
                    m_Sensoroutputconfigoptions = m_Environment.SensorOutPutServiceConnectorConfig;
                    m_Sensorinputconfigoptions = m_Environment.SensorInputServiceConnectorConfig;

                    m_serDev = m_Environment.SerDev;

                    m_ProcessorGPIOEvents = m_Environment.ProcessorGPIOEvents;

                    m_ProcessorGPIOEvents.NotifyEvent += ProcessorGPIOEvents_NotifyEvent;



                }


            }
            base.OnNavigatedTo(e);
        }

        private async void ProcessorGPIOEvents_NotifyEvent(object sender, FingerEvent args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            { // your code should be here
                m_DataSets.Insert(0, args);
                
                while (m_DataSets.Count > 50)
                {
                    m_DataSets.RemoveAt(50);
                }
                

            });


          //  GetEventData();
        }

        private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
        {

            ToggleSwitch toggle = sender as ToggleSwitch;
            toggle.Toggled += ToggleSwitch_Toggled;
            if (toggle.Tag!=null)
            {

            }




        }

        private void ToggleSwitch_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

            if (this.PivotGPIOConnectors.SelectedItem == null) return;

            GPIOEnvironmentConnector con = this.PivotGPIOConnectors.SelectedItem as GPIOEnvironmentConnector;
            if (con == null) return;

            ToggleSwitch tgl = sender as ToggleSwitch;
            if (tgl != null)
            {

                GPIOObject GPIOObj = tgl.Tag as GPIOObject;
                if (GPIOObj != null)
                {
                    GPIOObj.SetValue = tgl.IsOn ? 1 : 0;
                    con.UpdateInputPropertySets(GPIOObj);
                }

            }

        }

        async void GetEventData()
        {
            
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            { // your code should be here
                DateTime dateTime = DateTime.Now;
                TimeSpan sp = -TimeSpan.FromDays(14);
                dateTime = dateTime.AddTicks(sp.Ticks);
               // m_FingertEventDatabase.GetDataSetsGreaterThanDateTime(dateTime);


            });




        }



        void stopRecording_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StopConnectors();
        }


        void startRecording_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StartConnectors();
        }

        void FingerSensorCmds_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AppBarButton buttton = sender as AppBarButton;
            if (buttton != null)
            {
                if (buttton.Name == "InitSensor")
                {
                    SensorCMDs.InitSensor(m_Sensorinputconfigoptions);
                    //   CMD = Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerSensorInitialize");
                    //   m_inputconfigoptions["FingerPrint.CMD"] = PropertyValue.CreateInt32(CMD);
   //                 SensorInitialized = false;
                    this.m_Environment.SensorInitialized = false;
                }
                else if (buttton.Name == "ReadSensor")
                {
                    SensorCMDs.VerifyFingerId(m_Sensorinputconfigoptions);
                }

            }
        }

        /*
        
        void Update_SEN0188_NotifyChangeState(IPropertySet Outputpropertys)
        {

            Object Valout;
            Int16 state = -1;
            String cmdState = "...";
            bool doactFilledSensorId = false;
            if (Outputpropertys.TryGetValue("FingerPrint.CMDState", out Valout))
            {
                if (Valout != null)
                {
                    state = (Int16)Valout;
                }


            }

            if (Outputpropertys.TryGetValue("FingerPrint.CMDTextState", out Valout))
            {
                String cmd = Valout as String;
                if (cmd != null)
                {
                    cmdState = cmd;

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
                           // SensorInitialized = true;
                            doactFilledSensorId = true;
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
                                            if (dataset.AccessRights_Bit0) // Master Bit
                                            {
                                            
  

                                            }


                                        }
                                    }
                                }


                            }
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
                            m_SensorID = System.Text.Encoding.UTF8.GetString(array, 0, array.Length);
                         }
                    }
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

        async private void Connector_SEN0188_Failed(object sender, string args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async  () =>
            { // your code should be here
                SensorInitialized = false;
                SensorConnecorInitialized = false;
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
                SensorInitialized = false;
                SensorConnecorInitialized = false;
                SensorStopConnector();


            });
        }

        async private void Connector_SEN0188_startStreaming(object sender, SerDevice args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,  () =>
            {
                SensorConnecorInitialized = true;

                SensorInitialized = false;

            });
        }

        private void PivotGPIOConnectors_Loaded(object sender, RoutedEventArgs e)
        {

            return ;

            Pivot pivot = sender as Pivot;

 
            if (pivot == null) return;
            int i = 0;
            while (i < pivot.Items.Count)
            {
                GPIOEnvironmentConnector item = pivot.Items[i] as GPIOEnvironmentConnector;
                if (item != null)
                {
                    if (!item.GPIOConnectorEnable)
                    {
                        pivot.Items.Remove(item);
                        continue;
                    }

                }
                i++;

            }
        }
        */
    }
}
