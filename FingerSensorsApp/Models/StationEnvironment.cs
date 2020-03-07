using SEN0188_SQLite;
using FingerPrintSensor_SEN0188;
using System.ComponentModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using GPIOServiceConnector;

using System.IO;
using FingerSensorsApp.Helpers;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using FingerSensorsApp.Services;
using Windows.System.Threading;

namespace FingerSensorsApp.Models
{
    public class GPIOEnvironmentConnector : INotifyPropertyChanged
    {

        string m_HostName;
        int m_Port;
        public event PropertyChangedEventHandler PropertyChanged;
        GPIOServiceConnector.GPIOConnector m_GPIOConnector;
        GPIOOInOutBanks m_GPIOInOutBanks;

        GPIOOInOutBanks m_ActiveGPIOInOutBanks;

        PropertySet m_GPIOOutPutServiceConnectorConfig;
        PropertySet m_GPIOInputServiceConnectorConfig;

      //  ConfigProcessItems m_ConfigProcessItems;

        GPIOOBank m_OutPuts;
        GPIOOBank m_Inputs;
        GPIOOBank m_ActiveOutPuts;
        GPIOOBank m_ActiveInputs;


        bool m_GPIOConnectorEnable;
        bool m_GPIOConnecorInitialized;



        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        public GPIOEnvironmentConnector(string hostName, int Port)
        {
            m_HostName = hostName;
            m_Port = Port;
            m_GPIOOutPutServiceConnectorConfig = new PropertySet();
            m_GPIOInputServiceConnectorConfig = new PropertySet();

  
            m_GPIOConnector = new GPIOConnector();
            m_GPIOInOutBanks = null;
            m_ActiveGPIOInOutBanks = null;
            m_OutPuts = null;
            m_Inputs = null;

            m_ActiveOutPuts = null;
            m_ActiveInputs = null;



            m_GPIOConnectorEnable = false;
            m_GPIOConnecorInitialized = false;
        }

        public async Task<bool> InitializeAsync()
        {

            m_GPIOInOutBanks = await GPIOOInOutBanks.GPIOOInOutBanksAsync(m_GPIOInputServiceConnectorConfig);

            m_Inputs = m_GPIOInOutBanks.InOutBanks[0];
            m_OutPuts = m_GPIOInOutBanks.InOutBanks[1];
            return (m_GPIOInOutBanks.InOutBanks.Count>0);


        }

        public void UpdateInputPropertySets(GPIOObject GPIOObj)
        {
   
            //OutPut.readImages();
            //  string keyPinValue = string.Format("GPIO.{0:00}", OutPut.PinNumber);
            string keyPinValue = GPIOObj.PinName;

            Object Valout;
            if (m_GPIOInputServiceConnectorConfig.TryGetValue(keyPinValue, out Valout))
            {
                if (Valout != null)
                {
                    string nwLine = (string)GPIOObj.getPropertyLine();
                    Valout = nwLine;
                    m_GPIOInputServiceConnectorConfig[keyPinValue] = Valout;

                }

            }



        }

        public void UpdateOutputPropertySets(GPIOObject GPIOObj)
        {

            string keyPinValue = GPIOObj.PinName;
            double dblValue = 0;
            Object Obj = null;
            if (this.GPIOOutPutServiceConnectorConfig.TryGetValue(keyPinValue, out Obj))
            {
                if (Obj != null)
                {
                    dblValue = (double)Obj;
                    if (GPIOObj.PinValue!= dblValue)
                    {
                        if (GPIOObj.GPIOtyp == GPIOObject.GPIOTyp.output)
                        {
                            if (GPIOObj.SetValue != dblValue)
                            {
                                GPIOObj.SetValue = dblValue;
                                double PulseTime = GPIOObj.PulseTime; // save pulseTime
                                GPIOObj.PulseTime = 0; // pulsetime set to 0
                                UpdateInputPropertySets(GPIOObj);
                                GPIOObj.PulseTime = PulseTime; // store back PulseTime
                            }
                        }
                        GPIOObj.PinValue = dblValue;
                    }

                }
            }

        }

