using UnityEngine;

public abstract class LegacySO : ScriptableObject
{
    public int id;
    public EWarrior warrior;
    public ELegacyPreservation preservation;

    public virtual void SetWarrior(EWarrior warrior)
    {
        this.warrior = warrior;
    }
}