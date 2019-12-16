//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"

using namespace Sen0188TestApp;

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::UI::Core;
using namespace FingerPrintSensor_SEN0188;
//using namespace SEN0188_SQLite;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

MainPage::MainPage()
{
	InitializeComponent();
	m_AvailabeleDevices = ref new  Platform::Collections::Vector<FingerPrintSensor_SEN0188::SerDevice^>();

	m_FilledFingerLib = ref new  Platform::Collections::Vector<unsigned int>();

	m_SensorID = ref new  Platform::Collections::Vector<unsigned char>();


	//m_ScanFordevices = ref new ScanDevices();
	m_Connector_SEN0188 = ref new Connector_SEN0188();
	m_outputconfigoptions = ref new Windows::Foundation::Collections::PropertySet();
	m_inputconfigoptions = ref new Windows::Foundation::Collections::PropertySet();
	m_Connector_SEN0188_startStreaming = m_Connector_SEN0188->startStreaming += ref new Windows::Foundation::TypedEventHandler<Platform::Object ^, FingerPrintSensor_SEN0188::SerDevice ^>(this, &Sen0188TestApp::MainPage::OnstartStreaming);
	m_Connector_SEN0188_stopStreaming = m_Connector_SEN0188->stopStreaming += ref new Windows::Foundation::TypedEventHandler<Platform::Object ^, Platform::String ^>(this, &Sen0188TestApp::MainPage::OnstopStreaming);
	m_Connector_SEN0188_NotifyChangeState = m_Connector_SEN0188->NotifyChangeState += ref new Windows::Foundation::TypedEventHandler<Platform::Object ^, Windows::Foundation::Collections::IPropertySet ^>(this, &Sen0188TestApp::MainPage::OnNotifyChangeState);


	m_applicationSuspendingEventToken =
		Application::Current->Suspending += ref new SuspendingEventHandler(this, &MainPage::Application_Suspending);
	m_applicationResumingEventToken =
		Application::Current->Resuming += ref new EventHandler<Object^>(this, &MainPage::Application_Resuming);


	readAvailableDevices();

}

MainPage::~MainPage()
{

	Application::Current->Suspending -= m_applicationSuspendingEventToken;
	Application::Current->Resuming -= m_applicationResumingEventToken;
	m_Connector_SEN0188->startStreaming -= m_Connector_SEN0188_startStreaming;
	m_Connector_SEN0188->stopStreaming -= m_Connector_SEN0188_stopStreaming;
	m_Connector_SEN0188->NotifyChangeState -= m_Connector_SEN0188_NotifyChangeState;

}

Windows::Foundation::IAsyncOperation<Windows::Devices::Enumeration::DeviceInformationCollection ^> ^MainPage::ListAvailableSerialDevicesAsync(void)

{

	// Construct AQS String for all serial devices on system

	Platform::String ^serialDevices_aqs = Windows::Devices::SerialCommunication::SerialDevice::GetDeviceSelector();


	// Identify all paired devices satisfying query

	return Windows::Devices::Enumeration::DeviceInformation::FindAllAsync(serialDevices_aqs);

}


Concurrency::task<void>  MainPage::readAvailableDevices()
{
	auto tsk = ListAvailableSerialDevicesAsync();

	auto rettsk = Concurrency::create_task(tsk).then([this](task<Windows::Devices::Enumeration::DeviceInformationCollection ^ > serialDeviceCollectionTask)
		{
			try
			{
				Windows::Devices::Enumeration::DeviceInformationCollection ^_deviceCollection = serialDeviceCollectionTask.get();
				// start with an empty list

				m_AvailabeleDevices->Clear();


				for (auto &&device : _deviceCollection)
				{

					m_AvailabeleDevices->Append(ref new SerDevice(device->Id));

				}


			}
			catch (Exception ^  ex) {

			}

		});
	return rettsk;
}


void MainPage::Application_Suspending(Object^ sender, Windows::ApplicationModel::SuspendingEventArgs^ e)
{
	// Handle global application events only if this page is active
	if (Frame->CurrentSourcePageType.Name == Interop::TypeName(MainPage::typeid).Name)
	{
		m_Connector_SEN0188->stopProcessingPackagesAsync();



	}
}

void MainPage::Application_Resuming(Platform::Object^ sender, Platform::Object^ args)
{
	// Handle global application events only if this page is active
	if (Frame->CurrentSourcePageType.Name == Interop::TypeName(MainPage::typeid).Name)
	{

	}
}