        public void resetAllOutputs()
        {
            if (m_GPIOConnectorEnable && m_GPIOConnecorInitialized)
            {
                UpdateState(0);
                GPIOOBank bank = m_OutPuts;

                foreach (GPIOObjects OutPuts in bank.GPIOBanks)
                {
                    foreach (GPIOObject OutPut in OutPuts.GPIOs)
                    {
                        OutPut.SetValue = OutPut.InitValue;
                        double PulseTime = OutPut.PulseTime; // save pulseTime
                        OutPut.PulseTime = 0; // pulsetime set to 0
                        OutPut.IsFlankActive = true; // Active set
                        UpdateInputPropertySets(OutPut);
                        OutPut.IsFlankActive = false;
                        OutPut.PulseTime = PulseTime; // store back PulseTime
                    }

                }
                UpdateState(1);
            }

        }


        public void UpdateState(int updateValue)
        {
            Object Valout;
            if (m_GPIOInputServiceConnectorConfig.TryGetValue("UpdateState", out Valout))
            {
                if (Valout != null)
                {
                    int state = (int)Valout;
                    if (state != updateValue)
                    {
                        Valout = (int)updateValue;
                        m_GPIOInputServiceConnectorConfig["UpdateState"] = Valout;
                    }
                }

            }



        }
        public async void stopConnector()
        {
            await m_GPIOConnector.stopProcessingPackagesAsync();
        }
        public async void startConnector()
        {

            if (m_GPIOConnectorEnable && !m_GPIOConnecorInitialized)
            {
                m_GPIOInputServiceConnectorConfig["HostName"] = HostName;
                m_GPIOInputServiceConnectorConfig["Port"] = Port;
                m_GPIOInputServiceConnectorConfig["UpdateState"] = PropertyValue.CreateInt32(0);

                //    m_GPIOInputServiceConnectorConfig.Add("UpdateState", PropertyValue.CreateInt32(0));
                
                for (int i = 0; i < m_ActiveGPIOInOutBanks.InOutBanks.Count; i++)
                {
                    GPIOOBank bank = m_ActiveGPIOInOutBanks.InOutBanks[i];

                    foreach (GPIOObjects OutPuts in bank.GPIOBanks)
                    {
                        foreach (GPIOObject GPIOObj in OutPuts.GPIOs)
                        {
                            UpdateInputPropertySets(GPIOObj);
                        }

                    }
                }
                UpdateState(1);
                await m_GPIOConnector.startProcessingPackagesAsync(m_GPIOInputServiceConnectorConfig, m_GPIOOutPutServiceConnectorConfig);
            }
        }


        public void InitializeActiveBanks()
        {

            m_GPIOInputServiceConnectorConfig.Clear();
            m_ActiveGPIOInOutBanks = m_GPIOInOutBanks.GPIOOActiveInOutBanks(m_GPIOInputServiceConnectorConfig);

            //  m_ActiveGPIOInOutBanks = await GPIOOInOutBanks.GPIOOInOutBanksAsync(m_GPIOInputServiceConnectorConfig);

            m_ActiveInputs = m_ActiveGPIOInOutBanks.InOutBanks[0];
            m_ActiveOutPuts = m_ActiveGPIOInOutBanks.InOutBanks[1];


        }





        public string VisibleKeyName
        {
            get {
                return m_HostName;

            }

        }

        public GPIOOBank OutPuts
        {
            get { return m_OutPuts; }

        }

        public GPIOOBank Inputs
        {
            get { return m_Inputs; }

        }

        public GPIOOBank ActiveOutPuts
        {
            get { return m_ActiveOutPuts; }

        }

        public GPIOOBank ActiveInputs
        {
            get { return m_ActiveInputs; }

        }


        public GPIOConnector GPIOConnector
        {
            get
            {
                return m_GPIOConnector;
            }

        }

