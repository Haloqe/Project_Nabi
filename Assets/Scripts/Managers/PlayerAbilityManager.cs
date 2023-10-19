using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Player.Abilities.Base;

public class PlayerAbilityManager : Singleton<PlayerAbilityManager>
{
    private GameObject _attachedAbilityContainer;
    private GameObject _detachedAbilityContainer;
    private PlayerInput _playerInput;
    private Dictionary<int, SAbilityData> _abilities;
    private Dictionary<int, SAbilityData>[] _abilitiesByMetals;
    private ActiveAbilityBase[] _activeAbilities = new ActiveAbilityBase[5];
    private PassiveAbilityBase[] _passiveAbilities = new PassiveAbilityBase[3];
    private List<ActiveAbilityBase> _manualUpdateAbilities = new List<ActiveAbilityBase>();

    protected override void Awake()
    {
        base.Awake();
        _playerInput = FindObjectOfType<PlayerInput>();
        _abilities = new Dictionary<int, SAbilityData>();
        _abilitiesByMetals = new[]
        {
            new Dictionary<int, SAbilityData>(),    // Copper
            new Dictionary<int, SAbilityData>(),    // Gold
        };
    }

    private void Start()
    {
        _attachedAbilityContainer = PlayerController.Instance.transform.Find("Abilities").gameObject;
        _detachedAbilityContainer = GameObject.Find("DetachedAbilities");
    }

    private void Update()
    {
        UpdateAbilities();
    }

    // NOTE THERE IS NO ERROR CHECKING OF THE DATA !!!!
    // The field names and data types must be correct and reasonable
    public void Init(string dataPath)
    {
        Debug.Log("Initialising Player Abilities");
        Debug.Assert(dataPath != null && File.Exists(dataPath));

        using (var reader = new StreamReader(dataPath))
        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                // Fill ability info
                SAbilityData data = new SAbilityData
                {
                    ShouldOverride = bool.Parse(csv.GetField("ShouldOverride")), // TEMP

                    Id           = int.Parse(csv.GetField("Id")),
                    ClassName    = csv.GetField("ClassName"),
                    Name_EN      = csv.GetField("Name_EN"),
                    Name_KO      = csv.GetField("Name_KO"),
                    Des_EN       = csv.GetField("Description_EN"),
                    Des_KO       = csv.GetField("Description_KO"),
                    IconPath     = csv.GetField("Icon"),
                    PrefabPath   = csv.GetField("Prefab"),
                    IsAttached   = bool.Parse(csv.GetField("IsAttached")),
                    Interaction  = (EInteraction)Enum.Parse(typeof(EInteraction), csv.GetField("Interaction")),
                    Type         = (EAbilityType)Enum.Parse(typeof(EAbilityType), csv.GetField("Type")),
                    MetalType    = (EAbilityMetalType)Enum.Parse(typeof(EAbilityMetalType), csv.GetField("MetalType")),
                    CoolDownTime = float.Parse(csv.GetField("CoolDownTime")),
                    LifeTime     = float.Parse(csv.GetField("LifeTime"))
                };

                // Fill damage info
                SDamageInfo damageInfo = new SDamageInfo();
                damageInfo.AbilityMetalType = data.MetalType;
                string[] damages = csv.GetField("Damages").Split('|');
                string[] statusEffects = csv.GetField("StatusEffects").Split('|');

                if (data.Type == EAbilityType.Active)
                {
                    // Damage type & amount data
                    if (damages.Length % 3 == 0)
                    {
                        damageInfo.Damages = new List<SDamage>();
                        for (int i = 0; i < damages.Length; i += 3)
                        {
                            SDamage temp = new SDamage
                            {
                                Type = (EDamageType)Enum.Parse(typeof(EDamageType), damages[i]),
                                Amount = float.Parse(damages[i+1]),
                                Duration = float.Parse(damages[i+2]),
                            };
                            damageInfo.Damages.Add(temp);
                        }
                    }

                    // Status effect data
                    if (statusEffects.Length % 3 == 0)
                    {
                        damageInfo.StatusEffects = new List<SStatusEffect>();
                        for (int i = 0; i < statusEffects.Length; i += 3)
                        {
                            SStatusEffect temp = new SStatusEffect
                            {
                                Effect = (EStatusEffect)Enum.Parse(typeof(EStatusEffect), statusEffects[i]),
                                Strength = float.Parse(statusEffects[i + 1]),
                                Duration = float.Parse(statusEffects[i + 2]),
                            };
                            damageInfo.StatusEffects.Add(temp);
                        }
                    }
                }
                data.DamageInfo = damageInfo;

                _abilities.Add(data.Id, data);
                _abilitiesByMetals[(int)data.MetalType].Add(data.Id, data);
                Debug.Log("Ability [" + data.Name_EN + "][" + data.Id + "] added to the dictionary.");
            }
        }
