﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Threading;
using Common;
using Entity;
using BLL;
using SmartWaterSystem;

namespace GCGPRSService
{
    public delegate void HTTPReceiveMessage(HTTPMsgEventArgs e);
    public class HTTPMsgEventArgs : EventArgs
    {
        private string _msg = "";
        public string Msg
        {
            get { return _msg; }
            set { _msg = value; }
        }

        public HTTPMsgEventArgs()
        {
        }

        public HTTPMsgEventArgs(string msg)
        {
            this._msg = msg;
        }
    }

    public class HTTPService
    {
        NLog.Logger logger = NLog.LogManager.GetLogger("HTTPService");
        private bool isrun = false;
        HttpListener listener = new HttpListener();
        HttpDataBLL bll = new HttpDataBLL();

        public event HTTPReceiveMessage HTTPMessageEvent;

        public void Start()
        {
            if (string.IsNullOrEmpty(Settings.Instance.GetString(SettingKeys.HTTPServiceURL)))
            {
                OnReceiveMsg(DateTime.Now.ToString() + " 启动HTTP服务失败,URL为空");
                return;
            }
            if (string.IsNullOrEmpty(Settings.Instance.GetString(SettingKeys.HTTPMD5Key)))
            {
                OnReceiveMsg(DateTime.Now.ToString() + " 启动HTTP服务失败,MD5 Key为空");
                return;
            }
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Prefixes.Add(Settings.Instance.GetString(SettingKeys.HTTPServiceURL));
            OnReceiveMsg(DateTime.Now.ToString() + " 启动HTTP服务,URL:" + Settings.Instance.GetString(SettingKeys.HTTPServiceURL));
            isrun = true;
            Thread t = new Thread(new ThreadStart(ServiceThread));
            listener.Start();
            t.Start();
            OnReceiveMsg(DateTime.Now.ToString() + " 开启HTTP接收线程完成!");
        }

        public void Stop()
        {
            isrun = false;
            listener.Close();

            OnReceiveMsg("停止HTTP服务...");
        }

        public virtual void OnReceiveMsg(string str)
        {
            if (HTTPMessageEvent != null)
                HTTPMessageEvent(new HTTPMsgEventArgs(str));
        }

