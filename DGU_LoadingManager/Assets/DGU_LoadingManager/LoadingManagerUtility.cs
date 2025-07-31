
using UnityEngine;

namespace DGU_LoadingManager
{
    /// <summary>
    /// 로딩 메니저에서 사용하는 전용 유틸
    /// </summary>
    internal class LoadingManagerUtility
    {
        private readonly string GameObjectName = "MainScript";

        internal SceneCommonInterface Find_SceneCommonInterface()
        {
            SceneCommonInterface returnIns = default;

            // 로드된 씬에서 "MainScript"라는 이름의 GameObject를 찾습니다.
            // 모든 씬에 'MainScript'라는 이름의 오브젝트가 있다고 가정합니다.
            GameObject mainScriptObject = GameObject.Find(this.GameObjectName);

            if (mainScriptObject != null)
            {
                // MainScript 오브젝트에 있는 첫 번째 스크립트에서 ILevelInitializer 인터페이스를 찾습니다.
                // GetComponent<ILevelInitializer>()는 해당 오브젝트의 모든 컴포넌트를 순회하여
                // ILevelInitializer를 구현하는 첫 번째 컴포넌트를 반환합니다.
                returnIns = mainScriptObject.GetComponent<SceneCommonInterface>();

                if (null == returnIns)
                {
                    Debug.LogWarning($"ILevelInitializer component not found on {this.GameObjectName} object in the loaded scene.");
                }
            }
            else
            {
                Debug.LogWarning($"{this.GameObjectName} object not found in the loaded scene.");
            }



            return returnIns;
        }
    }
}