void MainPage::comPortInput_Click(Object^ sender, RoutedEventArgs^ e)
{
	auto selectionIndex = ConnectDevices->SelectedIndex;

	if (selectionIndex < 0)
	{
		status->Text = L"Select a device and connect";
		return;
	}


	FingerPrintSensor_SEN0188::SerDevice^ serDev = m_AvailabeleDevices->GetAt(selectionIndex);
	InitDevice(serDev);

}


void MainPage::closeDevice_Click(Object^ sender, RoutedEventArgs^ e)
{
	m_Connector_SEN0188->stopProcessingPackagesAsync();

}



void MainPage::CMD_Verifying_Click(Object^ sender, RoutedEventArgs^ e)
{
	unsigned int CMD = 2;
	m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));

	status->Text = "wait...";

}

void MainPage::CMD_Initialize_Click(Object^ sender, RoutedEventArgs^ e)
{
	unsigned int CMD = 4;
	m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));

	status->Text = "wait...";

}

void MainPage::InitDevice(FingerPrintSensor_SEN0188::SerDevice^ serDev) {


	//if (m_Devices->Size > 0) 
	{
		//	FingerPrintSensor_SEN0188::SerDevice^ serDev = m_Devices->GetAt(0);
		Windows::Foundation::TimeSpan _timeOut;
		_timeOut.Duration = 10000000L;
		serDev->BaudRate = 57600;
		serDev->WriteTimeout = _timeOut;
		serDev->ReadTimeout = _timeOut;
		serDev->Parity = Windows::Devices::SerialCommunication::SerialParity::None;
		serDev->StopBits = Windows::Devices::SerialCommunication::SerialStopBitCount::One;
		serDev->DataBits = 8;
		serDev->Handshake = Windows::Devices::SerialCommunication::SerialHandshake::None;


		m_inputconfigoptions->Insert("UpdateState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(1)));

		auto tsk = m_Connector_SEN0188->startProcessingPackagesAsync(serDev, m_inputconfigoptions, m_outputconfigoptions);
		auto ret = Concurrency::create_task(tsk).then([this](task<void> ret) {
			try
			{
				ret.get();
			}
			catch (Platform::Exception ^ex)
			{
				bool bOK = false;


			}

			});


	}

}

void MainPage::OnNavigatedTo(Windows::UI::Xaml::Navigation::NavigationEventArgs^ e)
{


	//param->ScenarioView->SelectedIndex = 0;
	Page::OnNavigatedTo(e);

}
void MainPage::OnNavigatingFrom(Windows::UI::Xaml::Navigation::NavigatingCancelEventArgs^ e)
{
	// Handling of this event is included for completeness, as it will only fire when navigating between pages and this sample only includes one page
	m_Connector_SEN0188->stopProcessingPackagesAsync();
	Page::OnNavigatingFrom(e);
}



void MainPage::OnNotifyChangeState(Platform::Object ^sender, Windows::Foundation::Collections::IPropertySet ^args) {

	Windows::ApplicationModel::Core::CoreApplication::MainView->CoreWindow->Dispatcher->RunAsync(
		CoreDispatcherPriority::Normal,
		ref new Windows::UI::Core::DispatchedHandler([this, args]()
			{
				int instruction = -1;
				int state = -1;

				if (args->HasKey("FingerPrint.CMDInstruction")) {

					Platform::Object^ Value = args->Lookup("FingerPrint.CMDInstruction");
					instruction = safe_cast<IPropertyValue^>(Value)->GetInt16();


				}



				if (args->HasKey("FingerPrint.CMDState")) {

					Platform::Object^ Value = args->Lookup("FingerPrint.CMDState");
					state = safe_cast<IPropertyValue^>(Value)->GetInt32();
					if (state == 0) {
						check_CMD_ProcessingOK->IsChecked = true;
					}

				}

				if (args->HasKey("FingerPrint.CMD")) {

					Platform::Object^ Value = args->Lookup("FingerPrint.CMD");
					bool doactFilledFingerLib = false;
					bool doactFilledSensorId = false;
					int CMD = safe_cast<IPropertyValue^>(Value)->GetInt32();
					if (CMD == Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerSensorInitialize")) {
						if (state == 0) {
							check_CMD_ProcessingOK->IsChecked = true;
							check_Device_Initialize->IsChecked = true;
							doactFilledFingerLib = true;
							doactFilledSensorId = true;
						}

			

					}
					else	if (CMD == Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerRegistration") || CMD == Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerAutoRegistration")) {


						if (args->HasKey("FingerPrint.RegististrationInformation")) {
							Platform::Object^ Value = args->Lookup("FingerPrint.RegististrationInformation");
							Platform::String^ state = safe_cast<IPropertyValue^>(Value)->GetString();
							recv_CMD_RegistrationState->Text = state;

						}

						if (args->HasKey("FingerPrint.RegististrationState")) {
							Platform::Object^ Value = args->Lookup("FingerPrint.RegististrationState");
							unsigned int state = safe_cast<IPropertyValue^>(Value)->GetInt32();
							if (state >= 5) {
								check_CMD_RegistrationState->IsChecked = true;
								if (args->HasKey("FingerPrint.Registration_FingerID")) {
									Platform::Object^ Value = args->Lookup("FingerPrint.Registration_FingerID");
									unsigned int state = safe_cast<IPropertyValue^>(Value)->GetInt32();
									wchar_t szbuf[50];
									swprintf(szbuf, sizeof(szbuf), L"%02d", state);
									recv_CMD_Registration_PageID->Text = ref new Platform::String(szbuf);

								}
								doactFilledFingerLib = true;


							}
							else  check_CMD_RegistrationState->IsChecked = false;
							// status->Text = state;

						}


						if (state == 0) {
							if (args->HasKey("FingerPrint.CHARUpLoad")) {
								Platform::Object^ Value = args->Lookup("FingerPrint.CHARUpLoad");
								Platform::Array<unsigned char>^ CHARDownLoadArray;
								safe_cast<IPropertyValue^>(Value)->GetUInt8Array(&CHARDownLoadArray);
								rcv_CMD_CHARUpload->Text = CHARDownLoadArray->ToString();

							}
						}
						else {
							rcv_CMD_CHARUpload->Text = "...";
						}

					}

					else	if (CMD == Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerVerifiying") || (CMD == Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerAutoVerifiying"))) {
						if (state == 0) {
							if (args->HasKey("FingerPrint.Search_FingerID")) {
								Platform::Object^ Value = args->Lookup("FingerPrint.Search_FingerID");
								unsigned int state = safe_cast<IPropertyValue^>(Value)->GetInt16();
								wchar_t szbuf[50];
								swprintf(szbuf, sizeof(szbuf), L"%02d", state);
								recv_CMD_Verifiying_PageId->Text = ref new Platform::String(szbuf);

							}

							if (args->HasKey("FingerPrint.Search_MatchScore")) {
								Platform::Object^ Value = args->Lookup("FingerPrint.Search_MatchScore");
								unsigned int matchscore = safe_cast<IPropertyValue^>(Value)->GetInt16();
								wchar_t szbuf[50];
								swprintf(szbuf, sizeof(szbuf), L"%02d", matchscore);
								recv_CMD_Verifiying_MatchScore->Text = ref new Platform::String(szbuf);
							}
						}
						else {
							recv_CMD_Verifiying_PageId->Text = "...";
							recv_CMD_Verifiying_MatchScore->Text = "...";
						}

					}

					else	if (CMD == Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerDeleteId")) {
						doactFilledFingerLib = true;
					}

					if (doactFilledFingerLib && args->HasKey("FingerPrint.FilledFingerLib")) {
						m_FilledFingerLib->Clear();

						Platform::Object^ Value = args->Lookup("FingerPrint.FilledFingerLib");
						Platform::Array<unsigned int>^ filledFingerLib;
						safe_cast<IPropertyValue^>(Value)->GetUInt32Array(&filledFingerLib);
						for (unsigned int i = 0; i < filledFingerLib->Length; i++) {
							m_FilledFingerLib->Append(filledFingerLib[i]);
						}
					}

					if (doactFilledSensorId && args->HasKey("FingerPrint.SensorID")) {
				
						m_SensorID->Clear();
						Platform::Object^ Value = args->Lookup("FingerPrint.SensorID");
						Platform::Array<unsigned char>^ SensorID;
						safe_cast<IPropertyValue^>(Value)->GetUInt8Array(&SensorID);
						for (unsigned int i = 0; i < SensorID->Length; i++) {
							m_SensorID->Append(SensorID[i]);
						}
					}
	

				}

				if (args->HasKey("FingerPrint.CMDTextState")) {

					Platform::Object^ Value = args->Lookup("FingerPrint.CMDTextState");
					Platform::String^ state = safe_cast<IPropertyValue^>(Value)->GetString();
					status->Text = state;

				}



			}));


};

