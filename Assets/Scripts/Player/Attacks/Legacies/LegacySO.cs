using UnityEngine;

public abstract class LegacySO : ScriptableObject
{
    public EWarrior Warrior;

    public virtual void SetWarrior(EWarrior warrior)
    {
        Warrior = warrior;
    }
}