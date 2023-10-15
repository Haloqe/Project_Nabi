using Enums.PlayerEnums;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

namespace Player.Abilities.Base
{
    public abstract class ActiveAbilityBase : AbilityBase
    {
        private Object prefabObj = null;
        protected float _elapsedActiveTime = 0.0f;
        protected float _elapsedCoolTime = 0.0f;

        protected void Update()
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
        public override void Init()
        {
            base.Init();

            if (_data.PrefabPath != "None")
            {
                Object prefab = Utility.LoadObjectFromFile("Prefabs/PlayerAbilities/" + _data.PrefabPath);
                if (_data.IsAttached)
                {
                    prefabObj = Instantiate(prefab, PlayerController.Instance.transform);
                }
                else
                {
                    prefabObj = Instantiate(prefab, PlayerController.Instance.transform.position, Quaternion.identity);
                }
                prefabObj.GameObject().SetActive(false);
                Debug.Log("[" + _data.Name_EN + "] Prefab found and instantiated");
            }
        }

        public override void Activate()
        {
            if (_state != EAbilityState.Ready)
            {
                Debug.Log("[" + _data.Name_EN + "] under cooldown");
                return;
            }
            base.Activate();
            if (prefabObj != null) TogglePrefab(true);
        }

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
            if (prefabObj != null) TogglePrefab(false);
        }

        protected virtual void TogglePrefab(bool shouldTurnOn)
        {
            // TODO check if need manual activation upon re-enable
            prefabObj.GameObject().SetActive(shouldTurnOn);

            // TODO disable/enable colliders 
            //ParticleSystem[] particleSystems = prefabObj.GameObject().GetComponentsInChildren<ParticleSystem>();
            //AudioSource[] audioSources = prefabObj.GameObject().GetComponentsInChildren<AudioSource>();
            //foreach (ParticleSystem particleSystem in particleSystems)
            //{
            //    particleSystem.Play();
            //}
            //foreach (AudioSource audioSource in audioSources)
            //{
            //    audioSource.Play();
            //}
        }

        public void CleanUpAbility()
        {
            if (prefabObj) Destroy(prefabObj);
            prefabObj = null;
        }
    }
}