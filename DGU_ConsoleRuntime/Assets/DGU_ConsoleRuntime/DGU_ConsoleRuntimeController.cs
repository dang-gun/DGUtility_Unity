
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DGUtility_Unity.ConsoleRuntime
{
    /// <summary>
    /// 문자열 하나만 전달하는 대리자
    /// </summary>
    /// <remarks>
    /// TextMeshPro를 사용하고 있어서 꼭 다운받아야 한다.
    /// </remarks>
    /// <param name="sData"></param>
    public delegate void StringDelegate(string sData);

    /// <summary>
    /// 유니티 런타임용 콘솔
    /// </summary>
    public class DGU_ConsoleRuntimeController : MonoBehaviour
    {
        #region 외부에서 연결할 이벤트
        /// <summary>
        /// 콘솔 명령이 입력되면 발생하는 이벤트
        /// </summary>
        public event StringDelegate OnCommandInput;
        /// <summary>
        /// 콘솔 명령이 입력되면 발생하는 이벤트 호출
        /// </summary>
        /// <param name="sCmd"></param>
        private void OnCommandInputCall(string sCmd)
        {
            if (this.OnCommandInput != null)
            {
                this.OnCommandInput(sCmd);
            }
        }
        #endregion

        
        //public LogItemPrefabController LogItem;
        public GameObject LogItem;

        
        /// <summary>
        /// 소속된 캔버스
        /// </summary>
        private Canvas CanvasThis { get; set; }

        /// <summary>
        /// 이 UI의 RectTransform
        /// </summary>
        private RectTransform RTThis { get; set; }
        /// <summary>
        /// 아이템의 마지막 레이블이 사용할 크기
        /// </summary>
        private float Item_LastLabel { get; set; }
        /// <summary>
        /// 마지막 레이블의 크기가 한번이상 설정되었었는지 여부
        /// </summary>
        private bool Item_LastLabel_IsSet = false;

        /// <summary>
        /// 스크롤
        /// </summary>
        private ScrollRect LogScrollView { get; set; }
        /// <summary>
        /// 리스트뷰 아이템이 출력될 위치
        /// </summary>
        private Transform LogScrollViewContentTransform { get; set; }

        /// <summary>
        /// 콘솔 입력 UI
        /// </summary>
        private InputField ConsoleInputText { get; set; }
        /// <summary>
        /// 콘솔 입력 버튼
        /// </summary>
        private Button ConsoleInputBtn { get; set; }


        #region 옵션 관련
        /// <summary>
        /// 시작하자마자 표시할지 여부
        /// </summary>
        public bool StartShowIs = false;

        /// <summary>
        /// 폰트 지정
        /// </summary>
        public TMP_FontAsset ConsoleFont = null;

        /// <summary>
        /// 폰트 사이즈 지정
        /// </summary>
        public int FontSize = 18;
        /// <summary>
        /// 폰트 사이즈 적용
        /// </summary>
        public void FontSize_Apply()
        {
            this.FontSize_Apply(this.FontSize);
        }
        /// <summary>
        /// 폰트 사이즈 적용
        /// </summary>
        /// <param name="sFontSize"></param>
        public void FontSize_Apply(int sFontSize)
        {
            this.FontSize = sFontSize;

            for (int i = 0; i < this.LogList.Count; ++i)
            {
                LogDisplayDataModel item = this.LogList[i];

                item.LogItemCont.FontSizeSet(sFontSize);
            }

            this.ReszieThis();
        }

        /// <summary>
        /// 스택 추적 텍스트 표시 여부
        /// </summary>
        public bool StackTraceText_ShowIs = false;
        /// <summary>
        /// 스택 추적 텍스트 표시 여부 적용
        /// </summary>
        public void StackTraceText_Apply()
        {
            this.StackTraceText_Apply(this.StackTraceText_ShowIs);
        }
        /// <summary>
        /// 스택 추적 텍스트 표시 여부 적용
        /// </summary>
        /// <param name="bShow"></param>
        public void StackTraceText_Apply(bool bShow)
        {
            for (int i = 0; i < this.LogList.Count; ++i)
            {
                LogDisplayDataModel item = this.LogList[i];

                item.LogItemCont.StackTraceTextShow(bShow);
            }
        }

        #endregion

        /// <summary>
        /// 로그 리스트UI에 사용할 프리팹
        /// </summary>
        //public GameObject LogItemPrefab;
        public readonly string LogItemPrefab_Path
            = "Assets/GameComponents/Utility/DGU_ConsoleRuntime/LogItemPrefab.prefab";

        /// <summary>
        /// 스레드 락용 오브젝트
        /// </summary>
        public object LockObj = new object();

        /// <summary>
        /// 전달된 로그 총 개수
        /// </summary>
        public long LogCount { get; private set; }

        /// <summary>
        /// 화면에 출력할 로그 리스트
        /// </summary>
        private readonly List<LogDisplayDataModel> LogList
            = new List<LogDisplayDataModel>();
        /// <summary>
        /// 로그리스트로 옮기기 전에 임시로 저장해두는 로그 리스트
        /// <para>유니티로부터 전달받은 로그를 임시로 저장해 둔다.</para>
        /// </summary>
        /// <remarks>
        /// LogList를 정렬하는건 너무 비용이 많이 드므로 여기에 임시저장을 하고 
        /// 정렬된 결과를 옮기는 방식으로 사용한다.
        /// </remarks>
        private List<LogDisplayDataModel> LogList_Temp
            = new List<LogDisplayDataModel>();


        /// <summary>
        /// 콘솔이 열려있는지 여부
        /// </summary>
        private bool OpenIs = false;

        private void Awake()
        {

            //임시로 직접 탐색한다.

            this.CanvasThis = this.GetComponent<Canvas>();
            this.RTThis = this.CanvasThis.GetComponent<RectTransform>();

            this.LogScrollView
                = this.transform.Find("LogScrollView").gameObject
                    .GetComponent<ScrollRect>();


            this.LogScrollViewContentTransform
                = this.transform.Find("LogScrollView/Viewport/Content").gameObject
                        .transform;

            this.ConsoleInputText
                = this.transform.Find("InputUI/ConsoleInputField").gameObject
                        .GetComponent<InputField>();

            this.ConsoleInputBtn
                = this.transform.Find("InputUI/ConsoleInputButton").gameObject
                        .GetComponent<Button>();
            this.ConsoleInputBtn.onClick.AddListener(() => { this.ConsoleCommand(); });
        }

        private void Start()
        {
            //비동기로 처리하여 UI스레드의 랙을 방지한다.
            //비슷한거 : Application.logMessageReceived
            Application.logMessageReceivedThreaded -= HandleLogThreaded;
            Application.logMessageReceivedThreaded += HandleLogThreaded;

            if (false == this.StartShowIs)
            {
                this.UI_Inactive();
            }


        }

        /// <summary>
        /// 사이즈를 다시 계산한다.
        /// <para>외부에 의해서 리사이즈 이슈가 있을때 호출해야한다.</para>
        /// </summary>
        public void ReszieThis()
        {
            if(0 < this.LogList.Count)
            {//내용이 있다.

                LogItemPrefabController item = this.LogList[0].LogItemCont;
                if(null != item)
                {//아이템 컨트롤러가 있다

                    //아이템 컨트롤러의 크기를 다시 계산한다.
                    this.Item_LastLabel
                        = this.RTThis.rect.width 
                            - 50f //좌우 여백
                            - item.WidthSizeGet_WithoutLastLable();


                    Item_AllResize();
                }
                
            }
        }

        private void Item_AllResize()
        {
            for (int i = 0; i < this.LogList.Count; ++i)
            {
                LogDisplayDataModel item = this.LogList[i];
                item.LogItemCont.WidthSizeSet_LastLabel(this.Item_LastLabel);
            }
        }

        /// <summary>
        /// 시스템으로부터 전달받은 로그를 큐에 담아둔다.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        void HandleLogThreaded(
            string message
            , string stackTrace
            , LogType type)
        {
            long LogCountTemp;
            lock (LockObj)
            {
                //로그 카운터를 올려준다.
                this.LogCount = this.LogCount + 1;
                LogCountTemp = this.LogCount;
            }//end lock LockObj


            //임시 리스트에 데이터를 넣는다.
            this.LogList_Temp
                .Add(new LogDisplayDataModel(
                        new LogDataModel(message
                                        , stackTrace
                                        , type
                                        , LogCountTemp)));

            //throw new Exception("테스트용 예외입니다.");
        }

        private void OnEnable()
        {
            this.ShowLog_RefreshAdd();
        }

        private void OnDisable()
        {

        }

        private void Update()
        {
            this.MoveLogList();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (true == this.OpenIs
                    && null != this.ConsoleInputText
                    && this.ConsoleInputText.text != string.Empty)
                {//콘솔이 열려 있고
                 //입력한 내용이 있다.

                    this.ConsoleCommand();
                }
            }
            
        }

        public void UI_Toggle()
        {
            if (true == this.OpenIs)
            {
                UI_Inactive();
            }
            else
            {
                UI_Active();
            }
        }

        public void UI_Active()
        {
            this.OpenIs = true;
            this.gameObject.SetActive(this.OpenIs);
        }
        public void UI_Inactive()
        {
            this.OpenIs = false;
            this.gameObject.SetActive(this.OpenIs);
        }

        /// <summary>
        /// 임시리스트에 있는 로그를 정렬하여 리스트로 옮긴다.
        /// </summary>
        private void MoveLogList()
        {
            if (0 < this.LogList_Temp.Count)
            {
                //입력순으로 정렬
                List<LogDisplayDataModel> listTemp
                    = this.LogList_Temp.OrderBy(x => x.LogData.idLog).ToList();
                //임시 리스트 비우기
                this.LogList_Temp.Clear();

                //로그 리스트에 입력
                this.LogList.AddRange(listTemp);

                if (0 < listTemp.Count)
                {//입력한 리스트가 있다.

                    //
                    if (true == OpenIs)
                    {//콘솔이 열려 있다.

                        //콘솔이 열려있으면 바로 UI에 추가해준다.
                        this.ShowLog_Add(listTemp);
                    }
                }
            }
        }//end MoveLogList

        /// <summary>
        /// 출력된 로그를 제거하고 지금 가지고 있는 로그를 화면에 출력한다.
        /// </summary>
        public void ShowLog_RefreshAll()
        {
            //기존 리스트 지우기
            foreach (Transform child in this.LogScrollViewContentTransform)
            {
                //0.01f이후에 파괴도록 등록하면 다음 프레임에 파괴된다.
                Destroy(child.gameObject, 0.01f);
            }

            //새로 출력
            this.ShowLog_Add(this.LogList);
        }

        /// <summary>
        /// 리스트에 출력되지 않은 요소만 다시 화면에 출력시킨다.
        /// </summary>
        public void ShowLog_RefreshAdd()
        {
            List<LogDisplayDataModel> listTemp
                = this.LogList
                        //출력되지 않은 항목
                        .Where(w => w.DisplayIs == false)
                        //idLog로 오른차순 정렬
                        .OrderBy(ob => ob.LogData.idLog)
                        .ToList();

            //추가 출력
            this.ShowLog_Add(listTemp);
        }

        /// <summary>
        /// 기준 출력에 지정된 리스트를 추가하여 화면에 출력한다.
        /// </summary>
        /// <param name="listShow"></param>
        public void ShowLog_Add(List<LogDisplayDataModel> listShow)
        {
            if(0 >= listShow.Count)
            {
                return;
            }

            //새 리스트 만들기
            for (int i = 0; i < listShow.Count; ++i)
            {
                LogDisplayDataModel item = listShow[i];

                GameObject goNew 
                    = Instantiate(this.LogItem
                                    , this.LogScrollViewContentTransform);

                item.LogItemCont = goNew.GetComponent<LogItemPrefabController>();
                item.LogItemCont.FontSet(this.ConsoleFont);
                item.LogItemCont.FontSizeSet(this.FontSize);

                item.DisplayIs = true;
                item.LogItemCont.DataSetting(item.LogData);
                

                item.LogItemCont.StackTraceTextShow(this.StackTraceText_ShowIs);

                //아이템 컨트롤러의 크기를 다시 계산한다.
                item.LogItemCont.WidthSizeSet_LastLabel(this.Item_LastLabel);
            }

            StartCoroutine(ScrollToBottomAfterDelay());
        }

        private System.Collections.IEnumerator ScrollToBottomAfterDelay()
        {
            // UI 업데이트 후에 실행
            yield return new WaitForEndOfFrame();
            //약간의 지연을 추가
            //지연이 없으면 컨탠츠에 아이템이 그려지기전에 스크롤이 되서 아이템 한개만큼 덜내려간다.
            //원인을 찾을때까지는 지연으로 처리한다.
            yield return new WaitForSeconds(0.1f); 

            Canvas.ForceUpdateCanvases(); // 캔버스 강제 업데이트
            this.LogScrollView.verticalNormalizedPosition = 0f; // 스크롤을 맨 아래로 이동

            if (false == this.Item_LastLabel_IsSet)
            {//마지막 레이블 크기가 수정되지 않았다.

                //크기를 재계산한다.
                this.Item_LastLabel_IsSet = true;
                this.ReszieThis();
            }
        }

        /// <summary>
        /// 지금 입력칸에 있는 텍스트를 명령어로 사용한다.
        /// </summary>
        public void ConsoleCommand()
        {
            this.ConsoleCommand(this.ConsoleInputText.text);
            this.ConsoleInputText.text = string.Empty;
        }
        /// <summary>
        /// 콘솔 명령 입력
        /// </summary>
        /// <param name="sCmd"></param>
        public void ConsoleCommand(string sCmd)
        {
            //Debug.Log("ConsoleCommand : " + sCmd);
            this.OnCommandInput(sCmd);
        }

    }

}
