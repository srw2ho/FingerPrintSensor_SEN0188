using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SEN0188_SQLite
{

    [Windows.UI.Xaml.Data.Bindable]
    public class FingerEvent : INotifyPropertyChanged
    {
        string m_SecondName;
        string m_FirstName;
        string m_EventType;
        byte[] m_SensorId;
        int m_FingerID;
        int m_MatchScore;
        int m_EventID;
        DateTime m_EventTime;
        int m_SensorState;
        string m_SensorTxtState;


        public event PropertyChangedEventHandler PropertyChanged;
        public FingerEvent()
        {
            m_EventType = "";
            m_SecondName = "";
            m_FirstName = "";
            m_FingerID = -1;
            m_SensorTxtState = "";
            m_SensorState = -1;
            m_EventTime = DateTime.Now;
            m_MatchScore = -1;
            m_SensorId = new byte[32];
            m_EventID = -1;

        }

        protected void OnPropertyChanged(string name)

        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {

                handler(this, new PropertyChangedEventArgs(name));

            }

        }

        public int SensorState
        {

            get { return m_SensorState; }
            set
            {
                m_SensorState = value;
                OnPropertyChanged("SensorState");
            }
        }

        public string SensorTxtState
        {

            get { return m_SensorTxtState; }
            set
            {
                m_SensorTxtState = value;
                OnPropertyChanged("SensorTxtState");
            }
        }



        public int EventID
        {

            get { return m_EventID; }
            set
            {
                m_EventID = value;
                OnPropertyChanged("EventID");
            }
        }


        public DateTime EventTime
        {
            get { return m_EventTime; }

            set
            {
                m_EventTime = value;
                OnPropertyChanged("EventTime");
            }
        }

        public int MatchScore
        {

            get { return m_MatchScore; }
            set
            {
                m_MatchScore = value;
                OnPropertyChanged("MatchScore");
            }
        }


        public byte[] SensorId
        {
            get { return m_SensorId; }
            set
            {
                int nSize = m_SensorId.Length;
                if (value.Length < nSize) nSize = value.Length;

                for (int i = 0; i < nSize; i++)
                {
                    m_SensorId[i] = value[i];
                }

                OnPropertyChanged("SensorId");
                OnPropertyChanged("SensorIdasString");
            }
        }

        public string SensorIdasString
        {
            get
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                return enc.GetString(m_SensorId);
            }

        }

        public string SecondName
        {
            get { return m_SecondName; }
            set
            {
                m_SecondName = value;
                OnPropertyChanged("SecondName");
            }
        }

        public string FirstName
        {
            get { return m_FirstName; }
            set
            {
                m_FirstName = value;
                OnPropertyChanged("FirstName");
            }
        }
        public string EventType
        {
            get
            {
                return m_EventType;
            }
            set
            {
                m_EventType = value;
                OnPropertyChanged("EventType");
            }
        }
        public int FingerID
        {
            get { return m_FingerID; }
            set
            {
                m_FingerID = value;
                OnPropertyChanged("FingerID");
            }
        }

    }
}