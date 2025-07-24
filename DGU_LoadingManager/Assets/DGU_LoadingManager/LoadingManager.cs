using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;


namespace DGU_LoadingManager
{
    public class LoadingManager : MonoBehaviour
    {
        /// <summary>
        /// 싱글톤 인스턴스
        /// </summary>
        public static LoadingManager Instance { get; private set; }


        [Header("Loading UI References")]

        /// <summary>
        /// 로딩 프리팹의 캔버스
        /// </summary>
        public Canvas LoadingCanvas;
        /// <summary>
        /// 로딩 이미지
        /// </summary>
        public Image LoadingIcon;
        /// <summary>
        /// 로딩 메시지
        /// </summary>
        public TextMeshProUGUI LoadingText;
        /// <summary>
        /// 진행 바
        /// </summary>
        public Slider ProgressBar;
        /// <summary>
        /// 진행율 텍스트
        /// </summary>
        public TextMeshProUGUI PercentageText;



        [Header("Loading Settings")]
        /// <summary>
        /// 최소 로딩 시간(s)
        /// </summary>
        public float MinLoadingTime = 1f;
        /// <summary>
        /// 아이콘 회전 속도
        /// </summary>
        public float IconRotationSpeed = 360f;


        /// <summary>
        /// 로딩이 진행중인지 여부
        /// </summary>
        /// <remarks>
        /// UI와 매칭되지 않는다.(UI가 표시되도 로딩은 끝날 수 있다.)<br />
        /// bHideLoadingScreen에 영향 받지 않음
        /// </remarks>
        public bool LoadingIs { get; private set; } = false;
        /// <summary>
        /// 로딩UI가 열려있는지 여부
        /// </summary>
        public bool LoadingUiIs { get; private set; } = false;


        /// <summary>
        /// 최소 시간 대기 중인지 확인
        /// </summary>
        private bool WaitingForMinimumTimeIs = false;

        /// <summary>
        /// 코루틴 관리 개체
        /// </summary>
        private Coroutine currentLoadingCoroutine;


