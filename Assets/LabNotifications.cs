using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LabNotifications : MonoBehaviour
{

    public AudioSource guideAudioSrc;
    public Animator anim;


    public TextMeshPro labTitleText;
    public TextMeshPro labBodyText;
    public Image labPanelImg;


 


    Dictionary<string, GeneNotificationBlueprint> geneNotificationsDict;


    // Start is called before the first frame update

    private void Awake()
    {
        geneNotificationsDict = new Dictionary<string, GeneNotificationBlueprint>();
        geneNotificationsDict.Add("Bienvenida", LoadScriptableObject("BluePrints/LabNotifications/Bienvenida") as GeneNotificationBlueprint);
        geneNotificationsDict.Add("CrisprIntro", LoadScriptableObject("BluePrints/LabNotifications/CrisprIntro") as GeneNotificationBlueprint);
        geneNotificationsDict.Add("ModsIntro", LoadScriptableObject("BluePrints/LabNotifications/ModsIntro") as GeneNotificationBlueprint);
        geneNotificationsDict.Add("UsrAct1-CRISPR", LoadScriptableObject("BluePrints/LabNotifications/UsrAct1-CRISPR") as GeneNotificationBlueprint);
        geneNotificationsDict.Add("UsrAct2-CRISPR", LoadScriptableObject("BluePrints/LabNotifications/UsrAct2-CRISPR") as GeneNotificationBlueprint);
        geneNotificationsDict.Add("MiniGameCompleted", LoadScriptableObject("BluePrints/LabNotifications/MiniGameCompleted") as GeneNotificationBlueprint);


    }

    void Start()
    {
        StartCoroutine(LifeCyclePresentation());
    }



    public void ShowLabGuideNotifications(string helpId)
    {

        GeneNotificationBlueprint notification;
        if (!geneNotificationsDict.TryGetValue(helpId, out notification)) { Debug.LogError("FUCK! No hay ninguna notificación cargada on el id " + helpId); return; }

            guideAudioSrc.clip = notification.aAudio;
            labTitleText.SetText(notification.sTitleTextSpa);
            labBodyText.SetText(notification.sBodyTextSpa);
            labPanelImg.sprite = notification.spIcon;
            guideAudioSrc.Play();
        

    }

    //MAKE RE-PHRASING, que permita repetir las cosas


    public IEnumerator LifeCyclePresentation()
    {
        Debug.Log("ESPERANDO" + (getNotificationLengthById("Bienvenida") + 2f) + "Segundos");
        yield return new WaitForSecondsRealtime(getNotificationLengthById("Bienvenida") + 2f);
        anim.SetTrigger("PresentationDone");
        //Aparece Crispr
        yield return new WaitForSecondsRealtime(getNotificationLengthById("CrisprIntro") + 3f);
        anim.SetTrigger("CrisprIntroDone");
        //Aparecen Modificaciones Posibles
        yield return new WaitForSecondsRealtime(getNotificationLengthById("ModsIntro") + 3f);
        anim.SetTrigger("ModsIntroDone");
    }







public float getNotificationLengthById(string helpId)
    {
        GeneNotificationBlueprint notification;
        if (!geneNotificationsDict.TryGetValue(helpId, out notification)) { Debug.LogError("FUCK! No hay ninguna notificación cargada on el id " + helpId); return -1f; }
        return notification.aAudio.length;
    }


    public ScriptableObject LoadScriptableObject(string path)
    {
            return  Resources.Load<ScriptableObject>(path);
    }

}
