#include "pch.h"
#include "Connectors.h"
namespace FingerPrintSensor_SEN0188
{


	Connectors::Connectors()
	{
		this->m_Connectors = ref new Platform::Collections::Vector<FingerPrintSensor_SEN0188::Connector_SEN0188^>();


		m_SelectedIndex = -1;
	}


	Connectors::~Connectors()
	{
	}

	int Connectors::SelectedIndex::get() {
		return this->m_SelectedIndex;
	}
	void Connectors::SelectedIndex::set(int value) {
		this->m_SelectedIndex = value;
	}

	FingerPrintSensor_SEN0188::Connector_SEN0188^ Connectors::AddConnector(FingerPrintSensor_SEN0188::Connector_SEN0188^ connector) {

		this->m_Connectors->Append(connector);
		/*
		m_FailedEventRegister= listener->Failed += ref new Windows::Foundation::TypedEventHandler<Platform::Object ^, Platform::String ^>(this, &StationLib::SocketStationListeners::OnFailed);

		m_startStreamingEventRegister= listener->startStreaming += ref new Windows::Foundation::TypedEventHandler<Platform::Object ^, Windows::Networking::Sockets::StreamSocket ^>(this, &StationLib::SocketStationListeners::OnstartStreaming);

		m_stopStreamingEventRegister= listener->stopStreaming += ref new Windows::Foundation::TypedEventHandler<Platform::Object ^, Platform::String ^>(this, &StationLib::SocketStationListeners::OnstopStreaming);

		m_NotificationLevel= listener->NotificationLevel += ref new Windows::Foundation::TypedEventHandler<Platform::Object ^, int>(this, &StationLib::SocketStationListeners::OnNotificationLevel);
*/
		return connector;
	}

	FingerPrintSensor_SEN0188::Connector_SEN0188^  Connectors::getSelectedItem() {

		if (this->SelectedIndex == -1)
		{
			return nullptr;
		}
		if (this->SelectedIndex < m_Connectors->Size)	return(FingerPrintSensor_SEN0188::Connector_SEN0188^) m_Connectors->GetAt((unsigned int)SelectedIndex);
		else return nullptr;

	}

	bool Connectors::DeInitialization() {

		for (unsigned int i = 0; i < m_Connectors->Size; i++) {

			Connector_SEN0188^ listener = m_Connectors->GetAt(i);

			listener->stopProcessingPackagesAsync();

		}

		return false;
	}

}