void MainPage::OnstartStreaming(Platform::Object ^sender, FingerPrintSensor_SEN0188::SerDevice ^args) {
	Windows::ApplicationModel::Core::CoreApplication::MainView->CoreWindow->Dispatcher->RunAsync(
		CoreDispatcherPriority::Normal,
		ref new Windows::UI::Core::DispatchedHandler([this, args]()
			{
				check_Device_Connected->IsChecked = true;





			}));
};




void Sen0188TestApp::MainPage::OnstopStreaming(Platform::Object ^sender, Platform::String ^args) {
	Windows::ApplicationModel::Core::CoreApplication::MainView->CoreWindow->Dispatcher->RunAsync(
		CoreDispatcherPriority::Normal,
		ref new Windows::UI::Core::DispatchedHandler([this, args]()
			{
				check_Device_Connected->IsChecked = false;
			}));

}


void Sen0188TestApp::MainPage::CMD_Command_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	Button^ button = safe_cast<Button^> (sender);
	if (button != nullptr) {
		unsigned int CMD = 4;
		if (button->Name == "CMD_Initialize") {
			CMD = Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerSensorInitialize");
			m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));
			check_Device_Initialize->IsChecked = false;
			check_CMD_ProcessingOK->IsChecked = false;
		}
		else if (button->Name == "CMD_Verifiying") {
			CMD = Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerVerifiying");;
			m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));
		}
		else if (button->Name == "CMD_AutoVerifiying") {
			CMD = Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerAutoVerifiying");
			m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));
		}
		else if (button->Name == "CMD_Registration") {
			CMD = Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerRegistration");
			m_inputconfigoptions->Insert("UpdateState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(0))); // nicht aktiv setzen
			m_inputconfigoptions->Insert("FingerPrint.FingerID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(10000)));
			m_inputconfigoptions->Insert("UpdateState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(1))); // nicht aktiv setzen

			m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));
		}
		else if (button->Name == "CMD_AutoRegistration") {
			m_inputconfigoptions->Insert("UpdateState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(0))); // nicht aktiv setzen

			CMD = Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerAutoRegistration");
			m_inputconfigoptions->Insert("FingerPrint.FingerID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(10000)));

			m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));
			m_inputconfigoptions->Insert("UpdateState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(1))); // nicht aktiv setzen
		}
		else if (button->Name == "CMD_CHARUpload") {
			CMD = Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerCharUpLoad");
			m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));


		}
		else if (button->Name == "CMD_DELFingerID") {

			CMD = Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerDeleteId");
			m_inputconfigoptions->Insert("UpdateState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(0))); // nicht aktiv setzen
			m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));
			m_inputconfigoptions->Insert("FingerPrint.FingerID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(m_SelectedFingerID)));
			m_inputconfigoptions->Insert("UpdateState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(1))); // nicht aktiv setzen
		}
		else if (button->Name == "CMD_SetSensorID") {

			CMD = Connector_SEN0188_CMDs::getFingerPrintCmd("_doFingerWriteSensorID");

			m_inputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(CMD)));
			Platform::Array<uint8_t>^ Id = ref new Platform::Array<uint8_t> (32);
			Id[0] = 0x01;
			Id[1] = 0x02;

			m_inputconfigoptions->Insert("FingerPrint.PageID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt8(0)));
			m_inputconfigoptions->Insert("FingerPrint.SensorID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt8Array(Id)));
			m_inputconfigoptions->Insert("UpdateState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(1))); // nicht aktiv setzen
		}
		


		check_CMD_ProcessingOK->IsChecked = false;
		status->Text = "wait...";
	}


}


void Sen0188TestApp::MainPage::FingerLib_SelectionChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::SelectionChangedEventArgs^ e)
{

	ListView^ view = safe_cast<ListView^>(sender);
	if ((view != nullptr) && (view->SelectedItem!=nullptr) ) {
		unsigned int fingerItem = safe_cast<unsigned int>(view->SelectedItem);
		m_SelectedFingerID = fingerItem;
	}
}
