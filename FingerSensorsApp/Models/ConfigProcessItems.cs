using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FingerSensorsApp.Models
{
    /*
    public class GPIOOProcessItem
    {
        string m_ConnectorName;
        string m_GPIOName;
        ulong m_AccessRights;
        int m_ConnectorIDX;

        ulong m_EventType;

        public GPIOOProcessItem(string IdentName)
        {
            m_GPIOName = IdentName;

            m_ConnectorName = "";

            m_AccessRights = 0;
            m_ConnectorIDX = 0;
   
        }



        public int ConnectorIDX
        {
            get
            {
                return m_ConnectorIDX;
            }
            set
            {
                m_ConnectorIDX = value;
            }
        }

        public string ConnectorName
        {
            get
            {
                return m_ConnectorName;
            }
            set
            {
                m_ConnectorName = value;
            }
        }

        public string GPIOName
        {
            get
            {
                return m_GPIOName;
            }
            set
            {
                m_GPIOName = value;
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

    }


    public class ConfigProcessItem
    {

        string m_IdentName;
        private ObservableCollection<GPIOOProcessItem> m_GPIOInputProcessItems;

        private GPIOOProcessItem  m_GPIOInputProcessItem;

        private ObservableCollection<GPIOOProcessItem> m_GPIOOutputProcessItems;


        ulong m_AccessRights;

        ulong m_EventType;


        public ConfigProcessItem(string IdentName)
        {
            m_IdentName = IdentName;

            m_GPIOInputProcessItems = new ObservableCollection<GPIOOProcessItem>();
            m_GPIOOutputProcessItems = new ObservableCollection<GPIOOProcessItem>();
            m_GPIOInputProcessItem = new GPIOOProcessItem ("not set");
            m_AccessRights = 0;
            m_EventType = 0;
        }



        public ulong EventType
        {
            get
            {
                return m_EventType;
            }
            set
            {
                m_EventType = value;
            }
        }

        public string IdentName
        {
            get
            {
                return m_IdentName;
            }
            set
            {
                m_IdentName = value;
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


        public GPIOOProcessItem GPIOInputProcessItem
        {
            get
            {
                return m_GPIOInputProcessItem;
            }


        }

        public IList<GPIOOProcessItem> GPIOInputProcessItems
        {
            get
            {
                return m_GPIOInputProcessItems;
            }


        }

        public IList<GPIOOProcessItem> GPIOOutputProcessItems
        {
            get
            {
                return m_GPIOOutputProcessItems;
            }


        }


        public bool ProcessEvent()
        {

            return false;

        }

    }
    */
    public class ConfigProcessItems
    {
      //  private ObservableCollection<ConfigProcessItem> m_ProcessItems;


        GPIOEnvironmentConnectors m_GPIOEnvironmentConnectors;
        public ConfigProcessItems(GPIOEnvironmentConnectors environmentConnectors)
        {
        //    m_ProcessItems = new ObservableCollection<ConfigProcessItem>();
            m_GPIOEnvironmentConnectors = environmentConnectors;
        }
        /*
        public IList<ConfigProcessItem> ProcessItems
        {
            get
            {
                return m_ProcessItems;
            }


        }
        */
        public bool createProcessEvents(IList<ProcessGPIOEvents> processGPIOEvents)
        {
            processGPIOEvents.Clear();

            ProcessGPIOEvents processGPIOEvent;
            var map = new Dictionary<string, ProcessGPIOEvents>();

            foreach (GPIOEnvironmentConnector con in m_GPIOEnvironmentConnectors.EnvironmentConnectors)
            {
                var m_InOutBanks = con.ActiveGPIOInOutBanks.InOutBanks;
  
                for (int i = 0; i < m_InOutBanks.Count; i++)
                {
                    GPIOOBank bank = m_InOutBanks[i];

                    foreach (GPIOObjects OutPuts in bank.GPIOBanks)
                    {
                        foreach (GPIOObject GPIOObj in OutPuts.GPIOs)
                        {
                            if (GPIOObj.IsEnabled && GPIOObj.IsEventEnabled)
                            {
                                string eventName = GPIOObj.EventName;

                                eventName = Regex.Replace(eventName, @"\s", "");//remove with spaces
                           
                                string eventKey;

                                GPIOObject.GPIOTyp type = GPIOObj.GPIOtyp;
   
                                string[] array = eventName.Split(".");
                                if (array.Length > 1)
                                {
                                    eventKey = array[0];
                                    if (array[1].Equals("output", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        type = GPIOObject.GPIOTyp.output;
                                    }
                                    if (array[1].Equals("input", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        type = GPIOObject.GPIOTyp.input;
                                    }
                                }
                                else eventKey = eventName;

                                bool containsKey = map.TryGetValue(eventKey,out processGPIOEvent);

                                if (processGPIOEvent==null)
                                {
                                    processGPIOEvent = new ProcessGPIOEvents(eventKey);
                                    map[eventKey] = processGPIOEvent;
                                    //processGPIOEvent.AccessRights = GPIOObj.EventAccessRights;
                                    processGPIOEvents.Add(processGPIOEvent);
                                }

                                switch (type)
                                {
                                    case GPIOObject.GPIOTyp.output:
                                        {//output object determines Access rights
                                         //   GPIOOProcessItem gPIOOProcessItem = new GPIOOProcessItem(eventKey);
                                            GPIOObjectProcess OutObjectProcess = new GPIOObjectProcess();
                                            OutObjectProcess.GPIOEnvironmentConnector = con;
                                            OutObjectProcess.GPIOObject = GPIOObj;
                                            processGPIOEvent.AccessRights = GPIOObj.EventAccessRights;
                                            processGPIOEvent.GPIOOutputs.Add(OutObjectProcess);
                                            break;
                                        }

                                    case GPIOObject.GPIOTyp.input:
                                        {
                                          //  GPIOOProcessItem gPIOOProcessItem = new GPIOOProcessItem(eventKey);
                                            GPIOObjectProcess OutObjectProcess = new GPIOObjectProcess();
                                            OutObjectProcess.GPIOEnvironmentConnector = con;
                                            OutObjectProcess.GPIOObject = GPIOObj;
                                            processGPIOEvent.GPIOInputs.Add(OutObjectProcess);
                                            break;
                                        }


                                }
                            }

                        }

                    }
                }

            }
  
            return (processGPIOEvents.Count > 0);
        }
        /*
        public bool  createProcessEvents(IList<ProcessGPIOEvents> processGPIOEvents)
        {
            // List<ProcessGPIOEvents> processGPIOEvents = new List<ProcessGPIOEvents>();
            processGPIOEvents.Clear();
            for (int i = 0; i < ProcessItems.Count; i++)
            {
                ConfigProcessItem item = ProcessItems[i];

                GPIOOProcessItem inputItem = item.GPIOInputProcessItem;
                if (inputItem != null)
                {
                    var incon = m_GPIOEnvironmentConnectors.getGPIOOConnectorByHostName(inputItem.ConnectorName);
                    GPIOObject inobj = null;
                    if (incon != null)
                    {
                        GPIOOBank inputbank = incon.ActiveInputs;
                        inobj = inputbank.getGPIOByName(inputItem.GPIOName);
                        if (inobj == null)
                        {
                            inputbank = incon.ActiveOutPuts;
                            inobj = inputbank.getGPIOByName(inputItem.GPIOName);
                        }
                    }

                    if (inobj == null) continue;

                    ProcessGPIOEvents ev = new ProcessGPIOEvents(item.IdentName);

                    ev.GPIOInput.GPIOEnvironmentConnector = incon;
                    ev.GPIOInput.GPIOObject = inobj;

                    for (int k = 0; k < item.GPIOOutputProcessItems.Count; k++)
                    {

                        GPIOOProcessItem outputItem = item.GPIOOutputProcessItems[k];
                        //GPIOObjectProcess InObjectProcess = new GPIOObjectProcess();
                        //InObjectProcess.GPIOEnvironmentConnector = incon;
                        //InObjectProcess.GPIOObject = inobj;

                        //ev.GPIOInputs.Add(InObjectProcess);
                        var conoutput = m_GPIOEnvironmentConnectors.getGPIOOConnectorByHostName(outputItem.ConnectorName);

                        if (conoutput != null)
                        {
                            GPIOOBank outputbank = conoutput.ActiveOutPuts;
                            if (outputbank != null)
                            {
                                GPIOObject obj = outputbank.getGPIOByName(outputItem.GPIOName);
                                if (obj != null)
                                {
                                    GPIOObjectProcess OutObjectProcess = new GPIOObjectProcess();
                                    OutObjectProcess.GPIOEnvironmentConnector = conoutput;
                                    OutObjectProcess.GPIOObject = obj;
                                    ev.GPIOOutputs.Add(OutObjectProcess);
                                    ev.AccessRights = item.AccessRights;
                                    ev.Ident = item.IdentName;
                                }
                            }
                        }

                    }

                    if (ev.GPIOOutputs.Count > 0)
                    {
                         processGPIOEvents.Add(ev);
                    }

                }
            }
            return (processGPIOEvents.Count>0);
        }
 
        public ConfigProcessItem getConfigItemByIdent(string Ident)
        {

            for (int i = 0; i < m_ProcessItems.Count; i++)
            {

                if (m_ProcessItems[i].IdentName == Ident)
                {

                    return m_ProcessItems[i];
                }
            }
            return null;
        }
               */


    }
}