        public GPIOOInOutBanks GPIOOInOutBanks
        {
            get
            {
                return m_GPIOInOutBanks;
            }

        }

        public GPIOOInOutBanks ActiveGPIOInOutBanks
        {
            get
            {
                return m_ActiveGPIOInOutBanks;
            }

        }



        public PropertySet GPIOOutPutServiceConnectorConfig
        {
            get
            {
                return m_GPIOOutPutServiceConnectorConfig;
            }

        }

        public PropertySet GPIOInputServiceConnectorConfig
        {
            get
            {
                return m_GPIOInputServiceConnectorConfig;
            }

        }




        public async void GetDataAsync()
        {
            //readImages ();

            m_GPIOInOutBanks = await GPIOOInOutBanks.GPIOOInOutBanksAsync(m_GPIOInputServiceConnectorConfig);

        }


        public bool GPIOConnecorInitialized
        {
            get
            {
                if (m_GPIOConnectorEnable) return m_GPIOConnecorInitialized;
                else return true;
            }
            set
            {
                m_GPIOConnecorInitialized = value;
                OnPropertyChanged("GPIOConnecorInitialized");

            }

        }


        public bool GPIOConnectorEnable
        {
            get
            {
                return m_GPIOConnectorEnable;
            }
            set
            {
                m_GPIOConnectorEnable = value;
                OnPropertyChanged("GPIOConnectorEnable");
                OnPropertyChanged("GPIOConnecorInitialized");
            }

        }

        public string HostName
        {
            get
            {
                return m_HostName;
            }
            set
            {
                m_HostName = value;
                OnPropertyChanged("HostName");
                OnPropertyChanged("VisibleKeyName");
            }

        }
        public int Port
        {
            get
            {
                return m_Port;
            }
            set
            {
                m_Port = value;
                OnPropertyChanged("Port");
            }

        }

    }


    public class GPIOEnvironmentConnectors
    {

        private ObservableCollection<GPIOEnvironmentConnector> m_GPIOEnvironmentConnectors;


        public GPIOEnvironmentConnectors()
        {

            m_GPIOEnvironmentConnectors = new ObservableCollection<GPIOEnvironmentConnector>();

            //GPIOEnvironmentConnector con1 = new GPIOEnvironmentConnector("localhost", 3005);
            //GPIOEnvironmentConnector con2 = new GPIOEnvironmentConnector("localhost", 3005);
            //m_GPIOEnvironmentConnectors.Add(con1);
            //m_GPIOEnvironmentConnectors.Add(con2);

        }

        public System.Collections.Generic.IList<GPIOEnvironmentConnector> EnvironmentConnectors
        {
            get { return m_GPIOEnvironmentConnectors; }

        }

        public bool GPIOConnecorInitialized
        {
            get
            {
                bool GPIOsInit = true;

                for (int i = 0; i < m_GPIOEnvironmentConnectors.Count; i++)
                {
                    GPIOEnvironmentConnector con = m_GPIOEnvironmentConnectors[i];
                    if (!con.GPIOConnectorEnable) continue;
                    if (con.GPIOConnecorInitialized) continue;

                    GPIOsInit = false;
                    break;
                }
                return GPIOsInit;

            }

        }

        public async Task<GPIOEnvironmentConnector>  addConnector()
        {
            GPIOEnvironmentConnector con1 = new GPIOEnvironmentConnector("localhost", 3005);
            await con1.InitializeAsync();
            m_GPIOEnvironmentConnectors.Add(con1);
            return con1;

        }
        public void deleteConnector(GPIOEnvironmentConnector todeletecon)
        {
            for (int i = 0; i < m_GPIOEnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = m_GPIOEnvironmentConnectors[i];
                if (todeletecon == con)
                {
                    con.stopConnector();
                    m_GPIOEnvironmentConnectors.RemoveAt(i);
                    break;
                }

            }


        }

