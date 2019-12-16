#include "pch.h"

#include "SerialChunkReceiver.h"
#include "SerialHelpers.h"



#include "Sensor.h"

using namespace FingerPrintSensor_SEN0188;
using namespace Platform;
using namespace SerialCommunication;

#define DEFAULTTIMEOUT 5000  // milliseconds

namespace FingerPrintSensor_SEN0188 {

	Sensor::Sensor(Windows::Devices::SerialCommunication::SerialDevice ^_serialPort) :SerialChunkReceiver(_serialPort)
	{
		InitializeCriticalSection(&m_CritLock);
		m_RecvTmOut = DEFAULTTIMEOUT;
		m_DataPackeSize = 128;

		m_StatusRegister = 0;
		m_SystemIdentifier = 0;
		m_FingerLibrarySize = 1000;
		m_FingerPages = 4;
		m_SecurityLevel = 3;
		m_DeviceAddress = 0xffff;
		m_Password = 0x0000;
		fillFingerStateMap();
		m_FingerActiveSize = 0;
	}

	Sensor::~Sensor()
	{
		DeleteCriticalSection(&m_CritLock);
	}

	void Sensor::fillFingerStateMap() {
		m_fingerStateMap[FINGERPRINT_OK] = "command execution complete";
		m_fingerStateMap[FINGERPRINT_PACKETRECIEVEERR] = "error when receiving data package";
		m_fingerStateMap[FINGERPRINT_NOFINGER] = "no finger on the sensor";
		m_fingerStateMap[FINGERPRINT_IMAGEFAIL] = "fail to enroll the finger";
		m_fingerStateMap[FINGERPRINT_IMAGEMESS] = "fail to generate character file due to the over-disorderly fingerprint image";

		m_fingerStateMap[FINGERPRINT_FEATUREFAIL] = "fail to generate character file due to weakness of character point or over-smallness of fingerprint image ";
		m_fingerStateMap[FINGERPRINT_NOMATCH] = "finger doesn't match";
		m_fingerStateMap[FINGERPRINT_NOTFOUND] = "fail to find the matching finger";
		m_fingerStateMap[0x0a] = " fail to combine the character files";

		m_fingerStateMap[FINGERPRINT_BADLOCATION] = "addressing PageID is beyond the finger library";
		m_fingerStateMap[FINGERPRINT_DBRANGEFAIL] = "error when reading template from library or the template is invalid";
		m_fingerStateMap[FINGERPRINT_UPLOADFEATUREFAIL] = "error when uploading template";
		m_fingerStateMap[FINGERPRINT_PACKETRESPONSEFAIL] = " Module can't receive the following data packages";

		m_fingerStateMap[0x0f] = "error when uploading image";
		m_fingerStateMap[0x10] = "fail to delete the template";
		m_fingerStateMap[0x11] = "fail to clear finger library";
		m_fingerStateMap[0x15] = "fail to generate the image for the weakness of valid primary image";

		m_fingerStateMap[0x18] = " error when writing flash";
		m_fingerStateMap[0x1a] = "invalid register number";

		m_fingerStateMap[FINGERPRINT_RESIDUAL] = "residual fingerprint";
		m_fingerStateMap[0x23] = "The specified interval does not exist an effective fingerprint template";

		m_fingerStateMap[0x24] = " failure due to repeated registration(That is, the current registered fingerprint in fingerprint database already exists)";

		m_fingerStateMap[FINGERPRINT_TIMEOUT] = "#Errro:Receive Timeout";
	}

