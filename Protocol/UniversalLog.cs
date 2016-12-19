﻿using System;
using System.Collections.Generic;
using Common;
using System.Data;
using Entity;

namespace Protocol
{
    public class UniversalLog : RWData
    {
        public bool Reset(ConstValue.DEV_TYPE devtype,short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.RESET;
            package.DataLength = 1;
            byte[] data = new byte[package.DataLength];
            data[0] = (byte)2;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool SetTime(ConstValue.DEV_TYPE devtype, short Id, DateTime dt)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_TIME;
            byte[] data = new byte[6];
            data[0] = (byte)(dt.Year-2000);
            data[1] = (byte)dt.Month;
            data[2] = (byte)dt.Day;
            data[3] = (byte)dt.Hour;
            data[4] = (byte)dt.Minute;
            data[5] = (byte)dt.Second;
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool EnableCollect(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.EnableCollect;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public short ReadId(ConstValue.DEV_TYPE devtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            //package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_ID;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 4)
            {
                throw new Exception("数据损坏");
            }
            return result.DevID;

        }

        public DateTime ReadTime(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_TIME;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return DateTime.Now;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 6)
            {
                throw new Exception("数据损坏");
            }
            return new DateTime(Convert.ToInt32(result.Data[0])+2000, Convert.ToInt32(result.Data[1]), Convert.ToInt32(result.Data[2]),
                Convert.ToInt32(result.Data[3]), Convert.ToInt32(result.Data[4]), Convert.ToInt32(result.Data[5]));
        }

        public string ReadVer(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_VER;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return "";

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length <5)
            {
                throw new Exception("数据损坏");
            }
            return System.Text.Encoding.Default.GetString(result.Data);
        }

