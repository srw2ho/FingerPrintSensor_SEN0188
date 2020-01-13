#include "pch.h"
#include "serialchunkreceiver.h"
#include "Connector_SEN0188.h"
#include "serialhelpers.h"

using namespace Platform;

using namespace Platform;
using namespace Windows::Foundation;


using namespace Windows::UI::Core;
using namespace Windows::System::Threading;

using namespace SerialCommunication;
using namespace concurrency;

namespace FingerPrintSensor_SEN0188
{

	Connector_SEN0188::Connector_SEN0188()
	{

		m_pSerialCom = ref new SerialCommunication::SerialCom();

		m_bProcessingPackagesStarted = false;

		m_pPackageCancelTaskToken = nullptr;
		m_FailedEventRegister = m_pSerialCom->Failed += ref new Windows::Foundation::TypedEventHandler<Platform::Object ^, Platform::Exception ^>(this, &FingerPrintSensor_SEN0188::Connector_SEN0188::OnFailed);

		m_OnDeviceConnected = m_pSerialCom->OnDeviceConnected += ref new Windows::Foundation::TypedEventHandler<Windows::Devices::SerialCommunication::SerialDevice ^, int>(this, &FingerPrintSensor_SEN0188::Connector_SEN0188::OnOnDeviceConnected);

		m_stopStreamingEventRegister = m_pSerialCom->stopStreaming += ref new Windows::Foundation::TypedEventHandler<Windows::Devices::SerialCommunication::SerialDevice ^, Platform::Exception ^>(this, &FingerPrintSensor_SEN0188::Connector_SEN0188::OnstopStreaming);
		m_SerialErrorReceivedEventRegister = m_pSerialCom->onSerialErrorReceived += ref new Windows::Foundation::TypedEventHandler<Windows::Devices::SerialCommunication::SerialDevice ^, Windows::Devices::SerialCommunication::ErrorReceivedEventArgs ^>(this, &FingerPrintSensor_SEN0188::Connector_SEN0188::OnonSerialErrorReceived);

		m_FailedConnectionCount = 0;
		m_inputconfigoptions = nullptr;
		m_outputconfigoptions = nullptr;
		m_pSensor = nullptr;



		m_pOrderEventPackageQueue = new OrderEventPackageQueue();
		m_Serdevice = nullptr;

	}


	Connector_SEN0188::~Connector_SEN0188()
	{

		m_pOrderEventPackageQueue->Flush();
		delete m_pOrderEventPackageQueue;

		m_pSerialCom->Failed -= m_FailedEventRegister;
		m_pSerialCom->stopStreaming -= m_stopStreamingEventRegister;
		m_pSerialCom->OnDeviceConnected -= m_OnDeviceConnected;
		m_pSerialCom->onSerialErrorReceived -= m_SerialErrorReceivedEventRegister;
		//	m_GPIOClientInOut->deleteAllGPIOPins();



		delete m_pSerialCom;

		m_pSerialCom = nullptr;

	}
	void Connector_SEN0188::startProcessingPackages(FingerPrintSensor_SEN0188::SerDevice^ serDev, Windows::Foundation::Collections::IPropertySet^ inputconfigoptions, Windows::Foundation::Collections::IPropertySet^ outputconfigoptions)
	{
		//m_inputconfigoptions = inputconfigoptions;
		m_outputconfigoptions = outputconfigoptions;
		m_inputconfigoptions = inputconfigoptions;

		if (m_bProcessingPackagesStarted) return;
		bool doStart = false;
		if (serDev != nullptr) {
			m_Serdevice = serDev;
			doStart = true;
		}


		if (!doStart) return;

		if (m_Serdevice == nullptr) return;

		m_bProcessingPackagesStarted = true;
		m_ProcessingState = ProcessingState::_doWaitForCommand;

		restetFingerPrintCommand();

		m_pSerialCom->conntectToDevice(m_Serdevice->Id);

		m_InputConfigOptionsMapChanged = inputconfigoptions->MapChanged += ref new Windows::Foundation::Collections::MapChangedEventHandler<Platform::String ^, Platform::Object ^>(this, &FingerPrintSensor_SEN0188::Connector_SEN0188::OnMapChanged);


		if (m_pPackageCancelTaskToken != nullptr)
		{
			delete m_pPackageCancelTaskToken;

		}
		m_pPackageCancelTaskToken = new concurrency::cancellation_token_source();


		m_ProcessingPackagesTsk = create_task(doProcessPackages()).then([this](task<void> previous)
			{
				m_bProcessingPackagesStarted = false;
				try {
					previous.get();
				}
				catch (Exception^ exception)
				{

				}

			});

	}

