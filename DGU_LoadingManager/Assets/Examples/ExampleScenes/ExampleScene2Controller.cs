
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using DGU_LoadingManager;


/// <summary>
/// LoadingManager 사용 예제들을 보여주는 스크립트
/// </summary>
public class ExampleScene2Controller 
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

        // 씬이 완전히 로드된 후 필요한 리소스들을 로딩
        StartCoroutine(LoadSceneResources());
    }

    private IEnumerator LoadSceneResources()
    {
        // 약간의 지연을 두어 씬 로딩이 완전히 끝나길 기다림
        yield return new WaitForSeconds(1.0f);


        Debug.Log("Loading Test 03 Controller : LoadSceneResources");

        // 이제 리소스 로딩 시작
        List<string> resourceKeys = new List<string>
        {
            "TestPrefabLable" ,//레이블을 이용하여 로드
            "Assets/Examples/Prefab03.prefab",
            "Assets/Examples/Prefab04.prefab",
        };
        LoadingManager.Instance?.LoadMultipleAssets(
            true, resourceKeys, OnResourcesLoaded);
    }

    private void OnResourcesLoaded()
    {
        Debug.Log("OnResourcesLoaded resources loaded!");
        // 게임 시작 로직
    }

}