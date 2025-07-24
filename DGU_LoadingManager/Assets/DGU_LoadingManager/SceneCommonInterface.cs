
namespace DGU_LoadingManager
{
    /// <summary>
    /// 모든 씬이 구현해야할 인터페이스
    /// </summary>
    internal interface SceneCommonInterface
    {

        /// <summary>
        /// 씬이 로드가 끝났는지 여부
        /// </summary>
        /// <remarks>
        /// SceneLoadComplete()에서 true로 바꿔 준다.
        /// </remarks>
        public bool SceneLoadCompleteIs { get; }

        /// <summary>
        /// 씬 로드가 완료됨
        /// </summary>
        public void SceneLoadComplete();
    }

}