using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using FingerSensorsApp.Helpers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Core;
using System.Text.RegularExpressions;

namespace FingerSensorsApp.Models
{
    public class GPIOObject : INotifyPropertyChanged
    {
        public enum GPIOTyp
        {
            input   = 0,
            output  = 1,
            PWM     = 2,
            HC_SR04 = 3,
            BME280  = 4,
            inputShutdown = 5,
            PWM9685     = 6,
        };

        //   enum GPIOTyp { Sun, Mon, Tue, Wed, Thu, Fri, Sat };

        string m_PinName;
        string m_GPIOName;
        GPIOTyp m_GPIOTyp;
        double m_PinValue;      // Value
        double m_InitValue;     // Initialisierungs Value
        double m_SetValue;      // für Output und PWM-Output
        int m_PinNumber;
        BitmapImage m_IsOnBitmapImage;
        BitmapImage m_IsOffBitmapImage;
        bool m_IsEnabled;
        bool m_IsEventEnabled;
        double m_PulseTime;
        string m_ViewName;
        string m_EventName;
        ulong m_EventAccessRights;
        public event PropertyChangedEventHandler PropertyChanged;

        bool m_IsFlankActive;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public GPIOObject()
        {
            m_PinName = "";
            m_PulseTime = 0;
            m_GPIOTyp = GPIOTyp.input;
            m_PinValue = 0;
            m_InitValue = 0;
            m_SetValue = 0;
            m_PinNumber = 0;
            m_IsOnBitmapImage = null;
            m_IsOffBitmapImage = null;
            m_IsEnabled = true;
            m_ViewName = m_PinName;
            m_EventName = "no Event";
            m_IsEventEnabled = false;
            m_EventAccessRights = 0;
            m_IsFlankActive = false;
        }

        public GPIOObject(string Name, GPIOTyp typ, int PinNo, double initValue, double SetValue)
        {
            //m_ScatterLineSeries = lineSeries;
            m_PinName =  string.Format("{0}.{1:00}", Name, PinNo); ;
            switch (typ)
            {
                case GPIOTyp.input:
                    m_GPIOName = string.Format("GPI.{0:00}", PinNo); ;
                    break;
                case GPIOTyp.output:
                    m_GPIOName = string.Format("GPO.{0:00}", PinNo); ;
                    break;
                default:
                    m_GPIOName = string.Format("GPIO.{0:00}", PinNo); ;
                    break;
            }

            m_PulseTime = 0;
            m_GPIOTyp = typ;
            m_PinValue = 0;
            m_InitValue = initValue;
            m_SetValue = SetValue;
            m_PinNumber = PinNo;
            m_IsOnBitmapImage = null;
            m_IsOffBitmapImage = null;
            m_IsEnabled = true;
            m_ViewName = m_PinName;
            m_EventName = "no Event";
            m_IsEventEnabled = false;
            m_EventAccessRights = 0;
            m_IsFlankActive = false;

        }
        public string getPropertyLine()
        {
            string keyValue;
            if (m_GPIOTyp == GPIOTyp.input || m_PulseTime == 0)
            {
                keyValue = string.Format("PinName={0}; Typ={1}; PinNumber={2}; InitValue={3}; SetValue={4}; PinValue={5}", m_PinName, m_GPIOTyp.ToString(), m_PinNumber, m_InitValue, m_SetValue, m_PinValue); ;
            }
            else
            {
                keyValue = string.Format("PinName={0}; Typ={1}; PinNumber={2}; InitValue={3}; PulseTime ={4}; SetValue={5}; PinValue={6}", m_PinName, m_GPIOTyp.ToString(), m_PinNumber, m_InitValue, m_PulseTime, m_SetValue, m_PinValue); ;
            }

            return keyValue;

        }

        public string getPropertyforStorageSettings()
        {
            string keyValue;

            keyValue = string.Format("PinName={0}; Typ={1}; PinNumber={2}; InitValue={3}; PulseTime={4}; SetValue={5}; PinValue={6}; ViewName={7}; IsEnabled={8}; EventName={9}; IsEventEnabled={10}; EventAccessRights={11}", m_PinName, m_GPIOTyp.ToString(), m_PinNumber, m_InitValue, m_PulseTime, m_SetValue, m_PinValue, m_ViewName, m_IsEnabled, m_EventName, m_IsEventEnabled, m_EventAccessRights); ;

            return keyValue;

        }

