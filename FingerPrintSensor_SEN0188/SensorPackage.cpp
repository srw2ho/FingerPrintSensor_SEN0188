#include "pch.h"
#include "SensorPackage.h"

using namespace Platform;
using namespace FingerPrintSensor_SEN0188;

namespace FingerPrintSensor_SEN0188
{

	SensorPackage::SensorPackage()
	{
		InitializeCriticalSection(&m_CritLock);
		m_hWriteEvent = CreateEvent(
			NULL,               // default security attributes
			TRUE,               // manual-reset event
			FALSE,              // initial state is nonsignaled
			nullptr
			//TEXT("WriteEvent")  // object name
		);

		m_Confirm = FINGERPRINT_PACKETRECIEVEERR;
		m_SensorEcho = false;
		m_ProcessState = ProcessState::_doReadNothing;
		m_Adress = 0xffffffff;

	}


	SensorPackage::~SensorPackage()
	{
		CloseHandle(m_hWriteEvent);

		DeleteCriticalSection(&m_CritLock);

	}

	void SensorPackage::fillRecArray(DataReader^ reader, unsigned int len) {
		m_RecArray.resize(len);
		/*
		unsigned int Idx = 0;
		while (Idx < len)
		{
			m_RecArray[Idx++] =reader->ReadByte();


		//	reader->ReadBytes(::Platform::ArrayReference<unsigned char>(&m_RecArray[0], (unsigned int)m_RecArray.size()));
		}
		*/
		reader->ReadBytes(::Platform::ArrayReference<unsigned char>(&m_RecArray[0], (unsigned int)m_RecArray.size()));
	}

	void SensorPackage::fillUploadRecArray(DataReader^ reader, unsigned int len) {

		std::vector<unsigned char> UploadPartRecArray; // Payload-Data, in m_RecArray[0] steht Confirmation Byte

		UploadPartRecArray.resize(len);
		reader->ReadBytes(::Platform::ArrayReference<unsigned char>(&UploadPartRecArray[0], (unsigned int)UploadPartRecArray.size()));

		// insert parted array into m_UploadRecArray
		m_UploadRecArray.insert(std::end(m_UploadRecArray), std::begin(UploadPartRecArray), std::end(UploadPartRecArray));

		// m_UploadRecArray.assign(UploadPartRecArray.begin(), UploadPartRecArray.end());

	}

	void SensorPackage::cancelwaitForPacket() {
		::SetEvent(m_hWriteEvent);
	}

	bool SensorPackage::waitForPacketRecv(DWORD waitTime) {


		DWORD dwWaitResult = WaitForSingleObject(m_hWriteEvent, // event handle
			waitTime);    // indefinite wait
		if (dwWaitResult == WAIT_OBJECT_0) {
			return true;
		}

		return false;
	};



	Windows::Storage::Streams::IBuffer^ SensorPackage::createPackage(uint8_t type, uint8_t instruction, size_t length, uint8_t * data) {

		Lock();

		DataWriter^ writer = ref new DataWriter();

		writer->UnicodeEncoding = UnicodeEncoding::Utf8;

		writer->ByteOrder = ByteOrder::BigEndian;

		writer->WriteUInt16(FINGERPRINT_STARTCODE);
		writer->WriteUInt32(m_Adress);

		writer->WriteByte(type);	// type of FINGERPRINT_COMMANDPACKET 0x1, FINGERPRINT_DATAPACKET 0x2, FINGERPRINT_ENDDATAPACKET 0x8
		m_Activetype = type;


		uint16_t wire_length = (uint16_t)length + 3;

		writer->WriteUInt16(wire_length);	// CheckByte 2


		uint16_t sum = 0;
		sum = sum + type;
		sum = sum + ((wire_length) >> 8) + ((wire_length) & 0xFF);
		sum = sum + instruction;

		m_instruction = instruction;
		writer->WriteByte(instruction);	// instruction code

		if (data != nullptr && length > 0) {
			auto writedata = ref new Array<uint8_t>(length);
			int i = 0;
			while (i < length)
			{
				writedata[i] = *(data + i);
				sum += writedata[i];
				i++;
			}
			writer->WriteBytes(writedata);
		}


		writer->WriteUInt16(sum); // CheckSumm


		UnLock();


		return writer->DetachBuffer();
	}


