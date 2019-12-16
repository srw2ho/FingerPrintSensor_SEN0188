using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SEN0188_SQLite
{
    public class DBDataSetAccessBit
    {
        string m_BitName;
        ulong m_BitValue;
        int m_BitNumber;
        public DBDataSetAccessBit(string bitName, ulong bitValue,int bitNo)
        {
            m_BitName = bitName;
            m_BitValue = bitValue;
            m_BitNumber = bitNo;
        }

        public string BitName
        {
            get { return m_BitName; }
            set
            {
                m_BitName = value;

            }
        }

        public int BitNumber
        {
            get { return m_BitNumber; }
            set
            {
                m_BitNumber = value;

            }
        }
        public ulong BitValue
        {
            get { return m_BitValue; }
            set
            {
                m_BitValue = value;

            }
        }

    }

    public class DBDataSetAccessRight
    {


        static private ulong[] AccessRights_Bits = {
        0x00000001,  0x00000002, 0x00000004,  0x00000008,
        0x00000010,  0x00000020, 0x00000040,  0x00000080,
        0x00000100,  0x00000200, 0x00000400,  0x00000800,
        0x00001000,  0x00002000, 0x00004000,  0x00008000,
        0x00010000,  0x00020000, 0x00040000,  0x00080000,
        };

        public static  ulong getAccessBits(int Idx)
        {
            if (Idx < AccessRights_Bits.Length)
                return AccessRights_Bits[Idx];
            else return 0;
        }
        public static int getBitNumberByAccessBits(ulong accessRight)
        {
            for (int i = 0; i < AccessRights_Bits.Length; i++)
            {
                if (AccessRights_Bits[i] == accessRight)
                {
                    return i;
                }
            }
            return -1;
        }

        public static ObservableCollection<DBDataSetAccessBit> getAccessBitsCollection()
        {
            ObservableCollection<DBDataSetAccessBit> AccessCombo = new ObservableCollection<DBDataSetAccessBit>();

            for (int i = 0; i < AccessRights_Bits.Length; i++)
            {
                string bittext = String.Format("AccessValue_{0:X}", AccessRights_Bits[i]);
                DBDataSetAccessBit accessBit = new DBDataSetAccessBit(bittext, AccessRights_Bits[i],i);
   
                AccessCombo.Add(accessBit);
            }

            return AccessCombo;
        }


    }
    [Windows.UI.Xaml.Data.Bindable]
    public class DBDataSet : INotifyPropertyChanged
    {

        string m_SecondName;
        string m_FirstName;
        string m_Info;
        ulong m_AccessRights;
        byte[] m_SensorId;
        bool m_bIsChanged;
        byte[] m_FingerTemplate;
        int m_FingerID;

        int m_MatchScore;

        DateTime m_CreationTime;
        public event PropertyChangedEventHandler PropertyChanged;
        public DBDataSet()
        {
            m_AccessRights = 0;
            m_SecondName = "";
            m_FirstName = "";
            m_Info = "...";
            m_FingerID = -1;
            m_bIsChanged = false;
            m_CreationTime = DateTime.Now;
            m_MatchScore = -1;
            m_SensorId = new byte[32];

            m_FingerTemplate = new byte[512];

            //for (int i = 0; i < m_FingerTemplate.Length; i++)
            //{
            //    m_FingerTemplate[i] = 0;
            //}

        }

        protected void OnPropertyChanged(string name)
        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {

                handler(this, new PropertyChangedEventArgs(name));

            }

        }


        public bool IsChanged
        {
            get { return m_bIsChanged; }
            set
            {
                m_bIsChanged = value;
                OnPropertyChanged("IsChanged");
            }
        }

        public DateTime CreationTime
        {
            get { return m_CreationTime; }

            set {
                m_CreationTime = value;
                OnPropertyChanged("CreationTime");
            }
        }
        public bool FingerTemplateAssigned
        {
            get {
                bool bret = (m_FingerTemplate[0] != 0) && (m_FingerTemplate[1] != 0) && (m_FingerTemplate[2] != 0);

                return bret;

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

        public bool AccessRights_Bit0
        {
            
            get { return (m_AccessRights & DBDataSetAccessRight.getAccessBits(0)) != 0; }
            set
            {
                ulong msk = DBDataSetAccessRight.getAccessBits(0);
                if (value) m_AccessRights |= msk;
                else m_AccessRights &= ~msk;
                OnPropertyChanged("AccessRights_Bit0");
                OnPropertyChanged("AccessRights");
            }
        }

        public bool AccessRights_Bit1
        {

            get { return (m_AccessRights & DBDataSetAccessRight.getAccessBits(1)) != 0; }
            set
            {
                ulong msk = DBDataSetAccessRight.getAccessBits(1);
                if (value) m_AccessRights |= msk;
                else m_AccessRights &= ~msk;
                OnPropertyChanged("AccessRights_Bit1");
                OnPropertyChanged("AccessRights");
            }
        }
        public bool AccessRights_Bit2
        {

            get { return (m_AccessRights & DBDataSetAccessRight.getAccessBits(2)) != 0; }
            set
            {
                ulong msk = DBDataSetAccessRight.getAccessBits(2);
                if (value) m_AccessRights |= msk;
                else m_AccessRights &= ~msk;
                OnPropertyChanged("AccessRights_Bit2");
                OnPropertyChanged("AccessRights");
            }
        }
        public bool AccessRights_Bit3
        {

            get { return (m_AccessRights & DBDataSetAccessRight.getAccessBits(3)) != 0; }
            set
            {
                ulong msk = DBDataSetAccessRight.getAccessBits(3);
                if (value) m_AccessRights |= msk;
                else m_AccessRights &= ~msk;
                OnPropertyChanged("AccessRights_Bit3");
                OnPropertyChanged("AccessRights");
            }
        }
        public bool AccessRights_Bit4
        {

            get { return (m_AccessRights & DBDataSetAccessRight.getAccessBits(4)) != 0; }
            set
            {
                ulong msk = DBDataSetAccessRight.getAccessBits(4);
                if (value) m_AccessRights |= msk;
                else m_AccessRights &= ~msk;
                OnPropertyChanged("AccessRights_Bit4");
                OnPropertyChanged("AccessRights");
            }
        }

        public bool AccessRights_Bit5
        {

            get { return (m_AccessRights & DBDataSetAccessRight.getAccessBits(5)) != 0; }
            set
            {
                ulong msk = DBDataSetAccessRight.getAccessBits(5);
                if (value) m_AccessRights |= msk;
                else m_AccessRights &= ~msk;
                OnPropertyChanged("AccessRights_Bit5");
                OnPropertyChanged("AccessRights");
            }
        }

        public bool AccessRights_Bit6
        {

            get { return (m_AccessRights & DBDataSetAccessRight.getAccessBits(6)) != 0; }
            set
            {
                ulong msk = DBDataSetAccessRight.getAccessBits(6);
                if (value) m_AccessRights |= msk;
                else m_AccessRights &= ~msk;
                OnPropertyChanged("AccessRights_Bit6");
                OnPropertyChanged("AccessRights");
            }
        }
        public bool AccessRights_Bit7
        {

            get { return (m_AccessRights & DBDataSetAccessRight.getAccessBits(7)) != 0; }
            set
            {
                ulong msk = DBDataSetAccessRight.getAccessBits(7);
                if (value) m_AccessRights |= msk;
                else m_AccessRights &= ~msk;
                OnPropertyChanged("AccessRights_Bit7");
                OnPropertyChanged("AccessRights");
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
            get {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                return enc.GetString(m_SensorId);
            }

        }
        public string Info
        {
            get { return m_Info; }
            set
            {
                m_Info = value;
                OnPropertyChanged("Info");
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
        public ulong AccessRights
        {
            get {
                return m_AccessRights;
            }
            set
            {
                m_AccessRights = value;
                OnPropertyChanged("AccessRights");
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
        public byte[] FingerTemplate
        {
            get { return m_FingerTemplate; }
            set
            {
                int nSize = m_FingerTemplate.Length;
                if (value.Length < nSize) nSize = value.Length;

                for (int i = 0; i < nSize; i++)
                {
                    m_FingerTemplate[i] = value[i];
                }
                OnPropertyChanged("FingerTemplate");
                OnPropertyChanged("FingerTemplateAssigned");
                
            }
        }
    }
}