	Windows::Storage::Streams::IBuffer^ Sensor::getStartServiceCommand()
	{

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_GETECHO);
		m_SensorPackage.InitStates();
		return package;

	}

	uint8_t Sensor::SensorInitialize(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		this->clearRecvBuf();
		
		m_SensorID.clear();

		OpenLED(); // sign for initialize

		ret = TempleteNum(); // read Numbers of Page Sides
		if (ret == FINGERPRINT_OK) {
			ret = VfyPwd(m_Password); // Verify PW
			if (ret == FINGERPRINT_OK) {
				ret = ReadSysPara();
				if (ret == FINGERPRINT_OK) {
					ret = ReadCompleteConList(); // read fingerprint map;
					ReadNotepad(0, m_SensorID);
				}
			}
		}

		CloseLED(); // close LED

		UnLock();

		return ret;
	}

	bool Sensor::upateFingerStateById(uint32_t fingerID, uint8_t _state) {
		bool ret = false;
		Lock();
		fingerMapIt it = m_fingerMap.find(fingerID);
		if (it != m_fingerMap.end()) {
			MapItemState&state = it->second;
			state.m_State = _state;
			ret = true;
		}

		UnLock();
		return ret;
	}

	bool Sensor::getFingerStateById(uint32_t fingerID, MapItemState&state) {
		bool ret = false;
		Lock();
		fingerMapIt it = m_fingerMap.find(fingerID);
		if (it != m_fingerMap.end()) {
			state = it->second;
			ret = true;
		}

		UnLock();
		return ret;
	}



	uint8_t Sensor::Empty(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_EMPTY);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) {

			}

		}

		UnLock();

		return ret;
	}

  uint8_t Sensor::LoadChar(uint8_t bufferID, uint16_t PageNumber) // To read template from Flash library
  {
    Lock();
    uint8_t ret = FINGERPRINT_TIMEOUT;

    std::vector< uint8_t> _input(3);
    _input[0] = bufferID;
    _input[1] = PageNumber >> 8;
    _input[2] = PageNumber & 0xff;


    Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_LOAD, _input.size(), &_input[0]);
    m_SensorPackage.InitStates();
    SendData(package); // Send Data to Device

    bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
    if (ok) {
      ret = m_SensorPackage.getConfirm();
      if (ret == FINGERPRINT_OK) {

      }

    }

    UnLock();

    return ret;
  }

  uint8_t Sensor::WriteNotepad(uint8_t PagerNumber, const std::vector< uint8_t>& values) //  To write note pad
  {
    Lock();
    uint8_t ret = FINGERPRINT_TIMEOUT;

    std::vector< uint8_t> _input(33);
    _input[0] = PagerNumber;
    for (size_t i = 0; i < values.size(); i++) {
      if (i+1 < _input.size()) {
        _input[i+1] = values[i];
      }

    }

    Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_WRITENOTEPAD, _input.size(), &_input[0]);
    m_SensorPackage.InitStates();
    SendData(package); // Send Data to Device

    bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
    if (ok) {
      ret = m_SensorPackage.getConfirm();
      if (ret == FINGERPRINT_OK) {

      }

    }

    UnLock();

    return ret;
  }

  uint8_t Sensor::ReadNotepad(uint8_t PagerNumber, std::vector< uint8_t>& values) // To read note pad
  {
    Lock();
    uint8_t ret = FINGERPRINT_TIMEOUT;

    std::vector< uint8_t> _input(1);
    _input[0] = PagerNumber;


    Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_READNOTEPAD,  _input.size(), &_input[0]);
    m_SensorPackage.InitStates();
    SendData(package); // Send Data to Device

    bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
    if (ok) {
      ret = m_SensorPackage.getConfirm();
      if (ret == FINGERPRINT_OK) {

        SensorPackageByteArray& retArr = m_SensorPackage.getRecArray();
        if (retArr.size() > 32) {
          for (size_t i = 1; i < retArr.size(); i++) {
            values.push_back(retArr[i]);
         }

        }

      }

    }

    UnLock();

    return ret;
  }


	uint8_t Sensor::DeleteChar(uint16_t PageNumber, uint16_t numberOfTemplatestoDelete) //delete all stored fingers from library
	{
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		std::vector< uint8_t> _input(4);

		_input[0] = PageNumber>>8;
		_input[1] = PageNumber & 0xff;

		_input[2] = numberOfTemplatestoDelete >> 8;
		_input[3] = numberOfTemplatestoDelete & 0xff;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_DELETE,  _input.size(), &_input[0]);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) {

			}

		}

		UnLock();

		return ret;
	}


	uint8_t Sensor::TempleteNum(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_TEMPLATECOUNT);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) {
				SensorPackageByteArray& retArr = m_SensorPackage.getRecArray();
				if (retArr.size() > 2) {
					uint8_t*v = (uint8_t*)&retArr[1];
					uint16_t val = (v[0] << 8) | v[1];

					m_FingerActiveSize = val;


				}
			}

		}

		UnLock();

		return ret;
	}

	uint8_t Sensor::OpenLED(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_OPENLED);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
		}

		UnLock();

		return ret;
	}

	uint8_t Sensor::CloseLED(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_CLOSELED);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
		}

		UnLock();

		return ret;
	}

	uint8_t Sensor::GetImage(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_GENIMAGE);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
		}

		UnLock();

		return ret;
	}

	uint8_t Sensor::GetmageFree(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_GENIMAGEFREE);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
		}

		UnLock();

		return ret;
	}

	uint8_t Sensor::RegModel(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_REGMODEL);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
		}

		UnLock();

		return ret;
	}

	uint8_t Sensor::Store(uint8_t bufferId, uint16_t PageID) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		std::vector< uint8_t> _input(3);
		_input[0] = bufferId;
		_input[1] = PageID>>8;
		_input[2] = PageID & 0xff;
		//uint16_t *w = (uint16_t *)&_input[1];
		//*w = PageID;

		//uint8_t packet[] = { FINGERPRINT_STORE, 0x01, id >> 8, id & 0xFF };

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_STORE, _input.size(), &_input[0]);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
		}

		UnLock();

		return ret;
	}

	uint8_t Sensor::Match() {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_MATCH);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
		}

		UnLock();

		return ret;
	}


	uint8_t Sensor::Search(uint8_t bufferId, uint16_t StartPage, uint16_t PageNum, uint16_t& PageId, uint16_t& MatchScore) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(5);
		_input[0] = bufferId;
		_input[1] = StartPage >>8;
		_input[2] = StartPage && 0xff;
		_input[3] = PageNum >> 8;
		_input[4] = PageNum & 0xff;