        public async Task<bool> InitializeAsync()
        {

            bool ret = true;
            for (int i = 0; i < m_GPIOEnvironmentConnectors.Count; i++)
            {
                ret = await m_GPIOEnvironmentConnectors[i].InitializeAsync() && ret;
            }

            return ret;

        }

        public void resetAllOutputs()
        {
            for (int i = 0; i < EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = EnvironmentConnectors[i];

                con.resetAllOutputs();

            }

        }

        public  void startConnectors()
        {
            for (int i = 0; i < EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = EnvironmentConnectors[i];

                con.startConnector();

            }

        }

        public void stopConnectors()
        {
            for (int i = 0; i < EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = EnvironmentConnectors[i];
                con.stopConnector();

            }

        }

        public GPIOEnvironmentConnector getGPIOOConnectorByHostName(string name)
        {
            for (int i = 0; i < EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = EnvironmentConnectors[i];
                if (con.HostName == name)
                {
                    return con;
                }


            }
            return null;

        }


        public GPIOEnvironmentConnector getGPIOOConnectorByOutputPropertySet(IPropertySet propertys)
        {
            for (int i = 0; i < EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = EnvironmentConnectors[i];
                if (con.GPIOOutPutServiceConnectorConfig == propertys)
                {
                    return con;
                }


            }
            return null;

        }

        public GPIOEnvironmentConnector getGPIOOConnectorByGPIOConnector(GPIOConnector connnector)
        {
            for (int i = 0; i < EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = EnvironmentConnectors[i];
                if (con.GPIOConnector == connnector)
                {
                    return con;
                }

            }
            return null;

        }

        public void ProcessPropertysFromGPIOConnector(IPropertySet propertys)
        {

         //   this.m_GPIOEnvironmentConnectors.ProcessPropertysFromGPIOConnector(propertys);

            var con = getGPIOOConnectorByOutputPropertySet(propertys);

            if (con == null) return;

            var m_Banks = con.ActiveGPIOInOutBanks;     

            con.UpdateState(0);
            for (int i = 0; i < m_Banks.InOutBanks.Count; i++)
            {
                GPIOOBank bank = m_Banks.InOutBanks[i];

                foreach (GPIOObjects OutPuts in bank.GPIOBanks)
                {
                    foreach (GPIOObject GPIOObj in OutPuts.GPIOs)
                    {
                        con.UpdateOutputPropertySets(GPIOObj);

                    }

                }
            }

            con.UpdateState(1);
        }

        public void InitializeActiveBanks()
        {

            for (int i = 0; i < EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = EnvironmentConnectors[i];
                con.InitializeActiveBanks();
                con.ActiveGPIOInOutBanks.readImages();
            }


        }
    }
    public class StationEnvironment : INotifyPropertyChanged
    {
        private SerDevice m_serDev;
        private string m_HostName;
        private int m_Port;
        public event PropertyChangedEventHandler PropertyChanged;
        // GPIOServiceConnector.GPIOConnector m_GPIOConnector;
        private GPIOOInOutBanks m_GPIOInOutBanks;


        private PropertySet m_SensorOutPutServiceConnectorConfig;
        private PropertySet m_SensorInputServiceConnectorConfig;

        private PropertySet m_GPIOOutPutServiceConnectorConfig;
        private PropertySet m_GPIOInputServiceConnectorConfig;

        private Connector_SEN0188 m_Connector_SEN0188;
        private SEN0188SQLite m_SEN0188SQLite;
        private FingertEventDatabase m_FingertEventDatabase;
        private bool m_ConnectorSEN0188Enable;
        //        bool m_GPIOConnectorEnable;

        private GPIOEnvironmentConnectors m_GPIOEnvironmentConnectors;
        private ConfigProcessItems m_ConfigProcessItems;

        private ProcessorGPIOEvents m_ProcessorGPIOEvents;

        private bool m_SensorConnecorInitialized;
        private bool m_GPIOConnecorInitialized;
        private bool m_SensorInitialized;

        private bool m_IsLoggedIn;
        private bool m_isAuthorized;

        private UserData m_user;

