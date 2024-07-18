using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNAController : MonoBehaviour
{


    //- Hacer desaparecer automáticamente los elementos de modificación si no están activos
    //- Make disposed object disappear as the animation ends.
    //- Handle onTriggerEnter con crispr para cambio de estado a introduce mod.
    //- Apply changes to elements in the shop/ scriptable objects, etc.


    public Animator Animator_DNA;

    //Animator Parameters
    private int usedCRISPR_Hash =  Animator.StringToHash("UsedProtein");
    private int editedChainType_Hash = Animator.StringToHash("EditedChainType");
    private int animationFinished_Hash = Animator.StringToHash("AnimationFinished");

    private int animation_State_PendingDNAEdit_Hash = Animator.StringToHash("Base Layer.Idle | Pending Replacement");



    private void Awake()
    {
        Animator_DNA = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.name);
        //Es CRISPR ==> Cadena se desecha; mostrar animación
        if (other.CompareTag("CRISPR"))
        {
            Animator_DNA.SetTrigger(usedCRISPR_Hash);
            Debug.Log("CADENA Eliminada con CRISPR: ");

            other.gameObject.SetActive(false);
            //NOTIFY USER

        }
        //Es CHAIN y ya hemos usado la CRISPR ==> Miramos qué tipo de cadena es y mostramos animación de reemplazo
        else if (Animator_DNA.GetCurrentAnimatorStateInfo(0).fullPathHash == animation_State_PendingDNAEdit_Hash && other.CompareTag("MOD-CHAIN") )
        {
            //Obtener el tipo de cadena a añadir
            int modChainType = other.gameObject.GetComponent<ModChainController>().ChainType;
            Animator_DNA.SetInteger(editedChainType_Hash, modChainType);
            Debug.Log("CADENA CAMBIADA; tipo: " + modChainType.ToString());
            //Desactivo la cadena que he usado (porque ya sale en la animación)

            other.gameObject.SetActive(false);
        }


    }







}
