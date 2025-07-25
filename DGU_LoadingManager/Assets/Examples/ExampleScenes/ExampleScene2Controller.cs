
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

using DGU_LoadingManager;



/// <summary>
/// LoadingManager 사용 예제들을 보여주는 스크립트
/// </summary>
public class ExampleScene2Controller 
    : MonoBehaviour
    , SceneCommonInterface
{
    /// <summary>
    /// 프리팹을 생성하여 넣을 그룹
    /// </summary>
    private GameObject TestGroup = null;

    public bool SceneLoadCompleteIs { get; private set; } = false;

    private void Awake()
    {
        this.TestGroup = GameObject.Find("TestGroup").gameObject;

        Button Test1Btn = GameObject.Find("Canvas/Test1Btn").GetComponent<Button>();
        Test1Btn.onClick.AddListener(() => 
        {
            this.Test01();
        });
    }


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


        //진행바 재설정
        LoadingManager.Instance?.CompleteHideLoadingScreenIsSet(true);
        LoadingManager.Instance?.ProgressSet(0.2f, 1f);
        // 이제 리소스 로딩 시작
        List<string> resourceKeys = new List<string>
        {
            "TestPrefabLable" ,//레이블을 이용하여 로드
            "Assets/Examples/Prefab03.prefab",
            "Assets/Examples/Prefab04.prefab",
        };
        LoadingManager.Instance?.LoadMultipleAssets(resourceKeys, OnResourcesLoaded);
    }

    private void OnResourcesLoaded()
    {
        Debug.Log("OnResourcesLoaded resources loaded!");
        // 게임 시작 로직
    }



    private int TestCount = 0;

    /// <summary>
    /// 테스트01
    /// </summary>
    private void Test01()
    {
        for (int i = 0; i < 10; ++i)
        {
            string sPath = string.Empty;

            //어드레서블 경로 만들기
            sPath = string.Format("Assets/Examples/Prefab0{0}.prefab"
                                    , UnityEngine.Random.Range(1, 5));

            GameObject newGO = this.NewInstance(sPath, this.TestGroup.transform);
            PrefabTestInterface newPrefabIns = newGO.GetComponent<PrefabTestInterface>();
            newPrefabIns.TextSet((++TestCount).ToString());

            newGO.transform.position
                = new Vector3(UnityEngine.Random.Range(0f, 5f)
                                , UnityEngine.Random.Range(0f, 5f)
                                , UnityEngine.Random.Range(0f, 5f));
        }
    }

    /// <summary>
    /// 새로운 인스턴스를 만들어 지정된 위치에 추가한다.
    /// </summary>
    /// <param name="sPath"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    /// <exception cref="System.Exception"></exception>
    public GameObject NewInstance(
        string sPath
        , Transform transform)
    {
        //프리팹을 메모리에 로드한 결과
        AsyncOperationHandle<GameObject> asset;

        if (null != transform)
        {
            //asset = Addressables.InstantiateAsync(prefabIns.Prefab, transform);
            asset = Addressables.InstantiateAsync(sPath, transform);
        }
        else
        {
            //asset = Addressables.InstantiateAsync(prefabIns.Prefab);
            asset = Addressables.InstantiateAsync(sPath);
        }


        //로드 시도
        GameObject newTemp = asset.WaitForCompletion();

        if (asset.Status == AsyncOperationStatus.Succeeded)
        {//로드 성공

            //생성이 성공한 개체는 각자 알아서 보관한다.
        }
        else
        {
            throw new System.Exception("프리팹 인스턴스 생성 실패 : " + sPath);
        }

        return newTemp;
    }
}