        private UserDataService m_UserDataService => Singleton<UserDataService>.Instance;

        private IdentityService m_IdentityService => Singleton<IdentityService>.Instance;

        // Create the OnPropertyChanged method to raise the event

        private ThreadPoolTimer m_PeriodicTimerDelFingerEvents;
        private int m_EventHistoryinDays;

       protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        ~StationEnvironment()
        {
            m_IdentityService.LoggedIn -= OnLoggedIn;
            m_IdentityService.LoggedOut -= OnLoggedOut;
            m_UserDataService.UserDataUpdated -= OnUserDataUpdated;
        }

        public StationEnvironment()
        {
            m_HostName = "localhost";
            m_Port = 3005;
            m_serDev = new SerDevice("COM1");
            m_Connector_SEN0188 = new Connector_SEN0188();
            m_SEN0188SQLite = new SEN0188SQLite();
            m_FingertEventDatabase = new FingertEventDatabase();
            m_SensorOutPutServiceConnectorConfig = new PropertySet();
            m_SensorInputServiceConnectorConfig = new PropertySet();

            m_GPIOOutPutServiceConnectorConfig = new PropertySet();
            m_GPIOInputServiceConnectorConfig = new PropertySet();

     

            m_GPIOInputServiceConnectorConfig.Add("HostName", PropertyValue.CreateString("WilliRaspiPlus"));
            m_GPIOInputServiceConnectorConfig.Add("Port", PropertyValue.CreateInt32(3005));
            m_GPIOInputServiceConnectorConfig.Add("UpdateState", PropertyValue.CreateInt32(0));
            m_GPIOInOutBanks = null;

            m_ConnectorSEN0188Enable = false;
  
            m_GPIOEnvironmentConnectors = new GPIOEnvironmentConnectors();


            m_ConfigProcessItems = new ConfigProcessItems(m_GPIOEnvironmentConnectors);
            m_ProcessorGPIOEvents = new ProcessorGPIOEvents(this);

            m_SensorConnecorInitialized = false;
            m_GPIOConnecorInitialized = false;
            m_SensorInitialized = false;
            m_IsLoggedIn = false;
            m_isAuthorized = false;
            m_user = null;
            m_PeriodicTimerDelFingerEvents = null;
            m_EventHistoryinDays = 14; // 14 Tage

        }

        public async Task<bool> InitializeUserAsync()
        {
   
            m_IdentityService.LoggedIn += OnLoggedIn;
            m_IdentityService.LoggedOut += OnLoggedOut;
            m_UserDataService.UserDataUpdated += OnUserDataUpdated;
            IsLoggedIn = m_IdentityService.IsLoggedIn();
            User = await m_UserDataService.GetUserAsync();

            IsAuthorized = IsLoggedIn && await checkAuthorizationAsync(User);

            return (User!=null);
        }


        public async Task<bool> checkAuthorizationAsync(UserData user)
        {
            var ret = await Task.Run(() => checkAuthorization(user));
            return ret; ;
        }


        private bool checkAuthorization(UserData user)
        {
            if (user == null) return false;

            ObservableCollection<DBDataSet> DataSets = new ObservableCollection<DBDataSet>();
            m_SEN0188SQLite.GetDataSetsByName(user.GivenName, user.Surname, DataSets);
            foreach (var dataset in DataSets)
            {
                if (dataset.AccessRights_Bit0)
                {
                    return true;
                }
            }

            return false;
        }


        private async void OnUserDataUpdated(object sender, UserData userData)
        {
            User = userData;
            IsAuthorized = IsLoggedIn && await checkAuthorizationAsync(User);
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            IsLoggedIn = true;
            IsAuthorized = IsLoggedIn && await checkAuthorizationAsync(User);

        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            User = null;
            IsLoggedIn = false;
            IsAuthorized = false;

        }


        public int EventHistoryinDays
        {
            get { return m_EventHistoryinDays; }
            set
            {

                m_EventHistoryinDays = value;
                OnPropertyChanged("EventHistoryinDays");
            }
        }

