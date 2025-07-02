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

    /// <summary>
    /// 콘솔 명령어가 들어올때 할 동작
    /// </summary>
    /// <param name="sData"></param>
    private void ConsoleUI_OnCommandInput(string sData)
    {

    }
}
