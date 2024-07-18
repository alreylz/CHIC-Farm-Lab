using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "LabNotificationData", menuName = "CHIC-Project/LabNotificationData")]
public class GeneNotificationBlueprint : ScriptableObject
{
    public string sGSNotifId;
    public string sTitleTextSpa;
    public string sTitleTextEng;
    public string sBodyTextSpa;
    public string sBodyTextEng;
    public Sprite spIcon;
    public AudioClip aAudio;

}
