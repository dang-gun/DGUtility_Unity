using UnityEngine;


/// <summary>
/// 유니티 로그를 비주얼 스튜디오 로그창에 표시해주는 유틸
/// <para>사용하려는 씬에 추가해야 함</para>
/// </summary>
/// <remarks>
/// 유니티 로그가 비주얼 스튜디오에 나올때가 있고 아닐때가 있다.<br />
/// 이럴때는 유니티 로그를 System.Diagnostics.Debug로 바꿔서 출력하는 꼼수가 있다.<br />
/// 이 작업을 자동화 해주는 유틸이다.
/// </remarks>
public class DGU_UnityLogRedirector : MonoBehaviour
{
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        System.Diagnostics.Debug.WriteLine($"[{type}] {logString}");
    }
}