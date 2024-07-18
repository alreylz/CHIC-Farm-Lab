using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHIC;
using TMPro;
public class PrintLocalAppPath : MonoBehaviour
{
    [SerializeField]
    TextMeshPro deviceAssetText;
    [SerializeField]
    TextMeshPro projAssetText;


    private void Awake()
    {
        deviceAssetText.SetText(" DeviceAssetPath :"+ GlobalConstants.getBaseApplicationPath(SavePathType.DEVICE_ASSET));
        projAssetText.SetText(" ProjeAssetPath :" + GlobalConstants.getBaseApplicationPath(SavePathType.PROJECT_ASSET));
        
    }



}
