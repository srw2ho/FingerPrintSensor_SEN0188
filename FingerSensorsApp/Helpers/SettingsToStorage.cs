
using FingerSensorsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace FingerSensorsApp.Helpers
{
    public class SettingsToStorage
    {
        private LocalStorageSettings m_LocalStorageSettings;

        private LocalStorageItem m_localStorage;
        private StationEnvironment m_StationEnvironment;


        public SettingsToStorage(FingerSensorsApp.App app)
        {

            m_LocalStorageSettings = new LocalStorageSettings("FingerSensorsAppAppEnvironment");
   
            m_localStorage = new LocalStorageItem("FingerSensorsAppStation");
            m_StationEnvironment = app.Environment;



        }


        public bool writeStationEnvironmenttoLocalStorage(StationEnvironment StationEnvironment , Windows.Storage.ApplicationDataCompositeValue composite, int ListenerIdx)
        {

            if (m_localStorage == null) return false;
            m_localStorage.SetSourceIDName("StationEnvironment", ListenerIdx);

            int Idx = -1;

            bool bok = m_localStorage.writeSettingsToLocalStorage(composite, Idx);


            bok = m_localStorage.writeIntegerSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.ConnectorSEN0188Enable", Idx), Convert.ToInt32(StationEnvironment.ConnectorSEN0188Enable));

            bok = m_localStorage.writeStringSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.Id", Idx), StationEnvironment.SerDev.Id);

            bok = m_localStorage.writeDoubleSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.BaudRate", Idx), StationEnvironment.SerDev.BaudRate);

            bok = m_localStorage.writeDoubleSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.WriteTimeout.TotalMilliseconds", Idx), (double) StationEnvironment.SerDev.WriteTimeout.TotalMilliseconds);

            bok = m_localStorage.writeDoubleSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.ReadTimeout.TotalMilliseconds", Idx), StationEnvironment.SerDev.ReadTimeout.TotalMilliseconds);

            bok = m_localStorage.writeDoubleSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.Parity", Idx), (double) StationEnvironment.SerDev.Parity);

            bok = m_localStorage.writeDoubleSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.StopBits", Idx), (double) StationEnvironment.SerDev.StopBits);


            bok = m_localStorage.writeDoubleSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.Handshake", Idx), (double)StationEnvironment.SerDev.Handshake);

            bok = m_localStorage.writeIntegerSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.EventHistoryinDays", Idx), StationEnvironment.EventHistoryinDays);


            


            return bok;

        }

        public bool writeGPIOStationEnvironmenttoLocalStorage(GPIOEnvironmentConnector StationEnvironment, Windows.Storage.ApplicationDataCompositeValue composite, int ListenerIdx)
        {

            if (m_localStorage == null) return false;

            m_localStorage.SetSourceIDName("StationEnvironment.GPIO", ListenerIdx);


            int Idx = -1;

            bool bok  = m_localStorage.writeSettingsToLocalStorage(composite, Idx);

            bok  = m_localStorage.writeStringSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.HostName", Idx), StationEnvironment.HostName);
            bok = m_localStorage.writeIntegerSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.Port", Idx), StationEnvironment.Port);

            bok = m_localStorage.writeIntegerSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.GPIOConnectorEnable", Idx), Convert.ToInt32(StationEnvironment.GPIOConnectorEnable));



            for (int i = 0; i < StationEnvironment.GPIOOInOutBanks.InOutBanks.Count; i++)
            {
                GPIOOBank bank = StationEnvironment.GPIOOInOutBanks.InOutBanks[i];

                foreach (GPIOObjects OutPuts in bank.GPIOBanks)
                {
                    foreach (GPIOObject GPIOObj in OutPuts.GPIOs)
                    {
                        string property = GPIOObj.getPropertyforStorageSettings();
                        bok = m_localStorage.writeStringSettingsToLocalStorage(composite, m_localStorage.getCompositePropertyIDName(GPIOObj.GPIOName, Idx), property) && bok;

                    }

                }
            }



            return bok;

        }

        public bool readStationEnvironmentDatafromLocalStorage(StationEnvironment StationEnvironment, Windows.Storage.ApplicationDataCompositeValue composite, int ListenerIdx)
        {

            if (m_localStorage == null) return false;
            m_localStorage.SetSourceIDName("StationEnvironment", ListenerIdx);


            int Idx = -1;
            bool bStoreOk = m_localStorage.readSettingsfromLocalStorage(composite, Idx);

            if (bStoreOk)
            {
                string StringValue;

                int IntValue;
                double OutValue;
  


                bool bok = m_localStorage.readIntegerSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.ConnectorSEN0188Enable", Idx), out IntValue);
                StationEnvironment.ConnectorSEN0188Enable = Convert.ToBoolean(IntValue);

                bok = m_localStorage.readStringSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.Id", Idx), out StringValue);

                if (StringValue.Length == 0) StringValue = "COM1";

                StationEnvironment.SerDev.Id = StringValue;


                bok = m_localStorage.readDoubleSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.WriteTimeout.TotalMilliseconds", Idx), out OutValue);

                StationEnvironment.SerDev.WriteTimeout = TimeSpan.FromMilliseconds(OutValue);


                bok = m_localStorage.readDoubleSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.ReadTimeout.TotalMilliseconds", Idx), out OutValue);

                StationEnvironment.SerDev.ReadTimeout = TimeSpan.FromMilliseconds(OutValue);

                bok = m_localStorage.readDoubleSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.Parity", Idx),  out OutValue);

                StationEnvironment.SerDev.Parity = (SerialParity) OutValue;
                bok = m_localStorage.readDoubleSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.StopBits", Idx), out OutValue);

                StationEnvironment.SerDev.StopBits = (SerialStopBitCount) OutValue;

                bok = m_localStorage.readDoubleSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.SerDev.Handshake", Idx), out OutValue);

                StationEnvironment.SerDev.Handshake = (SerialHandshake) OutValue;

                bok = m_localStorage.readIntegerSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.EventHistoryinDays", Idx), out IntValue);
                if (IntValue<=0) IntValue = 4;
                StationEnvironment.EventHistoryinDays = IntValue;

            }
    


            return bStoreOk;


        }

        public bool readGPIOStationEnvironmentDatafromLocalStorage(GPIOEnvironmentConnector StationEnvironment, Windows.Storage.ApplicationDataCompositeValue composite, int ListenerIdx)
        {

            if (m_localStorage == null) return false;
            m_localStorage.SetSourceIDName("StationEnvironment.GPIO", ListenerIdx);


            int Idx = -1;
            bool bStoreOk = m_localStorage.readSettingsfromLocalStorage(composite, Idx);

            if (bStoreOk)
            {
                string StringValue;

                int IntValue;

                bool bok = m_localStorage.readStringSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.HostName", Idx), out StringValue);
                if (StringValue.Count() > 0)
                {
                    StationEnvironment.HostName = StringValue;
                }



                bok = m_localStorage.readIntegerSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.Port", Idx), out IntValue);
                if (IntValue > 0)
                {
                    StationEnvironment.Port = IntValue;
                }



                bok = m_localStorage.readIntegerSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName("StationEnvironment.GPIOConnectorEnable", Idx), out IntValue);
                StationEnvironment.GPIOConnectorEnable = Convert.ToBoolean(IntValue);

                for (int i = 0; i < StationEnvironment.GPIOOInOutBanks.InOutBanks.Count; i++)
                {
                    GPIOOBank bank = StationEnvironment.GPIOOInOutBanks.InOutBanks[i];

                    foreach (GPIOObjects OutPuts in bank.GPIOBanks)
                    {
                        foreach (GPIOObject GPIOObj in OutPuts.GPIOs)
                        {
                            string property;
                            bok = m_localStorage.readStringSettingsfromLocalStorage(composite, m_localStorage.getCompositePropertyIDName(GPIOObj.GPIOName, Idx), out property);
                            if (bok)
                            {
                                GPIOObj.CreateGPIOObjectByStorageSettings(property);
                            }

                        }

                    }
                }

            }
   
            return bStoreOk;
        }



   
        public bool writeListenerDatatoLocalStorage()
        {


            m_LocalStorageSettings.SetSourceIDName("FingerSensorsAppData");

            m_LocalStorageSettings.deleteCompositeValue(); // vor jedem Schreiben alles löschen



            Windows.Storage.ApplicationDataCompositeValue composite = m_LocalStorageSettings.getCompositeValue();

            int Idx = 0;
            writeStationEnvironmenttoLocalStorage(m_StationEnvironment, composite, Idx);

            for (int i = 0; i < m_StationEnvironment.GPIOEnvironmentConnectors.EnvironmentConnectors.Count; i++)
            {
                GPIOEnvironmentConnector con = m_StationEnvironment.GPIOEnvironmentConnectors.EnvironmentConnectors[i];
                writeGPIOStationEnvironmenttoLocalStorage(con, composite, i);

            }

      //      writeGPIOStationEnvironmenttoLocalStorage(m_StationEnvironment, composite, Idx);

            m_LocalStorageSettings.writeCompositeValuetoLocalStorage();

            return true;
        }

        public async Task<bool> readListenerDatafromLocalStorage()
        {


            m_LocalStorageSettings.SetSourceIDName("FingerSensorsAppData");

            Windows.Storage.ApplicationDataCompositeValue composite = m_LocalStorageSettings.getCompositeValue();
            int Idx = 0;


            bool bdata = readStationEnvironmentDatafromLocalStorage(m_StationEnvironment, composite, Idx);

            while (true)
            {
               GPIOEnvironmentConnector con = await m_StationEnvironment.GPIOEnvironmentConnectors.addConnector();

                bdata = readGPIOStationEnvironmentDatafromLocalStorage(con, composite, Idx);
                if (!bdata)
                {
                    // listener wieder aus Queue löshen, wenn nicht benötigt
                    m_StationEnvironment.GPIOEnvironmentConnectors.deleteConnector(con);
                    //    delete listener;
                    break;
                }
                Idx++;
            }

            if (m_StationEnvironment.GPIOEnvironmentConnectors.EnvironmentConnectors.Count == 0) // Add Dummy
            {
                GPIOEnvironmentConnector con = await m_StationEnvironment.GPIOEnvironmentConnectors.addConnector();
            }



            return bdata;
        }




        public bool writeDatatoLocalStorage()
        {


            writeListenerDatatoLocalStorage();


            return true;
        }

        public async Task<bool> readDatafromLocalStorage()
        {

           bool ret =  await readListenerDatafromLocalStorage();
           return ret;
   
        }


    }
}
