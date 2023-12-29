using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEditor;

public class PlayerAttackManager : Singleton<PlayerAttackManager>
{
    private PlayerInput _playerInput;

    private List<SWarrior> _warriors;
    private List<SLegacyData> _legacies;
    private List<List<SLegacyData>> _legaciesByWarrior;

    private List<int> _collectedLegacies_Active = new List<int>();
    private List<int> _collectedLegacies_Passive = new List<int>();
    
    private Sprite[] _legacyIconSpriteSheet;
    private Image[] _activeLegacyIcons = new Image[4];

    protected override void Awake()
    {
        base.Awake();
        _legacyIconSpriteSheet = Resources.LoadAll<Sprite>("Sprites/Icons/Abilities/PlayerAbilitySpritesheet");
    }

    public void InitInGameVariables()
    {
        _playerInput = FindObjectOfType<PlayerInput>();

        var abilityGroup = GameObject.Find("AbilityLayoutGroup").transform;
        for (int i = 0; i < 4; i++)
        {
            _activeLegacyIcons[i] = abilityGroup.Find("Slot_" + i).Find("AbilityIcon").GetComponent<Image>();
        }
    }

    private void Update()
    {
    }

    public void Init()
    {
        Init_WarriorData();
        Init_LegacyData();
    }

    private void Init_WarriorData()
    {
        _warriors = new List<SWarrior>((int)EWarrior.MAX);
        string dataPath = Application.dataPath + "/Tables/WarriorsTable.csv";
        Debug.Assert(dataPath != null && File.Exists(dataPath));

        using (var reader = new StreamReader(dataPath))
        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                List<string> names = new List<string>();
                for (int i = 0; i < (int)ELocalisation.MAX; i++)
                {
                    string name = csv.GetField("Name_" + ((ELocalisation)i).ToString());
                    names.Add(name);
                }

                SWarrior data = new SWarrior
                {
                    Names = names,
                    EffectBase = (EStatusEffect)Enum.Parse(typeof(EStatusEffect), csv.GetField("Effect_Base")),
                    EffectUpgraded = (EStatusEffect)Enum.Parse(typeof(EStatusEffect), csv.GetField("Effect_Upgraded"))
                };

                _warriors.Add(data);
            }
        }
    }

    private void Init_LegacyData()
    {
        // Initialise list
        _legacies = new List<SLegacyData>();
        _legaciesByWarrior = new List<List<SLegacyData>>();
        for (int i = 0; i < (int)EWarrior.MAX; i++)
        {
            _legaciesByWarrior.Add(new List<SLegacyData>());
        }
        
        // Retrieve data
        string dataPath = Application.dataPath + "/Tables/LegaciesTable.csv";
        Debug.Assert(dataPath != null && File.Exists(dataPath));

        using (var reader = new StreamReader(dataPath))
        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                // Name and Description
                List<string> names = new List<string>();
                List<string> descs = new List<string>();
                for (int i = 0; i < (int)ELocalisation.MAX; i++)
                {
                    string name = csv.GetField("Name_" + Utility.GetLocalisationString(i));
                    string desc = csv.GetField("Desc_" + Utility.GetLocalisationString(i));
                    names.Add(name);
                    descs.Add(desc);
                }

                // Other data
                SLegacyData data = new SLegacyData
                {
                    ID = int.Parse(csv.GetField("ID")),
                    ClassName = csv.GetField("ClassName"),
                    Names = names,
                    Descs = descs,
                    IconIndex = int.Parse(csv.GetField("IconIndex")),
                    Warrior = (EWarrior)Enum.Parse(typeof(EWarrior), csv.GetField("Warrior")),
                    Type = (ELegacyType)Enum.Parse(typeof(ELegacyType), csv.GetField("Type")),
                };

                // Prerequisites and Stats
                string prerequisites = csv.GetField("Prerequisites");
                string stats = csv.GetField("StatsByPreservation");
                if (prerequisites != "")
                    data.Prerequisites = Array.ConvertAll(prerequisites.Split('|'), int.Parse);
                if (stats != "")
                    data.StatByPreservation = Array.ConvertAll(stats.Split('|'), int.Parse);

                _legacies.Add(data);
                _legaciesByWarrior[(int)data.Warrior].Add(data);
            }
        }

        // Create Assets
        // Values uninitialised