        private void Awake()
        {
            // 싱글톤 패턴 구현
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // 씬 로드 이벤트 등록
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 시작 시 로딩 화면 비활성화
            if (LoadingCanvas != null)
            {
                LoadingCanvas.gameObject.SetActive(false);

                // 자동으로 UI 컴포넌트들 찾기 (할당되지 않은 경우)
                AutoAssignUIComponents();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 씬이 로드된 후 로딩 화면 숨기기
            //if (loadingCanvas != null && loadingCanvas.gameObject.activeSelf)
            if (LoadingCanvas != null
                && LoadingCanvas.gameObject.activeSelf
                && !WaitingForMinimumTimeIs)
            {
                StartCoroutine(HideLoadingAfterDelay(0.5f));
            }
        }

        #region Public Methods

        /// <summary>
        /// 어드레서블 씬 로드
        /// </summary>
        /// <param name="bHideLoadingScreen">로딩을 끌지 여부
        /// <para>이 값이 false이면 연계되어 리소스를 로드하거나 수동으로 꺼야 한다.</para>
        /// </param>
        /// <param name="sceneKey"></param>
        /// <param name="mode">씬은 특수한 경우가 아니면 
        /// LoadSceneMode.Single만 사용하도록 구성하는 것이 좋다.</param>
        public void LoadScene(
            bool bHideLoadingScreen
            , string sceneKey
            , LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (LoadingIs) return;

            var operation = new LoadingOperationDataModel(
                $"Scene loading: {sceneKey}",
                () => Addressables.LoadSceneAsync(sceneKey, mode)
            );

            //StartLoading(new List<LoadingOperation> { operation });
            StartLoading(
                bHideLoadingScreen
                , new List<LoadingOperationDataModel> { operation }
                , () =>
                { //씬 로드 완료
                    this.LoadingIs = false;
                    Debug.Log("LoadScene : ");

                    // 로드된 씬에서 "MainScript"라는 이름의 GameObject를 찾습니다.
                    // 모든 씬에 'MainScript'라는 이름의 오브젝트가 있다고 가정합니다.
                    GameObject mainScriptObject = GameObject.Find("MainScript");

                    if (mainScriptObject != null)
                    {
                        // MainScript 오브젝트에 있는 첫 번째 스크립트에서 ILevelInitializer 인터페이스를 찾습니다.
                        // GetComponent<ILevelInitializer>()는 해당 오브젝트의 모든 컴포넌트를 순회하여
                        // ILevelInitializer를 구현하는 첫 번째 컴포넌트를 반환합니다.
                        SceneCommonInterface levelInitializer
                            = mainScriptObject.GetComponent<SceneCommonInterface>();

                        if (levelInitializer != null)
                        {
                            //SceneLoadComplete 함수 호출
                            levelInitializer.SceneLoadComplete();
                        }
                        else
                        {
                            Debug.LogWarning("ILevelInitializer component not found on MainScript object in the loaded scene.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("MainScript object not found in the loaded scene.");
                    }
                });
        }

        /// <summary>
        /// 어드레서블 에셋 로드
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bHideLoadingScreen">로딩을 끌지 여부
        /// <para>이 값이 false이면 연계되어 리소스를 로드하거나 수동으로 꺼야 한다.</para>
        /// </param>
        /// <param name="assetKey"></param>
        /// <param name="onComplete"></param>
        public void LoadAsset<T>(
            bool bHideLoadingScreen
            , string assetKey
            , Action<T> onComplete = null) where T : UnityEngine.Object
        {
            if (LoadingIs) return;

            var operation = new LoadingOperationDataModel(
                $"Asset loading: {assetKey}",
                () => Addressables.LoadAssetAsync<T>(assetKey)
            );

            StartLoading(bHideLoadingScreen
                , new List<LoadingOperationDataModel> { operation }
                , () =>
                {
                    var handle = Addressables.LoadAssetAsync<T>(assetKey);
                    if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        onComplete?.Invoke(handle.Result);
                    }
                });
        }

        /// <summary>
        /// 여러 에셋을 동시에 로드
        /// </summary>
        /// <param name="bHideLoadingScreen">로딩을 끌지 여부
        /// <para>이 값이 false이면 연계되어 리소스를 로드하거나 수동으로 꺼야 한다.</para>
        /// </param>
        /// <param name="assetKeys"></param>
        /// <param name="onComplete"></param>
        public void LoadMultipleAssets(
            bool bHideLoadingScreen
            , List<string> assetKeys
            , Action onComplete = null)
        {
            if (LoadingIs) return;

            var operations = new List<LoadingOperationDataModel>();

            foreach (string key in assetKeys)
            {
                operations.Add(new LoadingOperationDataModel(
                    $"Multiple Assets loading: {key}",
                    () => Addressables.LoadAssetAsync<UnityEngine.Object>(key)
                ));
            }

            StartLoading(bHideLoadingScreen, operations, onComplete);
        }

        /// <summary>
        /// 커스텀 로딩 작업 실행
        /// </summary>
        /// <param name="bHideLoadingScreen">로딩을 끌지 여부
        /// <para>이 값이 false이면 연계되어 리소스를 로드하거나 수동으로 꺼야 한다.</para>
        /// </param>
        /// <param name="operations"></param>
        /// <param name="onComplete"></param>
        public void StartCustomLoading(
            bool bHideLoadingScreen
            , List<LoadingOperationDataModel> operations
            , Action onComplete = null)
        {
            if (LoadingIs) return;
            StartLoading(bHideLoadingScreen, operations, onComplete);
        }

        /// <summary>
        /// 간단한 로딩 화면 표시 (시간 기반)
        /// </summary>
        public void ShowLoading(float duration, string message = "Now loading...")
        {
            if (LoadingIs) return;
            StartCoroutine(ShowLoadingForDuration(duration, message));
        }

        #endregion

        #region Auto Assignment Methods

        /// <summary>
        /// UI 컴포넌트들을 자동으로 찾아서 할당
        /// </summary>
        private void AutoAssignUIComponents()
        {
            if (LoadingCanvas == null) return;

            // ProgressBar 자동 찾기
            if (ProgressBar == null)
            {
                ProgressBar = LoadingCanvas.GetComponentInChildren<Slider>();
            }

            // Text 컴포넌트들 자동 찾기
            if (LoadingText == null || PercentageText == null)
            {
                TextMeshProUGUI[] texts = LoadingCanvas.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.name.ToLower().Contains("loading") && LoadingText == null)
                    {
                        LoadingText = text;
                    }
                    else if (text.name.ToLower().Contains("percent") && PercentageText == null)
                    {
                        PercentageText = text;
                    }
                }
            }

            // LoadingIcon 자동 찾기
            if (LoadingIcon == null)
            {
                Image[] images = LoadingCanvas.GetComponentsInChildren<Image>();
                foreach (Image image in images)
                {
                    if (image.name.ToLower().Contains("icon") || image.name.ToLower().Contains("loading"))
                    {
                        LoadingIcon = image;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bHideLoadingScreen">로딩을 끌지 여부
        /// <para>이 값이 false이면 연계되어 리소스를 로드하거나 수동으로 꺼야 한다.</para>
        /// </param>
        /// <param name="operations"></param>
        /// <param name="onComplete"></param>
        private void StartLoading(
            bool bHideLoadingScreen
            , List<LoadingOperationDataModel> operations
            , Action onComplete = null)
        {
            if (currentLoadingCoroutine != null)
            {
                StopCoroutine(currentLoadingCoroutine);
            }

            currentLoadingCoroutine
                = StartCoroutine(LoadingCoroutine(bHideLoadingScreen
                                                    , operations
                                                    , onComplete));
        }

        /// <summary>
        /// 로딩 화면을 표시하고 로딩을 진행한 후 로딩 화면을 끈다.
        /// </summary>
        /// <param name="bHideLoadingScreen">로딩을 끌지 여부
        /// <para>이 값이 false이면 연계되어 리소스를 로드하거나 수동으로 꺼야 한다.</para>
        /// </param>
        /// <param name="operations"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        private IEnumerator LoadingCoroutine(
            bool bHideLoadingScreen
            , List<LoadingOperationDataModel> operations
            , Action onComplete = null)
        {
            LoadingIs = true;
            WaitingForMinimumTimeIs = false;
            ShowLoadingScreen();

            float startTime = Time.realtimeSinceStartup;
            float totalWeight = 0f;

            // 최소 로딩 시간이 설정되어 있다면 미리 플래그 설정
            if (MinLoadingTime > 0)
            {
                WaitingForMinimumTimeIs = true;
            }
            else
            {
                WaitingForMinimumTimeIs = false;
            }



            // 전체 가중치 계산
            foreach (var op in operations)
            {
                totalWeight += op.Weight;
            }

            float currentProgress = 0f;

            // 각 작업 실행
            foreach (var operation in operations)
            {
                UpdateLoadingText(operation.OperationMessage);

                var handle = operation.Operation.Invoke();

                // 작업 진행률 모니터링
                while (!handle.IsDone)
                {
                    float operationProgress = handle.PercentComplete;
                    float weightedProgress = (operationProgress * operation.Weight) / totalWeight;
                    float totalProgress = currentProgress + weightedProgress;

                    UpdateProgress(totalProgress);
                    yield return null;
                }

                // 현재 작업 완료 후 진행률 업데이트
                currentProgress += operation.Weight / totalWeight;
                UpdateProgress(currentProgress);

                // 에러 체크
                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError($"Loading failed: {operation.OperationMessage}");
                }

                yield return new WaitForEndOfFrame();
            }

            // 최소 로딩 시간 보장
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime < MinLoadingTime)
            {
                WaitingForMinimumTimeIs = true; // 최소 시간 대기 시작

                float remainingTime = MinLoadingTime - elapsedTime;
                //Debug.Log($"Waiting additional {remainingTime:F2} seconds to meet minimum loading time");
                //Debug.Log($"Time.timeScale: {Time.timeScale}");


                yield return new WaitForSecondsRealtime(remainingTime);


                // 남은 시간 동안 "로딩 완료" 메시지와 함께 대기
                UpdateLoadingText("Loading complete...");
                UpdateProgress(1f);

                WaitingForMinimumTimeIs = false; // 최소 시간 대기 완료
            }
            else
            {
                // 최소 시간을 이미 넘었다면 짧은 완료 표시만
                UpdateProgress(1f);
                UpdateLoadingText("Loading complete!");
                yield return new WaitForSecondsRealtime(0.5f);
            }

            UpdateProgress(1f);
            UpdateLoadingText("Loading complete!");

            yield return new WaitForSecondsRealtime(0.5f);

            if (true == bHideLoadingScreen)
            {//창닫기 허용됨.

                HideLoadingScreen();
            }

            onComplete?.Invoke();

            LoadingIs = false;
            currentLoadingCoroutine = null;
        }

        private IEnumerator ShowLoadingForDuration(float duration, string message)
        {
            LoadingIs = true;
            WaitingForMinimumTimeIs = true; // 시간 기반 로딩도 최소 시간 대기로 취급
            ShowLoadingScreen();
            UpdateLoadingText(message);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                UpdateProgress(elapsed / duration);
                yield return null;
            }

            UpdateProgress(1f);
            yield return new WaitForSecondsRealtime(0.2f);

            WaitingForMinimumTimeIs = false;
            HideLoadingScreen();
            LoadingIs = false;
        }

