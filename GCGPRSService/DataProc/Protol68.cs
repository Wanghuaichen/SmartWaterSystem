﻿using Common;
using Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GCGPRSService
{
    public class Protol68
    {
        public void ProcData(StateObject state,Package pack, string str_frame,out bool bNeedCheckTime)
        {
            bNeedCheckTime = false;
            float volvalue = -1;  //电压,如果是没有这个电压值的,赋值为-1，保存至数据库时根据-1保存空
            Int16 field_strength = -1; //场强(0-31,99表示没信号)
            if (pack.ID3 == (byte)Entity.ConstValue.DEV_TYPE.Data_CTRL)
            {
                #region 压力终端
                bool addtion_voldata = false;   //是否在数据段最后增加了两个字节的电压数据
                bool addtion_strength = false;  //是否在数据段最后增加了两个字节的电压数据和一个字节的场强数据
                if (pack.C1 == (byte)GPRS_READ.READ_PREDATA)  //从站向主站发送压力采集数据
                {
                    int dataindex = (pack.DataLength - 2 - 1) % 8;
                    if (dataindex != 0)
                    {
                        if (dataindex == 2)
                        {
                            dataindex = (pack.DataLength - 2 - 1 - 2) / 8;      //带电压
                            addtion_voldata = true;
                        }
                        else if(dataindex == 3)
                        {
                            dataindex = (pack.DataLength - 2 - 1 - 3) / 8;      //带电压和场强
                            addtion_strength = true;
                        }
                        else
                        {
                            throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+8*n)或(2+1+8*n+2)规则");  //GPRS远程压力终端在数据段最后增加两个字节的电压数据
                        }
                    }
                    else
                        dataindex = (pack.DataLength - 2 - 1) / 8;

                    StringBuilder str_alarm = new StringBuilder();
                    int preFlag = 0;
                    
                    /****************************************宿州校准压力值********************************************/
                    //double[] RectifyValue = new double[] {  //修偏数组
                    //    -0.009, 0, -0.03, 0.013, -0.029, -0.029, 0, 0, 0, -0.011,
                    //    -0.008, -0.026, -0.009, -0.006, -0.009, -0.021, 0, -0.01, 0, -0.01,
                    //    -0.007, -0.019, -0.021, -0.04, -0.01, -0.007, -0.014, -0.013, 0, -0.023 };
                    /**************************************************************************************************/

                    preFlag = Convert.ToInt16(pack.Data[2]);

                    GPRSPreFrameDataEntity framedata = new GPRSPreFrameDataEntity();
                    framedata.TerId = pack.ID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;
                    
                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    float pressuevalue = 0;
                    
                    if (addtion_voldata)
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 1], pack.Data[pack.DataLength - 2] }, 0)) / 1000;
                    if(addtion_strength)
                    {
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 2], pack.Data[pack.DataLength - 3] }, 0)) / 1000;
                        field_strength = (Int16)pack.Data[pack.DataLength - 1];
                    }
                    for (int i = 0; i < dataindex; i++)
                    {
                        year = 2000 + Convert.ToInt16(pack.Data[i * 8 + 3]);
                        month = Convert.ToInt16(pack.Data[i * 8 + 4]);
                        day = Convert.ToInt16(pack.Data[i * 8 + 5]);
                        hour = Convert.ToInt16(pack.Data[i * 8 + 6]);
                        minute = Convert.ToInt16(pack.Data[i * 8 + 7]);
                        sec = Convert.ToInt16(pack.Data[i * 8 + 8]);

                        pressuevalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[i * 8 + 10], pack.Data[i * 8 + 9] }, 0)) / 1000;
                        float TmpRectifyValue = 0;
                        if (pack.ID > 0 && pack.ID <= 30)
                        {
                            //TmpRectifyValue = (float)RectifyValue[pack.ID - 1];
                            pressuevalue += TmpRectifyValue;
                            if (pressuevalue < 0)
                                pressuevalue = 0;
                        }
                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|压力终端[{1}]|压力标志({2})|采集时间({3})|压力值:{4}MPa(纠偏值{5})|电压值:{6}V|信号强度:{7}",
                            i, pack.ID, preFlag, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, pressuevalue, TmpRectifyValue, volvalue, field_strength)));

                        GPRSPreDataEntity data = new GPRSPreDataEntity();
                        data.PreValue = pressuevalue;
                        data.Voltage = volvalue;
                        data.FieldStrength = field_strength;
                        try
                        {
                            data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                        }
                        catch { data.ColTime = ConstValue.MinDateTime; }
                        bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);
                        framedata.lstPreData.Add(data);
                    }

                    Dictionary<int, string> dictalarms = AlarmProc.GetAlarmName(pack.ID3, pack.C1, pack.Data[1], pack.Data[0]);
                    if (dictalarms != null && dictalarms.Count > 0)
                    {
                        GPRSAlarmFrameDataEntity alarmframedata = new GPRSAlarmFrameDataEntity();
                        alarmframedata.Frame = str_frame;
                        alarmframedata.TerId = pack.DevID.ToString();
                        alarmframedata.TerminalType = TerType.PreTer;
                        try
                        {
                            alarmframedata.ModifyTime = new DateTime(year, month, day, hour, minute, sec);
                        }
                        catch { alarmframedata.ModifyTime = ConstValue.MinDateTime; }

                        alarmframedata.AlarmId = new List<int>();
                        foreach (var de in dictalarms)
                        {
                            alarmframedata.AlarmId.Add(de.Key);
                            str_alarm.Append(de.Value + " ");
                            
                        }
                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("压力终端[{0}] {1}",
                            pack.ID, str_alarm)));

                        GlobalValue.Instance.GPRS_AlarmFrameData.Enqueue(alarmframedata);
                        GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertAlarm);
                    }

                    GlobalValue.Instance.GPRS_PreFrameData.Enqueue(framedata);  //通知存储线程处理
                    GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertPreValue);
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_FLOWDATA)  //从站向主站发送流量采集数据
                {
                    bool isBCD = false;  //数据是否为BCD码
                    int dataindex = (pack.DataLength - 2 - 1) % 18;
                    if (dataindex != 0)
                    {
                        if (dataindex == 2)   //有带电压值
                        {
                            addtion_voldata = true;
                            dataindex = 0;
                        }
                        else if(dataindex == 3)
                        {
                            addtion_strength = true;
                            dataindex = 0;
                        }
                        else
                        {
                            dataindex = (pack.DataLength - 2 - 1) % 10;
                            isBCD = true;
                            if (dataindex == 2)     //有带电压值
                            {
                                addtion_voldata = true;
                                dataindex = 0;
                            }
                            else if(dataindex == 3)
                            {
                                addtion_strength = true;
                                dataindex = 0;
                            }
                        }

                    }
                    if (dataindex != 0)
                    {
                        throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+(18/10)*n)或(2+1+(18/10)*n+2)规则");
                    }

                    if (isBCD)
                    {
                        if (addtion_voldata)
                            dataindex = (pack.DataLength - 2 - 1 - 2) / 10;
                        else if(addtion_strength)
                            dataindex = (pack.DataLength - 2 - 1 - 3) / 10;
                        else
                            dataindex = (pack.DataLength - 2 - 1) / 10;
                    }
                    else
                    {
                        if (addtion_voldata)
                            dataindex = (pack.DataLength - 2 - 1 - 2) / 18;
                        else if(addtion_strength)
                            dataindex = (pack.DataLength - 2 - 1 - 3) / 18;
                        else
                            dataindex = (pack.DataLength - 2 - 1) / 18;
                    }

                    int alarmflag = 0;
                    int flowFlag = 0;

                    //报警标志
                    alarmflag = BitConverter.ToInt16(new byte[] { pack.Data[0], pack.Data[1] }, 0);
                    flowFlag = Convert.ToInt16(pack.Data[2]);
                    if (addtion_voldata)
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 1], pack.Data[pack.DataLength - 2] }, 0)) / 1000;
                    else if(addtion_strength)
                    {
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 2], pack.Data[pack.DataLength - 3] }, 0)) / 1000;
                        field_strength = (Int16)pack.Data[pack.DataLength - 1];
                    }

                    GPRSFlowFrameDataEntity framedata = new GPRSFlowFrameDataEntity();
                    framedata.TerId = pack.ID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    double forward_flowvalue = 0, reverse_flowvalue = 0, instant_flowvalue = 0;
                    for (int i = 0; i < dataindex; i++)
                    {
                        year = 2000 + Convert.ToInt16(pack.Data[i * 18 + 3]);
                        month = Convert.ToInt16(pack.Data[i * 18 + 4]);
                        day = Convert.ToInt16(pack.Data[i * 18 + 5]);
                        hour = Convert.ToInt16(pack.Data[i * 18 + 6]);
                        minute = Convert.ToInt16(pack.Data[i * 18 + 7]);
                        sec = Convert.ToInt16(pack.Data[i * 18 + 8]);

                        if (!isBCD)
                        {
                            //前向流量
                            forward_flowvalue = BitConverter.ToSingle(new byte[] { pack.Data[i * 18 + 12], pack.Data[i * 18 + 11], pack.Data[i * 18 + 10], pack.Data[i * 18 + 9] }, 0);
                            //反向流量
                            reverse_flowvalue = BitConverter.ToSingle(new byte[] { pack.Data[i * 18 + 16], pack.Data[i * 18 + 15], pack.Data[i * 18 + 14], pack.Data[i * 18 + 13] }, 0);
                            //瞬时流量
                            instant_flowvalue = BitConverter.ToSingle(new byte[] { pack.Data[i * 18 + 20], pack.Data[i * 18 + 19], pack.Data[i * 18 + 18], pack.Data[i * 18 + 17] }, 0);

                            GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|流量终端[{1}]|报警标志({2})|流量标志({3})|采集时间({4})|正向流量值:{5}|反向流量值:{6}|瞬时流量值:{7}|电压值:{8}V|信号强度:{9}",
                                i, pack.ID, alarmflag, flowFlag, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, forward_flowvalue, reverse_flowvalue, instant_flowvalue, volvalue, field_strength)));
                        }
                        else
                        {
                            string flowvalue = String.Format("{0:X2}", pack.Data[i * 18 + 12]) + String.Format("{0:X2}", pack.Data[i * 18 + 11]) + String.Format("{0:X2}", pack.Data[i * 18 + 10]) + String.Format("{0:X2}", pack.Data[i * 18 + 9]);
                            forward_flowvalue = Convert.ToDouble(flowvalue) / 100;
                            GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|流量终端[{1}]|报警标志({2})|流量标志({3})|采集时间({4})|日累计流量值:{5}|电压值:{6}V|信号强度:{7}",
                                i, pack.ID, alarmflag, flowFlag, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, forward_flowvalue, volvalue, field_strength)));
                        }

                        GPRSFlowDataEntity data = new GPRSFlowDataEntity();
                        data.Forward_FlowValue = forward_flowvalue;
                        data.Reverse_FlowValue = reverse_flowvalue;
                        data.Instant_FlowValue = instant_flowvalue;
                        data.Voltage = volvalue;
                        data.FieldStrength = field_strength;
                        try
                        {
                            data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                        }
                        catch { data.ColTime = ConstValue.MinDateTime; }
                        bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);
                        framedata.lstFlowData.Add(data);
                    }
                    GlobalValue.Instance.GPRS_FlowFrameData.Enqueue(framedata);  //通知存储线程处理
                    GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertFlowValue);
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_ALARMINFO)  //从站向主站发送设备报警信息
                {
                    if (pack.DataLength != 7 && pack.DataLength != 9 && pack.DataLength!=11)   //pack.DataLength == 9 带电压值;pack.DataLength == 11 带电压值和信号强度且报警标志为2个字节(旧的为1个字节)
                    {
                        throw new ArgumentException(DateTime.Now.ToString() + " " + "帧数据长度[" + pack.DataLength + "]不符合7、9、11位规则");
                    }

                    StringBuilder str_alarm = new StringBuilder();

                    if (pack.DataLength == 9)   //pack.DataLength == 9 带电压值
                    {
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 1], pack.Data[pack.DataLength - 2] }, 0)) / 1000;
                    }
                    else if(pack.DataLength == 11)
                    {
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 2], pack.Data[pack.DataLength - 3] }, 0)) / 1000;
                        field_strength = (Int16)pack.Data[pack.DataLength - 1];
                    }

                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    year = 2000 + Convert.ToInt16(pack.Data[1]);
                    month = Convert.ToInt16(pack.Data[2]);
                    day = Convert.ToInt16(pack.Data[3]);
                    hour = Convert.ToInt16(pack.Data[4]);
                    minute = Convert.ToInt16(pack.Data[5]);
                    sec = Convert.ToInt16(pack.Data[6]);
                    

                    Dictionary<int, string> dictalarms = null;
                    if (pack.DataLength == 11)      //pack.DataLength == 11 报警标志为2个字节(旧的为1个字节)
                    {
                        dictalarms = AlarmProc.GetAlarmName(pack.ID3, pack.C1, pack.Data[1], pack.Data[0]);

                        year = 2000 + Convert.ToInt16(pack.Data[2]);
                        month = Convert.ToInt16(pack.Data[3]);
                        day = Convert.ToInt16(pack.Data[4]);
                        hour = Convert.ToInt16(pack.Data[5]);
                        minute = Convert.ToInt16(pack.Data[6]);
                        sec = Convert.ToInt16(pack.Data[7]);
                    }
                    else
                    {
                        dictalarms = AlarmProc.GetAlarmName(pack.ID3, pack.C1, pack.Data[0]);

                        year = 2000 + Convert.ToInt16(pack.Data[1]);
                        month = Convert.ToInt16(pack.Data[2]);
                        day = Convert.ToInt16(pack.Data[3]);
                        hour = Convert.ToInt16(pack.Data[4]);
                        minute = Convert.ToInt16(pack.Data[5]);
                        sec = Convert.ToInt16(pack.Data[6]);
                    }
                    if (month == 0)
                        month = 1;
                    if (day == 0)
                        day = 1;

                    if (dictalarms != null && dictalarms.Count > 0)
                    {
                        GPRSAlarmFrameDataEntity alarmframedata = new GPRSAlarmFrameDataEntity();
                        alarmframedata.Frame = str_frame;
                        alarmframedata.TerId = pack.DevID.ToString();
                        alarmframedata.TerminalType = TerType.PreTer;
                        alarmframedata.ModifyTime = new DateTime(year, month, day, hour, minute, sec);
                        alarmframedata.AlarmId = new List<int>();
                        foreach (var de in dictalarms)
                        {
                            alarmframedata.AlarmId.Add(de.Key);
                            str_alarm.Append(de.Value + " ");
                        }
                        alarmframedata.Voltage = volvalue;
                        alarmframedata.FieldStrength = field_strength;

                        GlobalValue.Instance.GPRS_AlarmFrameData.Enqueue(alarmframedata);
                        GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertAlarm);
                    }

                    bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(new DateTime(year, month, day, hour, minute, sec));
                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("压力终端[{0}]{1}|时间({2})|电压值:{3}V|信号强度:{4}",
                         pack.ID, str_alarm, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, volvalue, field_strength)));
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_PREFLOWDATA) //从站向主站发送流量采集数据(水质终端)
                {
                    #region 上海肯特(KENT)水表
                    if (pack.Data[2] == 0x02)   //上海肯特(KENT)水表
                    {
                        int dataindex = (pack.DataLength - 2 - 2 - 1) % (6 + 36);  //两字节报警,1字节厂家类型,
                        if (dataindex != 0)
                        {
                            dataindex = (pack.DataLength - 2 - 2 - 2) % (6 + 36);  //两字节报警,1字节厂家类型,一个字节信号强度
                            if (dataindex != 0)
                                throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+(6+36)*n)规则");
                            dataindex = (pack.DataLength - 2 - 2 - 2) / (6 + 36);
                        }
                        else
                            dataindex = (pack.DataLength - 2 - 2 - 1) / (6 + 36);

                        GPRSFlowFrameDataEntity framedata = new KERTFlow().ProcessData(dataindex, "压力流量终端", pack.ID.ToString(), str_frame, pack.Data, pack.DataLength, out bNeedCheckTime);

                        GlobalValue.Instance.GPRS_FlowFrameData.Enqueue(framedata);  //通知存储线程处理
                        GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertFlowValue);
                    }
                    else
                    {
                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs("压力流量终端[" + pack.ID + "]错误未知水表类型!"));
                    }
                    #endregion
                }
                #endregion
            }
            else if (pack.ID3 == (byte)Entity.ConstValue.DEV_TYPE.PRESS_CTRL)
            {
                #region 压力控制器
                if (pack.C1 == (byte)PRECTRL_COMMAND.READ_DATA)  //从站向主站发送采集数据
                {
                    int dataindex = (pack.DataLength) % 24;
                    if (dataindex != 0)
                        throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(24*n)规则");  //GPRS远程压力终端在数据段最后增加两个字节的电压数据
                    else
                        dataindex = (pack.DataLength) / 24;

                    string parmalarm = "", alarm = "";

                    //报警
                    /*
                     参数报警标识说明：相应的位为“1”，则表示有相应报警，如下所示，
                    A0-进口压力上限报警
                    A1-进口压力下限报警
                    A2-压力调整超时报警
                    A3～A7-备用
                      设备报警标识说明：相应的位为“1”，则表示有相应报警，如下所示，
                    A0-电池报警
                    A1-进口压力传感器报警
                    A2-出口压力传感器报警
                    A3-流量器报警
                    A4～A7-备用
                     */

                    GPRSPrectrlFrameDataEntity framedata = new GPRSPrectrlFrameDataEntity();
                    framedata.TerId = pack.DevID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    for (int i = 0; i < dataindex; i++)
                    {
                        parmalarm = "";
                        alarm = "";
                        if ((pack.Data[i * 24 + 22] & 0x01) == 1)  //进口压力上限报警
                            parmalarm += "进口压力上限报警";
                        else if (((pack.Data[i * 24 + 22] & 0x02) >> 1) == 1)   //进口压力下限报警
                            parmalarm += "进口压力下限报警";
                        else if (((pack.Data[i * 24 + 22] & 0x04) >> 2) == 1)   //压力调整超时报警
                            parmalarm += "压力调整超时报警";

                        if ((pack.Data[i * 24 + 23] & 0x01) == 1)  //电池报警
                            alarm += "电池报警";
                        else if (((pack.Data[i * 24 + 23] & 0x02) >> 1) == 1)   //进口压力传感器报警
                            alarm += "进口压力传感器报警";
                        else if (((pack.Data[i * 24 + 23] & 0x04) >> 2) == 1)  //出口压力传感器报警
                            alarm += "出口压力传感器报警";
                        else if (((pack.Data[i * 24 + 23] & 0x08) >> 3) == 1)  //流量器报警
                            alarm += "流量器报警";

                        //发送时间(6bytes)+进口压力(2bytes)+出口压力(2bytes)+正向累积流量 (4bytes)+反向累积流量(4bytes)+瞬时流量(4bytes)+参数报警标识(1byte)+设备报警标识(1byte)
                        int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                        year = 2000 + Convert.ToInt16(pack.Data[i * 24]);
                        month = Convert.ToInt16(pack.Data[i * 24 + 1]);
                        day = Convert.ToInt16(pack.Data[i * 24 + 2]);
                        hour = Convert.ToInt16(pack.Data[i * 24 + 3]);
                        minute = Convert.ToInt16(pack.Data[i * 24 + 4]);
                        sec = Convert.ToInt16(pack.Data[i * 24 + 5]);

                        //进口压力
                        float entrance_prevalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[i * 24 + 7], pack.Data[i * 24 + 6] }, 0)) / 1000;
                        //出口压力
                        float outlet_prevalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[i * 24 + 9], pack.Data[i * 24 + 8] }, 0)) / 1000;
                        //前向流量
                        float forward_flowvalue = BitConverter.ToSingle(new byte[] { pack.Data[i * 24 + 13], pack.Data[i * 24 + 12], pack.Data[i * 24 + 11], pack.Data[i * 24 + 10] }, 0);
                        //反向流量
                        float reverse_flowvalue = BitConverter.ToSingle(new byte[] { pack.Data[i * 24 + 17], pack.Data[i * 24 + 16], pack.Data[i * 24 + 15], pack.Data[i * 24 + 14] }, 0);
                        //瞬时流量
                        float instant_flowvalue = BitConverter.ToSingle(new byte[] { pack.Data[i * 24 + 21], pack.Data[i * 24 + 20], pack.Data[i * 24 + 19], pack.Data[i * 24 + 18] }, 0);

                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|压力控制器[{1}]|参数报警({2})|设备报警({3})|采集时间({4})|进口压力:{5}MPa|出口压力:{6}MPa|正向流量值:{7}|反向流量值:{8}|瞬时流量值:{9}",
                                i, pack.DevID, parmalarm, alarm, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, entrance_prevalue, outlet_prevalue, forward_flowvalue, reverse_flowvalue, instant_flowvalue)));

                        GPRSPrectrlDataEntity data = new GPRSPrectrlDataEntity();
                        data.Entrance_preValue = entrance_prevalue;
                        data.Outlet_preValue = outlet_prevalue;
                        data.Forward_FlowValue = forward_flowvalue;
                        data.Reverse_FlowValue = reverse_flowvalue;
                        data.Instant_FlowValue = instant_flowvalue;
                        byte balarm = (byte)((pack.Data[i * 24 + 22]) << 4);  //balarm存放pack.Data[22]的低四位到高四位位置,存放pack.Data[23]的低四位到低四位位置
                        balarm |= 0x0f;
                        balarm = (byte)(balarm & (pack.Data[i * 24 + 23] | 0xF0));

                        data.AlarmCode = balarm;
                        data.AlarmDesc = parmalarm;
                        if (!string.IsNullOrEmpty(data.AlarmDesc))
                            data.AlarmDesc += ",";
                        data.AlarmDesc += alarm;   //存放两个报警信息
                        try
                        {
                            data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                        }
                        catch { data.ColTime = ConstValue.MinDateTime; }
                        bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);
                        framedata.lstPrectrlData.Add(data);
                    }

                    GlobalValue.Instance.GPRS_PrectrlFrameData.Enqueue(framedata);  //通知存储线程处理
                    GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertPrectrlValue);
                }
                #endregion
            }

            else if (pack.ID3 == (byte)Entity.ConstValue.DEV_TYPE.UNIVERSAL_CTRL)
            {
                #region 通用终端
                if ((pack.C1 == (byte)GPRS_READ.READ_UNIVERSAL_SIM1) || (pack.C1 == (byte)GPRS_READ.READ_UNIVERSAL_SIM2) ||
                    (pack.C1 == (byte)UNIVERSAL_COMMAND.CalibartionSimualte1) || (pack.C1 == (byte)UNIVERSAL_COMMAND.CalibartionSimualte2))  //接受通用终端发送的模拟数据(包含招测数据)
                {
                    #region 通用终端模拟数据
                    string name = "";
                    string sequence = "";
                    if (pack.C1 == (byte)GPRS_READ.READ_UNIVERSAL_SIM1)
                    {
                        name = "1";
                        sequence = "1";
                    }
                    else if (pack.C1 == (byte)UNIVERSAL_COMMAND.CalibartionSimualte1)
                    {
                        name = "招测1";
                        sequence = "1";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_UNIVERSAL_SIM2)
                    {
                        name = "2";
                        sequence = "2";
                    }
                    else if (pack.C1 == (byte)UNIVERSAL_COMMAND.CalibartionSimualte2)
                    {
                        name = "招测2";
                        sequence = "2";
                    }
                    int calibration = 819;// BitConverter.ToInt16(new byte[] { pack.Data[1], pack.Data[0] }, 0);
                    GPRSUniversalFrameDataEntity framedata = new GPRSUniversalFrameDataEntity();
                    framedata.TerId = pack.ID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    double datavalue = 0;

                    DataRow[] dr_TerminalDataConfig = null;
                    if (GlobalValue.Instance.UniversalDataConfig != null && GlobalValue.Instance.UniversalDataConfig.Rows.Count > 0)
                    {
                        dr_TerminalDataConfig = GlobalValue.Instance.UniversalDataConfig.Select("TerminalID='" + framedata.TerId + "' AND Sequence='" + sequence + "'"); //WayType
                    }
                    if (dr_TerminalDataConfig != null && dr_TerminalDataConfig.Length > 0)
                    {
                        float MaxMeasureRange = dr_TerminalDataConfig[0]["MaxMeasureRange"] != DBNull.Value ? Convert.ToSingle(dr_TerminalDataConfig[0]["MaxMeasureRange"]) : 0;
                        float MaxMeasureRangeFlag = dr_TerminalDataConfig[0]["MaxMeasureRangeFlag"] != DBNull.Value ? Convert.ToSingle(dr_TerminalDataConfig[0]["MaxMeasureRangeFlag"]) : 0;
                        int datawidth = dr_TerminalDataConfig[0]["FrameWidth"] != DBNull.Value ? Convert.ToInt16(dr_TerminalDataConfig[0]["FrameWidth"]) : 0;
                        int precision = dr_TerminalDataConfig[0]["precision"] != DBNull.Value ? Convert.ToInt32(dr_TerminalDataConfig[0]["precision"]) : 0;
                        if (MaxMeasureRangeFlag > 0 && datawidth > 0)
                        {
                            int loopdatalen = 6 + datawidth;  //循环部分数据宽度 = 时间(6)+配置长度
                            int dataindex = (pack.DataLength - 2 - 1) % loopdatalen;
                            if (dataindex != 0)
                                throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+" + loopdatalen + "*n)规则");
                            dataindex = (pack.DataLength - 2 - 1) / loopdatalen;
                            for (int i = 0; i < dataindex; i++)
                            {
                                year = 2000 + Convert.ToInt16(pack.Data[i * 8 + 3]);
                                month = Convert.ToInt16(pack.Data[i * 8 + 4]);
                                day = Convert.ToInt16(pack.Data[i * 8 + 5]);
                                hour = Convert.ToInt16(pack.Data[i * 8 + 6]);
                                minute = Convert.ToInt16(pack.Data[i * 8 + 7]);
                                sec = Convert.ToInt16(pack.Data[i * 8 + 8]);

                                if (datawidth == 2)
                                    datavalue = BitConverter.ToInt16(new byte[] { pack.Data[i * 8 + 10], pack.Data[i * 8 + 9] }, 0);
                                else if (datawidth == 4)
                                    datavalue = BitConverter.ToSingle(new byte[] { pack.Data[i * 8 + 12], pack.Data[i * 8 + 11], pack.Data[i * 8 + 10], pack.Data[i * 8 + 9] }, 0);

                                datavalue = (MaxMeasureRange / MaxMeasureRangeFlag) * (datavalue - calibration);  //根据设置和校准值计算
                                datavalue = Convert.ToDouble(datavalue.ToString("F" + precision));  //精度调整
                                if (datavalue < 0)
                                    datavalue = 0;
                                GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|通用终端[{1}]模拟{2}路|校准值({3})|采集时间({4})|{5}:{6}{7}",
                                    i, pack.ID, name, calibration, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, dr_TerminalDataConfig[0]["Name"].ToString().Trim(), datavalue, dr_TerminalDataConfig[0]["Unit"].ToString())));

                                GPRSUniversalDataEntity data = new GPRSUniversalDataEntity();
                                data.DataValue = datavalue;
                                data.Sim1Zero = calibration;
                                data.TypeTableID = Convert.ToInt32(dr_TerminalDataConfig[0]["ID"]);
                                try
                                {
                                    data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                                }
                                catch { data.ColTime = ConstValue.MinDateTime; }
                                bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);
                                framedata.lstData.Add(data);
                            }
                            GlobalValue.Instance.GPRS_UniversalFrameData.Enqueue(framedata);  //通知存储线程处理
                            GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertUniversalValue);
                        }
                        else
                        {
                            GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs("通用终端[" + framedata.TerId + "]数据帧解析规则配置错误,数据未能解析！"));
                        }
                    }
                    else
                    {
                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs("通用终端[" + framedata.TerId + "]未配置数据帧解析规则,数据未能解析！"));
                    }
                    #endregion
                }
                else if ((pack.C1 == (byte)GPRS_READ.READ_UNIVERSAL_PLUSE))  //接受水质终端发送的脉冲数据
                {
                    #region 通用终端脉冲
                    GPRSUniversalFrameDataEntity framedata = new GPRSUniversalFrameDataEntity();
                    framedata.TerId = pack.ID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    double datavalue = 0;

                    DataRow[] dr_TerminalDataConfig = null;
                    if (GlobalValue.Instance.UniversalDataConfig != null && GlobalValue.Instance.UniversalDataConfig.Rows.Count > 0)
                    {
                        dr_TerminalDataConfig = GlobalValue.Instance.UniversalDataConfig.Select("TerminalID='" + framedata.TerId + "' AND Sequence IN ('4','5','6','7','8')", "Sequence"); //WayType
                    }
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
                            int dataindex = (pack.DataLength - 2 - 1) % loopdatalen;
                            if (dataindex != 0)
                                throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+" + loopdatalen + "*n)规则");
                            dataindex = (pack.DataLength - 2 - 1) / loopdatalen;
                            for (int i = 0; i < dataindex; i++)
                            {
                                year = 2000 + Convert.ToInt16(pack.Data[i * loopdatalen + 3]);
                                month = Convert.ToInt16(pack.Data[i * loopdatalen + 4]);
                                day = Convert.ToInt16(pack.Data[i * loopdatalen + 5]);
                                hour = Convert.ToInt16(pack.Data[i * loopdatalen + 6]);
                                minute = Convert.ToInt16(pack.Data[i * loopdatalen + 7]);
                                sec = Convert.ToInt16(pack.Data[i * loopdatalen + 8]);

                                int freindex = 0;
                                for (int j = 0; j < waycount; j++)
                                {
                                    if (DataWidths[j] == 2)
                                    {
                                        datavalue = BitConverter.ToInt16(new byte[] { pack.Data[i * loopdatalen + 10 + freindex], pack.Data[i * loopdatalen + 9 + freindex] }, 0);
                                        freindex += 2;
                                    }
                                    else if (DataWidths[j] == 4)
                                    {
                                        datavalue = BitConverter.ToInt32(new byte[] { pack.Data[i * loopdatalen + 12 + freindex], pack.Data[i * loopdatalen + 11 + freindex], pack.Data[i * loopdatalen + 10 + freindex], pack.Data[i * loopdatalen + 9 + freindex] }, 0);
                                        freindex += 4;
                                    }

                                    datavalue = PluseUnits[j] * datavalue;  //脉冲计数*单位脉冲值
                                    datavalue = Convert.ToDouble(datavalue.ToString("F" + Precisions[j]));  //精度调整
                                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|通用终端[{1}]脉冲{2}路|采集时间({3})|{4}:{5}{6}",
                                        i, pack.ID, j + 1, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, Names[j], datavalue, Units[j])));

                                    GPRSUniversalDataEntity data = new GPRSUniversalDataEntity();
                                    data.DataValue = datavalue;
                                    data.TypeTableID = Convert.ToInt32(config_ids[j]);
                                    try
                                    {
                                        data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                                    }
                                    catch { data.ColTime = ConstValue.MinDateTime; }
                                    bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);
                                    framedata.lstData.Add(data);
                                }
                            }
                            GlobalValue.Instance.GPRS_UniversalFrameData.Enqueue(framedata);  //通知存储线程处理
                            GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertUniversalValue);
                        }
                        else
                        {
                            GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs("通用终端[" + framedata.TerId + "]数据帧解析规则配置错误,数据未能解析！"));
                        }
                    }
                    else
                    {
                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs("通用终端[" + framedata.TerId + "]未配置数据帧解析规则,数据未能解析！"));
                    }
                    #endregion
                }
                else if ((pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4851) || (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4852)
                    || (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4853) || (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4854)
                    || (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4855) || (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4856)
                    || (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4857) || (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4858))//接受通用终端发送的RS485 数据
                {
                    #region 通用终端RS485 ?路数据
                    string name = "";
                    string sequence = "";
                    if (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4851)
                    {
                        name = "1";
                        sequence = "9";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4852)
                    {
                        name = "2";
                        sequence = "10";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4853)
                    {
                        name = "3";
                        sequence = "11";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4854)
                    {
                        name = "4";
                        sequence = "12";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4855)
                    {
                        name = "5";
                        sequence = "13";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4856)
                    {
                        name = "6";
                        sequence = "14";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4857)
                    {
                        name = "7";
                        sequence = "15";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_UNVERSAL_RS4858)
                    {
                        name = "8";
                        sequence = "16";
                    }
                    int calibration = BitConverter.ToInt16(new byte[] { pack.Data[1], pack.Data[0] }, 0);
                    GPRSUniversalFrameDataEntity framedata = new GPRSUniversalFrameDataEntity();
                    framedata.TerId = pack.ID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    double datavalue = 0;

                    DataRow[] dr_TerminalDataConfig = null;
                    DataRow[] dr_DataConfig_Child = null;
                    bool ConfigHaveChild = false;
                    if (GlobalValue.Instance.UniversalDataConfig != null && GlobalValue.Instance.UniversalDataConfig.Rows.Count > 0)
                    {
                        dr_TerminalDataConfig = GlobalValue.Instance.UniversalDataConfig.Select("TerminalID='" + framedata.TerId + "' AND Sequence='" + sequence + "'"); //WayType
                        if (dr_TerminalDataConfig != null && dr_TerminalDataConfig.Length > 0)
                        {
                            dr_DataConfig_Child = GlobalValue.Instance.UniversalDataConfig.Select("ParentID='" + dr_TerminalDataConfig[0]["ID"].ToString().Trim() + "'", "Sequence");
                            if (dr_DataConfig_Child != null && dr_DataConfig_Child.Length > 0)
                            {
                                ConfigHaveChild = true;
                                dr_TerminalDataConfig = dr_DataConfig_Child;  //有子节点配置时，使用子节点配置
                            }
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
                            int dataindex = (pack.DataLength - 2 - 1) % loopdatalen;
                            if (dataindex != 0)
                                throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+" + loopdatalen + "*n)规则");
                            dataindex = (pack.DataLength - 2 - 1) / loopdatalen;
                            for (int i = 0; i < dataindex; i++)
                            {
                                year = 2000 + Convert.ToInt16(pack.Data[i * loopdatalen + 3]);
                                month = Convert.ToInt16(pack.Data[i * loopdatalen + 4]);
                                day = Convert.ToInt16(pack.Data[i * loopdatalen + 5]);
                                hour = Convert.ToInt16(pack.Data[i * loopdatalen + 6]);
                                minute = Convert.ToInt16(pack.Data[i * loopdatalen + 7]);
                                sec = Convert.ToInt16(pack.Data[i * loopdatalen + 8]);

                                int freindex = 0;
                                for (int j = 0; j < waycount; j++)
                                {
                                    if (DataWidths[j] == 2)
                                    {
                                        datavalue = BitConverter.ToInt16(new byte[] { pack.Data[i * loopdatalen + 10 + freindex], pack.Data[i * loopdatalen + 9 + freindex] }, 0);
                                        freindex += 2;
                                    }
                                    else if (DataWidths[j] == 4)
                                    {
                                        datavalue = BitConverter.ToInt32(new byte[] { pack.Data[i * loopdatalen + 12 + freindex], pack.Data[i * loopdatalen + 11 + freindex], pack.Data[i * loopdatalen + 10 + freindex], pack.Data[i * loopdatalen + 9 + freindex] }, 0);
                                        freindex += 4;
                                    }

                                    datavalue = MaxMeasureRanges[j] * datavalue;  //系数
                                    datavalue = Convert.ToDouble(datavalue.ToString("F" + Precisions[j]));  //精度调整
                                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|通用终端[{1}]RS485 {2}路|采集时间({3})|{4}:{5}{6}",
                                        i, pack.ID, name, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, Names[j], datavalue, Units[j])));

                                    GPRSUniversalDataEntity data = new GPRSUniversalDataEntity();
                                    data.DataValue = datavalue;
                                    data.TypeTableID = Convert.ToInt32(config_ids[j]);
                                    try
                                    {
                                        data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                                    }
                                    catch { data.ColTime = ConstValue.MinDateTime; }
                                    bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);
                                    framedata.lstData.Add(data);
                                }
                            }
                            GlobalValue.Instance.GPRS_UniversalFrameData.Enqueue(framedata);  //通知存储线程处理
                            GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertUniversalValue);
                        }
                        else
                        {
                            GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs("通用终端[" + framedata.TerId + "]数据帧解析规则配置错误,数据未能解析！"));
                        }
                    }
                    else
                    {
                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs("通用终端[" + framedata.TerId + "]未配置数据帧解析规则,数据未能解析！"));
                    }
                    #endregion
                }
                #endregion
            }
            else if (pack.ID3 == (byte)Entity.ConstValue.DEV_TYPE.OLWQ_CTRL)
            {
                #region 水质终端
                bool addtion_voldata = false;   //是否在数据段最后增加了两个字节的电压数据
                if ((pack.C1 == (byte)GPRS_READ.READ_TURBIDITY) || (pack.C1 == (byte)GPRS_READ.READ_RESIDUALCL) ||
                    (pack.C1 == (byte)GPRS_READ.READ_PH) || (pack.C1 == (byte)GPRS_READ.READ_TEMP))  //从站向主站发送水质采集数据
                {
                    int dataindex = (pack.DataLength - 2 - 1) % 8;
                    if (dataindex != 0)
                    {
                        if (dataindex == 2)
                        {
                            dataindex = (pack.DataLength - 2 - 1 - 2) / 8;
                            addtion_voldata = true;
                        }
                        else
                        {
                            throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+8*n)或(2+1+8*n+2)规则");  //最后增加两个字节的电压数据
                        }
                    }
                    else
                        dataindex = (pack.DataLength - 2 - 1) / 8;

                    GPRSOLWQFrameDataEntity framedata = new GPRSOLWQFrameDataEntity();
                    framedata.TerId = pack.ID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    float value = 0;
                    string name = "";
                    string unit = "";
                    string valuecolumnname = "";
                    if (pack.C1 == (byte)GPRS_READ.READ_TURBIDITY)
                    {
                        name = "浊度";
                        unit = "NTU";
                        valuecolumnname = "Turbidity";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_RESIDUALCL)
                    {
                        name = "余氯";
                        unit = "PPM";
                        valuecolumnname = "ResidualCl";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_PH)
                    {
                        name = "PH";
                        unit = "ph";
                        valuecolumnname = "PH";
                    }
                    else if (pack.C1 == (byte)GPRS_READ.READ_TEMP)
                    {
                        name = "温度";
                        unit = "℃";
                        valuecolumnname = "Temperature";
                    }

                    if (addtion_voldata)  //电压
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 1], pack.Data[pack.DataLength - 2] }, 0)) / 1000;

                    for (int i = 0; i < dataindex; i++)
                    {
                        year = 2000 + Convert.ToInt16(pack.Data[i * 8 + 3]);
                        month = Convert.ToInt16(pack.Data[i * 8 + 4]);
                        day = Convert.ToInt16(pack.Data[i * 8 + 5]);
                        hour = Convert.ToInt16(pack.Data[i * 8 + 6]);
                        minute = Convert.ToInt16(pack.Data[i * 8 + 7]);
                        sec = Convert.ToInt16(pack.Data[i * 8 + 8]);

                        value = (float)BitConverter.ToInt16(new byte[] { pack.Data[i * 8 + 10], pack.Data[i * 8 + 9] }, 0);
                        GPRSOLWQDataEntity data = new GPRSOLWQDataEntity();
                        data.ValueColumnName = valuecolumnname;
                        if (pack.C1 == (byte)GPRS_READ.READ_TURBIDITY)
                        {
                            value = value / 100;
                            data.Turbidity = value;
                        }
                        else if (pack.C1 == (byte)GPRS_READ.READ_RESIDUALCL)
                        {
                            data.ResidualCl = value / 1000;
                        }
                        else if (pack.C1 == (byte)GPRS_READ.READ_PH)
                        {
                            data.PH = value / 100;
                        }
                        else if (pack.C1 == (byte)GPRS_READ.READ_TEMP)
                        {
                            data.Temperature = value;
                        }

                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|水质终端[{1}]|采集时间({2})|{3}值:{4}{5}|电压值:{6}V",
                            dataindex, pack.ID, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, name, value, unit, volvalue)));
                        data.Voltage = volvalue;
                        try
                        {
                            data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                        }
                        catch { data.ColTime = ConstValue.MinDateTime; }
                        bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);
                        framedata.lstOLWQData.Add(data);
                    }

                    GlobalValue.Instance.GPRS_OLWQFrameData.Enqueue(framedata);  //通知存储线程处理
                    GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertOLWQValue);
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_CONDUCTIVITY)  //从站向主站发送电导率采集数据
                {
                    int dataindex = (pack.DataLength - 2 - 1) % 12;
                    if (dataindex != 0)
                    {
                        if (dataindex == 2)
                        {
                            dataindex = (pack.DataLength - 2 - 1 - 2) / 12;
                            addtion_voldata = true;
                        }
                        else
                        {
                            throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+10*n)或(2+1+10*n+2)规则");  //最后增加两个字节的电压数据
                        }
                    }
                    else
                        dataindex = (pack.DataLength - 2 - 1) / 12;

                    GPRSOLWQFrameDataEntity framedata = new GPRSOLWQFrameDataEntity();
                    framedata.TerId = pack.ID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    float Condvalue = 0;
                    float Tempvalue = 0;

                    if (addtion_voldata)  //电压
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 1], pack.Data[pack.DataLength - 2] }, 0)) / 1000;

                    for (int i = 0; i < dataindex; i++)
                    {
                        year = 2000 + Convert.ToInt16(pack.Data[i * 12 + 3]);
                        month = Convert.ToInt16(pack.Data[i * 12 + 4]);
                        day = Convert.ToInt16(pack.Data[i * 12 + 5]);
                        hour = Convert.ToInt16(pack.Data[i * 12 + 6]);
                        minute = Convert.ToInt16(pack.Data[i * 12 + 7]);
                        sec = Convert.ToInt16(pack.Data[i * 12 + 8]);

                        Condvalue = ((float)BitConverter.ToInt32(new byte[] { pack.Data[i * 12 + 12], pack.Data[i * 12 + 11], pack.Data[i * 12 + 10], pack.Data[i * 12 + 9] }, 0)) / 100;
                        Tempvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[i * 12 + 14], pack.Data[i * 12 + 13] }, 0)) / 10;

                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("index({0})|水质终端[{1}]|采集时间({2})|电导率值:{3}us/cm,温度:{4}℃|电压值:{5}V",
                            i, pack.ID, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, Condvalue.ToString("f2"), Tempvalue.ToString("f1"), volvalue)));

                        GPRSOLWQDataEntity data = new GPRSOLWQDataEntity();
                        data.Conductivity = Condvalue;
                        data.Temperature = Tempvalue;
                        data.ValueColumnName = "Conductivity";
                        data.Voltage = volvalue;
                        try
                        {
                            data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                        }
                        catch { data.ColTime = ConstValue.MinDateTime; }
                        bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);
                        framedata.lstOLWQData.Add(data);
                    }

                    GlobalValue.Instance.GPRS_OLWQFrameData.Enqueue(framedata);  //通知存储线程处理
                    GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertOLWQValue);
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_OLWQFLOW) //从站向主站发送流量采集数据(水质终端)
                {
                    #region 上海肯特(KENT)水表
                    if (pack.Data[2] == 0x02)   //上海肯特(KENT)水表
                    {
                        int dataindex = (pack.DataLength - 2 - 2 - 1) % (6 + 36);  //两字节报警,1字节厂家类型,
                        if (dataindex != 0)
                        {
                            throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2+1+(6+36)*n)规则");
                        }
                        dataindex = (pack.DataLength - 2 - 2 - 1) / (6 + 36);

                        GPRSFlowFrameDataEntity framedata = new KERTFlow().ProcessData(dataindex, "水质终端", pack.ID.ToString(), str_frame, pack.Data, pack.DataLength, out bNeedCheckTime);

                        GlobalValue.Instance.GPRS_FlowFrameData.Enqueue(framedata);  //通知存储线程处理
                        GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertFlowValue);
                    }
                    else
                    {
                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs("水质终端[" + pack.ID + "]错误未知水表类型!"));
                    }
                    #endregion
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_OLWQALARM)  //从站向主站发送设备报警信息(水质终端)
                {
                    if (pack.DataLength != 7 && pack.DataLength != 9)   //pack.DataLength == 9 带电压值
                    {
                        throw new ArgumentException(DateTime.Now.ToString() + " " + "帧数据长度[" + pack.DataLength + "]不符合(2+1+18*n)或(2+1+18*n+2)规则");
                    }

                    string alarm = "";
                    //报警
                    /*
                     * A0—电池低压报警。
                     * A1—浊度报警。
                     * A2—余氯报警。
                     * A3—流量报警。
                     * A4—PH报警。
                     * A5—电导率报警。
                     * A6～A7—备用
                     */

                    if ((pack.Data[0] & 0x01) == 1)  //电池低压报警
                        alarm += "电池低压报警";
                    else if (((pack.Data[0] & 0x02) >> 1) == 1)   //浊度报警
                        alarm += "浊度报警";
                    else if (((pack.Data[0] & 0x04) >> 2) == 1)   //余氯报警
                        alarm += "余氯报警";
                    else if (((pack.Data[0] & 0x08) >> 3) == 1)  //流量报警
                        alarm += "流量报警";
                    else if (((pack.Data[0] & 0x10) >> 4) == 1)  //PH报警
                        alarm += "PH报警";
                    else if (((pack.Data[0] & 0x20) >> 5) == 1)  //电导率报警
                        alarm += "电导率报警";

                    int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                    year = 2000 + Convert.ToInt16(pack.Data[1]);
                    month = Convert.ToInt16(pack.Data[2]);
                    day = Convert.ToInt16(pack.Data[3]);
                    hour = Convert.ToInt16(pack.Data[4]);
                    minute = Convert.ToInt16(pack.Data[5]);
                    sec = Convert.ToInt16(pack.Data[6]);

                    if (pack.DataLength == 9)   //pack.DataLength == 9 带电压值
                    {
                        volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 1], pack.Data[pack.DataLength - 2] }, 0)) / 1000;
                    }
                    if (month == 0)
                        month = 1;
                    if (day == 0)
                        day = 1;
                    bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(new DateTime(year, month, day, hour, minute, sec));
                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("水质终端[{0}]{1}|时间({2})|电压值:{3}V",
                         pack.ID, alarm, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, volvalue)));
                }
                #endregion
            }
            else if (pack.ID3 == (byte)Entity.ConstValue.DEV_TYPE.HYDRANT_CTRL)
            {
                #region 消防栓
                GPRSHydrantFrameDataEntity framedata = new GPRSHydrantFrameDataEntity();
                framedata.TerId = pack.DevID.ToString();
                framedata.ModifyTime = DateTime.Now;
                framedata.Frame = str_frame;

                int year = 0, month = 0, day = 0, hour = 0, minute = 0, sec = 0;
                year = 2000 + Convert.ToInt16(pack.Data[0]);
                month = Convert.ToInt16(pack.Data[1]);
                day = Convert.ToInt16(pack.Data[2]);
                hour = Convert.ToInt16(pack.Data[3]);
                minute = Convert.ToInt16(pack.Data[4]);
                sec = Convert.ToInt16(pack.Data[5]);

                GPRSHydrantDataEntity data = new GPRSHydrantDataEntity();
                try
                {
                    data.ColTime = new DateTime(year, month, day, hour, minute, sec);
                }
                catch { data.ColTime = ConstValue.MinDateTime; }
                bNeedCheckTime = GlobalValue.Instance.SocketMag.NeedCheckTime(data.ColTime);

                if (pack.C1 == (byte)GPRS_READ.READ_HYDRANT_OPEN)
                {
                    int openangle = Convert.ToInt16(pack.Data[6]);
                    float prevalue = (float)BitConverter.ToInt16(new byte[] { pack.Data[8], pack.Data[7] }, 0) / 1000;
                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("消防栓[{0}]被打开|时间({1})|开度:{2},压力:{3}MPa",
                            pack.DevID, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, openangle, prevalue.ToString("f3"))));
                    data.Operate = HydrantOptType.Open;
                    data.PreValue = prevalue;
                    data.OpenAngle = openangle;
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_HYDRANT_CLOSE)
                {
                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("消防栓[{0}]被关闭|时间({1})",
                               pack.DevID, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec)));
                    data.Operate = HydrantOptType.Close;
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_HYDRANT_OPENANGLE)
                {
                    int openangle = Convert.ToInt16(pack.Data[6]);
                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("消防栓[{0}]开度|时间({1})|开度:{2}",
                            pack.DevID, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, openangle)));
                    data.OpenAngle = openangle;
                    data.Operate = HydrantOptType.OpenAngle;
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_HYDRANT_IMPACT)
                {
                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("消防栓[{0}]被撞击|时间({1})",
                               pack.DevID, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec)));
                    data.Operate = HydrantOptType.Impact;
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_HYDRANT_KNOCKOVER)
                {
                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("消防栓[{0}]被撞倒|时间({1})",
                               pack.DevID, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec)));
                    data.Operate = HydrantOptType.KnockOver;
                }
                else if (pack.C1 == (byte)GPRS_READ.READ_HYDRANT_TIMEGPRS)       //定时远传
                {
                    //年、月、日、时、分、秒、开启/关闭、开度、被撞、撞倒、压力高位、压力低位
                    string strstate = "", stropenangle = "-", strprevalue = "未配置";
                    if (pack.Data[6] == 0x00)
                    {
                        data.Operate = HydrantOptType.Close;
                        strstate = "关闭";
                    }
                    if (pack.Data[6] == 0x01)
                    {
                        data.Operate = HydrantOptType.Open;
                        int openangle = Convert.ToInt16(pack.Data[7]);
                        data.OpenAngle = openangle;
                        strstate = "打开";
                        stropenangle = openangle.ToString();
                    }
                    if (pack.Data[8] == 0x01)
                    {
                        data.Operate = HydrantOptType.Impact;
                        strstate = "撞击";
                    }
                    if (pack.Data[9] == 0x01)
                    {
                        data.Operate = HydrantOptType.KnockOver;
                        strstate = "撞倒";
                    }
                    if (pack.Data[10] == 0x01)       //压力标志,0x01:表示有配置压力
                    {
                        float prevalue = (float)BitConverter.ToInt16(new byte[] { pack.Data[12], pack.Data[11] }, 0) / 1000;
                        data.PreValue = prevalue;
                        strprevalue = prevalue.ToString() + "MPa";
                    }
                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("消防栓[{0}]定时报|时间({1})|状态:{2}|开度:{3}|压力:{4}",
                            pack.DevID, year + "-" + month + "-" + day + " " + hour + ":" + minute + ":" + sec, strstate, stropenangle, strprevalue)));
                }

                framedata.lstHydrantData.Add(data);
                GlobalValue.Instance.GPRS_HydrantFrameData.Enqueue(framedata);  //通知存储线程处理
                GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertHydrantValue);
                #endregion
            }
            else if (pack.ID3 == (byte)Entity.ConstValue.DEV_TYPE.NOISE_CTRL)
            {
                #region 噪声数据远传控制器
                if (pack.C1 == (byte)GPRS_READ.READ_NOISEDATA)  //从站向主站发送噪声采集数据
                {
                    int dataindex = (pack.DataLength) % 2;
                    if (dataindex != 0)
                        throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合(2*n)规则");  //GPRS远程压力终端在数据段最后增加两个字节的电压数据
                    else
                        dataindex = (pack.DataLength) / 2;
                    GPRSNoiseFrameDataEntity framedata = new GPRSNoiseFrameDataEntity();
                    framedata.TerId = pack.ID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    GPRSHydrantDataEntity data = new GPRSHydrantDataEntity();
                    bNeedCheckTime = false;
                    volvalue = ((float)BitConverter.ToInt16(new byte[] { pack.Data[pack.DataLength - 1], pack.Data[pack.DataLength - 2] }, 0)) / 1000;

                    //记录仪ID（4byte）＋启动值（2byte）＋总帧数（1byte）＋帧号（1byte）＋ 数据（128byte）＋ 电压（2byte）
                    int logId = BitConverter.ToInt32(new byte[] { pack.Data[3], pack.Data[2], pack.Data[1], 0x00 }, 0);  //记录仪ID
                    int standvalue = BitConverter.ToInt16(new byte[] { pack.Data[5], pack.Data[4] }, 0);      //启动值
                    int sumpackcount = Convert.ToInt32(pack.Data[6]);
                    int curpackindex = Convert.ToInt32(pack.Data[7]);

                    if (curpackindex == 1)
                        state.lstBuffer.Clear();   //收到第一包清空缓存
                    bool needprocess = true;  //是否处理当前包
                    if (curpackindex > 1 && (state.NoisePackIndex == curpackindex)) //如果当前包和上一包序号一样则不处理
                    {
                        needprocess = false;
                    }
                    if (needprocess)
                    {
                        state.NoisePackIndex = curpackindex;   //记录当前收到包的序号
                        if (sumpackcount != curpackindex && !pack.IsFinal)
                        {
                            for (int i = 8; i < pack.DataLength - 2; i++)  //多包时，当前不是最后一包时缓存数据至state.lstBuffer中
                            {
                                state.lstBuffer.Add(pack.Data[i]);
                            }
                        }
                        else
                        {
                            List<byte> lstbytes = new List<byte>();
                            if (curpackindex > 1)  //多包时获取缓存的数据拼接成完整的数据
                            {
                                lstbytes.AddRange(state.lstBuffer);
                                state.lstBuffer.Clear();
                            }
                            for (int i = 8; i < pack.DataLength - 2; i++)  //添加当前包数据
                            {
                                lstbytes.Add(pack.Data[i]);
                            }
                            UpLoadNoiseDataEntity noisedataentity = new UpLoadNoiseDataEntity();
                            noisedataentity.TerId = logId.ToString();  // pack.DevID.ToString();
                            noisedataentity.GroupId = "";
                            //启动值
                            noisedataentity.cali = standvalue;
                            noisedataentity.ColTime = DateTime.Now.ToString();
                            for (int i = 2; i + 1 < lstbytes.Count; i += 2)
                            {
                                noisedataentity.Data += BitConverter.ToInt16(new byte[] { lstbytes[i + 1], lstbytes[i] }, 0) + ",";
                            }
                            if (noisedataentity.Data.EndsWith(","))
                                noisedataentity.Data = noisedataentity.Data.Substring(0, noisedataentity.Data.Length - 1);
                            framedata.NoiseData = noisedataentity;

                            bNeedCheckTime = true;  //每天传一次,一天校时一次,不适用NeedCheckTime方法校时
                        }
                        string strcurnoisedata = "";  //当前包的数据,用于显示
                        for (int i = 8; i + 1 < pack.DataLength - 2; i += 2)
                        {
                            strcurnoisedata += BitConverter.ToInt16(new byte[] { pack.Data[i + 1], pack.Data[i] }, 0) + ",";
                        }
                        if (strcurnoisedata.EndsWith(","))
                            strcurnoisedata = strcurnoisedata.Substring(0, strcurnoisedata.Length - 1);
                        GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("噪声远传控制器[{0}]|记录仪[{1}]|启动值[{2}]|总包数:{3}、当前第{4}包|噪声数据:{5}|电压值:{6}V",
                               pack.DevID, logId, standvalue, sumpackcount, curpackindex, strcurnoisedata, volvalue)));

                        GlobalValue.Instance.GPRS_NoiseFrameData.Enqueue(framedata);  //通知存储线程处理
                        GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertNoiseValue);
                    }
                }
                #endregion
            }
            else if (pack.ID3 == (byte)Entity.ConstValue.DEV_TYPE.WATER_WORKS)
            {
                #region 水厂数据
                if (pack.C1 == (byte)GPRS_READ.READ_WATERWORKSDATA)  //水厂PLC采集数据
                {
                    if (pack.DataLength != 50)
                    {
                        throw new ArgumentException(DateTime.Now.ToString() + " 帧数据长度[" + pack.DataLength + "]不符合固定长度(50byte)规则");
                    }

                    GPRSWaterWorkerFrameDataEntity framedata = new GPRSWaterWorkerFrameDataEntity();
                    framedata.TerId = pack.DevID.ToString();
                    framedata.ModifyTime = DateTime.Now;
                    framedata.Frame = str_frame;

                    int i = 0;
                    string stractivenerge1 = String.Format("{0:X2}", pack.Data[i + 0]) + String.Format("{0:X2}", pack.Data[i + 1]) + String.Format("{0:X2}", pack.Data[i + 2]) + String.Format("{0:X2}", pack.Data[i + 3]);
                    double activenerge1 = Convert.ToSingle(stractivenerge1) / 100;         //1#有功电量
                    string strreactivenerge1 = String.Format("{0:X2}", pack.Data[i + 4]) + String.Format("{0:X2}", pack.Data[i + 5]) + String.Format("{0:X2}", pack.Data[i + 6]) + String.Format("{0:X2}", pack.Data[i + 7]);
                    double reactivenerge1 = Convert.ToSingle(strreactivenerge1) / 100;         //1#无功电量
                    string stractivenerge2 = String.Format("{0:X2}", pack.Data[i + 8]) + String.Format("{0:X2}", pack.Data[i + 9]) + String.Format("{0:X2}", pack.Data[i + 10]) + String.Format("{0:X2}", pack.Data[i + 11]);
                    double activenerge2 = Convert.ToSingle(stractivenerge2) / 100;         //2#有功电量
                    string strreactivenerge2 = String.Format("{0:X2}", pack.Data[i + 12]) + String.Format("{0:X2}", pack.Data[i + 13]) + String.Format("{0:X2}", pack.Data[i + 14]) + String.Format("{0:X2}", pack.Data[i + 15]);
                    double reactivenerge2 = Convert.ToSingle(strreactivenerge2) / 100;         //2#无功电量
                    string stractivenerge3 = String.Format("{0:X2}", pack.Data[i + 16]) + String.Format("{0:X2}", pack.Data[i + 17]) + String.Format("{0:X2}", pack.Data[i + 18]) + String.Format("{0:X2}", pack.Data[i + 19]);
                    double activenerge3 = Convert.ToSingle(stractivenerge3) / 100;         //3#有功电量
                    string strreactivenerge3 = String.Format("{0:X2}", pack.Data[i + 20]) + String.Format("{0:X2}", pack.Data[i + 21]) + String.Format("{0:X2}", pack.Data[i + 22]) + String.Format("{0:X2}", pack.Data[i + 23]);
                    double reactivenerge3 = Convert.ToSingle(strreactivenerge3) / 100;         //3#无功电量
                    string stractivenerge4 = String.Format("{0:X2}", pack.Data[i + 24]) + String.Format("{0:X2}", pack.Data[i + 25]) + String.Format("{0:X2}", pack.Data[i + 26]) + String.Format("{0:X2}", pack.Data[i + 27]);
                    double activenerge4 = Convert.ToSingle(stractivenerge4) / 100;         //4#有功电量
                    string strreactivenerge4 = String.Format("{0:X2}", pack.Data[i + 28]) + String.Format("{0:X2}", pack.Data[i + 29]) + String.Format("{0:X2}", pack.Data[i + 30]) + String.Format("{0:X2}", pack.Data[i + 31]);
                    double reactivenerge4 = Convert.ToSingle(strreactivenerge4) / 100;         //4#无功电量

                    double pressure = (double)BitConverter.ToInt16(new byte[] { pack.Data[i + 33], pack.Data[i + 32] }, 0) / 1000;         //出口压力
                    double liquidlevel = BitConverter.ToSingle(new byte[] { pack.Data[i + 37], pack.Data[i + 36], pack.Data[i + 35], pack.Data[i + 34] }, 0);         //液位
                    double flow1 = BitConverter.ToInt32(new byte[] { pack.Data[i + 41], pack.Data[i + 40], pack.Data[i + 39], pack.Data[i + 38] }, 0);         //流量1
                    double flow2 = BitConverter.ToInt32(new byte[] { pack.Data[i + 45], pack.Data[i + 44], pack.Data[i + 43], pack.Data[i + 42] }, 0);         //流量2
                                                                                                                                                               //2个字节表示一个开关状态,第一个字节没有用  0x00 0x01表示开  0x00 0x00表示关
                    bool switch1 = pack.Data[i + 46] > 0 ? true : false;            //开关状态1
                    bool switch2 = pack.Data[i + 47] > 0 ? true : false;            //开关状态2
                    bool switch3 = pack.Data[i + 48] > 0 ? true : false;            //开关状态3
                    bool switch4 = pack.Data[i + 49] > 0 ? true : false;            //开关状态4

                    GlobalValue.Instance.SocketMag.OnSendMsg(new SocketEventArgs(string.Format("水厂数据[{0}]|1#有功电量:{1}|1#无功电量:{2}|2#有功电量:{3}|2#无功电量:{4}|3#有功电量:{5}|3#无功电量:{6}|4#有功电量:{7}|4#无功电量:{8}|出口压力:{9}|液位:{10}|流量1:{11}|流量2:{12}|开关状态1:{13}|开关状态2:{14}|开关状态3:{15}|开关状态4:{16}",
                        pack.DevID, activenerge1, reactivenerge1, activenerge2, reactivenerge2, activenerge3, reactivenerge3, activenerge4, reactivenerge4,
                        pressure, liquidlevel, flow1, flow2, switch1 ? "开" : "关", switch2 ? "开" : "关", switch3 ? "开" : "关", switch4 ? "开" : "关")));

                    framedata.Activenerge1 = activenerge1;
                    framedata.Rectivenerge1 = reactivenerge1;
                    framedata.Activenerge2 = activenerge2;
                    framedata.Rectivenerge2 = reactivenerge2;
                    framedata.Activenerge3 = activenerge3;
                    framedata.Rectivenerge3 = reactivenerge3;
                    framedata.Activenerge4 = activenerge4;
                    framedata.Rectivenerge4 = reactivenerge4;
                    framedata.Pressure = pressure;
                    framedata.LiquidLevel = liquidlevel;
                    framedata.Flow1 = flow1;
                    framedata.Flow2 = flow2;
                    framedata.Switch1 = switch1;
                    framedata.Switch2 = switch2;
                    framedata.Switch3 = switch3;
                    framedata.Switch4 = switch4;

                    //bNeedCheckTime = NeedCheckTime(framedata.ColTime);

                    GlobalValue.Instance.GPRS_WaterworkerFrameData.Enqueue(framedata);  //通知存储线程处理
                    GlobalValue.Instance.SocketSQLMag.Send(SQLType.InsertWaterworkerValue);
                }
                #endregion
            }
        }
    }
}