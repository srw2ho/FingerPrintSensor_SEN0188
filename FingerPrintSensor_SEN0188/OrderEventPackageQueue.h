#pragma once
#include <queue>
#include <sensor.h>
#include "OrderEventPackageQueue.h"


namespace FingerPrintSensor_SEN0188
{
	class   OrderEventPackage {

	private:
    unsigned int m_ProcessingCMD;
    SensorPackageByteArray m_SensorPackageByteArray;
    uint16_t m_FingerId;
	uint8_t m_PageId;
	public:
		OrderEventPackage(void);
		OrderEventPackage(unsigned int PinNo);
		virtual ~OrderEventPackage();
    unsigned int& getProcessingCMD();
    
    
    // Parameters

    uint16_t& getFingerId() { return m_FingerId; };

	uint8_t& getPageId() { return m_PageId; };

    SensorPackageByteArray& getSensorPackageByteArray() {return m_SensorPackageByteArray;};



	};

	class  OrderEventPackageQueue

	{
	private:
		HANDLE m_hWriteEvent;

		std::vector<OrderEventPackage*>* m_packetQueue;
		CRITICAL_SECTION m_CritLock;

	public:

		OrderEventPackageQueue(void);
		virtual ~OrderEventPackageQueue();

		virtual void cancelwaitForPacket();

		virtual void Flush();
		virtual void PushPacket(OrderEventPackage* ppacket);

		virtual size_t waitForPacket(OrderEventPackage** Packet, DWORD waitTime = INFINITE);

		virtual OrderEventPackage* PopPacket();
		void Lock();

		void UnLock();
	};

}