	Windows::Storage::Streams::IBuffer^ SensorPackage::createDataPackage(SensorPackageByteArray& DownChar, size_t transferlength) {

	//	Lock();

		DataWriter^ writer = ref new DataWriter();


		writer->WriteUInt16(FINGERPRINT_STARTCODE);
		writer->WriteUInt32(m_Adress);


		uint8_t type = FINGERPRINT_DATAPACKET;
		if (DownChar.size() - transferlength > 0) {
			type = FINGERPRINT_DATAPACKET;
		}
		else {
			type = FINGERPRINT_ENDDATAPACKET;
		}
		writer->WriteByte(type);	// type of FINGERPRINT_COMMANDPACKET 0x1, FINGERPRINT_DATAPACKET 0x2, FINGERPRINT_ENDDATAPACKET 0x8

		m_Activetype = type;


		uint16_t wire_length = (uint16_t)transferlength + 2;

		writer->WriteUInt16(wire_length);	// CheckByte 2


		uint16_t sum = 0;
		sum = sum + type;
		sum = sum + ((wire_length) >> 8) + ((wire_length) & 0xFF);


		if ((DownChar.size() > 0) && transferlength > 0)
		{
			auto writedata = ref new Array<byte>(transferlength);
			int i = 0;
			while (!DownChar.empty() && (i < transferlength))
			{
				writedata[i] = DownChar.front();
				DownChar.erase(DownChar.begin());
				sum += writedata[i];
				i++;
			}

			writer->WriteBytes(writedata);
		}


		writer->WriteUInt16(sum); // CheckSumm



	//	UnLock();
		return writer->DetachBuffer();
	}


	void SensorPackage::InitStates() {

		Lock();
		m_Confirm = FINGERPRINT_PACKETRECIEVEERR;
		m_RecArray.clear();
		m_UploadRecArray.clear();
		::ResetEvent(m_hWriteEvent);
		UnLock();

		m_ProcessState = ProcessState::_doReadHeader;
	}


	bool  SensorPackage::doReadHeader(DataReader^ reader)
	{
		bool bret = false;
		if (reader->UnconsumedBufferLength >= 2) // 1 bytes for verification StartCode or 0x55 ready byte
		{
			unsigned int startCode = reader->ReadUInt16();
			if (startCode == FINGERPRINT_STARTCODE) { //  StartCode
				m_ProcessState = ProcessState::_doReadModulAdress;
			}
			else {
				m_ProcessState = ProcessState::_doReadClearRecBuffer;
			}

		}
		else {
			m_doReadFurhterData = true;
		}


		return (m_ProcessState == ProcessState::_doReadModulAdress);

	}

	bool  SensorPackage::doReadModulAdress(DataReader^ reader)
	{
		unsigned int adress = 0;
		if (reader->UnconsumedBufferLength >= 5) {
			adress = reader->ReadUInt32(); //  Adress
			if (adress != m_Adress) {
				m_ProcessState = ProcessState::_doReadClearRecBuffer;

			}
			else {
				m_PackageIdent = reader->ReadByte(); //  PackageIdent

				m_ProcessState = ProcessState::_doReadPackageLen;
			}

		}
		else {
			m_doReadFurhterData = true;
		}

		return (m_ProcessState == ProcessState::_doReadPackageLen);

	}

	bool  SensorPackage::doReadPackageLen(DataReader^ reader)
	{
		if (reader->UnconsumedBufferLength >= 2) {
			m_PackageLenght = reader->ReadUInt16(); //  Len
			m_ProcessState = ProcessState::_doReadPackagePayload;
		}
		else
		{
			m_doReadFurhterData = true;
		}
		return (m_ProcessState == ProcessState::_doReadPackagePayload);

	}
	bool  SensorPackage::doReadPackagePayload(DataReader^ reader) //reading payload of CMD-package
	{
		if (reader->UnconsumedBufferLength >= m_PackageLenght) {
			if (m_PackageLenght > 2) {

				fillRecArray(reader, m_PackageLenght - 2);
				m_Confirm = this->m_RecArray[0];
				if (m_Confirm == 0x55) { // Sensor can receive Packets
					m_SensorEcho = true;
				}
			}
			unsigned int checksum = reader->ReadUInt16(); //  CheckSum

			m_ProcessState = ProcessState::_doReadFinished; // data package is finished read

		}
		else {
			m_doReadFurhterData = true;
		}

		return false;

	}