        public UserData User
        {
            get { return m_user; }
            set {

                m_user = value;
                OnPropertyChanged("User");
            }
        }


        public bool IsLoggedIn
        {
            get { return m_IsLoggedIn; }
            set {

                m_IsLoggedIn= value;
                OnPropertyChanged("IsLoggedIn");
            }
        }

        public bool IsAuthorized
        {
            get { return m_isAuthorized; }
            set
            {
                m_isAuthorized = value;
                OnPropertyChanged("IsAuthorized");
                OnPropertyChanged("SensorInitializedandIsAuthorized");
            }


        }


        public bool ConnecorsInitialized
        {
            get
            {
                return SensorConnecorInitialized && GPIOConnecorInitialized;
            }

        }

        public bool SensorInitializedandIsAuthorized
        {
            get
            {
                return SensorInitialized && IsAuthorized;
            }

        }

        public bool SensorInitialized
        {
            get {
                return m_SensorInitialized;
            }
            set
            {
                if (value!= m_SensorInitialized)
                {
                    m_SensorInitialized = value;
                    OnPropertyChanged("SensorInitialized");
                    OnPropertyChanged("SensorInitializedandIsAuthorized");
                }

            }

        }

  
        public bool SensorConnecorInitialized
        {
            get {
                if (this.ConnectorSEN0188Enable) return m_SensorConnecorInitialized;
                else return true;
            }
            set
            {
                m_SensorConnecorInitialized = value;
                OnPropertyChanged("SensorConnecorInitialized");
                OnPropertyChanged("ConnecorsInitialized");
                OnPropertyChanged("ConnectorSEN0188Enable");


            }

        }

        public bool GPIOConnecorInitialized
        {
            get
            {
                return m_GPIOConnecorInitialized;

            }
            set
            {
                m_GPIOConnecorInitialized = value;
                OnPropertyChanged("GPIOConnecorInitialized");
                OnPropertyChanged("ConnecorsInitialized");
                OnPropertyChanged("ConnectorSEN0188Enable");
            }

        }

        public ProcessorGPIOEvents ProcessorGPIOEvents
        {
            get { return m_ProcessorGPIOEvents; }

        }

        public ConfigProcessItems ConfigProcessItems
        {
            get { return m_ConfigProcessItems; }

        }


        public GPIOEnvironmentConnectors GPIOEnvironmentConnectors
        {
            get { return m_GPIOEnvironmentConnectors; }

        }


        public Connector_SEN0188 SensorConnector
        {
            get { return m_Connector_SEN0188; }
        }


        public SEN0188SQLite SEN0188SQLite
        {
            get { return m_SEN0188SQLite; }
        }

        public FingertEventDatabase FingertEventDatabase
        {
            get { return m_FingertEventDatabase; }
        }




        public PropertySet GPIOOutPutServiceConnectorConfig
        {
            get
            {
                return m_GPIOOutPutServiceConnectorConfig;
            }

        }

        public PropertySet GPIOInputServiceConnectorConfig
        {
            get
            {
                return m_GPIOInputServiceConnectorConfig;
            }

        }


        public PropertySet SensorOutPutServiceConnectorConfig
        {
            get
            {
                return m_SensorOutPutServiceConnectorConfig;
            }

        }

        public PropertySet SensorInputServiceConnectorConfig
        {
            get
            {
                return m_SensorInputServiceConnectorConfig;
            }

        }



        public async void GetDataAsync()
        {
            //readImages ();

            var dbPath = await FileCopy.GetFingerDataBaseFolder();

            var dbPathEvent = await FileCopy.GetFingerEventDataBaseFolder();


            m_SEN0188SQLite.DBDataBasePath = dbPath;

            m_FingertEventDatabase.DBDataBaseName = dbPathEvent;

            m_SEN0188SQLite.InitializeDatabase();
  

            m_FingertEventDatabase.InitializeDatabase();

            m_SEN0188SQLite.GetDataSets();
            m_GPIOInOutBanks = await GPIOOInOutBanks.GPIOOInOutBanksAsync(m_GPIOInputServiceConnectorConfig);

            await m_GPIOEnvironmentConnectors.InitializeAsync();

         



        }


