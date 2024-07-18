using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Lab_PlantVarietyExplorer_Controller))]
public class UILab_TargetedModification : UIScreen
{

    [SerializeField]
    IUIPanel<IUIPanelParams> _ActivePlantPanel;

    //Referencia al controlador de esta pantalla (que incluye todo lo que se refiere a estado).
    [SerializeField]
    Lab_PlantVarietyExplorer_Controller _SrcPlantVarietyController;

    void Awake()
    {
        if (_ActivePlantPanel == null) _ActivePlantPanel = CHIC.Utilities.GetAllComponentsOfTypeInChildren<IUIPanel<IUIPanelParams>>(transform)[0];
        
        if (_SrcPlantVarietyController == null) _SrcPlantVarietyController = GetComponent<Lab_PlantVarietyExplorer_Controller>();
    }

    private void UpdateActivePlantData(PlantVariety nuPlantVariety)
    {
        _ActivePlantPanel.Show(new UIPlantSummaryParams(nuPlantVariety));

    }


    private void OnEnable()
    {
        _SrcPlantVarietyController.onActivePlantVarietyUpdated.AddListener(UpdateActivePlantData);
    }

    private void OnDisable()
    {
        _SrcPlantVarietyController.onActivePlantVarietyUpdated.RemoveListener(UpdateActivePlantData);

    }

    public override void Show<T>(T screenParams)
    {
        throw new System.NotImplementedException();
    }
}
