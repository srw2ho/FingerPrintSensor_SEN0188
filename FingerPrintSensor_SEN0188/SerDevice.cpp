#include "pch.h"
#include "SerDevice.h"


namespace FingerPrintSensor_SEN0188
{
	/*
  SerDevice::SerDevice(Platform::String^ id, Windows::Devices::Enumeration::DeviceInformation^ deviceInfo)
  {
    m_id = id;
    m_deviceInformation = deviceInfo;
  }
  */
  SerDevice::SerDevice(Platform::String^ id)
  {
	  m_id = id;
	//  m_deviceInformation = deviceInfo;
  }

  SerDevice::~SerDevice()
  {
  }

  void SerDevice::NotifyPropertyChanged(Platform::String^ prop)
  {

    PropertyChanged(this, ref new PropertyChangedEventArgs(prop));

  }


}
