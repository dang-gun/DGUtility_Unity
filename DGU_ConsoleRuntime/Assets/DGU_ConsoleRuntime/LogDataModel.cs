
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DGUtility_Unity.ConsoleRuntime
{
    /// <summary>
    /// 로그의 화면 표시 정보
    /// </summary>
    public class LogDisplayDataModel
    {
        /// <summary>
        /// 사용할 로그 데이터
        /// </summary>
        public readonly LogDataModel LogData;

        /// <summary>
        /// 화면에 표시했는지 여부
        /// </summary>
        public bool DisplayIs { get; set; }

        public LogDisplayDataModel(LogDataModel dataLog)
        {
            this.LogData = dataLog;
        }

    }

    /// <summary>
    /// Console Runtime에서 사용될 로그 데이터 모델
    /// </summary>
    public readonly struct LogDataModel
    {
        /// <summary>
        /// 고유번호
        /// </summary>
        public readonly long idLog;
        
        /// <summary>
        /// 작성 시간
        /// </summary>
        public readonly DateTime WriteTime;
        
        /// <summary>
        /// 전달된 메시지
        /// </summary>
        public readonly string Message;
        /// <summary>
        /// 전달된 스택 추적
        /// </summary>
        public readonly string StackTrace;
        
        /// <summary>
        /// 로그의 타입
        /// </summary>
        public readonly LogType Type;


        public LogDataModel(string message, string stackTrace, LogType type, long idLog)
        {
            this.idLog = idLog;
            this.WriteTime = DateTime.Now;

            Message = message;
            StackTrace = stackTrace;
            Type = type;
        }

        /// <summary>
        /// 시스템에서 넘어온 데이터가 가지고 있는 데이터와 내용이 같은지 확인한다.
        /// </summary>
        /// <param name="dataLog"></param>
        /// <returns></returns>
        public bool Equals(LogDataModel dataLog)
        {
            return Message == dataLog.Message 
                    && StackTrace == dataLog.StackTrace 
                    && Type == dataLog.Type;
        }
    }
}
