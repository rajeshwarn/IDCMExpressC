﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDCM.Service.Common.Core
{
    /// <summary>
    /// 异步消息类型及附属参数的封装类
    /// </summary>
    public class AsyncMessage
    {
        public static readonly AsyncMessage DataPrepared = new AsyncMessage(MsgType.DataPrepared, "Data Prepared");
        public static readonly AsyncMessage RetryQuickStartConnect = new AsyncMessage(MsgType.RetryQuickStartConnect, "Retry Quick Start Connect");

        /// <summary>
        /// For iterator 
        /// </summary>
        public static IEnumerable<AsyncMessage> Values
        {
            get
            {
                yield return DataPrepared;
                yield return RetryQuickStartConnect;
            }
        }
        private readonly string msgTag;
        private readonly MsgType msgType;
        private readonly object[] parameters;

        public AsyncMessage(AsyncMessage amsg, params object[] parameters)
        {
            this.msgTag = amsg.msgTag;
            this.msgType = amsg.msgType;
            this.parameters = parameters;
        }

        protected AsyncMessage(MsgType msgType, string msgTag, object[] parameters = null)
        {
            this.msgTag = msgTag;
            this.msgType = msgType;
            this.parameters = parameters;
        }

        public string MsgTag { get { return msgTag; } }

        public MsgType MsgType { get { return msgType; } }

        public object[] Parameters { get { return parameters; } }

        public override string ToString()
        {
            return msgType+":"+msgTag;
        }
    }
    /// <summary>
    /// 预定义的消息类型
    /// </summary>
    public enum MsgType
    {
        DataPrepared = 0,
        RetryQuickStartConnect = 1
    }
}