#if UNITY_EDITOR
        for (int warrior = 0; warrior < (int)EWarrior.MAX; warrior++)
        {
            string basePath = "Assets/Resources/Legacies/" + ((EWarrior)warrior).ToString() + "/";
            foreach (var legacy in _legaciesByWarrior[warrior])
            {
                string assetPath = basePath + legacy.ClassName + ".asset";
                if (File.Exists(assetPath)) continue;
                var asset = ScriptableObject.CreateInstance("Legacy_" + legacy.Type.ToString());
                AssetDatabase.CreateAsset(asset, assetPath);
            }
        }
        AssetDatabase.SaveAssets();
#endif
    }


    //------------------------------------------------------------------------------------

    public bool IsAbilityAlreadyBound(int abilityIdx)
    {
        //foreach (var activeAbility in _activeAbilities)
        //{
        //    if (activeAbility == null) continue;
        //    if (activeAbility._data.Id == abilityIdx) return true;
        //}
        //foreach (var passiveAbility in _passiveAbilities)
        //{
        //    if (passiveAbility == null) continue;
        //    if (passiveAbility._data.Id == abilityIdx) return true;
        //}
        return false;
    }
    
    public void CollectLegacy(int legacyID)
    {
        //// TODO FIX 지금은 active ability만 바로 바인딩. 패시브 따로 빼야함
        //if (_nextAvailableBindIdx == 5 || _abilities[abilityIdx].Type != ELegacyType.Active) return;
        //_collectedAbilities.Add(abilityIdx);

        //// TEMP NEED FIX 일단 collect하면 순서대로 바인딩함 나중에 바인딩ui 추가하면서 픽스할 것.
        //BindActiveAbility(_nextAvailableBindIdx++, abilityIdx);
    }

    public void BindActiveLegacy(int legacyID)
    {
        //Debug.Log("Attempting to bind ability [" + abilityIdx + "] to action index [" + actionIdx + "]");
        //Debug.Assert(actionIdx >= 0 && actionIdx < 5 && abilityIdx <= _abilities.Count());
        //Debug.Assert(_abilities[abilityIdx].Type == ELegacyType.Active);

        //// Return if already bound (probably not needed in the future when we have ui)
        //if (IsAbilityAlreadyBound(abilityIdx))
        //{
        //    Debug.Log("The same action is already bound!");
        //    return;
        //}

        //// Update input action
        //UnbindAbility(actionIdx);
        //ChangeActionBinding(actionIdx, null, _abilities[abilityIdx].Interaction.ToString());

        //// Add the ability to the ability list
        //_activeAbilities[actionIdx] = (ActiveAbilityBase)InitiateAbility(abilityIdx);

        //// Set variables of the ability
        //_activeAbilities[actionIdx]._data = _abilities[abilityIdx];
        //_activeAbilities[actionIdx].BoundIndex = actionIdx;

        //// Update UI
        //_activeLegacyIcons[actionIdx].sprite = GetAbilityIconSprite(abilityIdx);
        //_activeLegacyIcons[actionIdx].color = Color.white;

        //// Register callbacks
        //// TODO Later pass context as parameter if needed
        //InputAction action = _playerInput.actions["Cast_" + actionIdx];
        //action.started += _ => _activeAbilities[actionIdx].Activate();
        //action.performed += _ => _activeAbilities[actionIdx].OnPerformed();
        //action.canceled += _ => _activeAbilities[actionIdx].OnCanceled();

        //Debug.Log("Successfully bound ability [" + abilityIdx + "] to action index [" + actionIdx + "]");
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
        //if (_activeAbilities[actionIdx] == null) return;

        //// Remove previous input action settings
        //InputAction action = _playerInput.actions["Cast_" + actionIdx];
        //action.RemoveAllBindingOverrides();
        //action.started -= _ => _activeAbilities[actionIdx].Activate();
        //action.performed -= _ => _activeAbilities[actionIdx].OnPerformed();
        //action.canceled -= _ => _activeAbilities[actionIdx].OnCanceled();

        //// Update UI
        //_activeLegacyIcons[actionIdx].sprite = null;
        //_activeLegacyIcons[actionIdx].color = Color.clear;

        //// Remove ability component
        //_activeAbilities[actionIdx].CleanUpAbility();
        //_activeAbilities[actionIdx] = null;
    }

    public List<SLegacyData> GetAbilitiesByWarrior(EWarrior warrior)
    {
        return _legaciesByWarrior[(int)warrior];    
    }

    public Sprite GetAbilityIconSprite(int legacyID)
    {
        return _legacyIconSpriteSheet[_legacies[legacyID].IconIndex];
    }
}
