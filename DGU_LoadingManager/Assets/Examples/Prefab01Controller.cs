using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;



/// <summary>
/// LoadingManager 사용 예제들을 보여주는 스크립트
/// </summary>
public class Prefab01Controller : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    private TextMeshProUGUI MainText = null;

    private void Awake()
    {
        this.MainText = GetComponent<TextMeshProUGUI>();

        // 씬이 완전히 로드된 후 필요한 리소스들을 로딩
        StartCoroutine(LoadSceneResources());
    }

    private void Start()
    {

        
    }

    private IEnumerator LoadSceneResources()
    {
        //임의의 시간을 두어 로드가 되는 것인것 처럼 보이게 한다.
        yield return new WaitForSeconds(3.0f);

        this.MainText.text = "Prefab01 Load Complete!!";
    }
}