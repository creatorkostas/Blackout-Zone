using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class fpsMeter : MonoBehaviour
{
    private TextMeshProUGUI fpsText;
    // Start is called before the first frame update
    void Start()
    {
        fpsText = this.gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        fpsText.text = ((int)(Time.frameCount / Time.time)).ToString(); 
    }
}
