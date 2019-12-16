#pragma once

#include <collection.h>
#include <ppltasks.h>
#include <wrl.h>
#include <robuffer.h>

#include "SerialCom.h"
#include "SensorPackage.h"

#include "SerialChunkReceiver.h"

//#include "GPIOInOut.h"

typedef struct  
{
  uint8_t m_State; // 1:0  occupied/non occupied
  uint8_t m_Page;  // PageNumber 0...3

} MapItemState;

typedef std::map<uint32_t, MapItemState> fingerMap;
typedef fingerMap::iterator fingerMapIt;

typedef std::map<uint32_t, std::string> fingerStateMap;
typedef fingerStateMap::iterator fingerStateMapIt;

namespace FingerPrintSensor_SEN0188
{
  class Sensor : public SerialCommunication::SerialChunkReceiver
  {
    SensorPackage m_SensorPackage;
    CRITICAL_SECTION m_CritLock;
    DWORD m_RecvTmOut;

	WORD m_StatusRegister;
	WORD m_SystemIdentifier;
	WORD m_FingerLibrarySize; //1000 
	WORD m_FingerPages; //4 
	WORD m_FingerActiveSize; //0...1000
	WORD m_SecurityLevel;
	unsigned int m_DeviceAddress;
	WORD m_DataPackeSize;
	WORD m_BaudSettings;
	
	unsigned int m_Password;
  fingerMap m_fingerMap;


  fingerStateMap m_fingerStateMap;

  SensorPackageByteArray m_SensorID;

  public:
    Sensor(Windows::Devices::SerialCommunication::SerialDevice ^_serialPort);
    virtual ~Sensor();

    uint8_t ReadSysPara(void);
    uint8_t GetImage(void);//GenImg:detecting  finger  and  store  the  detected  finger  image  in  ImageBuffer  while returning successful confirmation code; If there is no finger, returned confirmation code would be” can’t detect finger” . 

    uint8_t GetmageFree(void);//GenImg: Fingerprint get image free lighting Input
    uint8_t RegModel(void);//GenImg: Fingerprint get image free lighting Input

    uint8_t getEcho(void);
    uint8_t DownChar(uint8_t bufferID,const SensorPackageByteArray& DownChar); //download character file or template
    uint8_t UpChar(uint8_t bufferID,SensorPackageByteArray& DownChar); //upload character file or template

	uint8_t Empty(); //delete all stored fingers from library
	uint8_t DeleteChar(uint16_t PageNumber, uint16_t numberOfTemplatestoDelete); //delete all stored fingers from library
  uint8_t LoadChar(uint8_t bufferID, uint16_t PageNumber); // To read template from Flash library
   
	uint8_t TempleteNum(void);
    uint8_t Store(uint8_t bufferId, uint16_t PageID); 

    uint8_t Match();

    uint8_t Search(uint8_t bufferId, uint16_t StartPage, uint16_t PageNum, uint16_t& PageId, uint16_t& MatchScore);
    uint8_t SearchResBack(uint8_t bufferId, uint16_t StartPage, uint16_t PageNum, uint16_t& PageId, uint16_t& MatchScore);
    
    
	uint8_t AutoSearch(uint8_t waitTime, uint16_t StartPage, uint16_t PageNum, uint16_t& PageId, uint16_t& MatchScore);

	uint8_t AutoLogin(uint8_t waitTime, uint8_t numberOfTimes, uint16_t storedSeqNumber, uint8_t repeatedRegistration);


    uint8_t VfyPwd(uint32_t pw);
    uint8_t SetPwd(uint32_t pw);
    uint8_t SetAdder(uint32_t adress);
    
    uint8_t ReadConList(uint8_t indexPage);
    
    uint8_t Img2Tz(uint8_t bufferId);

    uint8_t Sensor::OpenLED(void);
    uint8_t Sensor::CloseLED(void);

    uint8_t WriteNotepad(uint8_t PagerNumber, const std::vector< uint8_t>& values); //  To write note pad
    uint8_t ReadNotepad(uint8_t PagerNumber, std::vector< uint8_t>& values); // To read note pad

    void cancelwaitForPacket();

    std::string getCmdStringByState(uint8_t);

	bool upateFingerStateById(uint32_t fingerID, uint8_t _state);
    bool getFingerStateById(uint32_t fingerID, MapItemState& state);
    uint8_t ReadCompleteConList();


    uint32_t getNextFreeFingerId();
	uint32_t getNextFreeFingerPage();
	uint8_t SensorInitialize(void);

    uint8_t getInstruction(void) { return m_SensorPackage.getInstruction(); }

	uint16_t getActiveFingerSize() {
		return m_FingerActiveSize;
	};


  uint32_t getfilledFingerLib(std::vector<uint32_t>& filled);

  SensorPackageByteArray& getSensorID() { return m_SensorID; };

  protected:
    virtual void DoProcessChunk(Windows::Storage::Streams::DataReader^ reader);

    virtual Windows::Storage::Streams::IBuffer^ getStartServiceCommand();

    void Lock();

    void UnLock();

	void fillFingerStateMap();


  };
}
