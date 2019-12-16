#pragma once

#include <collection.h>
#include <ppltasks.h>
#include <wrl.h>
#include <robuffer.h>

using namespace Windows::Storage::Streams;

#define FINGERPRINT_OK 0x00
#define FINGERPRINT_RESIDUAL 0x22

#define FINGERPRINT_PACKETRECIEVEERR 0x01
#define FINGERPRINT_NOFINGER 0x02
#define FINGERPRINT_IMAGEFAIL 0x03
#define FINGERPRINT_IMAGEMESS 0x06
#define FINGERPRINT_FEATUREFAIL 0x07
#define FINGERPRINT_NOMATCH 0x08
#define FINGERPRINT_NOTFOUND 0x09
#define FINGERPRINT_ENROLLMISMATCH 0x0A
#define FINGERPRINT_BADLOCATION 0x0B
#define FINGERPRINT_DBRANGEFAIL 0x0C
#define FINGERPRINT_UPLOADFEATUREFAIL 0x0D
#define FINGERPRINT_PACKETRESPONSEFAIL 0x0E
#define FINGERPRINT_UPLOADFAIL 0x0F
#define FINGERPRINT_DELETEFAIL 0x10
#define FINGERPRINT_DBCLEARFAIL 0x11
#define FINGERPRINT_PASSFAIL 0x13
#define FINGERPRINT_INVALIDIMAGE 0x15
#define FINGERPRINT_FLASHERR 0x18
#define FINGERPRINT_INVALIDREG 0x1A
#define FINGERPRINT_ADDRCODE 0x20
#define FINGERPRINT_PASSVERIFY 0x21

#define FINGERPRINT_STARTCODE 0xEF01

#define FINGERPRINT_COMMANDPACKET 0x1
#define FINGERPRINT_DATAPACKET 0x2
#define FINGERPRINT_ACKPACKET 0x7
#define FINGERPRINT_ENDDATAPACKET 0x8

#define FINGERPRINT_TIMEOUT 0xFF
#define FINGERPRINT_BADPACKET 0xFE

// Commands
#define FINGERPRINT_CMD_GENIMAGE 0x01
#define FINGERPRINT_CMD_GENIMAGEFREE 0x52

#define FINGERPRINT_CMD_IMAGE2TZ 0x02
#define FINGERPRINT_CMD_REGMODEL 0x05
#define FINGERPRINT_CMD_SEARCH 0x04
#define FINGERPRINT_CMD_AUTOLOGIN 0x54
#define FINGERPRINT_CMD_AUTOSEARCH 0x55
#define FINGERPRINT_CMD_SEARCH_RESBACK 0x56
#define FINGERPRINT_CMD_STORE 0x06
#define FINGERPRINT_CMD_MATCH 0x03
#define FINGERPRINT_CMD_LOAD 0x07
#define FINGERPRINT_CMD_UPLOAD 0x08
#define FINGERPRINT_CMD_DELETE 0x0C
#define FINGERPRINT_CMD_EMPTY 0x0D
#define FINGERPRINT_CMD_SETPASSWORD 0x12
#define FINGERPRINT_CMD_VERIFYPASSWORD 0x13
#define FINGERPRINT_CMD_SETADRESS 0x15

#define FINGERPRINT_CMD_TEMPLATECOUNT 0x1D
#define FINGERPRINT_CMD_GETECHO 0x53
#define FINGERPRINT_CMD_OPENLED 0x50
#define FINGERPRINT_CMD_CLOSELED 0x51
#define FINGERPRINT_CMD_DOWNCHAR 0x09
#define FINGERPRINT_CMD_UPCHAR 0x08
#define FINGERPRINT_CMD_DOWNIMAGE 0x0b
#define FINGERPRINT_CMD_UPUMAGE 0x0a
#define FINGERPRINT_CMD_READSYSPARA 0x0f
#define FINGERPRINT_CMD_HISPEEDSEARCH 0x1B
#define FINGERPRINT_CMD_READCONLIST 0x1f
#define FINGERPRINT_CMD_WRITENOTEPAD 0x18
#define FINGERPRINT_CMD_READNOTEPAD 0x19

typedef std::vector<unsigned char> SensorPackageByteArray;


namespace FingerPrintSensor_SEN0188
{
  class SensorPackage
  {
    enum ProcessState {
      _doReadNothing,

      _doReadHeader,
      _doReadModulAdress,
      _doReadPackageLen,
      _doReadPackagePayload,
      _doReadDataPackagePayload,
      _doReadDataPackagePayloadHeader,
      _doReadClearRecBuffer,
      _doReadFinished,
    };

    unsigned int m_Adress;
    uint8_t m_Activetype;
    uint8_t m_instruction; // CMD-Order

    HANDLE m_hWriteEvent;

   // Platform::Array<byte> ^ m_RecArray;
    byte m_Confirm;
    byte m_PackageIdent;

    CRITICAL_SECTION m_CritLock;
    bool m_SensorEcho;
    ProcessState m_ProcessState;
    unsigned int m_PackageLenght;

    SensorPackageByteArray m_RecArray; // Payload-Data, in m_RecArray[0] steht Confirmation Byte

    SensorPackageByteArray m_UploadRecArray; // Payload-Data, in m_RecArray[0] steht Confirmation Byte
    SensorPackageByteArray m_DownloadRecArray; // Payload-Data, in m_RecArray[0] steht Confirmation Byte

    bool m_doReadFurhterData;
  public:


    SensorPackage();
    virtual ~SensorPackage();

    SensorPackageByteArray& getRecArray() { return m_RecArray; };
    SensorPackageByteArray& getUploadRecArray() { return m_UploadRecArray; };
    SensorPackageByteArray& getDownloadRecArray() { return m_DownloadRecArray; };

    bool getSensorEcho() { return m_SensorEcho; };

    byte getConfirm() { return m_Confirm; };
    virtual void cancelwaitForPacket();
    Windows::Storage::Streams::IBuffer^ createPackage(uint8_t type, uint8_t instruction, size_t length = 0, uint8_t * data = nullptr);
    Windows::Storage::Streams::IBuffer^ createDataPackage(SensorPackageByteArray& DownChar, size_t transferlength);
    void InitStates();

    bool waitForPacketRecv(DWORD waitTime);


    void DoProcessChunk(DataReader^ reader);

    uint8_t getInstruction() { return m_instruction;};

	void Lock();

	void UnLock();
  protected:
    void fillRecArray(DataReader^ reader, unsigned int len);
    void fillUploadRecArray(DataReader^ reader, unsigned int len);

    uint8_t getActivetype() {return m_Activetype;};




    // for reading out Serial Interface
    bool  doReadHeader(DataReader^ reader);
    bool  doReadModulAdress(DataReader^ reader);
    bool  doReadPackageLen(DataReader^ reader);
    bool  doReadPackagePayload(DataReader^ reader);
    bool  doReadDataPackagePayload(DataReader^ reader);
    bool  doReadDataPackagePayloadHeader(DataReader^ reader);

    bool  doReadClearRecBuffer(DataReader^ reader);
    bool  doReadFinished(DataReader^ reader);



  };
}


