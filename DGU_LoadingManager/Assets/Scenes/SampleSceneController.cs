
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using DGU_LoadingManager;


/// <summary>
/// LoadingManager 사용 예제들을 보여주는 스크립트
/// </summary>
public class SampleSceneController : MonoBehaviour
{
    [Header("Example Asset References")]
    public string Scene1Path = "GameScene1";
    public string Scene2Path = "GameScene2";

    public string assetKey = "PlayerPrefab";
    public List<string> multipleAssetKeys
        = new List<string>
        {
            "LoadingTest03Lable" ,
            "Assets/LoadingTest03/Prefab03.prefab",
            "Assets/LoadingTest03/Prefab04.prefab",
        };


    [Header("UI Buttons (Optional)")]
    public UnityEngine.UI.Button SceneLoadBtn01;
    public UnityEngine.UI.Button SceneLoadBtn02;
    public UnityEngine.UI.Button AssetLoadBtn;
    public UnityEngine.UI.Button MultipleLoadBtn;
    public UnityEngine.UI.Button CustomLoadingBtn;

    private void Start()
    {
        // 버튼 이벤트 설정
        if (SceneLoadBtn01 != null)
        {
            SceneLoadBtn01.onClick.AddListener(ExampleLoadScene1);
        }
        if (SceneLoadBtn02 != null)
        {
            SceneLoadBtn02.onClick.AddListener(ExampleLoadScene2);
        }
            

        if (AssetLoadBtn != null)
        {
            AssetLoadBtn.onClick.AddListener(ExampleLoadAsset);
        }

        if (MultipleLoadBtn != null)
        {
            MultipleLoadBtn.onClick.AddListener(ExampleLoadMultipleAssets);
        }
            
        if (CustomLoadingBtn != null)
        {
            CustomLoadingBtn.onClick.AddListener(ExampleCustomLoading);
        }
            
    }


    #region 기본 사용 예제

    /// <summary>
    /// 예제 1 - 1: 씬 로딩
    /// <para>씬 단독 로딩</para>
    /// </summary>
    public void ExampleLoadScene1()
    {
        // 정적 메서드 사용
        LoadingManager.LoadSceneStatic(true, this.Scene1Path);

        // 또는 인스턴스 직접 사용
        // if (LoadingManager.Instance != null)
        // {
        //     LoadingManager.Instance.LoadScene(sceneKey);
        // }
    }
    /// <summary>
    /// 예제 1 - 2: 씬 로딩
    /// <para>씬 로딩 이후 씬에서 리소스 로드</para>
    /// </summary>
    public void ExampleLoadScene2()
    {
        // 정적 메서드 사용
        LoadingManager.LoadSceneStatic(
            false
            , this.Scene2Path);
    }



    /// <summary>
    /// 예제 2: 단일 에셋 로딩
    /// </summary>
    public void ExampleLoadAsset()
    {
        LoadingManager.LoadAssetStatic<GameObject>(
            true
            , assetKey
            , (loadedAsset) =>
            {
                if (loadedAsset != null)
                {
                    Debug.Log($"Asset loaded successfully: {loadedAsset.name}");
                    // 로드된 에셋 사용
                    Instantiate(loadedAsset);
                }
            });
    }

