using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stats", menuName = "Create Stats" )]
public class Stats : ScriptableObject
{
    public float maxHealth;
    public float stamina;
    public float sprintSpeed;
    public float speed;
    public float jumpHeight;
}
