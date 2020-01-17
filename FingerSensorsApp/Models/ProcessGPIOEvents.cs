using FingerPrintSensor_SEN0188;
using FingerSensorsApp.Helpers;
using GPIOServiceConnector;
using SEN0188_SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace FingerSensorsApp.Models
{


    public class GPIOObjectProcess
    {
        GPIOObject m_GPIOObject;
        GPIOEnvironmentConnector m_GPIOEnvironmentConnector;
        public GPIOObjectProcess()
        {
            m_GPIOObject = null;
            m_GPIOEnvironmentConnector = null;

        }

        public GPIOEnvironmentConnector GPIOEnvironmentConnector
        {
            get
            {
                return m_GPIOEnvironmentConnector;
            }
            set
            {
                m_GPIOEnvironmentConnector = value;
            }
        }

        public GPIOObject GPIOObject
        {
            get
            {
                return m_GPIOObject;
            }
            set
            {
                m_GPIOObject = value;
            }
        }

    }

    public class ProcessGPIOEvents
    {
        enum inputState
        {
            waitforInitFlank,
            waitforExecuteFlank,
        }
        private List<GPIOObjectProcess> m_GPIOInputs;

        private List<GPIOObjectProcess> m_GPIOOutputs;

        string m_Ident;
        ulong m_AccessRights;

        inputState m_InputState;

        GPIOEnvironmentConnector m_GPIOEnvironmentConnector;
        GPIOObject m_LookFor;
        long m_FlankTicks;
        public ProcessGPIOEvents(string Ident)
        {
            m_Ident = Ident;
            m_GPIOInputs = new List<GPIOObjectProcess>();
            m_GPIOOutputs = new List<GPIOObjectProcess>();
            m_AccessRights = 0;
            m_GPIOEnvironmentConnector = null;
            m_InputState = inputState.waitforInitFlank;
            m_LookFor = null;
            m_FlankTicks = 0;
        }

        public ProcessGPIOEvents( ProcessGPIOEvents right)
        {
            m_Ident = right.m_Ident;
            m_GPIOInputs = new List<GPIOObjectProcess>(right.m_GPIOInputs);
            m_GPIOOutputs = new List<GPIOObjectProcess>(right.m_GPIOOutputs);
            m_AccessRights = right.m_AccessRights;
            m_GPIOEnvironmentConnector = right.m_GPIOEnvironmentConnector;
            m_InputState = right.m_InputState;
            m_FlankTicks = right.m_FlankTicks;
        }


        public GPIOEnvironmentConnector GPIOEnvironmentConnector
        {
            get
            {
                return m_GPIOEnvironmentConnector;
            }
            set
            {
                m_GPIOEnvironmentConnector = value;
            }
        }



        public long FlankTicks
        {
            get
            {
                return m_FlankTicks;
            }

        }

        public string Ident
        {
            get
            {
                return m_Ident;
            }
            set
            {
                m_Ident = value;
            }
        }

        public ulong AccessRights
        {
            get
            {
                return m_AccessRights;
            }
            set
            {
                m_AccessRights = value;
            }
        }

        /*
        public GPIOObjectProcess GPIOInput
        {
            get
            {
                return m_GPIOInput;
            }


        }
        */

        public IList<GPIOObjectProcess> GPIOInputs
        {
            get
            {
                return m_GPIOInputs;
            }


        }
        public IList<GPIOObjectProcess> GPIOOutputs
        {
            get
            {
                return m_GPIOOutputs;
            }


        }

        public int getInitFlank()
        {
            // oder verknüfung
            int ret = 0;
            foreach (var GPIOInput in m_GPIOInputs)
            {
                bool activ = false;
                GPIOObject obj = GPIOInput.GPIOObject;
                if (obj.PinValue != obj.InitValue)
                {
                    if ((obj.InitValue == 0) && (obj.PinValue == 1)) activ = true;
                    else if ((obj.InitValue == 1) && (obj.PinValue == 0)) activ = true;
                }

                if (activ && !obj.IsFlankActive)
                {
                    long aktTicks = Environment.TickCount;
                    long span = aktTicks - m_FlankTicks;
                    if (span >= 300) // msec
                    {
                        m_FlankTicks = Environment.TickCount;
                        obj.IsFlankActive = true;
                        m_LookFor = obj;
                        ret = 1;
                    }
                    //else
                    //{
                    //    bool d = true;
                    //}

                }
                else if (obj.IsFlankActive)
                {
                    if ((obj.InitValue == 0) && (obj.PinValue == 0)) obj.IsFlankActive = false;
                    else if ((obj.InitValue == 1) && (obj.PinValue == 1)) obj.IsFlankActive = false;
                }
            }
            return ret;
        }
        /*
        public int getExecuteFlank()
        {
            int retValue = 0;
            if (m_LookFor == null) return -1;


            if ((m_LookFor.InitValue == 0) && (m_LookFor.PinValue == 0)) retValue = 1;
            else if ((m_LookFor.InitValue == 1) && (m_LookFor.PinValue == 1)) retValue = 1;



            return retValue;


        }
        */
        public bool InputActiv()
        {
            switch (m_InputState)
            {
                case inputState.waitforInitFlank:
                    if (getInitFlank() == 1)
                    {
                      //  m_InputState = inputState.waitforExecuteFlank;
                        return true;
                    }
                    break;
                    /*
                case inputState.waitforExecuteFlank:
                    {
                        int ece = getExecuteFlank();
                        if (ece == 1)
                        {
                            m_InputState = inputState.waitforInitFlank;

                            m_LookFor = null;
                            return true;
                        }
                        else if (ece == -1)
                        {
                            m_InputState = inputState.waitforInitFlank;
                            m_LookFor = null;
                            return false;
                        }
                    }

                    break;
                    */
            }


            return false;


        }



        public bool ProcessOutput()
        {
            bool ret = false;

            for (int i = 0; i < GPIOOutputs.Count; i++)
            {

//                if (GPIOOutputs[i].GPIOObject.SetValue == GPIOOutputs[i].GPIOObject.InitValue)
                {
                    GPIOOutputs[i].GPIOObject.SetValue = (GPIOOutputs[i].GPIOObject.InitValue > 0) ? 0 : 1;
                    GPIOOutputs[i].GPIOObject.IsFlankActive = true;
                    GPIOOutputs[i].GPIOEnvironmentConnector.UpdateInputPropertySets(GPIOOutputs[i].GPIOObject);
                    GPIOOutputs[i].GPIOObject.IsFlankActive = false;
                    ret = true;

                }
            }

            return ret;

        }

        public bool UpdateState(int state)
        {

            for (int i = 0; i < GPIOOutputs.Count; i++)
            {
                GPIOOutputs[i].GPIOEnvironmentConnector.UpdateState(state);
            }

            return true;

        }

    }



    public class ProcessorGPIOEvents
    {

        private Queue<ProcessGPIOEvents> m_EventQueue;

        Connector_SEN0188 m_Connector_SEN0188;

        GPIOEnvironmentConnectors m_GPIOEnvironmentConnectors;
        ConfigProcessItems m_ConfigProcessItems;

        SEN0188SQLite m_SEN0188SQLite;
        FingertEventDatabase m_FingertEventDatabase;


        private StationEnvironment m_Environment;

        private string m_SensorID;
        //private bool m_SensorInitialized;

        //   public event Windows.Foundation.TypedEventHandler<Object, int > NotifyEvent;

        public event Windows.Foundation.TypedEventHandler<Object, FingerEvent> NotifyEvent;

        private List<ProcessGPIOEvents> m_ProcessGPIOEvents;

        public ProcessorGPIOEvents(StationEnvironment Environment)
        {
            m_EventQueue = new Queue<ProcessGPIOEvents>();


            m_Environment = Environment;
            m_Connector_SEN0188 = m_Environment.SensorConnector;

            m_ConfigProcessItems = m_Environment.ConfigProcessItems;

            m_GPIOEnvironmentConnectors = m_Environment.GPIOEnvironmentConnectors;

            m_SEN0188SQLite = m_Environment.SEN0188SQLite;

            m_FingertEventDatabase = m_Environment.FingertEventDatabase;

            m_SensorID = "";
            //     m_SensorInitialized = false;
            m_ProcessGPIOEvents = new List<ProcessGPIOEvents>();

        }


        public async Task<bool> createProcessEvents()
        {


            var t = await Task.Run(() =>
            {

                return m_ConfigProcessItems.createProcessEvents(m_ProcessGPIOEvents);

            });

            return t;

        }

        public ProcessGPIOEvents getProcessGPIOEventsByIdent(string Ident)
        {
            for (int i = 0; i < m_ProcessGPIOEvents.Count; i++)
            {
                if (String.Compare(m_ProcessGPIOEvents[i].Ident, Ident, true) == 0)

                // if (m_ProcessGPIOEvents[i].Ident == Ident)
                {
                    return m_ProcessGPIOEvents[i];
                }

            }
            return null;

        }
        public async void InitializeProcessEvents()
        {

            var ret = await createProcessEvents();

            m_EventQueue.Clear();

        }

        public void Initialize()
        {


            if (m_Environment.ConnectorSEN0188Enable)
            {
                m_Connector_SEN0188.startStreaming += Connector_SEN0188_startStreaming;
                m_Connector_SEN0188.stopStreaming += Connector_SEN0188_stopStreaming;

                m_Connector_SEN0188.Failed += Connector_SEN0188_Failed;

                m_Connector_SEN0188.NotifyChangeState += Connector_SEN0188_NotifyChangeState;
            }


            for (int i = 0; i < m_GPIOEnvironmentConnectors.EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = m_GPIOEnvironmentConnectors.EnvironmentConnectors[i];
                if (con.GPIOConnectorEnable)
                {
                    con.GPIOConnector.ChangeGPIOs += GPIOConnector_ChangeGPIOs;
                    con.GPIOConnector.startStreaming += GPIOConnector_startStreaming;
                    con.GPIOConnector.stopStreaming += GPIOConnector_stopStreaming;
                    con.GPIOConnector.Failed += GPIOConnector_stopStreaming;

                }

            }


        }



        public void DeInitialize()
        {
            //     m_Connector_SEN0188.NotifyChangeState -= Connector_SEN0188_NotifyChangeState;

            m_Connector_SEN0188.startStreaming -= Connector_SEN0188_startStreaming;
            m_Connector_SEN0188.stopStreaming -= Connector_SEN0188_stopStreaming;
            m_Connector_SEN0188.NotifyChangeState -= Connector_SEN0188_NotifyChangeState;
            m_Connector_SEN0188.Failed -= Connector_SEN0188_Failed;


            for (int i = 0; i < m_GPIOEnvironmentConnectors.EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = m_GPIOEnvironmentConnectors.EnvironmentConnectors[i];
                if (con.GPIOConnectorEnable)
                {
                    con.GPIOConnector.ChangeGPIOs -= GPIOConnector_ChangeGPIOs;
                    con.GPIOConnector.startStreaming -= GPIOConnector_startStreaming;
                    con.GPIOConnector.stopStreaming -= GPIOConnector_stopStreaming;
                    con.GPIOConnector.Failed -= GPIOConnector_stopStreaming;
                }

            }

        }

        async private void Connector_SEN0188_Failed(object sender, string args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            { // your code should be here
                m_Environment.SensorInitialized = false;
                m_Environment.SensorConnecorInitialized = false;
                await m_Connector_SEN0188.stopProcessingPackagesAsync();
                var msg = "Serial Device: " + args;
                var messageDialog = new MessageDialog(msg);
                await messageDialog.ShowAsync();
            });

            //  throw new NotImplementedException();
        }
        async private void Connector_SEN0188_stopStreaming(object sender, string args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            { // your code should be here
                m_Environment.SensorInitialized = false;
                m_Environment.SensorConnecorInitialized = false;
                await m_Connector_SEN0188.stopProcessingPackagesAsync();
                // await m_Connector_SEN0188.stopProcessingPackagesAsync();


            });
        }

        async private void Connector_SEN0188_startStreaming(object sender, SerDevice args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                m_Environment.SensorConnecorInitialized = true;

                m_Environment.SensorInitialized = false;

            });
        }
        async private void GPIOConnector_startStreaming(object sender, Windows.Networking.Sockets.StreamSocket args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            { // your code should be here

                GPIOConnector GPIOcon = sender as GPIOConnector;

                GPIOEnvironmentConnector con = this.m_GPIOEnvironmentConnectors.getGPIOOConnectorByGPIOConnector(GPIOcon);
                if (con != null)
                {
                    con.GPIOConnecorInitialized = true;
                }

                m_Environment.GPIOConnecorInitialized = m_GPIOEnvironmentConnectors.GPIOConnecorInitialized;


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

                    m_Environment.GPIOConnecorInitialized = m_GPIOEnvironmentConnectors.GPIOConnecorInitialized;
                    //
                    con.stopConnector();

                    if (args.Length > 0)
                    {
                        var messageDialog = new MessageDialog(args);
                        await messageDialog.ShowAsync();
                    }


                });
            }


        }

        async private void GPIOConnector_ChangeGPIOs(object sender, IPropertySet propertys)
        {



            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            { // your code should be here


                m_GPIOEnvironmentConnectors.ProcessPropertysFromGPIOConnector(propertys);

                var con = m_GPIOEnvironmentConnectors.getGPIOOConnectorByOutputPropertySet(propertys);

                if (con == null) return;

                if (m_ProcessGPIOEvents == null) return;

                for (int i = 0; i < m_ProcessGPIOEvents.Count; i++)
                {
                    ProcessGPIOEvents item = m_ProcessGPIOEvents[i];
                    if (item.InputActiv())
                    {
   
                        removeOldEvents(); // ältere löschen, welche nach 5 sec. nicht beantwortet waren

                        if (item.AccessRights > 0)
                        {

                            if (m_Environment.SensorConnecorInitialized)
                            {
                                ProcessGPIOEvents Processitem = new ProcessGPIOEvents(item);
                                m_EventQueue.Enqueue(Processitem);
                                SensorCMDs.VerifyFingerId(m_Environment.SensorInputServiceConnectorConfig);
                            }
                            else
                            {
                                string cmdState = "Fingerprint Connector not initialized!";
                                int state = -1;
                                FingerEvent eventSet = createSensorEvent("John", "Doe", -1, -1, item.Ident, state, cmdState);
                                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                                eventSet.SensorId = enc.GetBytes(m_SensorID);
                                bool insert = m_FingertEventDatabase.InsertFingerEvent(eventSet);
                                NotifyEvent?.Invoke(this, eventSet);
                            }

                        }
                        else
                        {
                            item.UpdateState(0);
                            item.ProcessOutput();
                            item.UpdateState(1);
                            int state = -1;
                            string cmdState = "no FingerSensor used";
                            FingerEvent eventSet = createSensorEvent("John", "Doe", -1, -1, item.Ident, state, cmdState);
                            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                            eventSet.SensorId = enc.GetBytes(m_SensorID);
                            bool insert = m_FingertEventDatabase.InsertFingerEvent(eventSet);
                            NotifyEvent?.Invoke(this, eventSet);
                        }

                    }

                }


            });


        }

        FingerEvent createSensorEvent(string FirstName, string SecondName, int MatchScore, int fingerId, string fingerType, int sensorState, string cmdsensorState)
        {

            FingerEvent eventSet = new FingerEvent();
            eventSet.FirstName = FirstName;
            eventSet.SecondName = SecondName;
            eventSet.MatchScore = MatchScore;
            eventSet.FingerID = fingerId;
            eventSet.EventType = fingerType;
            eventSet.SensorState = sensorState;
            eventSet.SensorTxtState = cmdsensorState;
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            eventSet.SensorId = enc.GetBytes(m_SensorID);
            return eventSet;
        }



        void removeOldEvents()
        {
            if (m_EventQueue.Count == 0) return;

          
            long aktTicks = Environment.TickCount;

            ProcessGPIOEvents ev;
            while (m_EventQueue.Count>0)
            {
                ev = m_EventQueue.Peek();
                long span = aktTicks - ev.FlankTicks;
                if (span >= 4500) // alle löschen, welche nach > 4500 msec. nicht beantwortet waren
                {
                  m_EventQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }

        }


        void Update_SEN0188_NotifyChangeState(IPropertySet Outputpropertys)
        {


            ProcessGPIOEvents ev;

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
                            m_Environment.SensorInitialized = true;
                            //   m_SensorInitialized = true;
                            doactFilledSensorId = true;
                        }
                    }
                    else if (CMD == Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerAutoVerifiying"))
                    {
                        if (m_EventQueue.Count > 0)
                        {
                           ev = m_EventQueue.Dequeue(); // get Event
                        }
                        else return;

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
                                            FingerEvent eventSet;
                                            if (dataset.AccessRights_Bit0 || (dataset.AccessRights & ev.AccessRights) != 0) // Master Bit
                                            {

                                                ev.UpdateState(0); // update inativ setzen

                                                ev.ProcessOutput();

                                                cmdState = String.Format("Permission to Access: {0}", ev.Ident);
                                                eventSet = createSensorEvent(dataset.FirstName, dataset.SecondName, MatchScore, fingerId, ev.Ident, state, cmdState);

                                                ProcessGPIOEvents evOk = getProcessGPIOEventsByIdent("State_OK");
                                                if (evOk != null)
                                                {
                                                    evOk.ProcessOutput();
                                                }
                                                ev.UpdateState(1); // update aktiv setzen

                                            }
                                            else
                                            {
                                                cmdState = String.Format("no Permission to Access: {0}", ev.Ident);
                                                state = -2;
                                                eventSet = createSensorEvent(dataset.FirstName, dataset.SecondName, MatchScore, fingerId, ev.Ident, state, cmdState);

                                                ProcessGPIOEvents evNoPermiss = getProcessGPIOEventsByIdent("State_NoPermiss");
                                                if (evNoPermiss != null)
                                                {
                                                    evNoPermiss.UpdateState(0); // update inativ setzen
                                                    evNoPermiss.ProcessOutput();
                                                    evNoPermiss.UpdateState(1); // update aktiv setzen
                                                }


                                            }

                                            bool insert = m_FingertEventDatabase.InsertFingerEvent(eventSet);
                                            NotifyEvent?.Invoke(this, eventSet);
                                        }
                                    }
                                }


                            }
                        }
                        else
                        {
                            FingerEvent eventSet = createSensorEvent("John", "Doe", -1, -1, ev.Ident, state, cmdState);
                            ProcessGPIOEvents evStateError = getProcessGPIOEventsByIdent("State_Error");
                            if (evStateError != null)
                            {
                                evStateError.UpdateState(0); // update inativ setzen
                                evStateError.ProcessOutput();
                                evStateError.UpdateState(1); // update aktiv setzen
                            };

                            bool insert = m_FingertEventDatabase.InsertFingerEvent(eventSet);
                            NotifyEvent?.Invoke(this, eventSet);
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
            //Update_SEN0188_NotifyChangeState(args);


            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            { // your code should be here

                Update_SEN0188_NotifyChangeState(args);


            });


        }

        public bool AddEvent(ProcessGPIOEvents ev)
        {
            m_EventQueue.Enqueue(ev);

            return true;

        }

        public bool ProcessEvent()
        {
            ProcessGPIOEvents ev = m_EventQueue.Dequeue();


            return false;

        }

    }


}
