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

        private StationEnvironment m_Environment;


        SEN0188SQLite m_SEN0188SQLite;
        FingertEventDatabase m_FingertEventDatabase;
        Connector_SEN0188 m_Connector_SEN0188;
        //SerDevice m_serDev;
        Windows.Foundation.Collections.PropertySet m_Sensoroutputconfigoptions;
        Windows.Foundation.Collections.PropertySet m_Sensorinputconfigoptions;


        ObservableCollection<FingerEvent> m_DataSets;

        GPIOEnvironmentConnectors m_GPIOEnvironmentConnectors;

        ProcessorGPIOEvents m_ProcessorGPIOEvents;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();

            m_SEN0188SQLite = null;
            m_FingertEventDatabase = null;
            m_Sensoroutputconfigoptions = null;
            m_Sensorinputconfigoptions = null;

            m_DataSets = null;
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



        public IList<FingerEvent> FingerEventsDataSets
        {
            get { return m_DataSets; }

        }




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

                    m_Connector_SEN0188 = m_Environment.SensorConnector;
                    m_Sensoroutputconfigoptions = m_Environment.SensorOutPutServiceConnectorConfig;
                    m_Sensorinputconfigoptions = m_Environment.SensorInputServiceConnectorConfig;

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
                    bool oldValue = (GPIOObj.SetValue > 0)  ? true : false;

                    if (oldValue != tgl.IsOn)
                    {
                        GPIOObj.SetValue = tgl.IsOn ? 1 : 0;
                        GPIOObj.IsFlankActive = true;
                        con.UpdateInputPropertySets(GPIOObj);
                        GPIOObj.IsFlankActive = false;
                    }

                }

            }

        }


        void stopRecording_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StopConnectors();
        }


        void startRecording_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StartConnectors();
        }



        void resetAllOutputs_Click(Object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // alle Ausgänge auf den Init-State setzen
            this.m_Environment.GPIOEnvironmentConnectors.resetAllOutputs();
        }

    
    }
}
