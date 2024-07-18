using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DNAChainStatusMonitor : MonoBehaviour
{
    [SerializeField]
    private bool _LogsOn;
    public bool LogsOn { get => _LogsOn; set => _LogsOn=true; }

    public Animator DNAStateMachine; // Máquina de estados que se encarga de gestionar las animaciones asociadas a la cadena de ADN a modificar.

    public Action OnCrisprUsed; // Evento ejecutado al detectarse el uso de la CRISPR
    public Action<int> OnAppliedModification; //Evento que notifica de la selección de un tipo de modificación
    
    //Animator Parameters --> Identificadores de Variables asociadas a las transiciones en la máquina de estados.
    private int usedCRISPR_Hash = Animator.StringToHash("UsedCrispr"); // --> Indica si se ha usado la Crispr para cortar
    private int editedChainType_Hash = Animator.StringToHash("SelectedModIndex"); // --> Indica la cadena escogida para la modificación.
 

    private void Awake()
    {
        if( DNAStateMachine == null)
            DNAStateMachine = GetComponent<Animator>();
       
    }
    
    //Detectamos la interacción del usuario con la cadena de ADN
    private void OnTriggerEnter(Collider other)
    {
        
        //Uso de CRISPR
        //Es CRISPR ==> Cadena se desecha; mostrar animación
        if (DNAStateMachine.GetCurrentAnimatorStateInfo(0).IsName("Idle | Not replaced") && other.CompareTag("CRISPR"))
        {
            DNAStateMachine.SetTrigger(usedCRISPR_Hash);
            if (LogsOn)
                Debug.Log("CRISPR CUT PERFORMED SUCCESSFULLY");
            other.gameObject.SetActive(false);
            OnCrisprUsed?.Invoke();
        }
        //Es CHAIN (Cadena de modificación) y ya hemos usado la CRISPR ==> Miramos qué tipo de cadena es y mostramos animación de reemplazo
        else if (DNAStateMachine.GetCurrentAnimatorStateInfo(0).IsName("Idle | Pending Replacement") && other.CompareTag("MOD-CHAIN"))
        {
            //Obtener el tipo de cadena a añadir
            int modChainType = other.gameObject.GetComponent<ModChainController>().ChainType;
            DNAStateMachine.SetInteger(editedChainType_Hash, modChainType);
            if (LogsOn)
                Debug.Log("MODIFICATION WITH INDEX "+modChainType+" SUCCESSFULLY PERFORMED");
            OnAppliedModification?.Invoke(modChainType);
            //Desactivo la cadena que he usado (porque ya sale en la animación)
            other.gameObject.SetActive(false);
        }


    }
}