	Windows::Foundation::IAsyncAction ^ Connector_SEN0188::stopProcessingPackagesAsync() {

		return create_async([this]()
			{
				stopProcessingPackages();
			});
	}

	Windows::Foundation::IAsyncAction ^ Connector_SEN0188::startProcessingPackagesAsync(FingerPrintSensor_SEN0188::SerDevice^ serDev, Windows::Foundation::Collections::IPropertySet^ inputconfigoptions, Windows::Foundation::Collections::IPropertySet^ outputconfigoptions) {


		return create_async([this, serDev, inputconfigoptions, outputconfigoptions]()
			{
				startProcessingPackages(serDev, inputconfigoptions, outputconfigoptions);

			});

	}

	void Connector_SEN0188::stopProcessingPackages()
	{
		if (!m_bProcessingPackagesStarted) return;
		try {

			if (m_inputconfigoptions != nullptr) {
				m_inputconfigoptions->MapChanged -= m_InputConfigOptionsMapChanged;
			}

			m_bProcessingPackagesStarted = false;
			m_pSerialCom->CancelConnections();// alle Connections schliessen

			cancelPackageAsync();


			//	Sleep(100);
			  // Darf nicht in UI-Thread aufgerufen werden-> Blockiert UI-Thread-> gibt Exception
			m_ProcessingPackagesTsk.wait();

			m_outputconfigoptions->Insert("State", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(0)));

			NotifyChangeState(this, m_outputconfigoptions);


		}
		catch (Exception^ exception)
		{
			bool b = true;

		}


	}



	bool Connector_SEN0188::doFingerAutoRegistration()
	{
		uint16_t storedPageSeqNumber;
		uint8_t repeatedRegistration = 1;
		if (m_pPacket->getFingerId() >= 10000) {
			storedPageSeqNumber = m_pSensor->getNextFreeFingerId(); // next free FingerID
			repeatedRegistration = 1;
		}
		else
		{
			repeatedRegistration = 1;

			storedPageSeqNumber = m_pPacket->getFingerId();
		}
		
	
		uint8_t numberofTimes = 2;
		m_CMDState = m_pSensor->AutoLogin(31, numberofTimes, storedPageSeqNumber, repeatedRegistration); //download character file or template
		if (m_CMDState == FINGERPRINT_OK) {
			m_pSensor->upateFingerStateById(storedPageSeqNumber, 1);

			if (m_CMDState == FINGERPRINT_OK) {
				SensorPackageByteArray UpChar; // Platform::Array to std::Array

				m_CMDState = m_CMDState = m_pSensor->UpChar(1, UpChar); //Upload character file or template
				if (m_CMDState == FINGERPRINT_OK) {

					if (UpChar.size() > 0) {
						Platform::Array<unsigned char>^ data = ref new Platform::Array<unsigned char>(&UpChar[0], (unsigned int)UpChar.size());
						m_outputconfigoptions->Insert("FingerPrint.CHARUpLoad", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt8Array(data))); // CHARArray to map

					}
	
					m_outputconfigoptions->Insert("FingerPrint.Registration_FingerID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(storedPageSeqNumber)));

					std::vector<uint32_t> filled;

					m_pSensor->getfilledFingerLib(filled);
					if (filled.size() > 0) {
						Platform::Array<uint32_t>^ filleddata = ref new Platform::Array<uint32_t>(&filled[0], (unsigned int)filled.size());
						m_outputconfigoptions->Insert("FingerPrint.FilledFingerLib", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt32Array(filleddata))); // filled FingerLib Platform::Array

					}
	

					doFingerNotifyRegistrationInformation("Registration is completed done", 5);// O.K-State
				}
			}

		}

		m_ProcessingState = ProcessingState::_doFingerNotifyEvent;

		return false;
	}

	bool Connector_SEN0188::doFingerRegistration()
	{

		switch (m_InnerCMDProcessingState)
		{
		case 0: //reading first image
			m_CMDState = m_pSensor->GetImage(); //download character file or template
			if (m_CMDState == FINGERPRINT_OK) {
				m_CMDState = m_pSensor->Img2Tz(1); //download character file or template
				if (m_CMDState == FINGERPRINT_OK) {
					m_InnerCMDProcessingState++;
					doFingerNotifyRegistrationInformation("please remove finger from sensor", m_InnerCMDProcessingState);
				}
			}
			else {
				doFingerNotifyRegistrationInformation("please set finger 1 to sensor", m_InnerCMDProcessingState);
			}
			break;
		case 1: //remove finger from sensor
			m_CMDState = m_pSensor->GetImage(); //download character file or template
			if (m_CMDState == FINGERPRINT_NOFINGER) {
				m_InnerCMDProcessingState++;
				doFingerNotifyRegistrationInformation("please set finger 2 to sensor", m_InnerCMDProcessingState);

			}
			else {
				doFingerNotifyRegistrationInformation("please remove finger from sensor", m_InnerCMDProcessingState);
			}
			break;
		case 2: //reading second image
			m_CMDState = m_pSensor->GetImage(); //download character file or template
			if (m_CMDState == FINGERPRINT_OK) {
				m_CMDState = m_pSensor->Img2Tz(2); //download character file or template
				if (m_CMDState == FINGERPRINT_OK) {
					m_InnerCMDProcessingState++;
					doFingerNotifyRegistrationInformation("create finger Template Model", m_InnerCMDProcessingState);

				}
				else {
					m_InnerCMDProcessingState = 0;
				}
			}
			else {
				doFingerNotifyRegistrationInformation("please set finger 2 to sensor", m_InnerCMDProcessingState);
			}
			break;
		case 3: // Reg Modell
			m_CMDState = m_pSensor->RegModel(); //download character file or template
			if (m_CMDState == FINGERPRINT_OK) {
				doFingerNotifyRegistrationInformation("store into library", m_InnerCMDProcessingState);

				m_InnerCMDProcessingState++;
			}
			else  m_InnerCMDProcessingState = 0;
			break;


		case 4: //store image
			uint16_t storedSeqNumber;

			uint8_t repeatedRegistration = 1;
			if (m_pPacket->getFingerId() >= 10000) {

				storedSeqNumber = m_pSensor->getNextFreeFingerId(); // next free FingerID
				repeatedRegistration = 0;
			}
			else
			{
				storedSeqNumber = m_pPacket->getFingerId();
				repeatedRegistration = 1;

			}


			m_CMDState = m_pSensor->Store(1, storedSeqNumber); //store Finger Char to FingerStor with storedSeqNumber
			if (m_CMDState == FINGERPRINT_OK) {
				m_pSensor->upateFingerStateById(storedSeqNumber, 1);
				doFingerNotifyRegistrationInformation("Uploading finger CHARArray", m_InnerCMDProcessingState);
				SensorPackageByteArray UpChar; // Platform::Array to std::Array
				m_CMDState = m_CMDState = m_pSensor->UpChar(1, UpChar); //Upload character file or template
				if (m_CMDState == FINGERPRINT_OK) {
					if (UpChar.size() > 0) {
						Platform::Array<unsigned char>^ data = ref new Platform::Array<unsigned char>(&UpChar[0], (unsigned int)UpChar.size());

						m_outputconfigoptions->Insert("FingerPrint.CHARUpLoad", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt8Array(data))); // CHARArray to map

						m_outputconfigoptions->Insert("FingerPrint.FingerID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(storedSeqNumber)));
						std::vector<uint32_t> filled;

						m_pSensor->getfilledFingerLib(filled);
						if (filled.size() > 0) {
							Platform::Array<uint32_t>^ filleddata = ref new Platform::Array<uint32_t>(&filled[0], (unsigned int)filled.size());
							m_outputconfigoptions->Insert("FingerPrint.FilledFingerLib", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt32Array(filleddata))); // filled FingerLib Platform::Array

						}
						doFingerNotifyRegistrationInformation("Registration is completed done", 5);// O.K-State
					}




				}

				m_ProcessingState = ProcessingState::_doFingerNotifyEvent;

			}
			else {
				m_ProcessingState = ProcessingState::_doFingerNotifyEvent;
			}


		}


		return false;

	}

	bool Connector_SEN0188::doFingerVerifiying()
	{
		m_CMDState = m_pSensor->GetImage();
		if (m_CMDState == FINGERPRINT_OK) {

			m_CMDState = m_pSensor->Img2Tz(1);
			if (m_CMDState == FINGERPRINT_OK) {
				uint16_t PageId;
				uint16_t  MatchScore;

				m_CMDState = m_pSensor->Search(1, 0, 1000, PageId, MatchScore); //download character file or template
				if (m_CMDState == FINGERPRINT_OK) {

					m_outputconfigoptions->Insert("FingerPrint.FingerID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(PageId)));
					m_outputconfigoptions->Insert("FingerPrint.Search_MatchScore", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(MatchScore)));

				}
			}
		}

		m_ProcessingState = ProcessingState::_doFingerNotifyEvent;

		return false;
	}

	bool Connector_SEN0188::doAutoFingerVerifiying()
	{
		uint16_t PageId;
		uint16_t  MatchScore;
		m_CMDState = m_pSensor->AutoSearch(31, 0, 1000, PageId, MatchScore); //download character file or template
		if (m_CMDState == FINGERPRINT_OK) {

			m_outputconfigoptions->Insert("FingerPrint.FingerID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(PageId)));
			m_outputconfigoptions->Insert("FingerPrint.Search_MatchScore", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(MatchScore)));

		}

		m_ProcessingState = ProcessingState::_doFingerNotifyEvent;

		return false;
	}
	bool Connector_SEN0188::doFingerCharDownLoad()
	{
		if (this->m_pPacket != nullptr) {

			uint16_t storedSeqNumber = -1;
			storedSeqNumber = m_pPacket->getFingerId();

			if (storedSeqNumber != -1) {

				SensorPackageByteArray& DownChar = m_pPacket->getSensorPackageByteArray();

				m_CMDState = m_pSensor->DownChar(1, DownChar); //download character file or template
				if (m_CMDState == FINGERPRINT_OK) {

					m_CMDState = m_pSensor->Store(1, storedSeqNumber); //download character file or template
					if (m_CMDState == FINGERPRINT_OK) {
						m_pSensor->upateFingerStateById(storedSeqNumber, 1);

						m_outputconfigoptions->Insert("FingerPrint.Registration_FingerID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt16(storedSeqNumber)));

						std::vector<uint32_t> filled;

						m_pSensor->getfilledFingerLib(filled);
						if (filled.size() > 0) {
							Platform::Array<uint32_t>^ filleddata = ref new Platform::Array<uint32_t>(&filled[0], (unsigned int)filled.size());
							m_outputconfigoptions->Insert("FingerPrint.FilledFingerLib", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt32Array(filleddata))); // filled FingerLib Platform::Array

						}
					}
				}
			}

		}

		m_ProcessingState = ProcessingState::_doFingerNotifyEvent;

		return false;



	}

	bool Connector_SEN0188::doFingerCharUpLoad()
	{
		if (m_inputconfigoptions->HasKey("FingerPrint.CHARUpLoad")) {

			SensorPackageByteArray DownChar; // Platform::Array to std::Array

			m_CMDState = m_pSensor->UpChar(1, DownChar); //download character file or template
			if (m_CMDState == FINGERPRINT_OK) {
				if (DownChar.size() > 0) {
					Platform::Array<unsigned char>^ data = ref new Platform::Array<unsigned char>(&DownChar[0], (unsigned int)DownChar.size());

					m_outputconfigoptions->Insert("FingerPrint.CHARUpLoad", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt8Array(data))); // CHARArray to map


				}


			}


		}
		m_ProcessingState = ProcessingState::_doFingerNotifyEvent;
		return false;
	}


	bool Connector_SEN0188::doFingerWriteSensorID()
	{

		m_CMDState = m_pSensor->WriteNotepad(m_pPacket->getPageId(), m_pPacket->getSensorPackageByteArray());
		if (m_CMDState == FINGERPRINT_OK) {
	
		if (m_pSensor->getSensorID().size() > 0) {
				SensorPackageByteArray& sensorId = m_pPacket->getSensorPackageByteArray();
				Platform::Array<uint8_t>^ filleddata = ref new Platform::Array<uint8_t>(&sensorId[0], (unsigned int)sensorId.size());
				m_outputconfigoptions->Insert("FingerPrint.SensorID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt8Array(filleddata))); // filled FingerLib Platform::Array
			}

		}


		m_ProcessingState = ProcessingState::_doFingerNotifyEvent;
		return false;
	}

	bool Connector_SEN0188::doFingerSensorInitialize()
	{


		m_CMDState = m_pSensor->SensorInitialize();
		if (m_CMDState == FINGERPRINT_OK) {

			std::vector<uint32_t> filled;

			m_pSensor->getfilledFingerLib(filled);
			if (filled.size() > 0) {
				Platform::Array<uint32_t>^ filleddata = ref new Platform::Array<uint32_t>(&filled[0], (unsigned int)filled.size());
				m_outputconfigoptions->Insert("FingerPrint.FilledFingerLib", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt32Array(filleddata))); // filled FingerLib Platform::Array
			}

			if (m_pSensor->getSensorID().size() > 0) {
				SensorPackageByteArray& sensorId = m_pSensor->getSensorID();
				Platform::Array<uint8_t>^ filleddata = ref new Platform::Array<uint8_t>(&sensorId[0], (unsigned int)sensorId.size());
				m_outputconfigoptions->Insert("FingerPrint.SensorID", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt8Array(filleddata))); // filled FingerLib Platform::Array
			}

		}


		m_ProcessingState = ProcessingState::_doFingerNotifyEvent;
		return false;
	}

	bool Connector_SEN0188::doFingerDeleteId()
	{
		if (m_pPacket != nullptr) {
			uint16_t todeleteID = m_pPacket->getFingerId();
			bool doEmpty = todeleteID >= 10000;
			if (doEmpty) {
				m_CMDState = m_pSensor->Empty();
			}
			else {
				m_CMDState = m_pSensor->DeleteChar(todeleteID, 1);
			}

			if (m_CMDState == FINGERPRINT_OK) {
				if (doEmpty) {
					m_pSensor->ReadCompleteConList();
				}
				else {
					m_pSensor->upateFingerStateById(todeleteID, 0);
				}

				std::vector<uint32_t> filled;

				m_pSensor->getfilledFingerLib(filled);
				if (filled.size() > 0) {
					Platform::Array<uint32_t>^ filleddata = ref new Platform::Array<uint32_t>(&filled[0], (unsigned int)filled.size());
					m_outputconfigoptions->Insert("FingerPrint.FilledFingerLib", dynamic_cast<PropertyValue^>(PropertyValue::CreateUInt32Array(filleddata))); // filled FingerLib Platform::Array

				}
				

			}

		}


		m_ProcessingState = ProcessingState::_doFingerNotifyEvent;

		return false;
	}



	bool Connector_SEN0188::doFingerNotifyRegistrationInformation(Platform::String^ info1, unsigned int state)
	{
		bool domsg = false;

		m_outputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(m_pPacket->getProcessingCMD())));


		if (m_inputconfigoptions->HasKey("FingerPrint.RegististrationInformation")) {

			Platform::Object^ Value = m_inputconfigoptions->Lookup("FingerPrint.RegististrationInformation");
			Platform::String^ previousInfo = safe_cast<IPropertyValue^>(Value)->GetString();
			if (previousInfo != info1) {
				domsg = true;
			}
		}
		else domsg = true;

		if (m_inputconfigoptions->HasKey("FingerPrint.RegististrationState"))
		{
			Platform::Object^ Value = m_inputconfigoptions->Lookup("FingerPrint.RegististrationState");
			unsigned int previousState = safe_cast<IPropertyValue^>(Value)->GetUInt32();
			if (previousState != state) {
				domsg = true;
			}
		}
		else domsg = true;


		if (domsg) {
			m_outputconfigoptions->Insert("FingerPrint.RegististrationInformation", dynamic_cast<PropertyValue^>(PropertyValue::CreateString(info1)));
			m_outputconfigoptions->Insert("FingerPrint.RegististrationState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(state)));
			this->NotifyChangeState(this, m_outputconfigoptions);  // notify to caller
		}






		return false;
	}

	bool Connector_SEN0188::doFingerNotifyEvent()
	{
		m_outputconfigoptions->Insert("FingerPrint.CMDState", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt16(m_CMDState)));

		std::string state = m_pSensor->getCmdStringByState(m_CMDState);
		Platform::String^ cmd = SerialHelpers::StringFromAscIIChars(state.c_str());

		m_outputconfigoptions->Insert("FingerPrint.CMDTextState", dynamic_cast<PropertyValue^>(PropertyValue::CreateString(cmd)));

		m_outputconfigoptions->Insert("FingerPrint.CMDInstruction", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt16(m_pSensor->getInstruction())));

		m_outputconfigoptions->Insert("FingerPrint.CMD", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt32(m_pPacket->getProcessingCMD())));




  	this->NotifyChangeState(this, m_outputconfigoptions);  // notify to caller

		if (m_pPacket != nullptr) {
			delete m_pPacket;
			m_pPacket = nullptr;
		}


		m_ProcessingState = ProcessingState::_doWaitForCommand;
		return false;
	}

	bool Connector_SEN0188::dowaitForCommand(DWORD waitTime)
	{
		m_pPacket = nullptr;
		size_t size = m_pOrderEventPackageQueue->waitForPacket(&m_pPacket, waitTime);
		if (m_pPacket != nullptr) {
			unsigned int cmd = m_pPacket->getProcessingCMD();
			//  m_ProcessingCMD = cmd;
			m_ProcessingState = cmd;
			m_InnerCMDProcessingState = 0;
			return true;
		}
		else
		{
			return false;
		}



	}




	Concurrency::task<void> Connector_SEN0188::doProcessPackages()
	{
		auto token = m_pPackageCancelTaskToken->get_token();


		auto tsk = create_task([this, token]() -> void

			{
				bool dowhile = true;
				//	DWORD waitTime = INFINITE; // INFINITE ms Waiting-Time
				DWORD waitTime = 500; // 200 ms Waiting-Time

				bool doInit = true;
				while (dowhile) {
					try {
						if (m_pSensor != nullptr) {

							switch (m_ProcessingState) {

							case ProcessingState::_doWaitForCommand:
								dowaitForCommand(waitTime);

								break;


							case ProcessingState::_doFingerSensorInitialize:
								doFingerSensorInitialize();
								break;

							case ProcessingState::_doFingerWriteSensorID:
								doFingerWriteSensorID();
								break;
								
							case ProcessingState::_doFingerRegistration:
								doFingerRegistration();
								break;


							case ProcessingState::_doFingerVerifiying:
								doFingerVerifiying();
								break;

							case ProcessingState::_doFingerAutoVerifiying:
								doAutoFingerVerifiying();
								break;
							case ProcessingState::_doFingerAutoRegistration:
								doFingerAutoRegistration();
								break;


							case ProcessingState::_doFingerCharDownLoad:
								doFingerCharDownLoad();
								break;

							case ProcessingState::_doFingerCharUpLoad:
								doFingerCharUpLoad();
								break;

							case ProcessingState::_doFingerDeleteId:
								doFingerDeleteId();
								break;

							case ProcessingState::_doFingerNotifyEvent:
								doFingerNotifyEvent();
								break;

							}

						}

						if (token.is_canceled()) {
							cancel_current_task();
						}

					}
					catch (task_canceled&)
					{
						dowhile = false;

					}
					catch (const std::exception&)
					{
						dowhile = false;

					}

				}

			}, token);

		return tsk;
	}

	void Connector_SEN0188::cancelPackageAsync()
	{
		if (m_pSensor != nullptr) {
			m_pSensor->cancelwaitForPacket();
		}

		if (m_pPackageCancelTaskToken != nullptr) {
			m_pPackageCancelTaskToken->cancel();
		}
		m_pOrderEventPackageQueue->cancelwaitForPacket();



	}



	void FingerPrintSensor_SEN0188::Connector_SEN0188::OnFailed(Platform::Object ^sender, Platform::Exception ^args)
	{// Trying to connect failed
	//	throw ref new Platform::NotImplementedException();
		Platform::String^ err = ref new String();
		err = args->Message;
		m_FailedConnectionCount = m_FailedConnectionCount + 1;
		this->Failed(this, err);
	}


	void FingerPrintSensor_SEN0188::Connector_SEN0188::OnOnDeviceConnected(Windows::Devices::SerialCommunication::SerialDevice ^sender, int args)
	{

		// m_pSensor Object will be deleted from m_pSerialCom-Object
		// Communication_Propertys to Serial Device
		m_Serdevice->setCOMPropertysToSerialDevice(sender);

		m_pSensor = new FingerPrintSensor_SEN0188::Sensor(sender);
		// m_pSensor Object will be deleted from m_pSerialCom-Object

		SerialChunkReceiverWinRT ^ pBME280ChunkReceiverWinRT = ref new SerialChunkReceiverWinRT(m_pSensor); // wrapper for SocketChunkReceiver and its derived
		m_pSerialCom->AddChunkReceiver(pBME280ChunkReceiverWinRT); // 
		pBME280ChunkReceiverWinRT->geSocketChunkReceiver()->StartService(); // started communictation with sending getEcho-Command to SEN0188-Sensor!!!

		this->startStreaming(this, m_Serdevice);
		//m_outputconfigoptions->Insert("FingerPrint.SensorConnected", dynamic_cast<PropertyValue^>(PropertyValue::CreateInt16(1)));

		//this->NotifyChangeState(this, m_outputconfigoptions);  // notify to caller

	}






	void FingerPrintSensor_SEN0188::Connector_SEN0188::OnstopStreaming(Windows::Devices::SerialCommunication::SerialDevice ^sender, Platform::Exception ^args)
	{

		stopProcessingPackages(); //SerialDevice will be deleted-> stopProcessing is necesary
		m_pSensor = nullptr;

		Platform::String^ err = ref new String();
		if (args != nullptr) {
			err = args->Message;
			m_FailedConnectionCount = m_FailedConnectionCount + 1;
		}
		else err = "";



		this->stopStreaming(this, err);
		//Connection  error by remote or stopped by user (CancelConnections)
		//throw ref new Platform::NotImplementedException();
	}


	OrderEventPackage*  Connector_SEN0188::startFingerPrintCommand(OrderEventPackage* ppacket) {

		// OrderEventPackage* ppacket = nullptr;

			 //if (cmd != ProcessingState::_doWaitForCommand) 
		{



			m_pOrderEventPackageQueue->PushPacket(ppacket);

			//			m_ProcessingCMD = ppacket->getProcessingCMD();
			m_ProcessingState = ppacket->getProcessingCMD();
			m_InnerCMDProcessingState = 0;

			return ppacket;
		}

		return ppacket;
	}

	bool Connector_SEN0188::restetFingerPrintCommand() {

		m_pOrderEventPackageQueue->Flush();

		m_ProcessingState = ProcessingState::_doWaitForCommand;


		return false;
	}


	void FingerPrintSensor_SEN0188::Connector_SEN0188::OnMapChanged(Windows::Foundation::Collections::IObservableMap<Platform::String ^, Platform::Object ^> ^sender, Windows::Foundation::Collections::IMapChangedEventArgs<Platform::String ^> ^event)
	{
		auto propertyset = sender;

		//	Platform::String^ folder, int fps, int height, int width, int64_t bit_rate, PropertySet^ ffmpegOutputOptions, Platform::String^ outputformat, double deletefilesOlderFilesinHours, double RecordingInHours

		if (propertyset->HasKey("UpdateState")) {
			Platform::Object^ Value = propertyset->Lookup("UpdateState");
			int state = safe_cast<IPropertyValue^>(Value)->GetInt32();
			if (state != 1) { // es folgend noch weitere
				return;
			}
		}

		OrderEventPackage* pPacket = new OrderEventPackage();
		if (propertyset->HasKey("FingerPrint.CMD")) {

			bool doInsert = false;
			Platform::Object^ Value = propertyset->Lookup("FingerPrint.CMD");
			int state = safe_cast<IPropertyValue^>(Value)->GetInt32();
			ProcessingState processstate = ProcessingState::_doWaitForCommand;
			if (state == _doFingerRegistration) {
				processstate = ProcessingState::_doFingerRegistration;
				if (propertyset->HasKey("FingerPrint.FingerID")) {
		//			Platform::Array<unsigned char>^ SensorId;
					Platform::Object^ Value = propertyset->Lookup("FingerPrint.FingerID");
					uint16_t fingerId = safe_cast<IPropertyValue^>(Value)->GetUInt16();
					pPacket->getFingerId() = fingerId;

				}
				else 	pPacket->getFingerId() = 10000;
				doInsert = true;
			}

			else if (state == _doFingerVerifiying) {
				processstate = ProcessingState::_doFingerVerifiying;
				doInsert = true;
			}
			else if (state == _doFingerAutoVerifiying) {

				processstate = ProcessingState::_doFingerAutoVerifiying;
				doInsert = true;
			}

			else if (state == _doFingerSensorInitialize) {
				processstate = ProcessingState::_doFingerSensorInitialize;
				doInsert = true;
			}
			else if (state == _doFingerWriteSensorID) {
				processstate = ProcessingState::_doFingerWriteSensorID;
				if (propertyset->HasKey("FingerPrint.SensorID")) {
					Platform::Array<unsigned char>^ SensorId;
					Platform::Object^ Value = propertyset->Lookup("FingerPrint.SensorID");


					safe_cast<IPropertyValue^>(Value)->GetUInt8Array(&SensorId);
					if (SensorId->Length > 0) {

						if (propertyset->HasKey("FingerPrint.PageID")) {
							Platform::Object^ Value = propertyset->Lookup("FingerPrint.PageID");
							uint8_t pageId = safe_cast<IPropertyValue^>(Value)->GetUInt8();
							pPacket->getPageId() = pageId;
							SensorPackageByteArray DownChar(SensorId->begin(), SensorId->end()); // Platform::Array to std::Array
							pPacket->getSensorPackageByteArray() = DownChar;
							doInsert = true;
						}
					}

				}

			}
			else if (state == _doFingerAutoRegistration) {
				processstate = ProcessingState::_doFingerAutoRegistration;
				if (propertyset->HasKey("FingerPrint.FingerID")) {
					Platform::Array<unsigned char>^ SensorId;
					Platform::Object^ Value = propertyset->Lookup("FingerPrint.FingerID");
					uint16_t fingerId = safe_cast<IPropertyValue^>(Value)->GetUInt16();
					pPacket->getFingerId() = fingerId;

				}
				else 	pPacket->getFingerId() = 10000;
				doInsert = true;
			}
			else if (state == _doFingerCharDownLoad) {
				processstate = ProcessingState::_doFingerCharDownLoad;

				uint16_t storedSeqNumber = -1;
				if (propertyset->HasKey("FingerPrint.CHARDownLoad")) {
					Platform::Object^ Value = propertyset->Lookup("FingerPrint.CHARDownLoad");
					if (m_inputconfigoptions->HasKey("FingerPrint.FingerID"))
					{
						Platform::Object^ Value = propertyset->Lookup("FingerPrint.FingerID");
						storedSeqNumber = safe_cast<IPropertyValue^>(Value)->GetUInt16();
					}
					if (storedSeqNumber != -1) {
						pPacket->getFingerId() = storedSeqNumber;

						Platform::Array<unsigned char>^ CHARDownLoadArray;
						safe_cast<IPropertyValue^>(Value)->GetUInt8Array(&CHARDownLoadArray);
						if (CHARDownLoadArray->Length > 0) {

							SensorPackageByteArray DownChar(CHARDownLoadArray->begin(), CHARDownLoadArray->end()); // Platform::Array to std::Array
							pPacket->getSensorPackageByteArray() = DownChar;
							doInsert = true;

						}
					}
				}

			}
			else if (state == _doFingerCharUpLoad) {
				processstate = ProcessingState::_doFingerCharUpLoad;
				doInsert = true;
			}
			else if (state == _doFingerDeleteId) {
				processstate = ProcessingState::_doFingerDeleteId;
				if (propertyset->HasKey("FingerPrint.FingerID")) {
					Platform::Object^ Value = propertyset->Lookup("FingerPrint.FingerID");

					unsigned int todeleteID = safe_cast<IPropertyValue^>(Value)->GetUInt16();
					pPacket->getFingerId() = todeleteID;
					doInsert = true;
				}

			}


			if (!doInsert) {
				delete pPacket;
			}
			else {
				this->m_outputconfigoptions->Clear(); // alle löschen
				pPacket->getProcessingCMD() = processstate;
				startFingerPrintCommand(pPacket);
			}

		}



	}

	void FingerPrintSensor_SEN0188::Connector_SEN0188::OnonSerialErrorReceived(Windows::Devices::SerialCommunication::SerialDevice ^sender, Windows::Devices::SerialCommunication::ErrorReceivedEventArgs ^args)
	{
		Platform::String^ err = ref new String();

		err = "OnSerialErrorReceived: "  + args->Error.ToString();
		m_FailedConnectionCount = m_FailedConnectionCount + 1;
		this->Failed(this, err);

	}


}





