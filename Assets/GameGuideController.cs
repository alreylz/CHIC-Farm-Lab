using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGuideController : MonoBehaviour
{
    public Animator anim;
    
    private int animation_StateInit = Animator.StringToHash("Base Layer.Idle | Init");
    private int animation_StateTerrain = Animator.StringToHash("Base Layer.Idle | Terrain");
    private int animation_Planted = Animator.StringToHash("Base Layer.Idle | Planted");

    // Start is called before the first frame update
    void Start()
    {
        if (anim == null) anim = transform.GetComponent<Animator>();



        anim.SetBool("NewGame", true);

        RealWorldOverlay.Instance.OnScanStart += OnScanStart;
        RealWorldOverlay.Instance.OnScanEnd += OnScanEnd;
        

        RealWorldOverlay.Instance.OnOverlaidItemFirst += OnTerrainPlanted;
        RealWorldOverlay.Instance.OnPlantAdded += OnPlantAddedLocal;

    }

    private void OnScanEnd()
    {
        anim.SetBool("Scanning", false);
        //anim.SetBool("ScannedAtLeastOnce", true);
        anim.SetTrigger("ScannedOnce");
    }
    private void OnScanStart()
    {
        anim.SetBool("Scanning", true);
    }
    
    void OnTerrainPlanted()
    {
        anim.SetTrigger("TerrainPlanted");
    }
    void OnPlantAddedLocal()
    {
        anim.SetTrigger("PlantAdded");
    }


    IEnumerator checkStatusForHelp()
    {
        while (true)
        {
            int status = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
            if(status == animation_StateInit)
            {

            }
            else if (status == animation_StateTerrain)
            {
                
            }
            else if (status == animation_Planted)
            {

            }
            else
            {

            }

            }
        
            yield return new WaitForSecondsRealtime(4f);
        }

    



}
