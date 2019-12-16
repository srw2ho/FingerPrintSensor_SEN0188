#include "pch.h"
#include "ScanDevices.h"

using namespace Concurrency;
using namespace Platform;

namespace FingerPrintSensor_SEN0188 {
	ScanDevices::ScanDevices()
	{
		m_ScanSerialDevices = ref new ScanSerialDevices();

		m_AvailabeleDevices = ref new  Platform::Collections::Vector<FingerPrintSensor_SEN0188::SerDevice^>();


	}


	ScanDevices::~ScanDevices()
	{
	}




	Windows::Foundation::IAsyncAction ^ ScanDevices::readAvailableDevices()
	{
		return create_async([this]()
			->void {
				auto tsk = m_ScanSerialDevices->ListAvailableSerialDevicesAsync();

				Concurrency::create_task(tsk).then([this](task<Windows::Devices::Enumeration::DeviceInformationCollection ^ > serialDeviceCollectionTask)
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

			});


	}

	
	Concurrency::task<void>  ScanDevices::_readAvailableDevices()
	{
		auto tsk = m_ScanSerialDevices->ListAvailableSerialDevicesAsync();

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



}