using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnActivePlantVarietyUpdated : UnityEvent<PlantVariety> { };

public class Lab_PlantVarietyExplorer_Controller : MonoBehaviour
{
    
    public PlantVarietyLoader loader;
    //Caching
    [Header("Active Item Monitoring")]
    [SerializeField]
    private PlantVariety _ActiveVariety;
    [SerializeField]
    private int _VarietyIndex;

    [SerializeField]
    private List<PlantVariety> _AvailablePlantVarieties;

    [SerializeField]
    public OnActivePlantVarietyUpdated onActivePlantVarietyUpdated;


    private void Start()
    {
        if (loader == null)
            loader = FindObjectOfType<PlantVarietyLoader>();
        
        _AvailablePlantVarieties = loader.KnownPlantVarieties;
        SetNthVarietyAsActive(0);
    }
    
    public void SetNthVarietyAsActive(int index)
    {
        try
        {
            _ActiveVariety = _AvailablePlantVarieties[index];
            _VarietyIndex = index;
            onActivePlantVarietyUpdated?.Invoke(_ActiveVariety);
        }
        catch
        {
            Debug.LogError("Couldn't set [" + index + "] as active element in PlantVarietyExplorer controller. List Size "+_AvailablePlantVarieties.Count);
        }
    }
    public void SetRndVarietyAsActive()
    {
        _ActiveVariety = CHIC.Utilities.ChooseRandomlyFromList<PlantVariety>(_AvailablePlantVarieties, out _VarietyIndex);
        onActivePlantVarietyUpdated?.Invoke(_ActiveVariety);
    }
    public void SetNextVarietyAsActive()
    {
        _VarietyIndex = (_VarietyIndex + 1) % _AvailablePlantVarieties.Count;
        _ActiveVariety = _AvailablePlantVarieties[_VarietyIndex];
        onActivePlantVarietyUpdated?.Invoke(_ActiveVariety);

    }
    public void SetPrevVarietyAsActive()
    {
        _VarietyIndex = (_VarietyIndex - 1) % _AvailablePlantVarieties.Count;
        if (_VarietyIndex < 0) _VarietyIndex += _AvailablePlantVarieties.Count;
        _ActiveVariety = _AvailablePlantVarieties[_VarietyIndex];
        onActivePlantVarietyUpdated?.Invoke(_ActiveVariety);

    }
    

}
