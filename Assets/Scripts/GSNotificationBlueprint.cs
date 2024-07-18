using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "GSNotificationData", menuName ="CHIC-Project/GSNotificationData")]
public class GSNotificationBlueprint : ScriptableObject
{
    public string sGSNotifId;
    public string sTextSpa;
    public string sTextEng;
    public Sprite spIcon; // sprite de la carpeta de Sprite    
    public AudioClip aAudio;
    public float fDuration; 

    public GSNotificationBlueprint(string pGSNotifId, string pTextSpa, string pTextEng, Sprite pIcon, AudioClip pAudio, float pDuration)
    {
        sGSNotifId = pGSNotifId;
        sTextSpa   = pTextSpa;
        sTextEng   = pTextEng;
        spIcon     = pIcon;
        aAudio     = pAudio;
        fDuration  = pDuration;
    }

}
