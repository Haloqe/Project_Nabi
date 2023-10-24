using Player.Abilities.Base;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class ActiveAbilityBase : AbilityBase
{
    public int BoundIndex { get; set; }
    protected bool _isCleanseAbility = false;
    protected float _elapsedActiveTime = 0.0f;
    protected float _elapsedCoolTime = 0.0f;
    protected float _lifeTime = 0.0f;
    private Image _cooldownUI;


    protected override void Start()
    {
        base.Start();
        _lifeTime = _data.LifeTime == 0.0f ? 
            Utility.GetLongestParticleDuration(gameObject) + 0.05f : _data.LifeTime;
        TogglePrefab(false);
        _cooldownUI = PlayerAbilityManager.Instance.GetCooldownUI(BoundIndex);
        _cooldownUI.fillAmount = 0.0f;
        Initialise();
    }

    protected abstract void Initialise();

    public void Update()
    {
        if (_lifeTime == 0.0f) return;

        switch (_state)
        {
            case EAbilityState.Ready:

                break;

            case EAbilityState.Active:
                _elapsedActiveTime += Time.deltaTime;
                _elapsedCoolTime += Time.deltaTime;
                UpdateCooldownUI();

                if (_elapsedActiveTime >= _lifeTime)
                {
                    _state = EAbilityState.Cooldown;
                    _elapsedActiveTime = 0.0f;
                    OnFinished();
                }
                break;

            case EAbilityState.Cooldown:
                _elapsedCoolTime += Time.deltaTime;
                UpdateCooldownUI();

                if (_elapsedCoolTime >= _data.CooldownTime)
                {
                    _state = EAbilityState.Ready;
                    _elapsedCoolTime = 0.0f;
                    PlayerAbilityManager.Instance.RequestCancelManualUpdate(BoundIndex);
                }
                break;
        }
    }

    public virtual bool CanActivate()
    {
        if (_owner.IsSilenced)
        {
            Debug.Log("Silenced!");
            return false;
        }
        else if (_owner.IsSilencedExceptCleanse && !_isCleanseAbility)
        {
            Debug.Log("Silenced except cleanse!");
            return false;
        }

        if (_state != EAbilityState.Ready)
        {
            Debug.Log("[" + _data.Name_EN + "] under cooldown");
            return false;
        }
        return true;
    }

    public override void Activate()
    {
        if (!CanActivate()) return;
        Debug.Log("[" + _data.Name_EN + "] activated");
        TogglePrefab(true);
        _state = EAbilityState.Active;
        _cooldownUI.fillAmount = 1.0f;
        ActivateAbility();
    }

    protected abstract void ActivateAbility();

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
        PlayerAbilityManager.Instance.RequestManualUpdate(BoundIndex);
        TogglePrefab(false);
    }

    public virtual void CleanUpAbility()
    {
        Destroy(gameObject);
    }

    private void UpdateCooldownUI()
    {
        float coolTimeRatio = Mathf.Clamp((_data.CooldownTime - _elapsedCoolTime) / _data.CooldownTime, 0, 1);
        _cooldownUI.fillAmount = coolTimeRatio;
    }
}
