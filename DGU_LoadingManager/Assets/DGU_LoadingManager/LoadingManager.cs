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

        /// <summary>
        /// 코루틴 관리 개체
        /// </summary>
        private Coroutine currentLoadingCoroutine;


        #region 로딩 UI 개체참조
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
        #endregion


        #region 로딩 메니저 세팅
        [Header("Loading Manager Settings")]
        /// <summary>
        /// 최소 로딩 시간(s)
        /// </summary>
        public float MinLoadingTime = 1f;
        /// <summary>
        /// 아이콘 회전 속도
        /// </summary>
        public float IconRotationSpeed = 360f;

        #endregion


        #region 실시간 로딩 옵션
        /// <summary>
        /// 로딩이 끝나면 로딩화면을 끌지 여부
        /// <para>이 값이 false이면 로딩이 끝나도 로딩화면은 유지된다.</para>
        /// </summary>
        /// <remarks>
        /// 이 값이 false이면 수동으로 끄거나(HideLoadingScreen) 
        /// 이값을 true로 바꾸고 추가로 로딩을 진행하면 된다.
        /// <para></para>
        /// </remarks>
        private bool CompleteHideLoadingScreenIs = true;
        /// <summary>
        /// 로딩이 끝나면 로딩화면을 끌지 여부 수정
        /// </summary>
        /// <param name="bSet"></param>
        public void CompleteHideLoadingScreenIsSet(bool bSet)
        {
            this.CompleteHideLoadingScreenIs = bSet;
        }


        /// <summary>
        /// 프로그레스 최소 비율
        /// <para>1 == 100%</para>
        /// </summary>
        /// <remarks>
        /// 진행바가 지정된 %에서 부터 진행되게 하는 기능<br />
        /// 기존 계산된 값은 여기에 설정된 값만큼 비율로 자동으로 처리된다.
        /// </remarks>
        private float ProgressRate_Min = 0f;
        /// <summary>
        /// 프로그레스 최대 비율
        /// <para>1 == 100%</para>
        /// </summary>
        /// <remarks>
        /// 진행바가 100%가 되지못하게 막는 기능이다.<br />
        /// 기존 계산된 값은 여기에 설정된 값만큼 비율로 자동으로 처리된다.
        /// </remarks>
        private float ProgressRate_Max = 1f;
        /// <summary>
        /// 프로그레스 사용 범위(비율) 지정
        /// </summary>
        /// <param name="fMin"></param>
        /// <param name="fMax"></param>
        public void ProgressSet(float fMin, float fMax)
        {
            this.ProgressRate_Min = fMin;
            this.ProgressRate_Max = fMax;
        }
        #endregion


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
                this.AutoAssignUIComponents();
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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        /// <summary>
        /// 스크린 로드가 완료되면 할 동작
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 씬이 로드된 후 로딩 화면 숨기기
            //if (loadingCanvas != null && loadingCanvas.gameObject.activeSelf)
            if (LoadingCanvas != null
                && LoadingCanvas.gameObject.activeSelf
                && !WaitingForMinimumTimeIs)
            {
                StartCoroutine(LoadingAfterDelay_Hide(0.5f));
            }
        }


        #region 리소스 로드 메서드

        /// <summary>
        /// 어드레서블 씬 로드
        /// </summary>
        /// <param name="sSceneKey">어드레서블 주소, 레이블</param>
        /// <param name="mode">씬은 특수한 경우가 아니면 
        /// LoadSceneMode.Single만 사용하도록 구성하는 것이 좋다.</param>
        public void LoadScene(
            string sSceneKey
            , LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (LoadingIs) return;

            var operation = new LoadingOperationDataModel(
                $"Scene loading: {sSceneKey}",
                () => Addressables.LoadSceneAsync(sSceneKey, mode)
            );

            //StartLoading(new List<LoadingOperation> { operation });
            StartLoading(
                new List<LoadingOperationDataModel> { operation }
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
        /// 어드레서블 에셋 단일 로드
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sAssetKey">어드레서블 주소, 레이블</param>
        /// <param name="actionComplete"></param>
        public void LoadAsset<T>(
            string sAssetKey
            , Action<T> actionComplete = null) 
            where T : UnityEngine.Object
        {
            if (LoadingIs) return;

            var operation = new LoadingOperationDataModel(
                $"Asset loading: {sAssetKey}",
                () => Addressables.LoadAssetAsync<T>(sAssetKey)
            );

            StartLoading(
                new List<LoadingOperationDataModel> { operation }
                , () =>
                {
                    var handle = Addressables.LoadAssetAsync<T>(sAssetKey);
                    if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        actionComplete?.Invoke(handle.Result);
                    }
                });
        }

        /// <summary>
        /// 에셋을 여러개 로드
        /// </summary>
        /// <param name="listAssetKey">어드레서블 주소, 레이블 리스트</param>
        /// <param name="actionComplete"></param>
        public void LoadMultipleAssets(
            List<string> listAssetKey
            , Action actionComplete = null)
        {
            if (LoadingIs) return;

            var operations = new List<LoadingOperationDataModel>();

            foreach (string key in listAssetKey)
            {
                operations.Add(new LoadingOperationDataModel(
                    $"Multiple Assets loading: {key}",
                    () => Addressables.LoadAssetAsync<UnityEngine.Object>(key)
                ));
            }

            StartLoading(operations, actionComplete);
        }

        /// <summary>
        /// 커스텀 로딩 작업 실행
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="actionComplete"></param>
        public void StartCustomLoading(
            List<LoadingOperationDataModel> operations
            , Action actionComplete = null)
        {
            if (LoadingIs) return;
            this.StartLoading(operations, actionComplete);
        }

        /// <summary>
        /// 지정된 시간만큼 로딩 화면 표시
        /// </summary>
        /// <param name="fDuration">대기할 시간</param>
        /// <param name="sMessage"></param>
        public void ShowLoading(
            float fDuration
            , string sMessage = "Now loading...")
        {
            if (LoadingIs) return;
            StartCoroutine(ShowLoadingForDuration(fDuration, sMessage));
        }


        /// <summary>
        /// 지정한 시간만큼 로딩과 메시지를 표시한다.
        /// </summary>
        /// <param name="fDuration">대기할 시간(s)</param>
        /// <param name="sMessage"></param>
        /// <returns></returns>
        private IEnumerator ShowLoadingForDuration(
            float fDuration
            , string sMessage)
        {
            LoadingIs = true;
            //시간 기반 로딩도 최소 시간 대기로 취급
            this.WaitingForMinimumTimeIs = true;
            LoadingScreen_Show();
            LoadingTextSet(sMessage);

            float elapsed = 0f;
            while (elapsed < fDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                ProgressUiSet(elapsed / fDuration);
                yield return null;
            }

            ProgressUiSet(1f);
            yield return new WaitForSecondsRealtime(0.2f);

            WaitingForMinimumTimeIs = false;
            LoadingScreen_Hide();
            LoadingIs = false;
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


        #region 실제 동작 관련

        /// <summary>
        /// 로딩 시작
        /// </summary>
        /// <param name="operations">로딩용 데이터 리스트</param>
        /// <param name="onComplete">완료시 동작할 액션</param>
        private void StartLoading(
            List<LoadingOperationDataModel> operations
            , Action onComplete = null)
        {
            if (currentLoadingCoroutine != null)
            {
                StopCoroutine(currentLoadingCoroutine);
            }

            currentLoadingCoroutine
                = StartCoroutine(LoadingCoroutine(operations, onComplete));
        }

        /// <summary>
        /// 로딩 화면을 표시하고 로딩을 진행한 후 로딩 화면을 끈다.
        /// </summary>
        /// <param name="listOperations"></param>
        /// <param name="actionComplete"></param>
        /// <returns></returns>
        private IEnumerator LoadingCoroutine(
            List<LoadingOperationDataModel> listOperations
            , Action actionComplete = null)
        {
            this.LoadingIs = true;
            this.WaitingForMinimumTimeIs = false;
            this.LoadingScreen_Show();

            float startTime = Time.realtimeSinceStartup;
            float totalWeight = 0f;

            // 최소 로딩 시간이 설정되어 있다면 미리 플래그 설정
            if (this.MinLoadingTime > 0)
            {
                this.WaitingForMinimumTimeIs = true;
            }
            else
            {
                this.WaitingForMinimumTimeIs = false;
            }



            // 전체 가중치 계산
            foreach (var op in listOperations)
            {
                totalWeight += op.Weight;
            }

            float currentProgress = 0f;

            // 각 작업 실행
            foreach (var operation in listOperations)
            {
                this.LoadingTextSet(operation.OperationMessage);

                var handle = operation.Operation.Invoke();

                // 작업 진행률 모니터링
                while (!handle.IsDone)
                {
                    float operationProgress = handle.PercentComplete;
                    float weightedProgress = (operationProgress * operation.Weight) / totalWeight;
                    float totalProgress = currentProgress + weightedProgress;

                    this.ProgressUiSet(totalProgress);
                    yield return null;
                }

                // 현재 작업 완료 후 진행률 업데이트
                currentProgress += operation.Weight / totalWeight;
                this.ProgressUiSet(currentProgress);

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
            {//최소 대기 시간이 지나지 않았다.

                //최소 시간 대기 시작
                this.WaitingForMinimumTimeIs = true; 

                //남은 시간 계산
                float remainingTime = MinLoadingTime - elapsedTime;
                //Debug.Log($"Waiting additional {remainingTime:F2} seconds to meet minimum loading time");
                //Debug.Log($"Time.timeScale: {Time.timeScale}");


                //남은 시간 대기
                yield return new WaitForSecondsRealtime(remainingTime);

                //최소 시간 대기 완료
                WaitingForMinimumTimeIs = false;
            }
            else
            {//최소 시간을 이미 넘었다
            }


            //진행 완료
            this.ProgressUiSet(1f);
            this.LoadingTextSet("Loading complete!");

            yield return new WaitForSecondsRealtime(0.5f);

            if (true == this.CompleteHideLoadingScreenIs)
            {//창닫기 허용됨.

                this.LoadingScreen_Hide();
            }


            actionComplete?.Invoke();

            this.LoadingIs = false;
            this.currentLoadingCoroutine = null;
        }

        /// <summary>
        /// 로딩 UI를 표시한다.
        /// </summary>
        private void LoadingScreen_Show()
        {
            if (LoadingCanvas != null)
            {
                this.LoadingUiIs = true;
                LoadingCanvas.gameObject.SetActive(true);
                ProgressUiSet(0f);

                // 로딩 화면 표시 시 현재 씬의 캔버스 설정 적용
                SceneCanvas_SettingsApply();
            }
        }

        /// <summary>
        /// 현재 씬의 캔버스 설정을 로딩 캔버스에 적용
        /// </summary>
        private void SceneCanvas_SettingsApply()
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
                        if (MoreSuitableCanvasCheck(canvas, targetCanvas))
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
        /// <param name="candidate"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        private bool MoreSuitableCanvasCheck(
            Canvas candidate
            , Canvas current)
        {
            // Screen Space Camera 모드를 우선시
            if (candidate.renderMode == RenderMode.ScreenSpaceCamera 
                && current.renderMode != RenderMode.ScreenSpaceCamera)
            {
                return true;
            }
                

            // World Space 모드를 Screen Space Overlay보다 우선시 (단, Screen Space Camera보다는 낮음)
            if (candidate.renderMode == RenderMode.WorldSpace 
                && current.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return true;
            }
                

            // 같은 렌더 모드라면 sorting order가 높은 것을 우선시
            if (candidate.renderMode == current.renderMode)
            {
                return candidate.sortingOrder > current.sortingOrder;
            }
                

            return false;
        }

        /// <summary>
        /// 로딩 스크린 가리기
        /// </summary>
        public void LoadingScreen_Hide()
        {
            if (LoadingCanvas != null)
            {
                this.LoadingUiIs = false;
                LoadingCanvas.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 지정한 시간만큼 대기했다가 로딩 스크린 가리기
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator LoadingAfterDelay_Hide(float delay)
        {
            Debug.Log("HideLoadingAfterDelay : " + delay);
            yield return new WaitForSecondsRealtime(delay);
            LoadingScreen_Hide();
        }

        /// <summary>
        /// 프로그레스 바를 조정하고 프로그레스 텍스트를 출력한다.
        /// </summary>
        /// <param name="fProgressRate"></param>
        private void ProgressUiSet(float fProgressRate)
        {
            //사용할 범위값
            float fRate = this.ProgressRate_Max - this.ProgressRate_Min;

            //사용할 범위값에 맞게 진행 비율 조정
            float fRateValue = fProgressRate * fRate;



            //최소치에 계산된 비율값을 더해 최소치 보다 큰 값이 나오게 해준다.
            float fResult = this.ProgressRate_Min + fRateValue;

            //0.00~1.00사이 값으로 보정
            fResult = Mathf.Clamp01(fResult);



            //바에 표시
            if (this.ProgressBar != null)
            {
                this.ProgressBar.value = fResult;
            }

            //텍스트에 표시
            if (this.PercentageText != null)
            {
                this.PercentageText.text = $"{(fResult * 100):F0}%";
            }
        }

        /// <summary>
        /// 로딩 메시지 수정
        /// </summary>
        /// <param name="text"></param>
        private void LoadingTextSet(string text)
        {
            if (this.LoadingText != null)
            {
                this.LoadingText.text = text;
            }
        }

        #endregion



        #region 편의를 위한 정적 함수

        /// <summary>
        /// 정적 메서드로 씬 로드 (편의성)
        /// </summary>
        /// <param name="sceneKey"></param>
        public static void LoadSceneStatic(
            string sceneKey)
        {
            if (Instance != null)
            {
                Instance.LoadScene(sceneKey);
            }
            else
            {
                Debug.LogWarning("LoadingManager instance not found!");
            }
        }

        /// <summary>
        /// 정적 메서드로 에셋 로드 (편의성)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetKey"></param>
        /// <param name="onComplete"></param>
        public static void LoadAssetStatic<T>(
            string assetKey
            , Action<T> onComplete = null) where T : UnityEngine.Object
        {
            if (Instance != null)
            {
                Instance.LoadAsset(assetKey, onComplete);
            }
            else
            {
                Debug.LogWarning("LoadingManager instance not found!");
            }
        }

        #endregion
    }
}