        public void CreateGPIOObjectByStorageSettings(string propertyLine)
        {
            string [] array = propertyLine.Split(";");
            for (int i = 0; i < array.Length; i++)
            {
                string item = array[i];
       //         string repacedItem = Regex.Replace(item, @"\s", "");//remove with spaces

                string[] keyArray = item.Split("=");
                if (keyArray.Length > 1)
                {
                    string key = keyArray[0];

                    key = Regex.Replace(key, @"\s", "");//remove with spaces

                    string key_value = keyArray[1];


                    if (key == "InitValue")
                    {
                        m_InitValue = Convert.ToDouble(key_value);
                    }
                    if (key == "PulseTime")
                    {
                        m_PulseTime = Convert.ToDouble(key_value);
                    }
                    if (key == "ViewName")
                    {
                        m_ViewName = key_value;
                    }
                    if (key == "IsEnabled")
                    {
                        m_IsEnabled = Convert.ToBoolean(key_value); ;
                    }
                    if (key == "IsEnabled")
                    {
                        m_IsEnabled = Convert.ToBoolean(key_value); ;
                    }
                    if (key == "EventName")
                    {
                        m_EventName = key_value;
                    }
                    if (key == "IsEventEnabled")
                    {
                        m_IsEventEnabled = Convert.ToBoolean(key_value); ;
                    }
                    if (key == "EventAccessRights")
                    {
                        EventAccessRights = Convert.ToUInt64(key_value); ;
                    }
                    

                }

            }  

        }

        public void createPropertySet(IPropertySet property)
        {
            string keyValue = getPropertyLine();

            property.Add(m_PinName, PropertyValue.CreateString(keyValue));

        //    string keyValue = string.Format("PinName={0}; Typ={1}; PinNumber={2}; InitValue={3}; SetValue={4}", m_GPIOName, m_GPIOTyp.ToString(), m_PinNumber, m_InitValue,m_SetValue-SetValue); ;

        //    property.Add(m_GPIOName, PropertyValue.CreateString(keyValue));




        }

        public void readImages()
        {

            if (m_GPIOTyp == GPIOTyp.input)
            {
                if (m_IsOnBitmapImage == null)
                {
                    m_IsOnBitmapImage = ImageHelper.ImageFromImagesFile("Signal_1.bmp");
                }
                if (m_IsOffBitmapImage == null)
                {
                    m_IsOffBitmapImage = ImageHelper.ImageFromImagesFile("Signal_0.bmp");
                }
            }




        }

        public BitmapImage IsOnBitmapImage
        {
            get
            {
       
           //     m_IsOnBitmapImage = ImageHelper.ImageFromImagesFile("Signal_1.bmp");
           //     m_IsOffBitmapImage = ImageHelper.ImageFromImagesFile("Signal_0.bmp");

                return (m_PinValue > 0) ? m_IsOnBitmapImage : m_IsOffBitmapImage;

            }

        }
        public bool IsFlankActive
        {
            get
            {
                return m_IsFlankActive;
            }
            set
            {
                m_IsFlankActive = value;
                OnPropertyChanged("IsFlankActive");
            }

        }

        public string EventName
        {
            get
            {
                return m_EventName;
            }
            set
            {
                m_EventName = value;
                OnPropertyChanged("EventName");
            }

        }

        public string ViewName
        {
            get
            {
                return m_ViewName;
            }
            set
            {
                m_ViewName = value;
                OnPropertyChanged("ViewName");
            }

        }

        public string PinValueasString
        {
            get
            {
                string body = string.Format(" {0:00.00}", m_PinValue);
                return body;
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
                OnPropertyChanged("GPIOName");
            }

        }

        public ulong EventAccessRights
        {
            get
            {
                return m_EventAccessRights;
            }
            set
            {
                m_EventAccessRights = value;
                OnPropertyChanged("EventAccessRights");
            }
        }

        public bool IsEventEnabled
        {
            get
            {
                return m_IsEventEnabled;
            }
            set
            {
                m_IsEventEnabled = value;
                OnPropertyChanged("IsEventEnabled");
            }
        }

        public bool IsEnabled
        {
            get
            {
                return m_IsEnabled ;
            }
            set
            {
                m_IsEnabled = value;
                OnPropertyChanged("IsEnabled");
            }

        }
        public double PulseTime
        {
            get
            {
                return m_PulseTime;
            }
            set
            {
                m_PulseTime = value;
                OnPropertyChanged("PulseTime");
            }

        }


