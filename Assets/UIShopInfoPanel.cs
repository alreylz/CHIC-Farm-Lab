using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIShopInfoPanel : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _MoneyAmountValue;


    private void Start()
    {
        GameManager.Instance.subscribeToChanges(
            (gManager) =>
      {
          string money = gManager.GetStringMoney;
          _MoneyAmountValue.SetText(money);

      });
    }

}