/*

		_input[1] = PageID>>8;
		_input[2] = PageID && 0xff;

		uint16_t*v = (uint16_t*)&_input[1];
		*v = StartPage;
		v = (uint16_t*)&_input[3];
		*v = PageNum;
*/
// high speed search of slot #1 starting at page 0x0000 and page #0x00A3 
	//	uint8_t packet[] = { FINGERPRINT_HISPEEDSEARCH, 0x01, 0x00, 0x00, 0x00, 0xA3 };
		PageId = -1;
		MatchScore = -1;
		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_SEARCH, _input.size(), (uint8_t*)&_input[0]);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if ((ret == FINGERPRINT_OK)) {
				SensorPackageByteArray& retArr = m_SensorPackage.getRecArray();


				if (retArr.size() > 4) {
					uint8_t*v = (uint8_t*)&retArr[1];
					PageId =  (v[0] >> 8) |  (v[1] & 0xff); // big endian: higher byte first, than lower
					MatchScore = (v[2] >> 8) | (v[3] & 0xff);

				}

			}
		}

		UnLock();
		return ret;
	}


	uint8_t Sensor::SearchResBack(uint8_t bufferId, uint16_t StartPage, uint16_t PageNum, uint16_t& PageId, uint16_t& MatchScore) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(5);
		_input[0] = bufferId;
		_input[1] = StartPage >> 8;
		_input[2] = StartPage && 0xff;
		_input[3] = PageNum >> 8;
		_input[4] = PageNum & 0xff;

	//	uint16_t*v = (uint16_t*)&_input[1];
	//	*v = StartPage;
	//	v = (uint16_t*)&_input[3];
	//	*v = PageNum;

		PageId = -1;
		MatchScore = -1;
		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_SEARCH_RESBACK, _input.size(), (uint8_t*)&_input[0]);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if ((ret == FINGERPRINT_OK) || (ret == FINGERPRINT_RESIDUAL)) {
				SensorPackageByteArray& retArr = m_SensorPackage.getRecArray();
				if (retArr.size() > 4) {
					uint8_t*v = (uint8_t*)&retArr[1];
					PageId = (v[0] >> 8) | (v[1] & 0xff); // big endian: higher byte first, than lower
					MatchScore = (v[2] >> 8) | (v[3] & 0xff);
				}

			}
		}

		UnLock();
		return ret;
	}



	uint8_t Sensor::AutoSearch(uint8_t waitTime, uint16_t StartPage, uint16_t PageNum, uint16_t& PageId, uint16_t& MatchScore) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(5);

		_input[0] = waitTime;
		_input[1] = StartPage >> 8;
		_input[2] = StartPage && 0xff;
		_input[3] = PageNum >> 8;
		_input[4] = PageNum & 0xff;



		PageId = -1;
		MatchScore = -1;
		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_AUTOSEARCH, _input.size(), (uint8_t*)&_input[0]);
		double AdditionalTimeoutinSec = double(2 / double(31)) *waitTime;
		AdditionalTimeoutinSec = AdditionalTimeoutinSec * 1000;
		AdditionalTimeoutinSec += m_RecvTmOut;


		m_SensorPackage.InitStates();
		clearRecvBuf();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(DWORD( AdditionalTimeoutinSec));
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if ((ret == FINGERPRINT_OK) || (ret == FINGERPRINT_RESIDUAL)) {
				SensorPackageByteArray& retArr = m_SensorPackage.getRecArray();
				if (retArr.size() > 4) {
					uint8_t*v = (uint8_t*)&retArr[1];
					PageId = (v[0] >> 8) | (v[1] & 0xff); // big endian: higher byte first, than lower
					MatchScore = (v[2] >> 8) | (v[3] & 0xff);
				}
				//	  ret = FINGERPRINT_OK;
			}
		}

		UnLock();
		return ret;
	}


	uint8_t Sensor::AutoLogin(uint8_t waitTime, uint8_t numberOfTimes, uint16_t storedSeqNumber, uint8_t repeatedRegistration) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(5);
		_input[0] = waitTime;
		_input[1] = numberOfTimes;

		_input[2] = storedSeqNumber >> 8;
		_input[3] = storedSeqNumber & 0xff;
		_input[4] = repeatedRegistration;





		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_AUTOLOGIN, _input.size(), (uint8_t*)&_input[0]);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		double AdditionalTimeoutinSec = double(2 / double(31)) *waitTime;
		AdditionalTimeoutinSec = AdditionalTimeoutinSec * 1000;
		AdditionalTimeoutinSec +=  m_RecvTmOut;

		bool ok = m_SensorPackage.waitForPacketRecv(DWORD (AdditionalTimeoutinSec));
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if ((ret == FINGERPRINT_OK) || (ret == 0x56) || (ret == 0x57)) {
				m_SensorPackage.InitStates();
				bool ok = m_SensorPackage.waitForPacketRecv(DWORD(AdditionalTimeoutinSec));
				if (ok) {
					ret = m_SensorPackage.getConfirm();
					if ((ret == FINGERPRINT_OK) || (ret == 0x56) ) {
						if (ret == 0x56) {
							m_SensorPackage.InitStates();
							bool ok = m_SensorPackage.waitForPacketRecv(DWORD(AdditionalTimeoutinSec));
							if (ok) {
								ret = m_SensorPackage.getConfirm();
							}
						}
					}
				}

			}
		}

		UnLock();
		return ret;
	}

	uint8_t Sensor::VfyPwd(uint32_t pw) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(4);
		_input[0] = pw >> 24;
		_input[1] = pw >> 16;
		_input[2] = pw >> 8;
		_input[3] = pw & 0xff;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_VERIFYPASSWORD,  _input.size(), (uint8_t*)&_input[0]);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) {


			}
		}

		UnLock();
		return ret;
	}

	uint8_t Sensor::SetPwd(uint32_t pw) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(4);
		_input[0] = pw >> 24;
		_input[1] = pw >> 16;
		_input[2] = pw >> 8;
		_input[3] = pw & 0xff;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_SETPASSWORD,  _input.size(), (uint8_t*)&_input[0]);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) {


			}
		}

		UnLock();
		return ret;
	}

	uint8_t Sensor::SetAdder(uint32_t adress) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		std::vector< uint8_t> _input(4);
		_input[0] = adress >> 24;
		_input[1] = adress >> 16;
		_input[2] = adress >> 8;
		_input[3] = adress & 0xff;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_SETADRESS,  _input.size(), (uint8_t*)&_input[0]);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) {


			}
		}

		UnLock();
		return ret;
	}

	uint32_t Sensor::getNextFreeFingerId() {

		Lock();
		uint32_t freeId = -1;


		MapItemState state;
		for (size_t Id = 0; Id < m_FingerLibrarySize; Id++) {

			bool IsPresent = getFingerStateById(uint32_t(Id) , state);
			if (IsPresent) {
				if (state.m_State == 0) { // frei
					freeId = (uint32_t)Id;
					break;
				}
			}

		}

		UnLock();


		return freeId;

	}

  uint32_t Sensor::getfilledFingerLib(std::vector<uint32_t> &filled) {

    Lock();


    MapItemState state;

    for (size_t Id = 0; Id < m_FingerLibrarySize; Id++) {
      bool IsPresent = getFingerStateById((uint32_t)Id, state);
      if (IsPresent) {
        if (state.m_State == 1) { // belegt
          filled.push_back(uint32_t(Id));
        }
      }

    }

    UnLock();


    return ((uint32_t) filled.size());

  }

	uint32_t Sensor::getNextFreeFingerPage() {

		Lock();
		uint32_t freeId = -1;

		MapItemState state;

		for (size_t Id = 0; Id < m_FingerLibrarySize; Id++) {

			bool IsPresent = getFingerStateById(uint32_t(Id), state);
			if (IsPresent) {
				if (state.m_State == 0) { // frei
					freeId = state.m_Page;
					break;
				}
			}

		}

		UnLock();


		return freeId;

	}

	uint8_t Sensor::ReadCompleteConList() {

		Lock();
		m_fingerMap.clear();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		for (uint8_t Page = 0; Page < this->m_FingerPages; Page++) { // PageIds 0...3 lesen
			ret = ReadConList(Page);
			if (ret != FINGERPRINT_OK) {
				break;
			}
		}

		UnLock();
		return ret;
	}


	uint8_t Sensor::ReadConList(uint8_t indexPage) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(1);
		_input[0] = indexPage;
		uint8_t saveInputPage = indexPage;
		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_READCONLIST, 1 * _input.size(), (uint8_t*)&_input[0]);
		uint32_t fingerId = 0;
		while (indexPage > 0) {
			fingerId = fingerId + 256;
			indexPage--;
		}

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) {
				SensorPackageByteArray& ret = m_SensorPackage.getRecArray();
				if (ret.size() > 32) {

					MapItemState state;
					for (size_t i = 1; i < ret.size(); i++) {
						uint8_t*v = (uint8_t*)&ret[i];
						uint8_t msk = 0x01;
						for (size_t Bit = 0; Bit < 8; Bit++) {
							byte bitset = (*v & msk) == msk;
							state.m_Page = saveInputPage;
							state.m_State = bitset;
							m_fingerMap[fingerId++] = state;
							msk = msk << 1;
						}
					}

				}

			}
		}

		UnLock();
		return ret;
	}





	uint8_t Sensor::Img2Tz(uint8_t bufferId) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(1);
		_input[0] = bufferId;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_IMAGE2TZ, _input.size(), (uint8_t*)&_input[0]);

		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();

		}

		UnLock();
		return ret;
	}





	uint8_t Sensor::ReadSysPara(void) {//Read system Parameter:   Read Module’s status register and system basic configuration parameters（  Refer to 4.4 for system configuration parameter and 4.5 for system status register
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_READSYSPARA);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) { // ready for data transfer
				SensorPackageByteArray& retarray = m_SensorPackage.getRecArray();
				if (retarray.size() >= 16) {

					uint8_t* sysParam = (uint8_t*)&retarray[1];

					m_StatusRegister = (sysParam[0] << 8) | sysParam[1]; // Big endian highger byte is coming first
					m_SystemIdentifier = (sysParam[2] << 8) | sysParam[3];
					m_FingerLibrarySize = (sysParam[4] << 8) | sysParam[5];
					m_SecurityLevel = (sysParam[6] << 8) | sysParam[7];
					m_DeviceAddress = (sysParam[8] << 32) | (sysParam[9] << 16) | (sysParam[10] << 8) | (sysParam[11] && 0xffff);
					uint16_t pck = sysParam[12] << 8 | sysParam[13];

					uint16_t i = 0;
					m_DataPackeSize = 32;
					while (i < pck) { m_DataPackeSize = m_DataPackeSize * 2; i++; }

					m_BaudSettings = (sysParam[14] << 8 | sysParam[15]) * 9600;
				}


			}

		}


		UnLock();

		return ret;
	}

	uint8_t Sensor::getEcho(void) {
		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_GETECHO);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			// ret ==55H : sensor can Receive Commands
		}

		UnLock();

		return ret;
	}

	uint8_t Sensor::DownChar(uint8_t bufferID, const SensorPackageByteArray& DownChar) {

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(1);
		_input[0] = bufferID;



		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_DOWNCHAR, _input.size(), (uint8_t*)&_input[0]);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut * 2);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) { // ready for data transfer
				SensorPackageByteArray& transferarray = m_SensorPackage.getDownloadRecArray();
				transferarray = DownChar;

				m_SensorPackage.Lock();
			
	//			Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_DOWNCHAR);
				while (!transferarray.empty()) {
					Windows::Storage::Streams::IBuffer^ datapackage = m_SensorPackage.createDataPackage(transferarray, m_DataPackeSize);
					SendData(datapackage); // Send Data to Device
				//	Sleep(100);
				}

				m_SensorPackage.UnLock();
			}

		}

		UnLock();

		return ret;

	}


	uint8_t Sensor::UpChar(uint8_t bufferID, SensorPackageByteArray& UpChar) //upload character file or template
	{

		Lock();
		uint8_t ret = FINGERPRINT_TIMEOUT;
		std::vector< uint8_t> _input(1);
		_input[0] = bufferID;

		Windows::Storage::Streams::IBuffer^ package = m_SensorPackage.createPackage(FINGERPRINT_COMMANDPACKET, FINGERPRINT_CMD_UPCHAR, _input.size(), (uint8_t*)&_input[0]);
		m_SensorPackage.InitStates();
		SendData(package); // Send Data to Device

		bool ok = m_SensorPackage.waitForPacketRecv(m_RecvTmOut * 3);
		if (ok) {
			ret = m_SensorPackage.getConfirm();
			if (ret == FINGERPRINT_OK) { // ready for data transfer

				UpChar = m_SensorPackage.getUploadRecArray(); // in array zurück


			}

		}

		UnLock();

		return ret;
	}






	std::string Sensor::getCmdStringByState(uint8_t state) {

		fingerStateMapIt it = m_fingerStateMap.find(state);
		if (it != m_fingerStateMap.end()) {
			return it->second;
		}

		return "unkwon command from sensor";
	}

	void Sensor::DoProcessChunk(DataReader^ reader) // answer from Device
	{
		m_SensorPackage.DoProcessChunk(reader);

		m_acceptingData = true;
	}

	void Sensor::Lock() {
		EnterCriticalSection(&m_CritLock);
	}

	void Sensor::UnLock() {
		LeaveCriticalSection(&m_CritLock);
	}



	void Sensor::cancelwaitForPacket() {
		m_SensorPackage.cancelwaitForPacket();

	}

}
