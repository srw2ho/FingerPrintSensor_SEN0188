using FingerPrintSensor_SEN0188;
using System;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace FingerSensorsApp.Helpers
{
    public class SensorCMDs
    {
        static public void InitSensor(IPropertySet Inputpropertys)
        {
            Int32 CMD = Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerSensorInitialize");
            Inputpropertys["FingerPrint.CMD"] = PropertyValue.CreateInt32(CMD);
        }
        static public void VerifyFingerId(IPropertySet Inputpropertys)
        {
            Int32 CMD = Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerAutoVerifiying");
            Inputpropertys["FingerPrint.CMD"] = PropertyValue.CreateInt32(CMD);
        }
        static public void RegisterFingerId(IPropertySet Inputpropertys, UInt16 fingerId)
        {
            Inputpropertys["UpdateState"] = PropertyValue.CreateInt32(0);
            Int32 CMD = Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerAutoRegistration");
            Inputpropertys["FingerPrint.CMD"] = PropertyValue.CreateInt32(CMD);

            Inputpropertys["FingerPrint.FingerID"] = PropertyValue.CreateUInt16((UInt16)fingerId);
            Inputpropertys["UpdateState"] = PropertyValue.CreateInt32(1);
        }

        static public void DeleteFingerId(IPropertySet Inputpropertys, UInt16 fingerId)
        {
            Inputpropertys["UpdateState"] = PropertyValue.CreateInt32(0);
            Int32 CMD = Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerDeleteId");
            Inputpropertys["FingerPrint.CMD"] = PropertyValue.CreateInt32(CMD);

            Inputpropertys["FingerPrint.FingerID"] = PropertyValue.CreateUInt16((UInt16)fingerId);
            Inputpropertys["UpdateState"] = PropertyValue.CreateInt32(1);
        }

        static public void DownloadFingerId(IPropertySet Inputpropertys, UInt16 fingerId, byte[] array)
        {
            Inputpropertys["UpdateState"] = PropertyValue.CreateInt32(0);
            Int32 CMD = Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerCharDownLoad");
            Inputpropertys["FingerPrint.CMD"] = PropertyValue.CreateInt32(CMD);

            Inputpropertys["FingerPrint.FingerID"] = PropertyValue.CreateUInt16((UInt16)fingerId);
            Inputpropertys["FingerPrint.CHARDownLoad"] = PropertyValue.CreateUInt8Array(array);
            
            Inputpropertys["UpdateState"] = PropertyValue.CreateInt32(1);
        }
        static public void SetSensorID(IPropertySet Inputpropertys,  byte[] SensorID)
        {
            Inputpropertys["UpdateState"] = PropertyValue.CreateInt32(0);
            Int32 CMD = Connector_SEN0188_CMDs.getFingerPrintCmd("_doFingerWriteSensorID");
            Inputpropertys["FingerPrint.CMD"] = PropertyValue.CreateInt32(CMD);
            Inputpropertys["FingerPrint.SensorID"] = PropertyValue.CreateUInt8Array(SensorID);
            Inputpropertys["FingerPrint.PageID"] = PropertyValue.CreateUInt8(0);

            Inputpropertys["UpdateState"] = PropertyValue.CreateInt32(1);
        }

        



    }
}
