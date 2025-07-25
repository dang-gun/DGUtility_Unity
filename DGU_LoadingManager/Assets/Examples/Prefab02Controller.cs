using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;



/// <summary>
/// 프리팹2
/// </summary>
public class Prefab02Controller : MonoBehaviour, PrefabTestInterface
{
    /// <summary>
    /// 텍스트를 출력할 개체
    /// </summary>
    private TextMeshProUGUI MainText = null;

    private void Awake()
    {
        this.MainText = GetComponent<TextMeshProUGUI>();

    }

    private void Start()
    {

        this.MainText.text = "Prefab02 Load Complete!!";
    }

    /// <summary>
    /// 텍스트 출력
    /// </summary>
    /// <param name="sMsg">출력할 메시지</param>
    public void TextSet(string sMsg)
    {
        this.MainText.text = "Prefab02 : " + sMsg;
    }
}