        private void ShowLoadingScreen()
        {
            if (LoadingCanvas != null)
            {
                this.LoadingUiIs = true;
                LoadingCanvas.gameObject.SetActive(true);
                UpdateProgress(0f);

                // 로딩 화면 표시 시 현재 씬의 캔버스 설정 적용
                ApplySceneCanvasSettings();
            }
        }

        /// <summary>
        /// 현재 씬의 캔버스 설정을 로딩 캔버스에 적용
        /// </summary>
        private void ApplySceneCanvasSettings()
        {
            if (LoadingCanvas == null) return;



            // 현재 활성화된 씬의 모든 캔버스를 찾기
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Canvas targetCanvas = null;

            // DontDestroyOnLoad가 아닌 캔버스 중에서 찾기
            foreach (Canvas canvas in allCanvases)
            {
                // DontDestroyOnLoad 씬의 오브젝트는 제외
                if (canvas.gameObject.scene.name != "DontDestroyOnLoad" && canvas != LoadingCanvas)
                {
                    // 우선순위: Screen Space Camera > World Space > Screen Space Overlay
                    if (targetCanvas == null)
                    {
                        targetCanvas = canvas;
                    }
                    else
                    {
                        // 더 적절한 캔버스 선택 로직
                        if (IsMoreSuitableCanvas(canvas, targetCanvas))
                        {
                            targetCanvas = canvas;
                        }
                    }
                }
            }



            if(null != targetCanvas)
            {//캔버스 개체가 있다.

                //값을 다시 설정
                LoadingCanvas.renderMode = targetCanvas.renderMode;


                switch (targetCanvas.renderMode)
                {
                    case RenderMode.ScreenSpaceCamera:
                        LoadingCanvas.worldCamera = targetCanvas.worldCamera ?? Camera.main;
                        LoadingCanvas.planeDistance = targetCanvas.planeDistance;
                        break;

                    case RenderMode.WorldSpace:
                        LoadingCanvas.worldCamera = targetCanvas.worldCamera ?? Camera.main;
                        break;

                    case RenderMode.ScreenSpaceOverlay:
                        // Overlay 모드에서는 카메라가 필요 없음
                        break;
                }

                // Sorting Layer 설정
                LoadingCanvas.sortingLayerID = targetCanvas.sortingLayerID;
                // 로딩 화면은 다른 UI보다 위에 표시되어야 하므로 sorting order를 높게 설정
                LoadingCanvas.sortingOrder = targetCanvas.sortingOrder + 1000;
            }
            
        }