        public SerDevice SerDev
        {
            get
            {
                return m_serDev;
            }

        }

        /*
        public bool GPIOConnectorEnable
        {
            get
            {
                return m_GPIOConnectorEnable;
            }
            set
            {
                m_GPIOConnectorEnable = value;
                OnPropertyChanged("GPIOConnectorEnable");
            }

        }
        */

        public bool ConnectorSEN0188Enable
        {
            get
            {
                return m_ConnectorSEN0188Enable;
            }
            set
            {
                m_ConnectorSEN0188Enable = value;
                OnPropertyChanged("ConnectorSEN0188Enable");
            }

        }


        public string HostName
        {
            get
            {
                return m_HostName;
            }
            set
            {
                m_HostName = value;
                OnPropertyChanged("HostName");
            }

        }
        public int Port
        {
            get
            {
                return m_Port;
            }
            set
            {
                m_Port = value;
                OnPropertyChanged("Port");
            }

        }

        async public void SensorStopConnector()
        {

            await m_Connector_SEN0188.stopProcessingPackagesAsync();

        }

        async public void SensorStartConnector()
        {
            if (m_serDev != null)
            {
                if (ConnectorSEN0188Enable)
                {
                    if (!m_Connector_SEN0188.ProcessingPackagesStarted || !m_SensorConnecorInitialized){
                        TimeSpan _timeOut = TimeSpan.FromMilliseconds(1000);
                        m_serDev.BaudRate = (uint)57600;
                        m_serDev.WriteTimeout = _timeOut;
                        m_serDev.ReadTimeout = _timeOut;
                        m_serDev.Parity = Windows.Devices.SerialCommunication.SerialParity.None;
                        m_serDev.StopBits = Windows.Devices.SerialCommunication.SerialStopBitCount.One;
                        m_serDev.DataBits = 8;
                        m_serDev.Handshake = Windows.Devices.SerialCommunication.SerialHandshake.None;


                        m_SensorInputServiceConnectorConfig["UpdateState"] = PropertyValue.CreateInt32(1);


                        await m_Connector_SEN0188.startProcessingPackagesAsync(m_serDev, m_SensorInputServiceConnectorConfig, m_SensorOutPutServiceConnectorConfig);

                    }

                }


            }




        }
        public void GPIOStopConnector()
        {
            this.m_GPIOEnvironmentConnectors.stopConnectors();

        }

        public void GPIOStartConnector()
        {

            this.m_GPIOEnvironmentConnectors.startConnectors();

        }
        public void StopConnectors()
        {

            SensorStopConnector();
            GPIOStopConnector();
        }

        public void StartConnectors()
        {

            SensorStartConnector();
            GPIOStartConnector();



        }

        public void startDeleteFingerEventsTimer(TimeSpan period)
        {

            if (m_PeriodicTimerDelFingerEvents != null) return;
      
            m_PeriodicTimerDelFingerEvents = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                // alle löschen, welche um 14 Tage zurückliegen
                DateTime dateTime = DateTime.Now;
                TimeSpan sp = -TimeSpan.FromDays(EventHistoryinDays);
                dateTime = dateTime.AddTicks(sp.Ticks);
                this.FingertEventDatabase.deleteDataSetsLesserThanDateTime(dateTime);
                //
                // TODO: Work
                //

                //
                // Update the UI thread by using the UI core dispatcher.
                //
                /*
                Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                    //
                    // UI components can be accessed within this scope.
                    //

                });
                */
            }, period);
        }

        public void stoppDeleteFingerEventsTimer()
        {
            if (m_PeriodicTimerDelFingerEvents == null) return;
            m_PeriodicTimerDelFingerEvents.Cancel();
            m_PeriodicTimerDelFingerEvents = null;
        

        }
    }

}