        public string PinName
        {
            get
            {
                return m_PinName;
            }
            set
            {
                m_PinName = value;
                OnPropertyChanged("PinName");
            }

        }

        public GPIOTyp GPIOtyp
        {
            get { return m_GPIOTyp; }
            set
            {
                m_GPIOTyp = value;
                OnPropertyChanged("GPIOtyp");
            }

        }



        public double SetValue
        {
            get { return m_SetValue; }
            set
            {
                if (value != m_SetValue)
                {
                    m_SetValue = value;
                    OnPropertyChanged("SetValue");
                }

            }

        }

        public int PinNumber
        {
            get { return m_PinNumber; }
            set
            {
                m_PinNumber = value;
                OnPropertyChanged("PinNumber");
            }

        }
        public bool IsOn
        {
            get {
                return m_PinValue > 0;
            }
            set
            {
                m_PinValue = (value) ? 1:0;
                OnPropertyChanged("IsOn");
            }

        }

        public double PinValue
        {
            get { return m_PinValue; }
            set
            {
                if (value!= m_PinValue)
                {
                    m_PinValue = value;
                    OnPropertyChanged("PinValue");
                    OnPropertyChanged("PinValueasString");
                    OnPropertyChanged("IsOn");
                    OnPropertyChanged("IsOnBitmapImage");
                    
                }

            }

        }

        public double InitValue
        {
            get { return m_InitValue; }
            set
            {
                m_InitValue = value;
                OnPropertyChanged("InitValue");
            }

        }

    }

    public class GPIOObjects
    {
        ObservableCollection<GPIOObject> m_GPIOs;
        string m_BankName;

        public GPIOObjects(string Name)
        {
            m_BankName = Name;
            m_GPIOs = new ObservableCollection<GPIOObject>();
          

        }
        public void createPropertySet(IPropertySet property)
        {
            for (int i = 0; i < m_GPIOs.Count; i++)
            {
                GPIOObject obj = m_GPIOs[i];
                if (obj.IsEnabled)
                {
                    obj.createPropertySet(property);
                }


            }

        }

   

        public void readImages()
        {
            for (int i = 0; i < m_GPIOs.Count; i++)
            {
                GPIOObject obj = m_GPIOs[i];
                obj.readImages();

            }

        }

        public bool IsEnabled
        {
            get
            {
                for (int i = 0; i < m_GPIOs.Count; i++)
                {
                    GPIOObject obj = m_GPIOs[i];
                    if (obj.IsEnabled) return true;

                }
                return false;
            }


        }


        public GPIOObject getGPIOByName(string name)
        {

            for (int i = 0; i < m_GPIOs.Count; i++)
            {
                GPIOObject obj = m_GPIOs[i];
                if (obj.PinName == name)
                {
                    return obj;
                }

            }
            return null;

        }

        public ObservableCollection<GPIOObject> GPIOs
        {
            get
            {
                return m_GPIOs;
            }
        }

        public string BankName
        {
            get
            {
                return m_BankName;
            }
            set
            {
                m_BankName = value;
            }

        }

    }
    public class GPIOOBank
    {
        ObservableCollection<GPIOObjects> m_GPIOBanks;
        string m_BankName;

        public GPIOOBank(string Name)
        {
            m_BankName = Name;
            m_GPIOBanks = new ObservableCollection<GPIOObjects>();


        }

        public void createPropertySet(IPropertySet property)
        {
            for (int i = 0; i < m_GPIOBanks.Count; i++)
            {
                GPIOObjects obj = m_GPIOBanks[i];
                obj.createPropertySet(property);

            }

        }

        public void readImages()
        {
            for (int i = 0; i < m_GPIOBanks.Count; i++)
            {
                GPIOObjects obj = m_GPIOBanks[i];
                obj.readImages();

            }

        }

        public bool IsEnabled
        {
            get
            {
                for (int i = 0; i < m_GPIOBanks.Count; i++)
                {
                    GPIOObjects obj = m_GPIOBanks[i];
                    if (obj.IsEnabled) return true;

                }
                return false;
            }


        }

