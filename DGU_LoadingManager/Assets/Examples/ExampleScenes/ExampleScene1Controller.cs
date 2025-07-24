
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using DGU_LoadingManager;


/// <summary>
/// 추가 리소스 로드가 없는 씬
/// </summary>
public class ExampleScene1Controller 
    : MonoBehaviour
    , SceneCommonInterface
{
    public bool SceneLoadCompleteIs { get; private set; } = false;

    private void Start()
    {
        // 씬이 완전히 로드된 후 필요한 리소스들을 로딩
        //StartCoroutine(LoadSceneResources());

        if (false == LoadingManager.Instance?.LoadingUiIs)
        {//로딩UI가 꺼져있다.

            //로딩UI가 꺼져있으면 리소스 로드를 수동으로 시도해야 한다.
            this.SceneLoadComplete();
        }
    }

    public void SceneLoadComplete()
    {
        this.SceneLoadCompleteIs = true;

        //씬이 로드되고 바로 추가 로드할것이 없으므로 그냥 로딩창을 닫는다.
        LoadingManager.Instance?.HideLoadingScreen();
    }

}