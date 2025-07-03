
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
        /// <summary>
        /// 앞쪽 대괄호
        /// </summary>
        private TextMeshProUGUI SquareBeforeLable { get; set; }
        /// <summary>
        /// 날짜
        /// </summary>
        private TextMeshProUGUI DateLable { get; set; }
        /// <summary>
        /// 시간
        /// </summary>
        private TextMeshProUGUI TimeLable { get; set; }
        /// <summary>
        /// 뒤쪽 대괄호
        /// </summary>
        private TextMeshProUGUI SquareAfterLable { get; set; }


        /// <summary>
        /// 타입
        /// </summary>
        private TextMeshProUGUI TypeLable { get; set; }

        /// <summary>
        /// 로그 출력용 텍스트
        /// </summary>
        private TextMeshProUGUI LogText { get; set; }
        /// <summary>
        /// 추적 스택용 텍스트
        /// </summary>
        private TextMeshProUGUI StackTraceText { get; set; }

        private void Awake()
        {
            this.SquareBeforeLable
                = this.transform.Find("LogPanel/SquareBeforeLable").gameObject
                                .GetComponent<TextMeshProUGUI>();
            this.DateLable
                = this.transform.Find("LogPanel/DateLable").gameObject
                                .GetComponent<TextMeshProUGUI>();
            this.TimeLable
                = this.transform.Find("LogPanel/TimeLable").gameObject
                                .GetComponent<TextMeshProUGUI>();
            this.SquareAfterLable
                = this.transform.Find("LogPanel/SquareAfterLable").gameObject
                                .GetComponent<TextMeshProUGUI>();


            this.TypeLable
                = this.transform.Find("LogPanel/TypeLable").gameObject
                                .GetComponent<TextMeshProUGUI>();

            this.LogText 
                = this.transform.Find("LogPanel/LogText").gameObject
                                .GetComponent<TextMeshProUGUI>();
            this.StackTraceText
                = this.transform.Find("StackTraceText").gameObject
                                .GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// 로그 출력
        /// </summary>
        /// <param name="dataLog"></param>
        public void DataSetting(LogDataModel dataLog)
        {
            this.DateLable.text
                = string.Format("{0:yyyy-MM-dd} ", dataLog.WriteTime);
            this.TimeLable.text
                = string.Format(" {0:HH:mm:ss}", dataLog.WriteTime);

            Color color = Color.white;
            switch(dataLog.Type)
            {
                case LogType.Error:
                    color = Color.red;
                    break;
                case LogType.Assert:
                    color = Color.blue;
                    break;
                case LogType.Warning:
                    color = Color.yellow;
                    break;
                
                case LogType.Exception:
                    color = Color.magenta;
                    break;

                case LogType.Log:
                default:
                    break;
            }

            this.TypeLable.text
                = string.Format("[{0,-10}]", dataLog.Type);
            this.TypeLable.color = color;

            this.LogText.text
                = string.Format("{0}", dataLog.Message);

            this.StackTraceText.text
                = string.Format("{0}", dataLog.StackTrace);
        }

        /// <summary>
        /// 마지막 아이템을 제외한 가로 크기를 반환한다.
        /// </summary>
        /// <returns></returns>
        public float WidthSizeGet_WithoutLastLable()
        {
            float fWidth = 0.0f;

            fWidth += this.SquareBeforeLable.preferredWidth;
            fWidth += this.DateLable.preferredWidth;
            fWidth += this.TimeLable.preferredWidth;
            fWidth += this.SquareAfterLable.preferredWidth;
            fWidth += this.TypeLable.preferredWidth;

            return fWidth;
        }

        /// <summary>
        /// 마지막 레이블의 가로 크기를 지정한다.
        /// </summary>
        /// <param name="fWidth"></param>
        public void WidthSizeSet_LastLabel(float fWidth)
        {
            this.LogText.rectTransform.sizeDelta
                = new Vector2(fWidth, this.LogText.rectTransform.sizeDelta.y);
        }

        /// <summary>
        /// 폰트 지정
        /// </summary>
        /// <param name="font">TMP 폰트</param>
        public void FontSet(TMP_FontAsset font)
        {
            if(null != font)
            {
                this.SquareBeforeLable.font = font;
                this.DateLable.font = font;
                this.TimeLable.font = font;
                this.SquareAfterLable.font = font;
                this.TypeLable.font = font;
                this.LogText.font = font;
                this.StackTraceText.font = font;
            }
        }

        /// <summary>
        /// 폰트 크기 변경
        /// </summary>
        /// <param name="size"></param>
        public void FontSizeSet(int size)
        {
            this.SquareBeforeLable.fontSize = size;
            this.DateLable.fontSize = size;
            this.TimeLable.fontSize = size;
            this.SquareAfterLable.fontSize = size;
            this.TypeLable.fontSize = size;
            this.LogText.fontSize = size;
            this.StackTraceText.fontSize = size;
        }

        /// <summary>
        /// 스택 추적 텍스트 표시 여부 설정
        /// </summary>
        /// <param name="bShow"></param>
        public void StackTraceTextShow(bool bShow)
        {
            this.StackTraceText.gameObject.SetActive(bShow);
        }
    }
}
