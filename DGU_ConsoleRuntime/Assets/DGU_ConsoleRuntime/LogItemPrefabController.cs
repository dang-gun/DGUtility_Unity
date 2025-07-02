
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DGUtility_Unity.ConsoleRuntime
{
    /// <summary>
    /// 로그 아이템 컨트롤러
    /// </summary>
    public class LogItemPrefabController : MonoBehaviour
    {
        public Text LogText { get; private set; }
        public Text StackTraceText { get; private set; }

        private void Awake()
        {
            this.LogText 
                = this.transform.Find("LogText").gameObject
                                .GetComponent<Text>();
            this.StackTraceText
                = this.transform.Find("StackTraceText").gameObject
                                .GetComponent<Text>();
        }

        public void DataSetting(LogDataModel dataLog)
        {
            this.LogText.text
                = string.Format("[{0}] {1, 10} {2}"
                                , dataLog.WriteTime
                                , dataLog.Type
                                , dataLog.Message
                                , dataLog.StackTrace);

            this.StackTraceText.text
                = string.Format("{0}", dataLog.StackTrace);
        }
    }
}