        public string ReadFieldStrength(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_FIELDSTRENGTH;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package, 35, 1);
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 3)
            {
                throw new Exception("数据损坏");
            } 
            return "场强:"+result.Data[0]+",电压:"+ ((float)BitConverter.ToInt16(new byte[] { result.Data[2], result.Data[1] }, 0)) / 1000; 
        }

        public string ReadCellPhone(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_CELLPHONE;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return "";
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 11)
            {
                throw new Exception("数据损坏");
            }
            string number = "";
            for (int i = 0; i < result.DataLength; i++)
            {
                number += (char)result.Data[i];
            }
            return number;
        }

        public bool ReadModbusExeFlag(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_MODBUSEXEFLAG;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return true;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 1)
            {
                throw new Exception("数据损坏");
            }
            return (Convert.ToInt32(result.Data[0]) == 1) ? true : false;
        }

        public int Read485Baud(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType =devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_485BAUD;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 1)
            {
                throw new Exception("数据损坏");
            }
            return Convert.ToInt32(result.Data[0]);
        }

        public int ReadNetworkType(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_NETWORKTYPE;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 1)
            {
                throw new Exception("数据损坏");
            }
            return Convert.ToInt32(result.Data[0]);
        }

        public double ReadLimit(ConstValue.DEV_TYPE devtype, short Id, UniversalFlagType flagtype, UniversalAlarmType alarmtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            byte flag = 0x00;
            switch(flagtype)
            {
                case UniversalFlagType.Pressure1:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_PREUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_PRELOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_PRESLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_PRESLOPLOWLIMIT;
                    flag = 0x01;
                    break;
                case UniversalFlagType.Pressure2:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_PREUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_PRELOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_PRESLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_PRESLOPLOWLIMIT;
                    flag = 0x02;
                    break;
                case UniversalFlagType.Simulate1:
                    if(alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMLOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMSLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMSLOPLOWLIMIT;
                    flag = 0x01;
                    break;
                case UniversalFlagType.Simulate2:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMLOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMSLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMSLOPLOWLIMIT;
                    flag = 0x02;
                    break;
                case UniversalFlagType.Flow:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_FLOWUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_FLOWLOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_FLOWSLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.READ_FLOWSLOPLOWLIMIT;
                    flag = 0x01;
                    break;
            }
            package.DataLength = 1;
            byte[] data = new byte[package.DataLength];
            data[0] = flag;
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            switch (flagtype)
            {
                case UniversalFlagType.Pressure1:
                case UniversalFlagType.Pressure2:
                    if (result.Data.Length != 3)
                    {
                        throw new Exception("数据损坏");
                    }
                    return ((double)BitConverter.ToInt16(new byte[] { result.Data[2], result.Data[1] }, 0))/1000;
                case UniversalFlagType.Simulate1:
                case UniversalFlagType.Simulate2:
                    if (result.Data.Length != 7)
                    {
                        throw new Exception("数据损坏");
                    }
                    double range = 0;
                    range += BitConverter.ToInt16(new byte[] { result.Data[2], result.Data[1] }, 0);    //整数部分
                    range += ((double)BitConverter.ToInt16(new byte[] { result.Data[4], result.Data[3] }, 0)) / 1000;    //小数部分
                    return ((double)(BitConverter.ToInt16(new byte[] { result.Data[6], result.Data[5] }, 0))) * range / (ConstValue.UniversalSimRatio);
                case UniversalFlagType.Flow:
                    if (result.Data.Length != 5)
                    {
                        throw new Exception("数据损坏");
                    }
                    return ((double)BitConverter.ToInt32(new byte[] { result.Data[4], result.Data[3], result.Data[2], result.Data[1] }, 0))/1000;
                default:
                    return 0;
            }
        }
        
        public bool ReadAlarmEnable(ConstValue.DEV_TYPE devtype, short Id, UniversalFlagType flagtype, UniversalAlarmType alarmtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            if (alarmtype == UniversalAlarmType.UpAlarm)
                package.C1 = (byte)UNIVERSAL_COMMAND.READ_UPENABLE;
            else if (alarmtype == UniversalAlarmType.LowAlarm)
                package.C1 = (byte)UNIVERSAL_COMMAND.READ_LOWENABLE;
            else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                package.C1 = (byte)UNIVERSAL_COMMAND.READ_SLOPUPENABLE;
            else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                package.C1 = (byte)UNIVERSAL_COMMAND.READ_SLOPLOWENABLE;
            byte flag = 0x00;
            switch (flagtype)
            {
                case UniversalFlagType.Pressure1:
                    flag = 0x01;
                    break;
                case UniversalFlagType.Pressure2:
                    flag = 0x02;
                    break;
                case UniversalFlagType.Simulate1:
                    flag = 0x01;
                    break;
                case UniversalFlagType.Simulate2:
                    flag = 0x02;
                    break;
                case UniversalFlagType.Flow:
                    flag = 0x01;
                    break;
            }
            package.DataLength = 1;
            byte[] data = new byte[package.DataLength];
            data[0] = flag;
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return true;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 2)
            {
                throw new Exception("数据损坏");
            }
            return (result.Data[1] == 0x01) ? true : false;
        }

        public double ReadRange(ConstValue.DEV_TYPE devtype, short Id, UniversalFlagType flagtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            byte flag = 0x00;
            switch (flagtype)
            {
                case UniversalFlagType.Pressure1:
                    package.C1 = (byte)UNIVERSAL_COMMAND.READ_PRERANGE;
                    flag = 0x01;
                    break;
                case UniversalFlagType.Pressure2:
                    package.C1 = (byte)UNIVERSAL_COMMAND.READ_PRERANGE;
                    flag = 0x02;
                    break;
                case UniversalFlagType.Simulate1:
                    package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMRANGE;
                    flag = 0x01;
                    break;
                case UniversalFlagType.Simulate2:
                    package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMRANGE;
                    flag = 0x02;
                    break;
                //case UniversalFlagType.Flow:  //无流量
                //    flag = 0x01;
                //    break;
            }
            package.DataLength = 1;
            byte[] data = new byte[package.DataLength];
            data[0] = flag;
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (flagtype == UniversalFlagType.Pressure1 || flagtype == UniversalFlagType.Pressure2)   //压力
            {
                if (result.Data.Length != 3)
                {
                    throw new Exception("数据损坏");
                }
                return ((double)(BitConverter.ToInt16(new byte[] { result.Data[2], result.Data[1] }, 0))) /1000;
            }
            else     //模拟量
            {
                if (result.Data.Length != 5)
                {
                    throw new Exception("数据损坏");
                }
                double range = 0;
                range += BitConverter.ToInt16(new byte[] { result.Data[2], result.Data[1] }, 0);    //整数部分
                range += ((double)BitConverter.ToInt16(new byte[] { result.Data[4], result.Data[3] }, 0))/1000;    //小数部分
                return range;
            }
        }

        public double ReadPreOffset(ConstValue.DEV_TYPE devtype, short Id, UniversalFlagType flagtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_PREOFFSET;
            package.DataLength = 1;
            byte[] data = new byte[package.DataLength];
            byte flag = 0x00;
            if (flagtype == UniversalFlagType.Pressure1)
                flag = 0x01;
            else if (flagtype == UniversalFlagType.Pressure2)
                flag = 0x02;
            data[0] = flag;
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 3)
            {
                throw new Exception("数据损坏");
            }
            return ((double)BitConverter.ToInt16(new byte[] { result.Data[2], result.Data[1] }, 0))/1000;
        }

        public int ReadComType(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_COMTYPE;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 1)
            {
                throw new Exception("数据损坏");
            }
            return Convert.ToInt32(result.Data[0]);
        }

        public int[] ReadIP(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_IP;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return null;
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 15)
            {
                throw new Exception("数据损坏");
            }
            int[] ip = new int[4];
            for (int i = 0; i < 4; i++)
            {
                string tmp = "";
                for (int j = i * 4; j < i*4 + 3; j++)
                {
                    if ((char)result.Data[j] != '\0')
                        tmp += (char)result.Data[j];
                }
                if (string.IsNullOrEmpty(tmp))
                {
                    tmp = "0";
                }
                ip[i] = Convert.ToInt32(tmp);
            }

            return ip;
        }

        public int ReadPort(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_PORT;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 4 && result.Data.Length != 5)
            {
                throw new Exception("数据损坏");
            }
            string tmp = "";
            for (int i = 0; i < result.Data.Length; i++)
            {
                if ((char)result.Data[i] != '\0')
                    tmp += (char)result.Data[i];
            }
            if (string.IsNullOrEmpty(tmp))
            {
                tmp = "0";
            }
            return Convert.ToInt32(tmp);
        }

        public byte ReadCollectConfig(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_COLLECTCONFIG;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 1)
            {
                throw new Exception("数据损坏");
            }
            return result.Data[0];
        }

        public int ReadHeartInterval(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_HEARTINTERVAL;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 2)
            {
                throw new Exception("数据损坏");
            }
            return BitConverter.ToInt16(new byte[] { result.Data[1], result.Data[0] }, 0);
        }

        public int ReadVolInterval(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_VOLINTERVAL;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 2)
            {
                throw new Exception("数据损坏");
            }
            return BitConverter.ToInt16(new byte[] { result.Data[1], result.Data[0] }, 0);
        }

        public short ReadVolLower(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_VOLLOWER;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;
            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 1)
            {
                throw new Exception("数据损坏");
            }
            return Convert.ToInt16(result.Data[0]);
        }

        public short ReadSMSInterval(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_SMSINTERVAL;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 2)
            {
                throw new Exception("数据损坏");
            }
            return BitConverter.ToInt16(new byte[] { result.Data[1], result.Data[0] }, 0);
        }
        public short ReadPluseUnit(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_PLUSEUNIT;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return 0;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if (result.Data.Length != 1)
            {
                throw new Exception("数据损坏");
            }
            return Convert.ToInt16(result.Data[0]);
        }

        public DataTable ReadSimualteInterval(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_SIMINTERVAL;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return null;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if ((result.Data.Length%7) !=0)
            {
                throw new Exception("数据损坏");
            }
            DataTable dt_simulate = new DataTable();
            dt_simulate.Columns.Add("starttime");
            dt_simulate.Columns.Add("collecttime1");
            dt_simulate.Columns.Add("collecttime2");
            dt_simulate.Columns.Add("sendtime");
            for (int i = 0; i < result.DataLength; i+=7)
            {
                DataRow dr = dt_simulate.NewRow();
                dr["starttime"] = Convert.ToInt32(result.Data[i]);
                dr["sendtime"] = BitConverter.ToInt16(new byte[]{result.Data[i+2],result.Data[i+1]}, 0);
                dr["collecttime1"] = BitConverter.ToInt16(new byte[] { result.Data[i + 4], result.Data[i + 3] }, 0);
                dr["collecttime2"] = BitConverter.ToInt16(new byte[] { result.Data[i + 6], result.Data[i + 5] }, 0);
                dt_simulate.Rows.Add(dr);
            }

            return dt_simulate;
        }
        public DataTable ReadPluseInterval(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_PLUSEINTERVAL;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return null;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if ((result.Data.Length % 7) != 0)
            {
                throw new Exception("数据损坏");
            }
            DataTable dt_pluse = new DataTable();
            dt_pluse.Columns.Add("starttime");
            dt_pluse.Columns.Add("precollecttime");
            dt_pluse.Columns.Add("plusecollecttime");
            dt_pluse.Columns.Add("sendtime");
            for (int i = 0; i < result.DataLength; i += 7)
            {
                DataRow dr = dt_pluse.NewRow();
                dr["starttime"] = Convert.ToInt32(result.Data[i]);
                dr["sendtime"] = BitConverter.ToInt16(new byte[] { result.Data[i + 2], result.Data[i + 1] }, 0);
                dr["precollecttime"] = BitConverter.ToInt16(new byte[] { result.Data[i + 4], result.Data[i + 3] }, 0);
                dr["plusecollecttime"] = BitConverter.ToInt16(new byte[] { result.Data[i + 6], result.Data[i + 5] }, 0);
                dt_pluse.Rows.Add(dr);
            }

            return dt_pluse;
        }

        public DataTable ReadRS485Interval(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_485INTERVAL;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return null;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if ((result.Data.Length % 5) != 0)
            {
                throw new Exception("数据损坏");
            }
            DataTable dt_rs485 = new DataTable();
            dt_rs485.Columns.Add("starttime");
            dt_rs485.Columns.Add("collecttime");
            dt_rs485.Columns.Add("sendtime");
            for (int i = 0; i < result.DataLength; i += 5)
            {
                DataRow dr = dt_rs485.NewRow();
                dr["starttime"] = Convert.ToInt32(result.Data[i]);
                dr["sendtime"] = BitConverter.ToInt16(new byte[] { result.Data[i + 2], result.Data[i + 1] }, 0);
                dr["collecttime"] = BitConverter.ToInt16(new byte[] { result.Data[i + 4], result.Data[i + 3] }, 0);
                dt_rs485.Rows.Add(dr);
            }

            return dt_rs485;
        }

        public DataTable ReadModbusProtocol(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.READ_MODBUSPROTOCOL;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            Package result = Read(package);
            if (RWType == RWFunType.GPRS)
                return null;

            if (!result.IsSuccess || result.Data == null)
            {
                throw new Exception("获取失败");
            }
            if (result.Data.Length == 0)
            {
                throw new Exception("无数据");
            }
            if ((result.Data.Length % 7) != 0)
            {
                throw new Exception("数据损坏");
            }
            DataTable dt_485protocol = new DataTable();
            dt_485protocol.Columns.Add("baud");
            dt_485protocol.Columns.Add("ID");
            dt_485protocol.Columns.Add("funcode");
            dt_485protocol.Columns.Add("regbeginaddr");
            dt_485protocol.Columns.Add("regcount");
            for (int i = 0; i < result.DataLength; i += 7)
            {
                DataRow dr = dt_485protocol.NewRow();
                int baud = Convert.ToInt32(result.Data[i]);
                if (baud == 0)
                    dr["baud"] = 1200;
                else if (baud == 1)
                    dr["baud"] = 2400;
                else if (baud == 2)
                    dr["baud"] = 4800;
                else if (baud == 3)
                    dr["baud"] = 9600;
                dr["ID"] = Convert.ToInt32(result.Data[i+1]);
                dr["funcode"] = "0x"+String.Format("{0:X2}", result.Data[i + 2]);
                dr["regbeginaddr"] = "0x" + String.Format("{0:X2}", result.Data[i + 4]) + String.Format("{0:X2}", result.Data[i + 3]);//BitConverter.ToInt16(new byte[] { result.Data[i + 4], result.Data[i + 3] }, 0);
                dr["regcount"] = BitConverter.ToInt16(new byte[] { result.Data[i + 6], result.Data[i + 5] }, 0);
                dt_485protocol.Rows.Add(dr);
            }

            return dt_485protocol;
        }

        public DataTable ReadCallData(short Id,DataTable dt_config, CallDataTypeEntity calldataType)
        {
            DataTable dt = new DataTable("CallDataTable");
            dt.Columns.Add("CallDataType");
            dt.Columns.Add("CallData");
            dt.Columns.Add("Unit");
            Package package = new Package();
            
            if(calldataType.GetPre)
            {
                package = GetCallDataPackage(Id, UNIVERSAL_COMMAND.CallData_Pre1);
                Package result = Read(package);
                if (!result.IsSuccess || result.Data == null)
                {
                    throw new Exception("获取压力失败");
                }
                if (result.Data.Length == 0)
                {
                    throw new Exception("压力无数据");
                }
                AnalysisSim(Id, result, dt_config, ref dt);
            }
            if (calldataType.GetSim1)
            {
                package = GetCallDataPackage(Id, UNIVERSAL_COMMAND.CallData_Sim1);
                Package result = Read(package);
                if (!result.IsSuccess || result.Data == null)
                {
                    throw new Exception("获取模拟量1路失败");
                }
                if (result.Data.Length == 0)
                {
                    throw new Exception("模拟量1路无数据");
                }
                AnalysisSim(Id,result,dt_config,ref dt);
            }
            if (calldataType.GetSim2)
            {
                package = GetCallDataPackage(Id, UNIVERSAL_COMMAND.CallData_Sim2);
                Package result = Read(package);
                if (!result.IsSuccess || result.Data == null)
                {
                    throw new Exception("获取模拟量2路失败");
                }
                if (result.Data.Length == 0)
                {
                    throw new Exception("模拟量2路无数据");
                }
                AnalysisSim(Id, result, dt_config, ref dt);
            } 
            if (calldataType.GetPluse)
            {
                package = GetCallDataPackage(Id, UNIVERSAL_COMMAND.CallData_Pluse);
                Package result = Read(package);
                if (!result.IsSuccess || result.Data == null)
                {
                    throw new Exception("获取脉冲失败");
                }
                if (result.Data.Length == 0)
                {
                    throw new Exception("招测脉冲无数据");
                }
                AnalysisPluse(Id, result, dt_config, ref dt);
            }
            if(calldataType.GetRS4851)
            {
                package = GetCallDataPackage(Id, UNIVERSAL_COMMAND.CallData_RS4851);
                Package result = Read(package, 15, 1);
                if (!result.IsSuccess || result.Data == null)
                {
                    throw new Exception("获取RS485 1路失败");
                }
                if (result.Data.Length == 0)
                {
                    throw new Exception("招测RS485 1路无数据");
                }
                AnalysisRS485(Id, result, dt_config, ref dt);
            }
            if (calldataType.GetRS4852)
            {
                package = GetCallDataPackage(Id, UNIVERSAL_COMMAND.CallData_RS4852);
                Package result = Read(package, 15, 1);
                if (!result.IsSuccess || result.Data == null)
                {
                    throw new Exception("获取RS485 2路失败");
                }
                if (result.Data.Length == 0)
                {
                    throw new Exception("招测RS485 2路无数据");
                }
                AnalysisRS485(Id, result, dt_config, ref dt);
            }
            if (calldataType.GetRS4853)
            {
                package = GetCallDataPackage(Id, UNIVERSAL_COMMAND.CallData_RS4853);
                Package result = Read(package, 15, 1);
                if (!result.IsSuccess || result.Data == null)
                {
                    throw new Exception("获取RS485 3路失败");
                }
                if (result.Data.Length == 0)
                {
                    throw new Exception("招测RS485 3路无数据");
                }
                AnalysisRS485(Id, result, dt_config, ref dt);
            }
            if (calldataType.GetRS4854)
            {
                package = GetCallDataPackage(Id, UNIVERSAL_COMMAND.CallData_RS4854);
                Package result = Read(package, 15, 1);
                if (!result.IsSuccess || result.Data == null)
                {
                    throw new Exception("获取RS485 4路失败");
                }
                if (result.Data.Length == 0)
                {
                    throw new Exception("招测RS485 4路无数据");
                }
                AnalysisRS485(Id, result, dt_config, ref dt);
            }

            return dt;
        }

        private void AnalysisSim(short Id, Package pack,DataTable dt_config, ref DataTable dt)
        {
            string name = "";
            string sequence = "";
            if (pack.C1 == (byte)UNIVERSAL_COMMAND.CallData_Sim1)
            {
                sequence = "1";
            }
            else if (pack.C1 == (byte)UNIVERSAL_COMMAND.CallData_Sim2)
            {
                sequence = "2";
            }
            int calibration = BitConverter.ToInt16(new byte[] { pack.Data[7], pack.Data[6] }, 0);

            float datavalue = 0;

            DataRow[] dr_TerminalDataConfig = null;
            dr_TerminalDataConfig = dt_config.Select("TerminalID='" + Id + "' AND Sequence='" + sequence + "'"); //WayType
            if (dr_TerminalDataConfig != null && dr_TerminalDataConfig.Length > 0)
            {
                float MaxMeasureRange = dr_TerminalDataConfig[0]["MaxMeasureRange"] != DBNull.Value ? Convert.ToSingle(dr_TerminalDataConfig[0]["MaxMeasureRange"]) : 0;
                float MaxMeasureRangeFlag = dr_TerminalDataConfig[0]["MaxMeasureRangeFlag"] != DBNull.Value ? Convert.ToSingle(dr_TerminalDataConfig[0]["MaxMeasureRangeFlag"]) : 0;
                int datawidth = dr_TerminalDataConfig[0]["FrameWidth"] != DBNull.Value ? Convert.ToInt16(dr_TerminalDataConfig[0]["FrameWidth"]) : 0;
                int precision = dr_TerminalDataConfig[0]["precision"] != DBNull.Value ? Convert.ToInt32(dr_TerminalDataConfig[0]["precision"]) : 0;
                name = dr_TerminalDataConfig[0]["Name"] != DBNull.Value ? dr_TerminalDataConfig[0]["Name"].ToString().Trim() : "";
                if (MaxMeasureRangeFlag > 0 && datawidth > 0)
                {
                    int loopdatalen = 6+2 + datawidth;  //循环部分数据宽度 = 时间(6)+配置长度
                    int dataindex = (pack.DataLength) % loopdatalen;
                    if (dataindex != 0)
                        throw new Exception("招测模拟数据帧长度[" + pack.DataLength + "]不符合[6+2+" + loopdatalen + "*n]规则");
                    dataindex = (pack.DataLength) / loopdatalen;
                    for (int i = 0; i < dataindex; i++)
                    {
                        //year = 2000 + Convert.ToInt16(pack.Data[i * 8 + 3]);
                        //month = Convert.ToInt16(pack.Data[i * 8 + 4]);
                        //day = Convert.ToInt16(pack.Data[i * 8 + 5]);
                        //hour = Convert.ToInt16(pack.Data[i * 8 + 6]);
                        //minute = Convert.ToInt16(pack.Data[i * 8 + 7]);
                        //sec = Convert.ToInt16(pack.Data[i * 8 + 8]);

                        if (datawidth == 2)
                            datavalue = BitConverter.ToInt16(new byte[] { pack.Data[i * 8 + 9], pack.Data[i * 8 + 8] }, 0);
                        else if (datawidth == 4)
                            datavalue = BitConverter.ToSingle(new byte[] { pack.Data[i * 8 + 11], pack.Data[i * 8 + 10], pack.Data[i * 8 + 9], pack.Data[i * 8 + 8] }, 0);

                        datavalue = (MaxMeasureRange / MaxMeasureRangeFlag) * (datavalue - calibration);  //根据设置和校准值计算
                        datavalue = Convert.ToSingle(datavalue.ToString("F" + precision));  //精度调整
                        if (datavalue < 0)
                            datavalue = 0;

                        DataRow dr = dt.NewRow();
                        dr["CallDataType"] = name.Trim();
                        dr["CallData"] = datavalue;
                        dr["Unit"] = dr_TerminalDataConfig[0]["Unit"].ToString().Trim();
                        dt.Rows.Add(dr);
                    }
                }
                else
                {
                    throw new Exception("通用终端[" + Id + "]数据帧解析规则配置错误,数据未能解析！");
                }
            }
            else
            {
                throw new Exception("通用终端[" + Id + "]未配置数据帧解析规则,数据未能解析！");
            }
        }

        private void AnalysisPluse(short Id, Package pack, DataTable dt_config, ref DataTable dt)
        {
            float datavalue = 0;

            DataRow[] dr_TerminalDataConfig = null;
            dr_TerminalDataConfig = dt_config.Select("TerminalID='" + Id + "' AND Sequence IN ('4','5','6','7','8')", "Sequence"); //WayType
            if (dr_TerminalDataConfig != null && dr_TerminalDataConfig.Length > 0)
            {
                int waycount = dr_TerminalDataConfig.Length;
                float[] PluseUnits = new float[waycount];
                int[] DataWidths = new int[waycount];
                int[] Precisions = new int[waycount];
                string[] Names = new string[waycount];
                string[] Units = new string[waycount];
                int[] config_ids = new int[waycount];

                int topdatawidth = 0;
                for (int i = 0; i < waycount; i++)
                {
                    PluseUnits[i] = dr_TerminalDataConfig[i]["MaxMeasureRange"] != DBNull.Value ? Convert.ToSingle(dr_TerminalDataConfig[i]["MaxMeasureRange"]) : 0;  //每个脉冲对应的单位采集量
                    DataWidths[i] = dr_TerminalDataConfig[i]["FrameWidth"] != DBNull.Value ? Convert.ToInt16(dr_TerminalDataConfig[i]["FrameWidth"]) : 0;
                    Precisions[i] = dr_TerminalDataConfig[i]["precision"] != DBNull.Value ? Convert.ToInt32(dr_TerminalDataConfig[i]["precision"]) : 0;
                    Names[i] = dr_TerminalDataConfig[i]["Name"] != DBNull.Value ? dr_TerminalDataConfig[i]["Name"].ToString().Trim() : "";
                    Units[i] = dr_TerminalDataConfig[i]["Unit"] != DBNull.Value ? dr_TerminalDataConfig[i]["Unit"].ToString().Trim() : "";
                    config_ids[i] = dr_TerminalDataConfig[i]["ID"] != DBNull.Value ? Convert.ToInt32(dr_TerminalDataConfig[i]["ID"]) : 0;
                    topdatawidth += DataWidths[i];
                }

                if (topdatawidth > 0)
                {
                    int loopdatalen = 6 + topdatawidth + (4 - waycount) * 4;  //循环部分数据宽度 = 时间(6)+固定4路*(每路长度)
                    int dataindex = (pack.DataLength) % loopdatalen;
                    if (dataindex != 0)
                        throw new Exception("脉冲帧数据长度[" + pack.DataLength + "]不符合" + loopdatalen + "*n规则");
                    dataindex = (pack.DataLength) / loopdatalen;
                    for (int i = 0; i < dataindex; i++)
                    {
                        //year = 2000 + Convert.ToInt16(pack.Data[i * loopdatalen + 3]);
                        //month = Convert.ToInt16(pack.Data[i * loopdatalen + 4]);
                        //day = Convert.ToInt16(pack.Data[i * loopdatalen + 5]);
                        //hour = Convert.ToInt16(pack.Data[i * loopdatalen + 6]);
                        //minute = Convert.ToInt16(pack.Data[i * loopdatalen + 7]);
                        //sec = Convert.ToInt16(pack.Data[i * loopdatalen + 8]);

                        int freindex = 0;
                        for (int j = 0; j < waycount; j++)
                        {
                            if (DataWidths[j] == 2)
                            {
                                datavalue = BitConverter.ToInt16(new byte[] { pack.Data[i * loopdatalen + 7 + freindex], pack.Data[i * loopdatalen + 6 + freindex] }, 0);
                                freindex += 2;
                            }
                            else if (DataWidths[j] == 4)
                            {
                                datavalue = BitConverter.ToInt32(new byte[] { pack.Data[i * loopdatalen + 9 + freindex], pack.Data[i * loopdatalen + 8 + freindex], pack.Data[i * loopdatalen + 7 + freindex], pack.Data[i * loopdatalen + 6 + freindex] }, 0);
                                freindex += 4;
                            }

                            datavalue = PluseUnits[j] * datavalue;  //脉冲计数*单位脉冲值
                            datavalue = Convert.ToSingle(datavalue.ToString("F" + Precisions[j]));  //精度调整

                            DataRow dr = dt.NewRow();
                            dr["CallDataType"] = Names[j].Trim();
                            dr["CallData"] = datavalue;
                            dr["Unit"] = Units[j].ToString().Trim();
                            dt.Rows.Add(dr);
                        }
                    }
                }
                else
                {
                    throw new Exception("通用终端[" + Id + "]数据帧解析规则配置错误,数据未能解析！");
                }
            }
            else
            {
                throw new Exception("通用终端[" + Id + "]未配置数据帧解析规则,数据未能解析！");
            }
        }

        private void AnalysisRS485(short Id, Package pack, DataTable dt_config, ref DataTable dt)
        {
            string sequence = "";
            string name = "";
            if (pack.C1 == (byte)UNIVERSAL_COMMAND.CallData_RS4851)
            {
                sequence = "9";name = "RS485 1路";
            }
            else if (pack.C1 == (byte)UNIVERSAL_COMMAND.CallData_RS4852)
            {
                sequence = "10";name = "RS485 2路";
            }
            else if (pack.C1 == (byte)UNIVERSAL_COMMAND.CallData_RS4853)
            {
                sequence = "11"; name = "RS485 3路";
            }
            else if (pack.C1 == (byte)UNIVERSAL_COMMAND.CallData_RS4854)
            {
                sequence = "12"; name = "RS485 4路";
            }
            int calibration = BitConverter.ToInt16(new byte[] { pack.Data[1], pack.Data[0] }, 0);

            int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
            float datavalue = 0;

            DataRow[] dr_TerminalDataConfig = null;
            DataRow[] dr_DataConfig_Child = null;
            bool ConfigHaveChild = false;

            dr_TerminalDataConfig = dt_config.Select("TerminalID='" + Id + "' AND Sequence='" + sequence + "'"); //WayType
            if (dr_TerminalDataConfig != null && dr_TerminalDataConfig.Length > 0)
            {
                dr_DataConfig_Child = dt_config.Select("ParentID='" + dr_TerminalDataConfig[0]["ID"].ToString().Trim() + "'", "Sequence");
                if (dr_DataConfig_Child != null && dr_DataConfig_Child.Length > 0)
                {
                    ConfigHaveChild = true;
                    dr_TerminalDataConfig = dr_DataConfig_Child;  //有子节点配置时，使用子节点配置
                }
            }
            if (dr_TerminalDataConfig != null && dr_TerminalDataConfig.Length > 0)
            {
                int waycount = dr_TerminalDataConfig.Length;
                float[] MaxMeasureRanges = new float[waycount];
                float[] MaxMeasureRangeFlags = new float[waycount];
                int[] DataWidths = new int[waycount];
                int[] Precisions = new int[waycount];
                string[] Names = new string[waycount];
                string[] Units = new string[waycount];
                int[] config_ids = new int[waycount];

                int topdatawidth = 0;
                for (int i = 0; i < waycount; i++)
                {
                    MaxMeasureRanges[i] = dr_TerminalDataConfig[i]["MaxMeasureRange"] != DBNull.Value ? Convert.ToSingle(dr_TerminalDataConfig[i]["MaxMeasureRange"]) : 0;
                    MaxMeasureRangeFlags[i] = dr_TerminalDataConfig[0]["MaxMeasureRangeFlag"] != DBNull.Value ? Convert.ToSingle(dr_TerminalDataConfig[0]["MaxMeasureRangeFlag"]) : 0;
                    DataWidths[i] = dr_TerminalDataConfig[i]["FrameWidth"] != DBNull.Value ? Convert.ToInt16(dr_TerminalDataConfig[i]["FrameWidth"]) : 0;
                    Precisions[i] = dr_TerminalDataConfig[i]["precision"] != DBNull.Value ? Convert.ToInt32(dr_TerminalDataConfig[i]["precision"]) : 0;
                    Names[i] = dr_TerminalDataConfig[i]["Name"] != DBNull.Value ? dr_TerminalDataConfig[i]["Name"].ToString().Trim() : "";
                    Units[i] = dr_TerminalDataConfig[i]["Unit"] != DBNull.Value ? dr_TerminalDataConfig[i]["Unit"].ToString().Trim() : "";
                    config_ids[i] = dr_TerminalDataConfig[i]["ID"] != DBNull.Value ? Convert.ToInt32(dr_TerminalDataConfig[i]["ID"]) : 0;
                    topdatawidth += DataWidths[i];
                }

                if (topdatawidth > 0)
                {
                    int loopdatalen = 6 + topdatawidth;  //循环部分数据宽度
                    int dataindex = (pack.DataLength) % loopdatalen;
                    if (dataindex != 0)
                        throw new Exception(name+"帧数据长度[" + pack.DataLength + "]不符合" + loopdatalen + "*n规则");
                    dataindex = (pack.DataLength) / loopdatalen;
                    for (int i = 0; i < dataindex; i++)
                    {
                        year = 2000 + Convert.ToInt16(pack.Data[i * loopdatalen]);
                        month = Convert.ToInt16(pack.Data[i * loopdatalen + 1]);
                        day = Convert.ToInt16(pack.Data[i * loopdatalen + 2]);
                        hour = Convert.ToInt16(pack.Data[i * loopdatalen + 3]);
                        minute = Convert.ToInt16(pack.Data[i * loopdatalen + 4]);
                        sec = Convert.ToInt16(pack.Data[i * loopdatalen + 5]);

                        int freindex = 0;
                        for (int j = 0; j < waycount; j++)
                        {
                            if (DataWidths[j] == 2)
                            {
                                datavalue = BitConverter.ToInt16(new byte[] { pack.Data[i * loopdatalen + 7 + freindex], pack.Data[i * loopdatalen + 6 + freindex] }, 0);
                                freindex += 2;
                            }
                            else if (DataWidths[j] == 4)
                            {
                                datavalue = BitConverter.ToInt32(new byte[] { pack.Data[i * loopdatalen + 9 + freindex], pack.Data[i * loopdatalen + 8 + freindex], pack.Data[i * loopdatalen + 7 + freindex], pack.Data[i * loopdatalen + 6 + freindex] }, 0);
                                freindex += 4;
                            }

                            datavalue = MaxMeasureRanges[j] * datavalue;  //系数
                            datavalue = Convert.ToSingle(datavalue.ToString("F" + Precisions[j]));  //精度调整
                            DataRow dr = dt.NewRow();
                            dr["CallDataType"] = Names[j].Trim();
                            dr["CallData"] = datavalue;
                            dr["Unit"] = Units[j].ToString().Trim();
                            dt.Rows.Add(dr);
                        }
                    }
                }
                else
                {
                    throw new Exception("通用终端[" + Id + "]数据帧解析规则配置错误,数据未能解析！");
                }
            }
            else
            {
                throw new Exception("通用终端[" + Id + "]未配置数据帧解析规则,数据未能解析！");
            }
        }

        private Package GetCallDataPackage(short Id,UNIVERSAL_COMMAND commandtype)
        {
            Package package = new Package();
            package.DevType = ConstValue.DEV_TYPE.UNIVERSAL_CTRL; ;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)commandtype;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();
            return package;
        }

        public bool CalibartionSimulate1(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.CalibartionSimualte1;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool CalibartionSimulate2(ConstValue.DEV_TYPE devtype, short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.CalibartionSimualte2;
            package.DataLength = 0;
            byte[] data = new byte[package.DataLength];
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool SetID(ConstValue.DEV_TYPE devtype,short Id)
        {
            Package package = new Package();
            package.DevType = devtype;
            //package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_ID;
            byte[] data = BitConverter.GetBytes((int)Id);
            Array.Reverse(data);
            data[0] = (byte)(ConstValue.DEV_TYPE.UNIVERSAL_CTRL);
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool Set(ConstValue.DEV_TYPE devtype, short Id,byte funcode, byte[] data)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = funcode;
            
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool Set(ConstValue.DEV_TYPE devtype, short Id, byte funcode, byte data)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = funcode;

            package.DataLength = 1;
            package.Data = new byte[] { data };
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool SetIP(ConstValue.DEV_TYPE devtype,short id, int[] ips)
        {
            string ip = "";
            for (int i = 0; i < ips.Length; i++)
            {
                ip += ips[i].ToString().PadLeft(3,'0')+".";
            }
            if (!string.IsNullOrEmpty(ip))
                ip = ip.Substring(0, ip.Length - 1);
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_IP;
            package.DataLength = 15;
            byte[] data = new byte[package.DataLength];
            for (int i = 0; i < ip.Length; i++)
            {
                data[i] = (byte)ip[i];
            }
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool SetCollectConfig(ConstValue.DEV_TYPE devtype, short Id, byte config)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_COLLECTCONFIG;
            byte[] data = new byte[1];
            data[0] = config;
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        /// <summary>
        /// 设置限值
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="limit"></param>
        /// <param name="range">量程</param>
        /// <param name="flagtype"></param>
        /// <param name="alarmtype"></param>
        public bool SetLimit(ConstValue.DEV_TYPE devtype, short Id,double limit,double range, UniversalFlagType flagtype, UniversalAlarmType alarmtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            byte[] data=new byte[3];
            byte flag = 0x00;
            switch (flagtype)
            {
                case UniversalFlagType.Pressure1:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_PREUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_PRELOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_PRESLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_PRESLOPLOWLIMIT;
                    flag = 0x01;
                    data = new byte[3];
                    Array.Copy(BitConverter.GetBytes((short)(limit*1000)), data, 2);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
                case UniversalFlagType.Pressure2:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_PREUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_PRELOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_PRESLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_PRESLOPLOWLIMIT;
                    flag = 0x02;
                    data = new byte[3];
                    Array.Copy(BitConverter.GetBytes((short)(limit * 1000)), data, 2);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
                case UniversalFlagType.Simulate1:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMLOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMSLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMSLOPLOWLIMIT;
                    flag = 0x01;
                    data = new byte[3];
                    Array.Copy(BitConverter.GetBytes((short)Math.Round(ConstValue.UniversalSimRatio * limit / range)), data, 2);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
                case UniversalFlagType.Simulate2:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMLOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMSLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMSLOPLOWLIMIT;
                    flag = 0x02;
                    data = new byte[3];
                    Array.Copy(BitConverter.GetBytes((short)Math.Round(ConstValue.UniversalSimRatio * limit / range)), data, 2);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
                case UniversalFlagType.Flow:
                    if (alarmtype == UniversalAlarmType.UpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_FLOWUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.LowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_FLOWLOWLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_FLOWSLOPUPLIMIT;
                    else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                        package.C1 = (byte)UNIVERSAL_COMMAND.SET_FLOWSLOPLOWLIMIT;
                    flag = 0x01;
                    data = new byte[5];
                    Array.Copy(BitConverter.GetBytes((int)(limit * 1000)), data, 4);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
            }
            package.DataLength = data.Length;
            
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }
        public bool SetAlarmEnable(ConstValue.DEV_TYPE devtype, short Id, bool enable,UniversalFlagType flagtype, UniversalAlarmType alarmtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            if (alarmtype == UniversalAlarmType.UpAlarm)
                package.C1 = (byte)UNIVERSAL_COMMAND.SET_UPENABLE;
            else if (alarmtype == UniversalAlarmType.LowAlarm)
                package.C1 = (byte)UNIVERSAL_COMMAND.SET_LOWENABLE;
            else if (alarmtype == UniversalAlarmType.SlopUpAlarm)
                package.C1 = (byte)UNIVERSAL_COMMAND.SET_SLOPUPENABLE;
            else if (alarmtype == UniversalAlarmType.SlopLowAlarm)
                package.C1 = (byte)UNIVERSAL_COMMAND.SET_SLOPLOWENABLE;
            byte flag = 0x00;
            switch (flagtype)
            {
                case UniversalFlagType.Pressure1:
                    flag = 0x01;
                    break;
                case UniversalFlagType.Pressure2:
                    flag = 0x02;
                    break;
                case UniversalFlagType.Simulate1:
                    flag = 0x01;
                    break;
                case UniversalFlagType.Simulate2:
                    flag = 0x02;
                    break;
                case UniversalFlagType.Flow:
                    flag = 0x01;
                    break;
            }
            byte[] data = new byte[2];
            data[0] = flag;
            data[1] = (byte)(enable ? 0x01 : 0x00);
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }
        
        public bool SetRange(ConstValue.DEV_TYPE devtype, short Id, double range, UniversalFlagType flagtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            byte flag = 0x00;
            byte[] data = new byte[3];
            switch (flagtype)
            {
                case UniversalFlagType.Pressure1:
                    package.C1 = (byte)UNIVERSAL_COMMAND.SET_PRERANGE;
                    flag = 0x01;
                    data = new byte[3];
                    Array.Copy(BitConverter.GetBytes((short)(range*1000)), data, 2);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
                case UniversalFlagType.Pressure2:
                    package.C1 = (byte)UNIVERSAL_COMMAND.SET_PRERANGE;
                    flag = 0x02;
                    data = new byte[3];
                    Array.Copy(BitConverter.GetBytes((short)(range * 1000)), data, 2);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
                case UniversalFlagType.Simulate1:
                    package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMRANGE;
                    flag = 0x01;
                    data = new byte[5];
                    short round = (short)range; //整数部分
                    short deci = (short)Math.Round((range - round) * 1000);   //小数部分*1000
                    Array.Copy(BitConverter.GetBytes(deci), data,2);
                    Array.Copy(BitConverter.GetBytes(round), 0, data, 2, 2);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
                case UniversalFlagType.Simulate2:
                    package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMRANGE;
                    flag = 0x02;
                    data = new byte[5];
                    round = (short)range; //整数部分
                    deci = (short)Math.Round((range - round) * 1000);   //小数部分*1000
                    Array.Copy(BitConverter.GetBytes(deci), data, 2);
                    Array.Copy(BitConverter.GetBytes(round), 0, data, 2, 2);
                    Array.Reverse(data);
                    data[0] = flag;
                    break;
                    //case UniversalFlagType.Flow:  //无流量
                    //    flag = 0x01;
                    //    break;
            }
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool SetPreOffset(ConstValue.DEV_TYPE devtype, short Id, double offset, UniversalFlagType flagtype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_PREOFFSET;
            byte flag = 0x00;
            switch (flagtype)       //偏移值只有压力有，模拟量和流量没有
            {
                case UniversalFlagType.Pressure1:
                    flag = 0x01;
                    break;
                case UniversalFlagType.Pressure2:
                    flag = 0x02;
                    break;
            }
            byte[] data = new byte[3];
            Array.Copy(BitConverter.GetBytes((short)(offset*1000)), data, 2);
            Array.Reverse(data);
            data[0] = flag;
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool SetCallEnable(ConstValue.DEV_TYPE devtype, short Id, bool Enable)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_CALLENABLE;
            byte[] data = new byte[1];
            data[0] = Enable ? (byte)0x01 : (byte)0x00; //1-使能招测 0 - 关闭招测
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        public bool SetSimulateInterval(ConstValue.DEV_TYPE devtype, short Id, DataTable dt)
        {
            Package package = GetSimulateIntervalPackage(devtype, Id, dt);

            return Write(package);
        }

        public Package GetSimulateIntervalPackage(ConstValue.DEV_TYPE devtype, short Id, DataTable dt)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_SIMINTERVAL;
            List<byte> lstdata = new List<byte>();
            byte[] data;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["starttime"] != DBNull.Value && dr["sendtime"] != DBNull.Value && dr["collecttime1"] != DBNull.Value && dr["collecttime2"] != DBNull.Value)
                {
                    lstdata.Add(Convert.ToByte(dr["starttime"]));
                    data = BitConverter.GetBytes(Convert.ToInt16(dr["sendtime"]));
                    lstdata.Add(data[1]);
                    lstdata.Add(data[0]);
                    data = BitConverter.GetBytes(Convert.ToInt16(dr["collecttime1"]));
                    lstdata.Add(data[1]);
                    lstdata.Add(data[0]);
                    data = BitConverter.GetBytes(Convert.ToInt16(dr["collecttime2"]));
                    lstdata.Add(data[1]);
                    lstdata.Add(data[0]);
                }
            }

            data = lstdata.ToArray();
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return package;
        }

        public bool SetPluseInterval(ConstValue.DEV_TYPE devtype, short Id, DataTable dt)
        {
            Package package = GetPluseIntervalPackage(devtype, Id, dt);

            return Write(package);
        }

        public Package GetPluseIntervalPackage(ConstValue.DEV_TYPE devtype, short Id, DataTable dt)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_PLUSEINTERVAL;
            List<byte> lstdata = new List<byte>();
            byte[] data;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["starttime"] != DBNull.Value && dr["sendtime"] != DBNull.Value && dr["precollecttime"] != DBNull.Value && dr["plusecollecttime"]!=DBNull.Value)
                {
                    lstdata.Add(Convert.ToByte(dr["starttime"]));
                    data = BitConverter.GetBytes(Convert.ToInt16(dr["sendtime"]));
                    lstdata.Add(data[1]);
                    lstdata.Add(data[0]);
                    data = BitConverter.GetBytes(Convert.ToInt16(dr["precollecttime"]));
                    lstdata.Add(data[1]);
                    lstdata.Add(data[0]);
                    data = BitConverter.GetBytes(Convert.ToInt16(dr["plusecollecttime"]));
                    lstdata.Add(data[1]);
                    lstdata.Add(data[0]);
                }
            }

            data = lstdata.ToArray();
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return package;
        }

        public bool SetRS485Interval(ConstValue.DEV_TYPE devtype, short Id, DataTable dt)
        {
            Package package = GetRS485IntervalPackage(devtype, Id, dt);

            return Write(package);
        }

        public Package GetRS485IntervalPackage(ConstValue.DEV_TYPE devtype, short Id, DataTable dt)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_485INTERVAL;
            List<byte> lstdata = new List<byte>();
            byte[] data;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["starttime"] != DBNull.Value && dr["sendtime"] != DBNull.Value && dr["collecttime"] != DBNull.Value)
                {
                    lstdata.Add(Convert.ToByte(dr["starttime"]));
                    data = BitConverter.GetBytes(Convert.ToInt16(dr["sendtime"]));
                    lstdata.Add(data[1]);
                    lstdata.Add(data[0]);
                    data = BitConverter.GetBytes(Convert.ToInt16(dr["collecttime"]));
                    lstdata.Add(data[1]);
                    lstdata.Add(data[0]);
                }
            }

            data = lstdata.ToArray();
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return package;
        }

        public bool SetModbusProtocol(ConstValue.DEV_TYPE devtype, short Id, DataTable dt)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            package.C1 = (byte)UNIVERSAL_COMMAND.SET_MODBUSPROTOCOL;
            List<byte> lstdata = new List<byte>();
            byte[] data;
            int validrow = 0;
            for(int i = 0; i<dt.Rows.Count;i++)
            {
                DataRow dr = dt.Rows[i];
                if (dr["baud"] != DBNull.Value && dr["ID"] != DBNull.Value && dr["funcode"] != DBNull.Value&&
                    dr["regbeginaddr"] != DBNull.Value && dr["regcount"] != DBNull.Value )
                {
                    int baud = Convert.ToInt32(dr["baud"]);
                    if (baud == 1200)
                        lstdata.Add(0x00);
                    else if (baud == 2400)
                        lstdata.Add(0x01);
                    else if (baud == 4800)
                        lstdata.Add(0x02);
                    else if (baud == 9600)
                        lstdata.Add(0x03);

                    string id = dr["ID"].ToString().Trim();
                    if (id.StartsWith("0x")&&id.Length>2)
                        lstdata.Add(Convert.ToByte(id.Substring(2), 16));
                    else
                        lstdata.Add(Convert.ToByte(dr["ID"]));

                    string funcode = dr["funcode"].ToString().Trim();
                    if (funcode.StartsWith("0x") && funcode.Length > 2)
                        lstdata.Add(Convert.ToByte(funcode.Substring(2), 16));
                    else
                        lstdata.Add(Convert.ToByte(dr["funcode"]));

                    string regbeginaddr = dr["regbeginaddr"].ToString().Trim();
                    if (regbeginaddr.StartsWith("0x") && regbeginaddr.Length > 2)
                    {
                        data = BitConverter.GetBytes(Convert.ToInt16(regbeginaddr.Substring(2), 16));
                        lstdata.Add(data[1]);
                        lstdata.Add(data[0]);
                    }
                    else
                    {
                        data = BitConverter.GetBytes(Convert.ToInt16(dr["regbeginaddr"]));
                        lstdata.Add(data[1]);
                        lstdata.Add(data[0]);
                    }

                    string regcount = dr["regcount"].ToString().Trim();
                    if (regcount.StartsWith("0x") && regcount.Length > 2)
                    {
                        data = BitConverter.GetBytes(Convert.ToInt16(regcount.Substring(2), 16));
                        lstdata.Add(data[1]);
                        lstdata.Add(data[0]); 
                    }
                    else
                    {
                        data = BitConverter.GetBytes(Convert.ToInt16(dr["regcount"]));
                        lstdata.Add(data[1]);
                        lstdata.Add(data[0]);
                    }
                    validrow++;
                }
            }
            for (; validrow < 4; validrow++)
            {
                lstdata.AddRange(new byte[] { 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });  //补齐行
            }

            data = lstdata.ToArray();
            package.DataLength = data.Length;
            package.Data = data;
            package.CS = package.CreateCS();

            return Write(package);
        }

        /// <summary>
        /// 设置第?路脉冲基准数
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="value">基准数</param>
        /// <param name="waytype">1:第一路脉冲,2:第二路脉冲,3:第三路脉冲,4:第四路脉冲</param>
        /// <returns></returns>
        public bool SetPluseBasic(ConstValue.DEV_TYPE devtype, short Id,UInt32 value,int waytype)
        {
            Package package = GetPluseBasicPackage(devtype, Id,value,waytype);

            return Write(package);
        }

        /// <summary>
        /// 获得第?路脉冲基准数数据包
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="value">基准数</param>
        /// <param name="waytype">1:第一路脉冲,2:第二路脉冲,3:第三路脉冲,4:第四路脉冲</param>
        /// <returns></returns>
        public Package GetPluseBasicPackage(ConstValue.DEV_TYPE devtype, short Id, UInt32 value, int waytype)
        {
            Package package = new Package();
            package.DevType = devtype;
            package.DevID = Id;
            package.CommandType = CTRL_COMMAND_TYPE.REQUEST_BY_MASTER;
            if (waytype == 1)
                package.C1 = (byte)UNIVERSAL_COMMAND.SET_PLUSEBASIC1;
            else if (waytype == 2)
                package.C1 = (byte)UNIVERSAL_COMMAND.SET_PLUSEBASIC2;
            else if (waytype == 3)
                package.C1 = (byte)UNIVERSAL_COMMAND.SET_PLUSEBASIC3;
            else if (waytype == 4)
                package.C1 = (byte)UNIVERSAL_COMMAND.SET_PLUSEBASIC4;
            byte[] data = BitConverter.GetBytes(value);
            List<byte> lstdata = new List<byte>();
            if (data.Length == 4)
            {
                lstdata.Add(data[3]);
                lstdata.Add(data[2]);
                lstdata.Add(data[1]);
                lstdata.Add(data[0]);
            }
            package.DataLength = data.Length;
            package.Data = lstdata.ToArray();
            package.CS = package.CreateCS();

            return package;
        }

    }
}
