using FingerSensorsApp.Models;
using SEN0188_SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
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
    public sealed partial class EventDashboard : Page, INotifyPropertyChanged
    {
        private ObservableCollection<FingerEvent> m_DataSets;
       // m_DataSets = new ObservableCollection<FingerEvent>();

        private StationEnvironment m_Environment;
        private FingertEventDatabase m_FingertEventDatabase;
        public event PropertyChangedEventHandler PropertyChanged;

        ThreadPoolTimer m_PeriodicTimerFingerEvents;


        bool m_DisplayMissingEvents;

        public EventDashboard()
        {
            this.InitializeComponent();
            m_FingertEventDatabase = null;
            m_FingertEventDatabase = null;
            m_DataSets = new ObservableCollection<FingerEvent>();
            m_PeriodicTimerFingerEvents = null;
            m_DisplayMissingEvents = false;
        }


        public bool DisplayMissingEvents
        {
            get { return m_DisplayMissingEvents; }
            set
            {
                Set(ref m_DisplayMissingEvents, value);
            }


        }

        public ObservableCollection<FingerEvent> FingerEventsDataSets
        {
            get { return m_DataSets; }
            set
            {
                Set(ref m_DataSets, value);
            }

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

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            
            base.OnNavigatingFrom(e);
            stoppDeleteFingerEventsTimer();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is FingerSensorsApp.App)
            {

                FingerSensorsApp.App AppEnvironment = e.Parameter as FingerSensorsApp.App;


                if (AppEnvironment != null)
                {
                    m_Environment = AppEnvironment.Environment;

                    m_FingertEventDatabase = m_Environment.FingertEventDatabase;

                    // m_DataSets = m_FingertEventDatabase.DataSets;

                    _DisplayallEvents.IsChecked = true;
                    _DisplayallOKEvents.IsChecked = false;
                    _DisplayallMissingEvents.IsChecked = false;
                    GetEventData();// Get Event Data
                    TimeSpan period = TimeSpan.FromSeconds(10); // alle 10 sec neue Daten anzeigen
                    startFingerEventsTimer(period);

                }


            }
            base.OnNavigatedTo(e);
        }


        public void startFingerEventsTimer(TimeSpan period)
        {

            if (m_PeriodicTimerFingerEvents != null) return;

            m_PeriodicTimerFingerEvents = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                // alle Daten anzeigen, welche 14 Tage zurückliegen
                DateTime dateTime = DateTime.Now;
                TimeSpan sp = -TimeSpan.FromDays(m_Environment.EventHistoryinDays);
                dateTime = dateTime.AddTicks(sp.Ticks);
     
                this.GetEventData();

            }, period);
        }

        public void stoppDeleteFingerEventsTimer()
        {
            if (m_PeriodicTimerFingerEvents == null) return;
            m_PeriodicTimerFingerEvents.Cancel();
            m_PeriodicTimerFingerEvents = null;


        }
        private void DisplayEvent_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.FingerEventsDataSets.Clear();
            GetEventData();
        }


        async void GetEventData()
        {

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            { // your code should be here
                DateTime dateTime = DateTime.Now;
                TimeSpan sp = -TimeSpan.FromDays(m_Environment.EventHistoryinDays);
                dateTime = dateTime.AddTicks(sp.Ticks);
                ObservableCollection<FingerEvent> dataSets = new ObservableCollection<FingerEvent>();



                if (_DisplayallEvents.IsChecked==true)
                {
                    m_FingertEventDatabase.GetDataSetsGreaterThanDateTime(dateTime, dataSets);
                }

                if (_DisplayallOKEvents.IsChecked==true)
                {
                    m_FingertEventDatabase.GetDataSetsGreaterThanDateTimeByIdenSensorState(dateTime, dataSets, 0);
                }


                if (_DisplayallMissingEvents.IsChecked==true)
                {
                    m_FingertEventDatabase.GetDataSetsGreaterThanDateTimeByNotIdenSensorState(dateTime, dataSets, 0);
                }

  
                if (dataSets.Count() != this.m_DataSets.Count())
                {
                    this.FingerEventsDataSets = dataSets;
                }
       
   
            });




        }
    }
}
