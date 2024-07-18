using CHIC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantModificationLoader : Singleton<PlantModificationLoader>
{

    #region Debug Property
    [SerializeField]
    private bool _LogsOn = false;
    public bool LogsOn { get { return _LogsOn; } set { _LogsOn = value; } }
    #endregion Debug Property
    
    [SerializeField]
    private List<PlantModification> _PossibleModifications;
    public List<PlantModification> PossibleModifications { get => _PossibleModifications; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if(_PossibleModifications == null || _PossibleModifications.Count == 0)
            _PossibleModifications = new List<PlantModification>(Resources.LoadAll<PlantModification>("BluePrints/PlantMods"));
        if (LogsOn)
            CHIC.Utilities.DebugList(_PossibleModifications, this.GetType().ToString(), gameObject, "OnEnable");
    }
}