	bool  SensorPackage::doReadDataPackagePayloadHeader(DataReader^ reader) // reading datapackage header
	{
		bool ret = false;

		if (reader->UnconsumedBufferLength >= 9) // to 9 bytes for data Package header
		{
			unsigned int startCode = reader->ReadUInt16();
			if (startCode == FINGERPRINT_STARTCODE) { //  StartCode
				{
					unsigned int adress = reader->ReadUInt32();
					m_PackageIdent = reader->ReadByte();

					if (reader->UnconsumedBufferLength >= 2) {
						m_PackageLenght = reader->ReadUInt16(); //  Len

						m_ProcessState = ProcessState::_doReadDataPackagePayload;

						ret = true;


					}
				}

			}
			else { // Startcode not correkt something is happened
				m_ProcessState = ProcessState::_doReadClearRecBuffer;

			}
		}
		else
		{
			m_doReadFurhterData = true;
		}


		return ret;

	}
	bool  SensorPackage::doReadDataPackagePayload(DataReader^ reader)  //reading datapackage data
	{
		bool ret = false;

		if (reader->UnconsumedBufferLength >= m_PackageLenght) // are data enough
		{
			if (m_PackageLenght > 2) {
				fillUploadRecArray(reader, m_PackageLenght - 2); // uploaded data in separate upload array
			}
			unsigned int checksum = reader->ReadUInt16(); //  CheckSum

			if (m_PackageIdent == FINGERPRINT_ENDDATAPACKET) {
				m_ProcessState = ProcessState::_doReadFinished;
			}
			else {// read furher data
				m_ProcessState = ProcessState::_doReadDataPackagePayloadHeader;
			}
		}
		else
		{
			m_doReadFurhterData = true;
		}


		return ret;

	}

	bool  SensorPackage::doReadClearRecBuffer(DataReader^ reader)
	{
		if (reader->UnconsumedBufferLength > 0) { // es stimmt etwas nicht, es dürfen keine Bytes mehr übrig sein
			IBuffer^  chunkBuffer = reader->ReadBuffer(reader->UnconsumedBufferLength);
			m_Confirm = FINGERPRINT_BADPACKET;
		}
		::SetEvent(m_hWriteEvent);
		return (m_Confirm == FINGERPRINT_BADPACKET);

	}
	bool  SensorPackage::doReadFinished(DataReader^ reader)
	{
		bool doreadPayload = (m_instruction == FINGERPRINT_CMD_UPUMAGE) || (m_instruction == FINGERPRINT_CMD_UPCHAR);

		if (doreadPayload && (m_PackageIdent != FINGERPRINT_ENDDATAPACKET)) { // es folgen weitere Daten, da werden erst die Payload-Header gelesen

			m_ProcessState = ProcessState::_doReadDataPackagePayloadHeader;
			return true;
		}
		else {


			if (reader->UnconsumedBufferLength > 0) { // es stimmt etwas nicht, es dürfen keine Bytes mehr übrig sein
				IBuffer^ chunkBuffer = reader->ReadBuffer(reader->UnconsumedBufferLength);
				m_Confirm = FINGERPRINT_BADPACKET;
			}
			else {

			}
			m_ProcessState = ProcessState::_doReadNothing;
			//m_ProcessState = ProcessState::_doReadNothing;
			::SetEvent(m_hWriteEvent);
			return (m_Confirm == FINGERPRINT_BADPACKET);
		}

		return false;
	}


	void SensorPackage::DoProcessChunk(DataReader^ reader)
	{
		Lock();
		bool doReadBuffer = true;
		m_doReadFurhterData = false; // erstmal keine weiteren Daten lesen

		do {

			switch (m_ProcessState) {
			case ProcessState::_doReadNothing:
				break;

			case ProcessState::_doReadHeader:
				doReadHeader(reader);
				break;
			case ProcessState::_doReadModulAdress:
				doReadModulAdress(reader);
				break;

			case ProcessState::_doReadPackageLen:
				doReadPackageLen(reader);
				break;

			case ProcessState::_doReadDataPackagePayloadHeader:
				doReadDataPackagePayloadHeader(reader);
				break;

			case ProcessState::_doReadPackagePayload:
				doReadPackagePayload(reader);
				break;
			case ProcessState::_doReadDataPackagePayload:
				doReadDataPackagePayload(reader);
				break;

			case ProcessState::_doReadClearRecBuffer:
				doReadClearRecBuffer(reader);
				doReadBuffer = false;
				break;

			case ProcessState::_doReadFinished:
				doReadBuffer = doReadFinished(reader);
				break;
			}
			if (m_doReadFurhterData) { // weietere Daten lesen, aber im gleichen Zustand bleiben
				doReadBuffer = false;
			}
		} while (doReadBuffer);


		UnLock();
	}

	void SensorPackage::Lock() {
		EnterCriticalSection(&m_CritLock);
	}

	void SensorPackage::UnLock() {
		LeaveCriticalSection(&m_CritLock);
	}

}