        /// <summary>
        /// 더 적절한 캔버스인지 판단
        /// </summary>
        private bool IsMoreSuitableCanvas(Canvas candidate, Canvas current)
        {
            // Screen Space Camera 모드를 우선시
            if (candidate.renderMode == RenderMode.ScreenSpaceCamera && current.renderMode != RenderMode.ScreenSpaceCamera)
                return true;

            // World Space 모드를 Screen Space Overlay보다 우선시 (단, Screen Space Camera보다는 낮음)
            if (candidate.renderMode == RenderMode.WorldSpace && current.renderMode == RenderMode.ScreenSpaceOverlay)
                return true;

            // 같은 렌더 모드라면 sorting order가 높은 것을 우선시
            if (candidate.renderMode == current.renderMode)
                return candidate.sortingOrder > current.sortingOrder;

            return false;
        }

        /// <summary>
        /// 로딩 스크린 가리기
        /// </summary>
        public void HideLoadingScreen()
        {
            if (LoadingCanvas != null)
            {
                this.LoadingUiIs = false;
                LoadingCanvas.gameObject.SetActive(false);
            }
        }

        private IEnumerator HideLoadingAfterDelay(float delay)
        {
            Debug.Log("HideLoadingAfterDelay : " + delay);
            yield return new WaitForSecondsRealtime(delay);
            HideLoadingScreen();
        }

        private void UpdateProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (ProgressBar != null)
            {
                ProgressBar.value = progress;
            }

            if (PercentageText != null)
            {
                PercentageText.text = $"{(progress * 100):F0}%";
            }
        }

        private void UpdateLoadingText(string text)
        {
            if (LoadingText != null)
            {
                LoadingText.text = text;
            }
        }

        private void Update()
        {
            // 로딩 아이콘 회전 애니메이션
            if (LoadingIs && LoadingIcon != null)
            {
                LoadingIcon.transform.Rotate(0, 0, -IconRotationSpeed * Time.unscaledDeltaTime);
            }
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// 정적 메서드로 씬 로드 (편의성)
        /// </summary>
        /// <param name="bHideLoadingScreen">로딩을 끌지 여부
        /// <para>이 값이 false이면 연계되어 리소스를 로드하거나 수동으로 꺼야 한다.</para>
        /// </param>
        /// <param name="sceneKey"></param>
        public static void LoadSceneStatic(
            bool bHideLoadingScreen
            , string sceneKey)
        {
            if (Instance != null)
            {
                Instance.LoadScene(bHideLoadingScreen, sceneKey);
            }
            else
            {
                Debug.LogWarning("LoadingManager instance not found!");
            }
        }

        /// <summary>
        /// 정적 메서드로 에셋 로드 (편의성)
        /// </summary>
        public static void LoadAssetStatic<T>(
            bool bHideLoadingScreen
            , string assetKey
            , Action<T> onComplete = null) where T : UnityEngine.Object
        {
            if (Instance != null)
            {
                Instance.LoadAsset(bHideLoadingScreen, assetKey, onComplete);
            }
            else
            {
                Debug.LogWarning("LoadingManager instance not found!");
            }
        }

        #endregion
    }
}