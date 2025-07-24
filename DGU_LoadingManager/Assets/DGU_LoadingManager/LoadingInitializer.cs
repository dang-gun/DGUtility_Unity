using UnityEngine;
using UnityEngine.AddressableAssets;


namespace DGU_LoadingManager
{
    /// <summary>
    /// 게임 시작 시 LoadingManager를 초기화하는 스크립트<br />
    /// 첫 번째 씬에 배치하거나 별도의 초기화 씬에서 사용
    /// </summary>
    public class LoadingInitializer : MonoBehaviour
    {
        [Header("Loading Prefab Settings")]
        /// <summary>
        /// 로딩 프리팹 개체
        /// </summary>
        [SerializeField] private AssetReference LoadingPrefabReference;


        private void Start()
        {
            InitializeLoadingSystem();
        }

        /// <summary>
        /// 로딩 시스템 초기화
        /// </summary>
        public async void InitializeLoadingSystem()
        {
            // 이미 LoadingManager가 존재하는지 확인
            if (LoadingManager.Instance != null)
            {
                Debug.Log("LoadingManager already exists.");
                return;
            }

            try
            {
                // DontDestroyOnLoad 캔버스 생성 (선택사항)
                this.CreateDontDestroyCanvas();

                // 어드레서블로 로딩 프리팹 로드
                if (LoadingPrefabReference != null && LoadingPrefabReference.RuntimeKeyIsValid())
                {
                    var handle = Addressables.InstantiateAsync(LoadingPrefabReference);
                    var loadingPrefab = await handle.Task;

                    if (loadingPrefab != null)
                    {
                        // LoadingManager 컴포넌트 확인
                        LoadingManager loadingManager = loadingPrefab.GetComponent<LoadingManager>();
                        if (loadingManager == null)
                        {
                            Debug.LogError("LoadingManager component not found on the instantiated prefab!");
                            Destroy(loadingPrefab);
                            return;
                        }

                        Debug.Log("Loading system initialized successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to instantiate loading prefab.");
                    }
                }
                else
                {
                    Debug.LogError("Loading prefab reference is not set or invalid.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize loading system: {e.Message}");
            }
        }

        /// <summary>
        /// DontDestroyOnLoad UI 캔버스 생성
        /// <para>이미 있다면 생성되지 않음</para>
        /// </summary>
        private void CreateDontDestroyCanvas()
        {
            // 기존에 DontDestroyOnLoad UI Canvas가 있는지 확인
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas1 in canvases)
            {
                if (canvas1.gameObject.scene.name == "DontDestroyOnLoad" &&
                    canvas1.name.Contains("DontDestroyUI"))
                {
                    return; // 이미 존재함
                }
            }

            // DontDestroyOnLoad UI Canvas 생성
            GameObject uiCanvasObj = new GameObject("DontDestroyUICanvas");
            Canvas canvas = uiCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // 높은 순서로 설정하여 다른 UI 위에 표시

            // CanvasScaler 추가
            var canvasScaler = uiCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            // GraphicRaycaster 추가
            uiCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            DontDestroyOnLoad(uiCanvasObj);

            Debug.Log("DontDestroyOnLoad UI Canvas created.");
        }
    }

}