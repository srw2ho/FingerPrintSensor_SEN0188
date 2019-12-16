using FingerPrintSensor_SEN0188;
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

    public class ProcessFingerEvent
    {

        private List<GPIOObjectProcess> m_GPIOInputs;
        private List<GPIOObjectProcess> m_GPIOOutputs;
        int m_FingerID;

        PropertySet m_Sensorinputconfigoptions;
        PropertySet m_Sensoroutputconfigoptions;
        ulong m_AccessRights;

        GPIOEnvironmentConnector m_GPIOEnvironmentConnector;
        public ProcessFingerEvent()
        {
            m_GPIOInputs = new List<GPIOObjectProcess>();
            m_GPIOOutputs = new List<GPIOObjectProcess>();
            m_AccessRights = 0;
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


        public bool ProcessEvent()
        {

            return false;

        }

    }



    public class ProcessprFingerEvents
    {

        private Queue<ProcessFingerEvent> m_EventQueue;

        Connector_SEN0188 m_Connector_SEN0188;

        GPIOEnvironmentConnectors m_GPIOEnvironmentConnectors;
        ConfigProcessItems m_ConfigProcessItems;

        SEN0188SQLite m_SEN0188SQLite;
        FingertEventDatabase m_FingertEventDatabase;
        private List<string> m_GPIOInputs;

        private StationEnvironment m_Environment;

        string m_SensorID;



        public ProcessprFingerEvents(StationEnvironment Environment)
        {
            m_EventQueue = new Queue<ProcessFingerEvent>();



            m_Environment = Environment;
            m_Connector_SEN0188 = m_Environment.SensorConnector;

            m_ConfigProcessItems = m_Environment.ConfigProcessItems;

            m_GPIOEnvironmentConnectors = m_Environment.GPIOEnvironmentConnectors;

            m_SEN0188SQLite = m_Environment.SEN0188SQLite;

            m_FingertEventDatabase = m_Environment.FingertEventDatabase;

            m_Connector_SEN0188.NotifyChangeState += Connector_SEN0188_NotifyChangeState;

            for (int i = 0; i < m_GPIOEnvironmentConnectors.EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = m_GPIOEnvironmentConnectors.EnvironmentConnectors[i];
                if (con.GPIOConnectorEnable)
                {
                    con.GPIOConnector.ChangeGPIOs += GPIOConnector_ChangeGPIOs;
                }

            }

            m_SensorID = "";
        }




       ~ProcessprFingerEvents()
        {
            m_Connector_SEN0188.NotifyChangeState -= Connector_SEN0188_NotifyChangeState;

            for (int i = 0; i < m_GPIOEnvironmentConnectors.EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = m_GPIOEnvironmentConnectors.EnvironmentConnectors[i];
                if (con.GPIOConnectorEnable)
                {
                    con.GPIOConnector.ChangeGPIOs -= GPIOConnector_ChangeGPIOs;
                }

            }
        }


        async private void GPIOConnector_ChangeGPIOs(object sender, IPropertySet propertys)
        {

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            { // your code should be here

                m_GPIOEnvironmentConnectors.ProcessPropertysFromGPIOConnector(propertys);

                var con = m_GPIOEnvironmentConnectors.getGPIOOConnectorByOutputPropertySet(propertys);

                if (con == null) return;


                GPIOOBank inputbank = con.ActiveInputs;
                for (int i = 0; i < m_ConfigProcessItems.ProcessItems.Count; i++)
                {
                    ConfigProcessItem item = m_ConfigProcessItems.ProcessItems[i];
                    for (int j= 0; j <  item.GPIOInputProcessItems.Count; j++)
                    {

                        GPIOOProcessItem pItem = item.GPIOInputProcessItems[j];

                        GPIOObject obj = inputbank.getGPIOByName(pItem.GPIOName);
                        if (obj != null)
                        {
                            if (obj.PinValue != obj.InitValue)
                            {


                                ProcessFingerEvent ev = new ProcessFingerEvent();
                                GPIOObjectProcess OutObjectProcess = new GPIOObjectProcess();
                                GPIOObjectProcess InObjectProcess = new GPIOObjectProcess();

                           
                                //item.GPIOOutputProcessItems
                                OutObjectProcess.GPIOEnvironmentConnector = m_GPIOEnvironmentConnectors.getGPIOOConnectorByHostName(pItem.ConnectorName);
                                OutObjectProcess.GPIOObject = obj;

                                InObjectProcess.GPIOEnvironmentConnector = con;
                                InObjectProcess.GPIOObject = obj;

                                ev.GPIOOutputs.Add(OutObjectProcess);
                                ev.GPIOInputs.Add(InObjectProcess);
                                ev.AccessRights = 0x1;

                                m_EventQueue.Enqueue(ev);


                            }

                        }


                    }


                }

            });

        }

        void Update_SEN0188_NotifyChangeState(IPropertySet Outputpropertys)
        {

            ProcessFingerEvent ev = m_EventQueue.Dequeue(); // get Event

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
                    //        SensorInitialized = true;
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
                         
                                            if (dataset.AccessRights_Bit0 || (dataset.AccessRights & ev.AccessRights) !=0) // Master Bit
                                            {
                                

                                                for (int i = 0; i < ev.GPIOOutputs.Count;i++)
                                                {
                           //                         ev.GPIOOutputs[i].GPIOEnvironmentConnector.UpdateState(0);
                                                    ev.GPIOOutputs[i].GPIOObject.SetValue = (ev.GPIOOutputs[i].GPIOObject.InitValue > 0) ? 1:0;
                                                    ev.GPIOOutputs[i].GPIOEnvironmentConnector.UpdateInputPropertySets(ev.GPIOOutputs[i].GPIOObject);
                                                }
              
                                                FingerEvent eventSet = new FingerEvent();
                                                eventSet.FirstName = dataset.FirstName;
                                                eventSet.SecondName = dataset.SecondName;
                                                eventSet.MatchScore = MatchScore;
                                                eventSet.FingerID = fingerId;
                                                eventSet.EventType = 0;
                                                eventSet.SensorState = state;
                                                eventSet.SensorTxtState = cmdState;
                                                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                                                eventSet.SensorId = enc.GetBytes(m_SensorID);

                                                bool insert = m_FingertEventDatabase.InsertFingerEvent(eventSet);

                                                ConfigProcessItem outstate = m_ConfigProcessItems.getConfigItemByIdent("State_OK");
                                                if (outstate != null)
                                                {
                                                    for (int i = 0; i < outstate.GPIOOutputProcessItems.Count; i++)
                                                    {
//                                                        outstate.GPIOOutputProcessItems[i].GPIOName


                                                    }
                                                }
  

                                                //             GetEventData();

                                            }
                                            else
                                            {
                                                ConfigProcessItem outstate = m_ConfigProcessItems.getConfigItemByIdent("State_NoPermiss");
                                                if (outstate != null)
                                                {
                                                    for (int i = 0; i < outstate.GPIOOutputProcessItems.Count; i++)
                                                    {
                                                        //                                                        outstate.GPIOOutputProcessItems[i].GPIOName


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
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            { // your code should be here

                Update_SEN0188_NotifyChangeState(args);


            });

        }

        public bool AddEvent(ProcessFingerEvent ev)
        {
            m_EventQueue.Enqueue(ev);

            return true;

        }

        public bool ProcessEvent()
        {
            ProcessFingerEvent ev = m_EventQueue.Dequeue();


            return false;

        }

    }


}
