using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModChainController : MonoBehaviour
{
    [Range(1,3)]
    public int ChainType = 0;



    private void Start()
    {
        List<Material> mList;
        mList = new List<Material>(gameObject.GetComponent<MeshRenderer>().materials);

        Color color = Color.black;
        switch (ChainType)
        {
            case 1:
                color = Color.green;
                break;
            case 2:
                color = Color.blue;
                break;
            case 3:
                color = Color.yellow;
                break;

        }

        foreach (var m in mList)
        {
            m.SetColor("_Color" ,color);
        }

    }
}
