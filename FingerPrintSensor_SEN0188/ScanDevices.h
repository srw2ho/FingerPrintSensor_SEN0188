#pragma once

#include "SerDevice.h"
#include "ScanSerialDevices.h"
using namespace SerialCommunication;

namespace FingerPrintSensor_SEN0188 {

  [Windows::Foundation::Metadata::WebHostHidden]
  [Windows::UI::Xaml::Data::Bindable]

  public ref class ScanDevices sealed
  {
    Windows::Foundation::Collections::IObservableVector<FingerPrintSensor_SEN0188::SerDevice^>^  m_AvailabeleDevices;
    SerialCommunication::ScanSerialDevices^ m_ScanSerialDevices;

  public:
    ScanDevices();
    virtual ~ScanDevices();

    property Windows::Foundation::Collections::IObservableVector<FingerPrintSensor_SEN0188::SerDevice^>^ AvailableDevices {

      Windows::Foundation::Collections::IObservableVector<FingerPrintSensor_SEN0188::SerDevice^>^ AvailableDevices::get() { return m_AvailabeleDevices; };

    }

    Windows::Foundation::IAsyncAction ^ readAvailableDevices();
  internal:
	Concurrency::task<void>  ScanDevices::_readAvailableDevices();

  };

}