        private void ServiceThread()
        {
            while (true)
            {
                try
                {
                    if (!isrun)
                    {
                        Thread.CurrentThread.Abort();
                        return;
                    }

                    HttpListenerContext context = listener.GetContext();
                    context.Response.StatusCode = 200;

                    string str_resp_err = "";
                    string str_resp = "";
                    byte[] buffer = new byte[1024];
                    if (context.Request.HttpMethod.ToLower().Equals("get"))
                    {
                        //GET请求处理
                        str_resp_err = "不支持GET方法";
                    }
                    else if (context.Request.HttpMethod.ToLower().Equals("post"))
                    {
                        //这是在POST请求时必须传参的判断默认注释掉
                        if (!context.Request.HasEntityBody)
                        {
                            str_resp_err = "请传入参数";
                        }
                        else
                        {
                            //POST请求处理
                            Stream SourceStream = context.Request.InputStream;
                            int readcount = -1;

                            List<byte> lstbytes = new List<byte>();
                            while ((readcount = SourceStream.Read(buffer, 0, 1024)) > 0)
                            {
                                for (int i = 0; i < readcount; i++)
                                {
                                    lstbytes.Add(buffer[i]);
                                }
                            }
                            string strrequest = Encoding.UTF8.GetString(lstbytes.ToArray());
                            try
                            {
                                strrequest = System.Web.HttpUtility.UrlDecode(strrequest, Encoding.UTF8); //UrlDecode
                                OnReceiveMsg("接收到请求[" + DateTime.Now.ToString() + "]:" + strrequest);
                                HTTPEntity httpentity = SmartWaterSystem.JSONSerialize.JsonDeserialize<HTTPEntity>(strrequest);  //jsondeSerialize

                                if (httpentity == null)
                                {
                                    str_resp_err = "无效数据,解析失败!";
                                    goto err;
                                }
                                if (string.IsNullOrEmpty(httpentity.Params))
                                {
                                    str_resp_err = "无效params,解析失败!";
                                    goto err;
                                }
                                if (string.IsNullOrEmpty(httpentity.timestamp))
                                {
                                    str_resp_err = "时间戳不能为空!";
                                    goto err;
                                }
                                else
                                {
                                    System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
                                    DateTime dtime = startTime.AddMilliseconds(Convert.ToDouble(httpentity.timestamp));
                                    TimeSpan ts = DateTime.Now - dtime;
                                    if (Math.Abs(ts.TotalMinutes) > Settings.Instance.GetInt(SettingKeys.HTTPReqSuviceTime))
                                    {
                                        str_resp_err = "该请求已失效!";
                                        goto err;
                                    }
                                }

                                //MD5(body +时间戳+Key)
                                string md5 = MD5Encrypt.MD5(System.Web.HttpUtility.UrlEncode(httpentity.Params + httpentity.timestamp + Settings.Instance.GetString(SettingKeys.HTTPMD5Key)).ToLower());
                                if (md5 != httpentity.digest)
                                {
                                    str_resp_err = "MD5校验失败!";
                                    goto err;
                                }

                                str_resp_err = "无效action";
                                string action = "";
                                
                                foreach (Match m in Regex.Matches(httpentity.Params, "\"action\" ?: ?\"(?<title>.*?)\"", RegexOptions.IgnoreCase))
                                {
                                    if (m.Success)
                                        action = m.Groups["title"].Value;
                                }

                                try
                                {
                                    switch (action)
                                    {
                                        case "getgroups":
                                            str_resp_err = "";
                                            GetGroupsRespEntity getgrouprespentity = bll.GetGroupsInfo();
                                            str_resp = SmartWaterSystem.JSONSerialize.JsonSerialize<GetGroupsRespEntity>(getgrouprespentity);
                                            break;
                                        case "uploadnoisedata":  //上传分组信息
                                            str_resp_err = "";
                                            UploadNoiseDataReqEntity parmentity = SmartWaterSystem.JSONSerialize.JsonDeserialize<UploadNoiseDataReqEntity>(httpentity.Params);
                                            HTTPRespEntity uploadrespentity = bll.UploadGroups(parmentity.TerData);
                                            str_resp = SmartWaterSystem.JSONSerialize.JsonSerialize<HTTPRespEntity>(uploadrespentity);
                                            break;
                                    }
                                }
                                catch
                                {
                                    str_resp_err = "解析异常";
                                }
                            }
                            catch (Exception ex)
                            {
                                str_resp_err = "无效参数类型";
                            }
                        }
                    }

                err:
                    if (!string.IsNullOrEmpty(str_resp_err) || string.IsNullOrEmpty(str_resp))
                    {
                        HTTPRespEntity respent = new HTTPRespEntity();
                        respent.code = -1;
                        if (!string.IsNullOrEmpty(str_resp_err))
                            respent.msg = str_resp_err;
                        respent.data = "";

                        str_resp = SmartWaterSystem.JSONSerialize.JsonSerialize<HTTPRespEntity>(respent);
                    }
                    OnReceiveMsg(DateTime.Now.ToString() + " 响应内容:" + str_resp);
                    str_resp = System.Web.HttpUtility.UrlEncode(str_resp);
                    byte[] buffer_resp = Encoding.UTF8.GetBytes(str_resp);
                    context.Response.OutputStream.Write(buffer_resp, 0, buffer_resp.Length);
                    context.Response.OutputStream.Flush();
                    context.Response.OutputStream.Close();
                    
                    context.Response.Close();
                }
                catch (Exception ex)
                {
                    logger.ErrorException("ServiceThread", ex);
                }
            }
        }

    }
}