        public GPIOObjects getGPIOBankByName(string name)
        {

            for (int i = 0; i < m_GPIOBanks.Count; i++)
            {
                GPIOObjects obj = m_GPIOBanks[i];
                if (obj.BankName == name)
                {
                    return obj;
                }

            }
            return null;

        }



        public GPIOObject getGPIOByName(string name)
        {

            for (int i = 0; i < m_GPIOBanks.Count; i++)
            {
                GPIOObjects obj = m_GPIOBanks[i];

                GPIOObject ret = obj.getGPIOByName(name);
                if (ret != null) return ret;
            }

            return null;

        }


        public ObservableCollection<GPIOObjects> GPIOBanks
        {
            get
            {
                return m_GPIOBanks;
            }
        }

        public string BankName
        {
            get
            {
                return m_BankName;
            }
            set
            {
                m_BankName = value;
            }

        }

    }





    public class GPIOOInOutBanks
    {
        ObservableCollection<GPIOOBank> m_InOutBanks;
   
        string m_BankName;

        public GPIOOInOutBanks(string Name)
        {
            m_BankName = Name;
            m_InOutBanks = new ObservableCollection<GPIOOBank>();


        }
    
        static GPIOOInOutBanks Allocate(IPropertySet property)
        {
            GPIOOInOutBanks m_GPIOInOutBanks = new GPIOOInOutBanks("");

            //   m_Banks = new List<GPIOOBank>();

            GPIOOBank m_OutPuts = new GPIOOBank("Outputs");


            GPIOOBank m_Inputs = new GPIOOBank("Inputs");
            //   ObservableCollection<GPIOObjects>m_GPIOOutputs = new ObservableCollection<GPIOObjects>();


            GPIOObjects m_GPIOOutPut5V = new GPIOObjects("GPIOOutPut.5V");

            m_GPIOOutPut5V.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 17, 0, 0));
            m_GPIOOutPut5V.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 27, 0, 0));
            m_GPIOOutPut5V.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 23, 0, 0));
            m_GPIOOutPut5V.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 22, 0, 0));

            GPIOObjects m_GPIOOutPut3V3 = new GPIOObjects("GPIOOutPut.3V3");

            m_GPIOOutPut3V3.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 13, 0, 0));
            m_GPIOOutPut3V3.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 19, 0, 0));
            m_GPIOOutPut3V3.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 11, 0, 0));
            m_GPIOOutPut3V3.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 21, 0, 0));

            GPIOObjects m_GPIOOutPutOC = new GPIOObjects("GPIOOutPut.OpenCol. ");

            m_GPIOOutPutOC.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 4, 0, 0));
            m_GPIOOutPutOC.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 10, 0, 0));
            m_GPIOOutPutOC.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 9, 0, 0));
            m_GPIOOutPutOC.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.output, 16, 0, 0));

            m_OutPuts.GPIOBanks.Add(m_GPIOOutPut5V);

            m_OutPuts.GPIOBanks.Add(m_GPIOOutPut3V3);

            m_OutPuts.GPIOBanks.Add(m_GPIOOutPutOC);

            GPIOObjects GPIOInputs5V = new GPIOObjects("GPIOInputs.4Bank");

            GPIOInputs5V.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 15, 0, 0));
            GPIOInputs5V.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 14, 0, 0));
            GPIOInputs5V.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 12, 0, 0));
            GPIOInputs5V.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 20, 0, 0));

            GPIOObjects GPIOInputs5V8 = new GPIOObjects("GPIOInputs.8Bank");

            GPIOInputs5V8.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 26, 0, 0));
            GPIOInputs5V8.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 25, 0, 0));
            GPIOInputs5V8.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 24, 0, 0));
            GPIOInputs5V8.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 5, 0, 0));

            GPIOObjects GPIOInputs5V4 = new GPIOObjects("GPIOInputs.8Bank");
            GPIOInputs5V4.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 6, 0, 0));
            GPIOInputs5V4.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 7, 0, 0));
            GPIOInputs5V4.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 8, 0, 0));
            GPIOInputs5V4.GPIOs.Add(new GPIOObject("GPIO", GPIOObject.GPIOTyp.input, 18, 0, 0));

            m_Inputs.GPIOBanks.Add(GPIOInputs5V);
            m_Inputs.GPIOBanks.Add(GPIOInputs5V8);
            m_Inputs.GPIOBanks.Add(GPIOInputs5V4);


            m_GPIOInOutBanks.InOutBanks.Add(m_Inputs);
            m_GPIOInOutBanks.InOutBanks.Add(m_OutPuts);

            m_GPIOInOutBanks.createPropertySet(property);

            return m_GPIOInOutBanks;
        }

        static async public Task<GPIOOInOutBanks> GPIOOInOutBanksAsync(IPropertySet property)
        {
            GPIOOInOutBanks banks = await Task.Run(() => Allocate(property));
            return  banks;;

        }

        GPIOOInOutBanks AllocateActiveGPIOs(IPropertySet property)
        {
            GPIOOInOutBanks GPIOInOutBanks = new GPIOOInOutBanks("");

            GPIOOBank m_OutPuts = new GPIOOBank("Outputs");


            GPIOOBank m_Inputs = new GPIOOBank("Inputs");
            //   ObservableCollection<GPIOObjects>m_GPIOOutputs = new ObservableCollection<GPIOObjects>();

            GPIOObjects m_GPIOOutputs = new GPIOObjects("GPIOOutPuts");
            GPIOObjects m_GPIOInputs = new GPIOObjects("GPIOInPuts");


            for (int i = 0; i < m_InOutBanks.Count; i++)
            {
                GPIOOBank bank = m_InOutBanks[i];

                foreach (GPIOObjects OutPuts in bank.GPIOBanks)
                {
                    foreach (GPIOObject GPIOObj in OutPuts.GPIOs)
                    {
                        if (GPIOObj.IsEnabled)
                        {
                            switch (GPIOObj.GPIOtyp)
                            {
                                case GPIOObject.GPIOTyp.output:
                                    m_GPIOOutputs.GPIOs.Add(GPIOObj);
                                    break;
                                case GPIOObject.GPIOTyp.input:
                                    m_GPIOInputs.GPIOs.Add(GPIOObj);
                                    break;

                            }
                        }

                    }

                }
            }
            if (m_GPIOOutputs.GPIOs.Count > 0)
            {
                m_Inputs.GPIOBanks.Add(m_GPIOInputs);

            }
            if (m_GPIOOutputs.GPIOs.Count > 0)
            {
                m_OutPuts.GPIOBanks.Add(m_GPIOOutputs);

            }

            GPIOInOutBanks.InOutBanks.Add(m_Inputs);
            GPIOInOutBanks.InOutBanks.Add(m_OutPuts);

            GPIOInOutBanks.createPropertySet(property);

            return GPIOInOutBanks;
        }

        public GPIOOInOutBanks GPIOOActiveInOutBanks(IPropertySet property)
        {
            GPIOOInOutBanks banks = AllocateActiveGPIOs(property);

      //      GPIOOInOutBanks banks = await Task.Run(() => AllocateActiveGPIOs(property));
            return banks; ;

        }

        async public Task<GPIOOInOutBanks> GPIOOActiveInOutBanksAsync(IPropertySet property)
        {

            GPIOOInOutBanks banks = await Task.Run(() => AllocateActiveGPIOs(property));
            return banks; ;

        }


        public void createPropertySet(IPropertySet property)
        {
            property.Clear();
            for (int i = 0; i < m_InOutBanks.Count; i++)
            {
                GPIOOBank obj = m_InOutBanks[i];
                obj.createPropertySet(property);

            }

        }

        public bool IsEnabled
        {
            get
            {
                for (int i = 0; i < m_InOutBanks.Count; i++)
                {
                    GPIOOBank obj = m_InOutBanks[i];
                    if (obj.IsEnabled) return true;

                }

                return false;
            }



        }

        public void readImages()
        {
            for (int i = 0; i < m_InOutBanks.Count; i++)
            {
                GPIOOBank obj = m_InOutBanks[i];
                obj.readImages();

            }

        }

      

        public GPIOOBank getGPIOBankByName(string name)
        {

            for (int i = 0; i < m_InOutBanks.Count; i++)
            {
                GPIOOBank obj = m_InOutBanks[i];
                if (obj.BankName == name)
                {
                    return obj;
                }

            }
            return null;

        }

        public ObservableCollection<GPIOOBank> InOutBanks
        {
            get
            {
                return m_InOutBanks;
            }
        }

        public string BankName
        {
            get
            {
                return m_BankName;
            }
            set
            {
                m_BankName = value;
            }

        }

    }
}
