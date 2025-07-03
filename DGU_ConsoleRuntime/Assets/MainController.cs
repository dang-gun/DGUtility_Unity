using UnityEngine;

using DGUtility_Unity.ConsoleRuntime;

public class MainController : MonoBehaviour
{
    /// <summary>
    /// 콘솔UI
    /// </summary>
    private DGU_ConsoleRuntimeController ConsoleUI { get; set; }

    void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        this.ConsoleUI
            = canvas.transform.Find("ConsoleRuntimeUi")
                        .GetComponent<DGU_ConsoleRuntimeController>();
        this.ConsoleUI.OnCommandInput += ConsoleUI_OnCommandInput;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            this.ConsoleUI.UI_Toggle();
        }
    }

    
    void OnRectTransformDimensionsChange()
    {
        Debug.Log("UI 요소의 크기가 변경되었습니다: " + GetComponent<RectTransform>().rect.size);
        // 여기에 UI 요소 크기 변화에 따른 로직을 추가합니다.
    }

    /// <summary>
    /// 콘솔 명령어가 들어올때 할 동작
    /// </summary>
    /// <param name="sData"></param>
    private void ConsoleUI_OnCommandInput(string sData)
    {
        Debug.Log("Console Command : " + sData);

        //소문자로 변환
        //띄어쓰기로 구분
        string[] sCut = sData.ToLower().Split(" ");

        switch (sCut[0])
        {
            case "st"://추적 스택 표시 여부
                if ("on" == sCut[1]) // st on
                {
                    this.ConsoleUI.StackTraceText_Apply(true);
                    Debug.Log("Stack Trace Text : Show");
                }
                else if ("off" == sCut[1])
                {
                    this.ConsoleUI.StackTraceText_Apply(false);
                    Debug.Log("Stack Trace Text : Hide");
                }
                break;

            case "fontsize"://폰트 사이즈 지정
                {
                    int nFontSize = 0;
                    //문자를 숫자로 변환
                    int.TryParse(sCut[1], out nFontSize);

                    if (0 < nFontSize)
                    {
                        this.ConsoleUI.FontSize_Apply(nFontSize);
                        Debug.Log("Font Size : " + nFontSize);
                    }
                    else
                    {
                        Debug.LogError("Font Size : Invalid Size");
                    }
                }
                break;
                

            case "logtype":
                switch(sCut[1])
                {
                    case "error":
                        Debug.LogError("Log Type : Error");
                        break;
                    case "assert":
                        Debug.LogAssertion("Log Type : Assert");
                        break;
                    case "warning":
                        Debug.LogWarning("Log Type : Warning");
                        break;
                    case "exception":
                        Debug.LogException(new System.Exception("Log Type : Exception"));
                        break;
                    case "log":
                        Debug.Log("Log Type : Log");
                        break;
                }
                break;
        }
    }
}
