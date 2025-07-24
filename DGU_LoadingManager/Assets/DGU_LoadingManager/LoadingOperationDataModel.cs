
using System;

using UnityEngine.ResourceManagement.AsyncOperations;


namespace DGU_LoadingManager
{
    /// <summary>
    /// 로딩 작업을 정의하는 클래스
    /// </summary>
    [System.Serializable]
    public class LoadingOperationDataModel
    {
        /// <summary>
        /// 화면에 출력될 메시지
        /// </summary>
        public string OperationMessage;

        /// <summary>
        /// 어드레서블의 비동기 작업
        /// </summary>
        public Func<AsyncOperationHandle> Operation;

        /// <summary>
        /// 전체 진행률에서 차지하는 비중
        /// </summary>
        public float Weight = 1f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">출력할 메시지</param>
        /// <param name="op">어드레서블의 비동기 작업<br />
        /// Addressables.LoadSceneAsync</param>
        /// <param name="weight">전체 진행률에서 차지하는 비중</param>
        public LoadingOperationDataModel(
            string name
            , Func<AsyncOperationHandle> op
            , float weight = 1f)
        {
            this.OperationMessage = name;
            this.Operation = op;
            this.Weight = weight;
        }
    }
}