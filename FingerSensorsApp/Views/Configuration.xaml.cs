using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FingerPrintSensor_SEN0188;
using System.Collections.ObjectModel;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using SEN0188_SQLite;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;
using FingerSensorsApp.Helpers;
using Windows.Storage;
using FingerSensorsApp.Models;
using FingerSensorsApp.Services;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FingerSensorsApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Configuration : Page
    {
        private ObservableCollection<FingerPrintSensor_SEN0188.SerDevice> m_listOfDevices;


        Models.StationEnvironment m_Environment;
        Connector_SEN0188 m_Connector_SEN0188;
        FingertEventDatabase m_FingertEventDatabase;
        SEN0188SQLite m_SEN0188SQLite;

     //   SerDevice m_serDev;
        Windows.Foundation.Collections.PropertySet m_Sensoroutputconfigoptions;
        Windows.Foundation.Collections.PropertySet m_Sensorinputconfigoptions;
        SettingsToStorage m_SettingsToStorage;

        GPIOOBank m_OutPuts;
        GPIOOBank m_Inputs;

    //    GPIOOInOutBanks m_Banks;


        public static ObservableCollection<DBDataSetAccessBit> m_AccessBitsCollection;


        GPIOEnvironmentConnectors m_GPIOEnvironmentConnectors;
        public Configuration()
        {
            this.InitializeComponent();
            m_Connector_SEN0188 = null;
            m_FingertEventDatabase = null;
            m_SEN0188SQLite = null;
            m_Environment = null;
            m_listOfDevices = new ObservableCollection<FingerPrintSensor_SEN0188.SerDevice>();
            m_Sensoroutputconfigoptions = null;
            m_Sensorinputconfigoptions = null;
         //   m_serDev = null;
            m_SettingsToStorage = null;
            m_OutPuts = null;
            m_Inputs = null;
           // m_Banks = null;
            m_GPIOEnvironmentConnectors = null;
            ListAvailablePorts();
            m_AccessBitsCollection = DBDataSetAccessRight.getAccessBitsCollection();
        }





        public System.Collections.Generic.IList<GPIOEnvironmentConnector> GPIOEnvironmentConnectors
        {
            get { return m_GPIOEnvironmentConnectors.EnvironmentConnectors; }

        }


        public System.Collections.Generic.IList<DBDataSetAccessBit> AccessBitsCollection
        {
            get { return m_AccessBitsCollection; }

        }

        public System.Collections.Generic.IList<FingerPrintSensor_SEN0188.SerDevice> Devices
        {
            get { return m_listOfDevices; }

        }

        public Models.StationEnvironment Environment
        {
            get { return m_Environment; }

        }
        public GPIOOBank OutPuts
        {
            get { return m_OutPuts; }

        }

        public GPIOOBank Inputs
        {
            get { return m_Inputs; }

        }




        protected override void OnNavigatingFrom(Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            if (m_SettingsToStorage != null)
            {
                m_SettingsToStorage.writeDatatoLocalStorage();
            }


            m_Environment.GPIOEnvironmentConnectors.InitializeActiveBanks();
            m_Environment.ProcessorGPIOEvents.Initialize();
            m_Environment.ProcessorGPIOEvents.InitializeProcessEvents();

            m_Environment.StartConnectors();

            base.OnNavigatingFrom(e);

        }
        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.Parameter is FingerSensorsApp.App)
            {

                FingerSensorsApp.App AppEnvironment = e.Parameter as FingerSensorsApp.App;


                if (AppEnvironment != null)
                {
                    m_Environment = AppEnvironment.Environment;
                    m_Connector_SEN0188 = m_Environment.SensorConnector;
                    m_Sensoroutputconfigoptions = m_Environment.SensorOutPutServiceConnectorConfig;
                    m_Sensorinputconfigoptions = m_Environment.SensorInputServiceConnectorConfig;
                    m_SettingsToStorage = AppEnvironment.SettingsToStorage;

                    m_FingertEventDatabase = m_Environment.FingertEventDatabase;
                    m_SEN0188SQLite = m_Environment.SEN0188SQLite;
                    m_GPIOEnvironmentConnectors = m_Environment.GPIOEnvironmentConnectors;



                    m_Environment.StopConnectors();



                }
            }
            base.OnNavigatedTo(e);
        }

        private void StopConnector()
        {


            m_Environment.SensorStopConnector();



        }

        private void StartConnector()
        {
            m_Environment.SensorStartConnector();

        
        }



        private  void comPortInput_Click(object sender, RoutedEventArgs e)
        {
            var selection = ConnectDevices.SelectedItem;
            SerDevice serDev = selection as SerDevice;

            if (selection==null)
            {
                status.Text = "Select a device and connect";
                return;
            }
            this.m_Environment.SerDev.Id = serDev.Id;
            StartConnector();


        }

        private void AccessCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


               ComboBox combo = sender as ComboBox;

                if (combo == null) return;

                GPIOObject GPIOObj = combo.Tag as GPIOObject;
                if (GPIOObj != null)
                {
                    DBDataSetAccessBit selected = combo.SelectedItem as DBDataSetAccessBit;
                    if (selected != null)
                    {
                      GPIOObj.EventAccessRights = selected.BitValue;
                    }

                }


        }

        private void AccessCombo_Loaded(object sender, RoutedEventArgs e)
        {


            ComboBox combo = sender as ComboBox;

            if (combo == null) return;
            combo.ItemsSource = m_AccessBitsCollection;

            GPIOObject GPIOObj = combo.Tag as GPIOObject;
            if (GPIOObj != null)
            {
                int idx = DBDataSetAccessRight.getBitNumberByAccessBits(GPIOObj.EventAccessRights);
                combo.SelectedIndex = idx;

            }

        }


        private void OnKeyUpHandler(object sender, KeyRoutedEventArgs e)
        {
            TextBox TeBox = sender as TextBox;
            if (TeBox != null)
            {

                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
                    e.Handled = true;
                }
            }
            base.OnKeyUp(e);
        }


        private void InitFingerDataBase_Click(object sender, RoutedEventArgs e)
        {
            m_SEN0188SQLite.dropTable();
            m_SEN0188SQLite.InitializeDatabase();
        }

        private void InitEventDataBase_Click(object sender, RoutedEventArgs e)
        {
            m_FingertEventDatabase.dropTable();
            m_FingertEventDatabase.InitializeDatabase();
        }

        private void ImportDataBase_Click(object sender, RoutedEventArgs e)
        {




            FileCopy.ImportDataBaseToLocalFolder();

        }

        private void ExportDataBase_Click(object sender, RoutedEventArgs e)
        {
         
            FileCopy.ExportFingerDataBaseToLocalFolder();

        }


        private void closeDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StopConnector();
                status.Text = "";

                ListAvailablePorts();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
        }

        private async void ListAvailablePorts()
        {
            try
            {
                m_listOfDevices.Clear();
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
             
                status.Text = "Select a device and connect";

                for (int i = 0; i < dis.Count; i++)
                {
                    SerDevice serSev = new SerDevice(dis[i].Id);
                    m_listOfDevices.Add(serSev);
                }


            //    OpenDevice.IsEnabled = (m_listOfDevices.Count > 0);

            //    closeDevice.IsEnabled = false;
                ConnectDevices.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
        }

        private void ButtonAddConnector(object sender, RoutedEventArgs e)
        {
            m_Environment.GPIOEnvironmentConnectors.addConnector();

        }

        private void ButtonDeleteConnecor(object sender, RoutedEventArgs e)
        {
            GPIOEnvironmentConnector toDelete = PivotSocketListener.SelectedItem as GPIOEnvironmentConnector;
            if (toDelete != null)
            {
                m_Environment.GPIOEnvironmentConnectors.deleteConnector(toDelete);
            }

        }

    }
}
