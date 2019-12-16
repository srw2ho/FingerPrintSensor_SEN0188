#pragma once


using namespace Windows::UI::Xaml;

using namespace Windows::UI::Xaml::Controls;

using namespace Windows::UI::Xaml::Data;

//[Windows::UI::Xaml::Data::Bindable] // in c++, adding this attribute to ref classes enables data binding for more info search for 'Bindable' on the page http://go.microsoft.com/fwlink/?LinkId=254639 
namespace FingerPrintSensor_SEN0188
{
  [Windows::Foundation::Metadata::WebHostHidden]

  [Windows::UI::Xaml::Data::Bindable]

  public ref class SerDevice sealed:  public INotifyPropertyChanged
  {

    Platform::String^ m_id;

 //   Windows::Devices::Enumeration::DeviceInformation^ m_deviceInformation;
    Windows::Foundation::TimeSpan m_WriteTimeout;
    Windows::Foundation::TimeSpan m_ReadTimeout;
    unsigned int m_BaudRate;
    Windows::Devices::SerialCommunication::SerialParity m_Parity;
    Windows::Devices::SerialCommunication::SerialStopBitCount m_StopBits;
    Windows::Devices::SerialCommunication::SerialHandshake m_Handshake;
    unsigned short m_DataBits;
    /*
    serial_device->WriteTimeout = _timeOut;
    serial_device->ReadTimeout = _timeOut;
    serial_device->BaudRate = 9600;
    serial_device->Parity = Windows::Devices::SerialCommunication::SerialParity::None;
    serial_device->StopBits = Windows::Devices::SerialCommunication::SerialStopBitCount::One;
    serial_device->DataBits = 8;
    serial_device->Handshake = Windows::Devices::SerialCommunication::SerialHandshake::None;*/

  public:
    SerDevice(Platform::String^ id);
	//SerDevice(Platform::String^ id, Windows::Devices::Enumeration::DeviceInformation^ deviceInfo);
    virtual ~SerDevice();

    virtual event Windows::UI::Xaml::Data::PropertyChangedEventHandler^ PropertyChanged;



    property Windows::Foundation::TimeSpan WriteTimeout
    {

      Windows::Foundation::TimeSpan get() { return m_WriteTimeout;}
      void set(Windows::Foundation::TimeSpan value) {
        m_WriteTimeout = value;
        NotifyPropertyChanged("WriteTimeout");
      }

    }
    property Windows::Foundation::TimeSpan ReadTimeout
    {

      Windows::Foundation::TimeSpan get() { return m_ReadTimeout; }
      void set(Windows::Foundation::TimeSpan value) {
        m_ReadTimeout = value;
        NotifyPropertyChanged("ReadTimeout");
      }

    }
    property Windows::Devices::SerialCommunication::SerialParity Parity
    {

      Windows::Devices::SerialCommunication::SerialParity get() { return m_Parity; }
      void set(Windows::Devices::SerialCommunication::SerialParity value) {
        m_Parity = value;
        NotifyPropertyChanged("SerialParity");
      }

    }
    property Windows::Devices::SerialCommunication::SerialStopBitCount StopBits
    {

      Windows::Devices::SerialCommunication::SerialStopBitCount get() { return m_StopBits; }
      void set(Windows::Devices::SerialCommunication::SerialStopBitCount value) {
        m_StopBits = value;
        NotifyPropertyChanged("StopBits");
      }

    }

    property Windows::Devices::SerialCommunication::SerialHandshake Handshake
    {

      Windows::Devices::SerialCommunication::SerialHandshake get() { return m_Handshake; }
      void set(Windows::Devices::SerialCommunication::SerialHandshake value) {
        m_Handshake = value;
        NotifyPropertyChanged("Handshake");
      }

    }


	property unsigned short DataBits
	{
		unsigned short get() { return m_DataBits; }
		void set(unsigned short value) {
			m_DataBits = value;
			NotifyPropertyChanged("DataBits");
		}

	}

    property unsigned int BaudRate
    {

      unsigned int get() { return m_BaudRate; }
      void set(unsigned int value) {
        m_BaudRate = value;
        NotifyPropertyChanged("BaudRate");
      }

    }

    property Platform::String^ Id

    {

      Platform::String^ get()

      {

        return m_id;

      }

      void set(Platform::String^ value) {
        m_id = value;
        NotifyPropertyChanged("Id");
      }
    }
	/*
    property Windows::Devices::Enumeration::DeviceInformation^ DeviceInfo
    {
      Windows::Devices::Enumeration::DeviceInformation^ get()
      {
        return m_deviceInformation;
      }
    }
	*/

    void setCOMPropertysToSerialDevice(Windows::Devices::SerialCommunication::SerialDevice ^ serial_device) {

      serial_device->WriteTimeout = m_WriteTimeout;
      serial_device->ReadTimeout = m_ReadTimeout;
      serial_device->BaudRate = m_BaudRate;
      serial_device->Parity = m_Parity;
      serial_device->StopBits = m_StopBits;
      serial_device->DataBits = m_DataBits;
      serial_device->Handshake =m_Handshake;

    }




    protected:

      void NotifyPropertyChanged(Platform::String^ prop);
  };

}