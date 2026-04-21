
using System;

/// <summary>
/// 유니티에서 Start 처리를 도와주는 유틸
/// </summary>
/// <remarks>
/// MonoBehaviour를 상속받는 스크립트의 경우 Awake가 처리되야 개체가 생성된다.<br />
/// Awake가 호출되기 전에 초기화 정보를 미리 넣는 경우 그때그때 처리해야 하는 문제가 있다.<br />
/// 이 유틸은 Awake가 종료되고 나서 호출되는 Start에서 호출하여 이러한 처리를 공통화해준다.
/// <para>Start가 실행전이면 Start가 호출될때가지 대기하고 이미 Start가 호출되었으면 바로 실행된다.</para>
/// <para>이 개체는 한번 동작하면 필요없으므로 적절한 타이밍에 제거하는 것이 좋다.</para>
/// </remarks>
public class DGU_UnityOnStartAssist
{
    /// <summary>
    /// 실행 되었는지 여부
    /// </summary>
    public bool RunIs { get; private set; } = false;

    /// <summary>
    /// Start가 이미 호출되었는지 여부
    /// <para>실행 여부와 상관없이 'StartCall'함수가 실행되면 true가 된다.</para>
    /// </summary>
    public bool StartIs { get; private set; } = false;

    /// <summary>
    /// 동작시킬 내용
    /// </summary>
    public Action RunFunc { get; private set; } = null;
    /// <summary>
    /// 동작시킬 내용 저장
    /// </summary>
    /// <param name="funcRun"></param>
    public void RunFuncSet(Action funcRun)
    {
        this.RunFunc = funcRun;

        if(true == this.StartIs)
        {//이미 시작했다.

            this.Run();
        }
    }

    /// <summary>
    /// 스타트가 호출되면 호출해야할 함수
    /// <para>이 개체를 가지고 있는 부모가 Start를 받았을때 호출되어야 하는 함수다.</para>
    /// </summary>
    public void StartCall()
    {
        this.StartIs = true;

        this.Run();
    }

    /// <summary>
    /// 저장된 함수를 실행한다.
    /// </summary>
    private void Run()
    {
        if(null != this.RunFunc)
        {
            this.RunIs = true;

            this.RunFunc();
        }
    }

    /// <summary>
    /// 지워도 되는지 여부
    /// </summary>
    /// <returns></returns>
    public bool PossibleRemoveCheck()
    {
        bool bReturn = false;

        if(true == this.StartIs && true == this.RunIs)
        {
            bReturn = true;
        }

        return bReturn;
    }
}