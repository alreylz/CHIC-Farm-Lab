using UnityEngine;
[CreateAssetMenu(fileName = "LabNotificationData", menuName = "CHIC-FarmLab/Lab/LabNotificationData", order = 1)]
public class SO_LabStepNotifications : ScriptableObject
{
    [SerializeField]
    private Sprite _UIBackground;
    public Sprite UIBackground { get => _UIBackground; }
    

    [SerializeField]
    private Sprite[] _UIItems;
    public Sprite[] UIItems { get => _UIItems; }
    
    [SerializeField]
    private AudioClip _NotificationClip;
    public AudioClip NotificationClip { get => _NotificationClip; }


}