#if UNITY_EDITOR
        CreateAbilityScripts();
#endif
    }

    private void CreateAbilityScripts()
    {
        if (_abilities.Count == 0)
        {
            Debug.LogWarning("No player abilities found!");
            return;
        }

        int count = 0; // temp

        // Check if directory exists
        // Seems unnecessary; might remove in the future
        var abilityDirPath = Application.dataPath + "/Scripts/Player/Abilities";
        if (!Directory.Exists(abilityDirPath))
        {
            Debug.Log("Generating <Player/Abilities> directory");
            Directory.CreateDirectory(abilityDirPath);
        }

        // Check if template files exist
        var activeTemplatePath = abilityDirPath + "/Base/ActiveAbilityTemplate.cs";
        var passiveTemplatePath = abilityDirPath + "/Base/PassiveAbilityTemplate.cs";
        Debug.Assert(File.Exists(activeTemplatePath));
        Debug.Assert(File.Exists(passiveTemplatePath));
        
        string activeTemplate = File.ReadAllText(activeTemplatePath);    
        string passiveTemplate = File.ReadAllText(passiveTemplatePath);
        Debug.Assert(activeTemplate.Length != 0 && passiveTemplate.Length != 0);
        
        // Create a script for each ability from template
        // NOTE this does not consider ability types other than PASSIVE / ACTIVE
        foreach (var ability in _abilities)
        {
            if (!ability.Value.ShouldOverride) continue;

            string temp = string.Empty;
            string ability_name = ability.Value.ClassName;
            //string ability_name = String.Concat(ability.Value.Name_EN.Where(c => !Char.IsWhiteSpace(c)));
            
            if (ability.Value.Type is EAbilityType.Active)
            {
                temp = activeTemplate;
                temp = temp.Replace("TemplateActiveAbility", ability_name);
            }
            else if (ability.Value.Type is EAbilityType.Passive)
            {
                temp = passiveTemplate;
                temp = temp.Replace("TemplatePassiveAbility", ability_name);
            }

            string outputPath = abilityDirPath + "/" + ability_name + ".cs";
            File.WriteAllText(outputPath, temp);

            count++; //temp
        }

        Debug.Log(count + " ability scripts added");
    }

    //------------------------------------------------------------------------------------
    
    //return true if the abilityIdx is not already bound
    //reutrn false if the abilityIdx already bound
    public bool CheckabilityAlreadyBound(int abilityIdx)
    {
        Dictionary<int, SAbilityData> _abilitiesByMetalForCheck;
        _abilitiesByMetalForCheck = GetAbilitiesByMetal(EAbilityMetalType.Gold);

        for (int i = 1; i <= _abilitiesByMetalForCheck.Count; i++)
        {
            if(_abilitiesByMetalForCheck[i].Id == abilityIdx)
            {
                return false;
            }
        }

        return true;
    }
    
    private AbilityBase InitiateAbility(int abilityIdx)
    {
        SAbilityData data = _abilities[abilityIdx];
        UnityEngine.Object prefabObj = null;
        UnityEngine.Object prefab = Utility.LoadObjectFromFile("Prefabs/PlayerAbilities/" + data.PrefabPath);
        Debug.Assert(prefab);

        if (data.IsAttached)
        {
            prefabObj = Instantiate(prefab, _attachedAbilityContainer.transform);
        }
        else
        {
            prefabObj = Instantiate(prefab, _detachedAbilityContainer.transform);
        }

        Component abilityObj = prefabObj.GameObject().GetComponent(data.ClassName);
        Debug.Assert(abilityObj);

        return (AbilityBase)abilityObj;
    }
    
    // NOTE currently does not consider if the same ability is bound to another key
    public void BindActiveAbility(int actionIdx, int abilityIdx)
    {
        Debug.Log("Attempting to bind ability [" + abilityIdx + "] to action index [" + actionIdx + "]");
        Debug.Assert(actionIdx >= 0 && actionIdx < 5 && abilityIdx <= _abilities.Count());
        Debug.Assert(_abilities[abilityIdx].Type == EAbilityType.Active);

        // Return if already bound (probably not needed in the future when we have ui)
        if (_activeAbilities[actionIdx] != null && _activeAbilities[actionIdx]._data.Id == abilityIdx)
        {
            Debug.Log("The same action is already bound!");
            return;
        }

        // Update input action
        UnbindAbility(actionIdx);
        ChangeActionBinding(actionIdx, null, _abilities[abilityIdx].Interaction.ToString());

        // Add the ability to the ability list
        _activeAbilities[actionIdx] = (ActiveAbilityBase)InitiateAbility(abilityIdx);

        // Set variables of the ability
        _activeAbilities[actionIdx]._data = _abilities[abilityIdx];

        // Register callbacks
        // TODO Later pass context as parameter if needed
        InputAction action = _playerInput.actions["Cast_" + actionIdx];
        action.started += _ => _activeAbilities[actionIdx].Activate();
        action.performed += _ => _activeAbilities[actionIdx].OnPerformed();
        action.canceled += _ => _activeAbilities[actionIdx].OnCanceled();

        Debug.Log("Successfully bound ability [" + abilityIdx + "] to action index [" + actionIdx + "]");
    }

    public void ChangeActionBinding(int actionIdx, string newBinding, string newInteraction)
    {
        InputAction action = _playerInput.actions["Cast_" + actionIdx];
        action.ApplyBindingOverride(new InputBinding
        {
            overridePath = String.IsNullOrEmpty(newBinding) ? action.bindings[0].overridePath : newBinding,
            interactions = String.IsNullOrEmpty(newInteraction) ? action.interactions : newInteraction
        });
    }

    private void UnbindAbility(int actionIdx)
    {
        if (_activeAbilities[actionIdx] == null) return;

        // Remove previous input action settings
        InputAction action = _playerInput.actions["Cast_" + actionIdx];
        action.RemoveAllBindingOverrides();
        action.started -= _ => _activeAbilities[actionIdx].Activate();
        action.performed -= _ => _activeAbilities[actionIdx].OnPerformed();
        action.canceled -= _ => _activeAbilities[actionIdx].OnCanceled();

        // Remove ability component
        _activeAbilities[actionIdx].CleanUpAbility();
        _activeAbilities[actionIdx] = null;
    }

    private void UpdateAbilities()
    {
        foreach (var ability in _manualUpdateAbilities.ToList())
        {
            ability.Update();
            if (ability.CanActivate())
            {
                _manualUpdateAbilities.Remove(ability);
            }
        }
    }

    public void RequestManualUpdate(ActiveAbilityBase ability)
    {
        _manualUpdateAbilities.Add(ability);
    }

    public Dictionary<int, SAbilityData> GetAbilitiesByMetal(EAbilityMetalType metalType)
    {
        return _abilitiesByMetals[(int)metalType];
    }
}
