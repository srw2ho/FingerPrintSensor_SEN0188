#include "pch.h"
#include "OrderEventPackageQueue.h"


namespace FingerPrintSensor_SEN0188
{

	OrderEventPackage::OrderEventPackage()
	{
    m_ProcessingCMD = -1;
	m_PageId = 0;

	}
  OrderEventPackage::OrderEventPackage(unsigned int PinNo)
	{
    m_ProcessingCMD = PinNo;

	}

  unsigned int& OrderEventPackage::getProcessingCMD() { return m_ProcessingCMD; };



  OrderEventPackage::~OrderEventPackage()
	{
	  m_SensorPackageByteArray.clear();
	}


	OrderEventPackageQueue::OrderEventPackageQueue()
	{
		m_packetQueue = new std::vector<OrderEventPackage*>();
		InitializeCriticalSection(&m_CritLock);
		m_hWriteEvent = CreateEvent(
			NULL,               // default security attributes
			TRUE,               // manual-reset event
			FALSE,              // initial state is nonsignaled
			nullptr
			//TEXT("WriteEvent")  // object name
		);
	}


	OrderEventPackageQueue::~OrderEventPackageQueue()
	{
		this->Flush();
		delete m_packetQueue;
		DeleteCriticalSection(&m_CritLock);
		CloseHandle(m_hWriteEvent);
	}

	void OrderEventPackageQueue::cancelwaitForPacket() {
		::SetEvent(m_hWriteEvent);
	}

	void OrderEventPackageQueue::Flush()
	{
		this->Lock();
		while (!m_packetQueue->empty())
		{
      OrderEventPackage* Packet = PopPacket();
			delete Packet;
		}
		this->UnLock();
	};


	void OrderEventPackageQueue::PushPacket(OrderEventPackage* ppacket) {


		this->Lock();
		m_packetQueue->push_back(ppacket);
		::SetEvent(m_hWriteEvent);

		this->UnLock();

	};

  OrderEventPackage* OrderEventPackageQueue::PopPacket() {
    OrderEventPackage*pPacketRet = nullptr;
		this->Lock();
		if (!m_packetQueue->empty())
		{
			pPacketRet = m_packetQueue->front();
			//	avPacket = m_packetQueue.front();
			m_packetQueue->erase(m_packetQueue->begin());

			//		m_packetQueue->pop();
			::ResetEvent(m_hWriteEvent);
		}
		this->UnLock();
		return pPacketRet;
	};


	size_t OrderEventPackageQueue::waitForPacket(OrderEventPackage** Packet, DWORD waitTime) {

		this->Lock();
		*Packet = nullptr;
		size_t size = m_packetQueue->size();
		if (size == 0) {//no packet then wait for
			this->UnLock();
			DWORD dwWaitResult = WaitForSingleObject(m_hWriteEvent, // event handle
				waitTime);    // indefinite wait
			if (dwWaitResult == WAIT_OBJECT_0) {}
			this->Lock();
			size = m_packetQueue->size();
			if (size > 0) {
				//	*Packet = m_packetQueue->at(size-1);
				//	DeleteFirstPackets(size - 2);
				*Packet = m_packetQueue->front();
				m_packetQueue->erase(m_packetQueue->begin());
			}
			::ResetEvent(m_hWriteEvent);
			this->UnLock();


		}
		else {
			*Packet = m_packetQueue->front();
			m_packetQueue->erase(m_packetQueue->begin());

			this->UnLock();
		}

		return size;
	};


	void OrderEventPackageQueue::Lock() {
		EnterCriticalSection(&m_CritLock);
	}

	void OrderEventPackageQueue::UnLock() {
		LeaveCriticalSection(&m_CritLock);
	}
}