    /// <summary>
    /// 예제 3: 여러 에셋 동시 로딩
    /// </summary>
    public void ExampleLoadMultipleAssets()
    {
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadMultipleAssets(
                true
                , multipleAssetKeys
                , () =>
                {
                    Debug.Log("All assets loaded successfully!");
                    // 모든 에셋 로드 완료 후 실행할 코드
                });
        }
    }

    /// <summary>
    /// 예제 4: 커스텀 로딩 작업
    /// </summary>
    public void ExampleCustomLoading()
    {
        //if (LoadingManager.Instance == null) return;


        var customOperations = new List<LoadingOperationDataModel>
        {
            new LoadingOperationDataModel(
                "Loading player data"
                , () => Addressables.LoadAssetAsync<ScriptableObject>("PlayerData")
                , 1f
            ),
            new LoadingOperationDataModel(
                "Loading setting data"
                , () => Addressables.LoadAssetAsync<ScriptableObject>("GameSettings")
                , 0.5f
            ),
            new LoadingOperationDataModel(
                "Loading audio clips"
                , () => Addressables.LoadAssetAsync<AudioClip>("BackgroundMusic")
                , 0.3f
            ),
        };

        LoadingManager.Instance.StartCustomLoading(
            true
            , customOperations
            , () =>
            {
                Debug.Log("Custom loading completed!");
                // 모든 커스텀 작업 완료 후 실행
            });
    }

    /// <summary>
    /// 예제 5: 간단한 시간 기반 로딩
    /// </summary>
    public void ExampleSimpleTimeBasedLoading()
    {
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.ShowLoading(3f, "Initializing...");
        }
    }

    #endregion

    #region 고급 사용 예제

    /// <summary>
    /// 예제 6: 게임 초기화 시퀀스
    /// </summary>
    public void ExampleGameInitializationSequence()
    {
        if (LoadingManager.Instance == null) return;

        var initOperations = new List<LoadingOperationDataModel>
        {
            new LoadingOperationDataModel(
                "Loading the game database"
                , () => Addressables.LoadAssetAsync<ScriptableObject>("GameDatabase")
                , 2f // 가장 큰 비중
            ),
            new LoadingOperationDataModel(
                "Loading UI prefabs"
                , () => Addressables.LoadAssetAsync<GameObject>("MainMenuUI")
                , 1f
            ),
            new LoadingOperationDataModel(
                "Loading sound bank"
                , () => Addressables.LoadAssetAsync<AudioClip>("SoundBank")
                , 1f
            ),
            new LoadingOperationDataModel(
                "Loading localization data"
                , () => Addressables.LoadAssetAsync<ScriptableObject>("LocalizationData")
                , 0.5f
            )
        };

        LoadingManager.Instance.StartCustomLoading(
            true
            , initOperations
            , () =>
            {
                Debug.Log("Game initialization completed!");
                OnGameInitializationComplete();
            });
    }

    /// <summary>
    /// 예제 7: 레벨별 리소스 로딩
    /// </summary>
    public void ExampleLevelResourceLoading(int levelNumber)
    {
        if (LoadingManager.Instance == null) return;

        var levelOperations = new List<LoadingOperationDataModel>
        {
            new LoadingOperationDataModel(
                $"Level {levelNumber} Scene loading"
                , () => Addressables.LoadSceneAsync($"Level{levelNumber}Scene")
                , 3f
            ),
            new LoadingOperationDataModel(
                "Level music loading"
                , () => Addressables.LoadAssetAsync<AudioClip>($"Level{levelNumber}Music")
                , 1f
            ),
            new LoadingOperationDataModel(
                "Level Prefab loading"
                , () => Addressables.LoadAssetAsync<GameObject>($"Level{levelNumber}Enemies")
                , 1f
            )
        };

        LoadingManager.Instance.StartCustomLoading(
            true
            , levelOperations
            , () =>
            {
                Debug.Log($"Level {levelNumber} resources loaded!");
                StartLevel(levelNumber);
            });
    }

    #endregion

    #region 콜백 메서드들

    private void OnGameInitializationComplete()
    {
        // 게임 초기화 완료 후 실행할 로직
        Debug.Log("Starting main menu...");
    }

    private void StartLevel(int levelNumber)
    {
        // 레벨 시작 로직
        Debug.Log($"Starting level {levelNumber}");
    }

    #endregion

    #region 키보드 테스트 (개발용)

    private void Update()
    {
        // 개발 중 테스트를 위한 키보드 단축키
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ExampleLoadScene1();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            ExampleLoadAsset();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            ExampleLoadMultipleAssets();

        if (Input.GetKeyDown(KeyCode.Alpha4))
            ExampleCustomLoading();

        if (Input.GetKeyDown(KeyCode.Alpha5))
            ExampleSimpleTimeBasedLoading();

        if (Input.GetKeyDown(KeyCode.G))
            ExampleGameInitializationSequence();
    }

    #endregion
}