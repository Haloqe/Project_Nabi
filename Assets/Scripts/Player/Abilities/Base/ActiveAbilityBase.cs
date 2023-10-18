﻿using Player.Abilities.Base;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ActiveAbilityBase : AbilityBase, IDamageDealer
{
    protected float _elapsedActiveTime = 0.0f;
    protected float _elapsedCoolTime = 0.0f;

    protected override void Start()
    {
        base.Start();
        TogglePrefab(false);
    }

    public void Update()
    {
        if (_data.LifeTime == 0.0f) return;

        switch (_state)
        {
            case EAbilityState.Ready:

                break;

            case EAbilityState.Active:
                _elapsedActiveTime += Time.deltaTime;
                if (_elapsedActiveTime >= _data.LifeTime)
                {
                    _state = EAbilityState.Cooldown;
                    _elapsedCoolTime = _elapsedActiveTime;
                    _elapsedActiveTime = 0.0f;
                    OnFinished();
                }
                break;

            case EAbilityState.Cooldown:
                _elapsedCoolTime += Time.deltaTime;
                if (_elapsedCoolTime >= _data.CoolDownTime)
                {
                    _state = EAbilityState.Ready;
                    _elapsedCoolTime = 0.0f;
                }
                break;
        }
    }

    public virtual bool CanActivate()
    {
        if (_state != EAbilityState.Ready)
        {
            //Debug.Log("[" + _data.Name_EN + "] under cooldown");
            return false;
        }
        return true;
    }

    public override void Activate()
    {
        if (!CanActivate()) return;
        _state = EAbilityState.Active;
        TogglePrefab(true);
        Initialise();
    }

    protected abstract void Initialise();

    public virtual void OnPerformed()
    {
        // register this when hold and release or such Performed event callback is needed
        //Debug.Log("[" + _data.Name_EN + "] performed");
    }

    public virtual void OnCanceled()
    {
        // register this when Canceled event is needed
        // mostly for key-down charge skills where something should happen once user releases the key
        //Debug.Log("[" + _data.Name_EN + "] canceled");
    }

    // An ability is finished when its lifetime ends (iff lifetime > 0)
    // (TODO) or after some time (in case its a one shot animation and no lifetime exists)
    public virtual void OnFinished()
    {
        Debug.Log("[" + _data.Name_EN + "] finished");
        PlayerAbilityManager.Instance.RequestManualUpdate(this);
        TogglePrefab(false);
    }

    public virtual void CleanUpAbility()
    {
        Destroy(gameObject);
    }
}