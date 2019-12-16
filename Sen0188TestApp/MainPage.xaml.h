//
// MainPage.xaml.h
// Declaration of the MainPage class.
//

#pragma once

#include "MainPage.g.h"
using namespace FingerPrintSensor_SEN0188;
using namespace Concurrency;


namespace Sen0188TestApp
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public ref class MainPage sealed
	{
//    SEN0188_SQLite::SEN0188SQLite^ m_SEN0188SQLite;

		Windows::Foundation::Collections::IObservableVector<FingerPrintSensor_SEN0188::SerDevice^>^  m_AvailabeleDevices;

		Connector_SEN0188^ m_Connector_SEN0188;
		Windows::Foundation::Collections::IPropertySet^ m_outputconfigoptions;
		Windows::Foundation::Collections::IPropertySet^ m_inputconfigoptions;
	//	FingerPrintSensor_SEN0188::ScanDevices^ m_ScanFordevices;

	

		Windows::Foundation::EventRegistrationToken m_Connector_SEN0188_startStreaming;
		Windows::Foundation::EventRegistrationToken m_Connector_SEN0188_stopStreaming;
		Windows::Foundation::EventRegistrationToken m_Connector_SEN0188_NotifyChangeState;
		Windows::Foundation::EventRegistrationToken m_applicationSuspendingEventToken;
		Windows::Foundation::EventRegistrationToken m_applicationResumingEventToken;

    Windows::Foundation::Collections::IObservableVector<unsigned int>^ m_FilledFingerLib;
	Windows::Foundation::Collections::IObservableVector<unsigned char>^ m_SensorID;
    unsigned int m_SelectedFingerID;
	public:
		MainPage();
		virtual ~MainPage();
	//	void ReadDevices();
		void InitDevice(FingerPrintSensor_SEN0188::SerDevice^ serDev);
		property Windows::Foundation::Collections::IObservableVector<FingerPrintSensor_SEN0188::SerDevice^>^ Devices
		{
			Windows::Foundation::Collections::IObservableVector<FingerPrintSensor_SEN0188::SerDevice^>^ get()
			{
				return m_AvailabeleDevices;
			}
		}

    property Windows::Foundation::Collections::IObservableVector<unsigned int>^ FilledFingerLib
    {
      Windows::Foundation::Collections::IObservableVector<unsigned int>^ get()
      {
        return m_FilledFingerLib;
      }
    }


	
	protected:
		virtual void OnNavigatedTo(Windows::UI::Xaml::Navigation::NavigationEventArgs^ e) override;
		virtual void OnNavigatingFrom(Windows::UI::Xaml::Navigation::NavigatingCancelEventArgs^ e) override;
		void Application_Suspending(Object^ sender, Windows::ApplicationModel::SuspendingEventArgs^ e);
		void Application_Resuming(Object^ sender, Object^ args);


		void OnNotifyChangeState(Platform::Object ^sender, Windows::Foundation::Collections::IPropertySet ^args); 

		void OnstartStreaming(Platform::Object ^sender, FingerPrintSensor_SEN0188::SerDevice ^args);

		void OnstopStreaming(Platform::Object ^sender, Platform::String ^args);

	private:
		void comPortInput_Click(Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
		void CMD_Initialize_Click(Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
		void closeDevice_Click(Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
		void CMD_Verifying_Click(Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);

		Concurrency::task<void>  readAvailableDevices();
		Windows::Foundation::IAsyncOperation<Windows::Devices::Enumeration::DeviceInformationCollection ^> ^ListAvailableSerialDevicesAsync(void);
    void CMD_Command_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
    void FingerLib_SelectionChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::SelectionChangedEventArgs^ e);
  };
}
