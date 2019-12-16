#pragma once

#include "Connector_SEN0188.h"

namespace FingerPrintSensor_SEN0188
{

	[Windows::Foundation::Metadata::WebHostHidden]
	public ref class Connectors sealed
	{
		Windows::Foundation::Collections::IObservableVector<FingerPrintSensor_SEN0188::Connector_SEN0188^>^ m_Connectors;
		int m_SelectedIndex;
	public:
		Connectors();
		virtual ~Connectors();
		property int SelectedIndex
		{
			int get();
			void set(int value);
		}
		FingerPrintSensor_SEN0188::Connector_SEN0188^ getSelectedItem();
		FingerPrintSensor_SEN0188::Connector_SEN0188^ AddConnector(FingerPrintSensor_SEN0188::Connector_SEN0188^ connector);
		bool DeInitialization();
	};
}
