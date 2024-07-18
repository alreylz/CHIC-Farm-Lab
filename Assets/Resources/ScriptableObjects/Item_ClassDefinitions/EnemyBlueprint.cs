using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Enemy", menuName = "chicProject/Enemy")]
public class EnemyBlueprint : ScriptableObject
{
    new public string name = "Default Enemy";
    public float health = 200;
    public float HitDamage = 30;
}
