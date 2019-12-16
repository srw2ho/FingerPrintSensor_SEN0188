#pragma once
#include<SerDevice.h>
#include<SerialCom.h>
#include<Sensor.h>
#include "OrderEventPackageQueue.h"

using namespace Windows::UI::Xaml;

using namespace Windows::UI::Xaml::Controls;

using namespace Windows::UI::Xaml::Data;

namespace FingerPrintSensor_SEN0188
{

  enum ProcessingState {
    _doWaitForCommand,
    _doFingerRegistration,
    _doFingerVerifiying,
    _doFingerCharDownLoad,
    _doFingerSensorInitialize,
    _doFingerCharUpLoad,
    _doFingerAutoVerifiying,
    _doFingerAutoRegistration,
    _doFingerDeleteId,
	_doFingerWriteSensorID,
    _doFingerNotifyEvent,
  };

  [Windows::Foundation::Metadata::WebHostHidden]

  [Windows::UI::Xaml::Data::Bindable]

  public ref class Connector_SEN0188 sealed
  {

    OrderEventPackageQueue* m_pOrderEventPackageQueue;
    OrderEventPackage* m_pPacket;

    SerialCommunication::SerialCom^  m_pSerialCom;
    FingerPrintSensor_SEN0188::Sensor* m_pSensor;
    concurrency::cancellation_token_source* m_pPackageCancelTaskToken;

    Windows::Foundation::Collections::IPropertySet^ m_outputconfigoptions;
    Windows::Foundation::Collections::IPropertySet^ m_inputconfigoptions;

    Windows::Foundation::EventRegistrationToken m_SerialErrorReceivedEventRegister;
    Windows::Foundation::EventRegistrationToken m_stopStreamingEventRegister;
    Windows::Foundation::EventRegistrationToken m_FailedEventRegister;
    Windows::Foundation::EventRegistrationToken m_OnDeviceConnected;
    Windows::Foundation::EventRegistrationToken m_InputConfigOptionsMapChanged;


    bool m_bProcessingPackagesStarted;
    concurrency::task<void> m_ProcessingPackagesTsk;

    unsigned int m_FailedConnectionCount;
    

    unsigned int m_ProcessingState;
	//unsigned int m_ProcessingCMD;

   // HANDLE m_hCommandEvent;

    uint8_t m_CMDState;

    SerDevice^ m_Serdevice; // input Device


    unsigned m_InnerCMDProcessingState;
  public:
    Connector_SEN0188();
    virtual ~Connector_SEN0188();

    event Windows::Foundation::TypedEventHandler<Platform::Object^, Windows::Foundation::Collections::IPropertySet^  >^ NotifyChangeState;
    event Windows::Foundation::TypedEventHandler<Platform::Object^, Platform::String ^>^ stopStreaming;
	event Windows::Foundation::TypedEventHandler<Platform::Object^, SerDevice ^>^ startStreaming;
    event Windows::Foundation::TypedEventHandler<Platform::Object^, Platform::String ^> ^ Failed;

    Windows::Foundation::IAsyncAction ^ startProcessingPackagesAsync(SerDevice^ serDev, Windows::Foundation::Collections::IPropertySet^ inputconfigoptions, Windows::Foundation::Collections::IPropertySet^ outputconfigoptions);
    Windows::Foundation::IAsyncAction ^ stopProcessingPackagesAsync();

    property unsigned int  FailedConnectionCount {
      unsigned int   get() { return m_FailedConnectionCount; };
      void set(unsigned int   value) { m_FailedConnectionCount = value; };
    }

	property bool  ProcessingPackagesStarted {
		bool   get() { return m_bProcessingPackagesStarted; };
	}
  private:
    Concurrency::task<void> doProcessPackages();
    void cancelPackageAsync();
    void OnFailed(Platform::Object ^sender, Platform::Exception ^args);

    void startProcessingPackages(SerDevice^ serDev,Windows::Foundation::Collections::IPropertySet^ inputconfigoptions, Windows::Foundation::Collections::IPropertySet^ outputconfigoptions);
    void stopProcessingPackages();
    void OnMapChanged(Windows::Foundation::Collections::IObservableMap<Platform::String ^, Platform::Object ^> ^sender, Windows::Foundation::Collections::IMapChangedEventArgs<Platform::String ^> ^event);
    void OnOnDeviceConnected(Windows::Devices::SerialCommunication::SerialDevice ^sender, int args);
    void OnstopStreaming(Windows::Devices::SerialCommunication::SerialDevice ^sender, Platform::Exception ^args);
    void OnonSerialErrorReceived(Windows::Devices::SerialCommunication::SerialDevice ^sender, Windows::Devices::SerialCommunication::ErrorReceivedEventArgs ^args);

    // command doings
    bool doFingerRegistration();

    bool doFingerVerifiying();

	bool doAutoFingerVerifiying();
	

    bool doFingerNotifyEvent();

    bool dowaitForCommand(DWORD waitTime);
    bool doFingerCharDownLoad();

    bool doFingerCharUpLoad();

	bool doFingerSensorInitialize();

	bool doFingerWriteSensorID();

    OrderEventPackage* startFingerPrintCommand(OrderEventPackage* ppacket);
    bool restetFingerPrintCommand();
    bool doFingerAutoRegistration();
    bool doFingerDeleteId();

    bool doFingerNotifyRegistrationInformation(Platform::String^ info1, unsigned int state);
  };

  public ref class Connector_SEN0188_CMDs sealed
  {
  public:
    static int getFingerPrintCmd(Platform::String^ inputCmd) {
      if ("_doFingerSensorInitialize" == inputCmd) {
        return ProcessingState::_doFingerSensorInitialize;
      }
      else if ("_doFingerRegistration" == inputCmd) {
        return ProcessingState::_doFingerRegistration;
      }
      else if ("_doFingerVerifiying" == inputCmd) {
        return ProcessingState::_doFingerVerifiying;
      }
      else if ("_doFingerAutoVerifiying" == inputCmd) {
        return ProcessingState::_doFingerAutoVerifiying;
      }
      else if ("_doFingerCharUpLoad" == inputCmd) {
        return ProcessingState::_doFingerCharUpLoad;
      }
	  else if ("_doFingerCharDownLoad" == inputCmd) {
		  return ProcessingState::_doFingerCharDownLoad;
	  }
      else if ("_doFingerAutoRegistration" == inputCmd) {
        return ProcessingState::_doFingerAutoRegistration;
      }
      else if ("_doFingerDeleteId" == inputCmd) {
        return ProcessingState::_doFingerDeleteId;
      }
	  else if ("_doFingerWriteSensorID" == inputCmd) {
		  return ProcessingState::_doFingerWriteSensorID;
	  }

	  

      return -1;
    }
    // int CMD_doFingerVerifiying = 1;
  };

}
