﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using IDCM.Data.Base;

namespace IDCM.Service.Common.GCMDAM
{
    public class SignExecutor
    {
        public static bool SignOff(AuthInfo authInfo = null, int timeout = 10000)
        {
            if (authInfo != null)
            {
                string signInUri = ConfigurationManager.AppSettings[SysConstants.SignOffUri];
                string url = string.Format(signInUri, new string[] { authInfo.Jsessionid });
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                log.Info("StrainViewQueryExecutor Request Url=" + url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "*/*";
                request.KeepAlive = true;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1)";
                request.Timeout = timeout;
                request.ReadWriteTimeout = timeout;
                Stream myRequestStream = request.GetRequestStream();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("UTF-8"));
                string resStr = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                log.Info("StrainViewQueryExecutor Response=" + resStr);
                bool res = parserToBool(resStr);
                return res;
            }
            return false;
        }

        public static AuthInfo SignIn(string username, string password, bool autoLogin = true, int timeout = 10000)
        {
            if (username != null && password != null)
            {
                string signInUri = ConfigurationManager.AppSettings[SysConstants.SignInUri];
                string url = string.Format(signInUri, new string[] { username, password });
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                log.Info("SignInExecutor Request Url=" + url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "*/*";
                request.KeepAlive = true;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1)";
                request.Timeout = timeout;
                request.ReadWriteTimeout = timeout;
                Stream myRequestStream = request.GetRequestStream();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("UTF-8"));
                string resStr = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                log.Info("SignInExecutor Response=" + resStr);
                AuthInfo auth = parserToAuthInfo(resStr);
                if (auth != null)
                {
                    auth.Username = username;
                    auth.Password = password;
                    auth.autoLogin = autoLogin;
                    auth.Timestamp = DateTime.Now.Ticks;
                }
                return auth;
            }
            return null;
        }

        protected static AuthInfo parserToAuthInfo(string jsonStr)
        {
            AuthInfo auth = new AuthInfo();
            AInfo ai = JsonConvert.DeserializeObject<AInfo>(jsonStr);
            auth.LoginFlag = Boolean.Parse(ai.loginFlag);
            auth.Jsessionid = ai.jsessionid;
            return auth;
        }
        protected static bool parserToBool(string jsonStr)
        {
            return jsonStr.ToLower().Contains("true");
        }

        private class AInfo
        {
            public string username { get; set; }
            public string loginFlag { get; set; }
            public string jsessionid { get; set; }
        }
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
    }
}
