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
    private PlayerDamageDealer _playerDamageDealer;

    private List<SWarrior> _warriors;
    private Dictionary<int, SLegacyData> _legacies;
    private List<List<SLegacyData>> _legaciesByWarrior;
    
    private LegacySO[][] _activeLegacySOByWarrior; //probs discard later
    private Dictionary<int, LegacySO> _activeLegacySODictionary;
    
    private Texture2D[][] _vfxTexturesByWarrior;
    private GameObject[] _bulletsByWarrior;
    
    private List<int> _collectedLegacies_Active = new List<int>();
    private List<int> _collectedLegacies_Passive = new List<int>();
    
    private Sprite[] _legacyIconSpriteSheet;
    private Image[] _activeLegacyIcons = new Image[4];

    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        _legacyIconSpriteSheet = Resources.LoadAll<Sprite>("Sprites/Icons/Abilities/PlayerAbilitySpritesheet");
    }

    public void InitInGameVariables()
    {
        _playerInput = FindObjectOfType<PlayerInput>();
        _playerDamageDealer = FindObjectOfType<PlayerDamageDealer>();

        var abilityGroup = GameObject.Find("AbilityLayoutGroup").transform;
        for (int i = 0; i < 4; i++)
        {
            _activeLegacyIcons[i] = abilityGroup.Find("Slot_" + i).Find("AbilityIcon").GetComponent<Image>();
        }
    }
    
    public void Init()
    {
        Init_WarriorData();
        Init_LegacyData();
        Init_LegacySOs();
        Init_WarriorVFXs();
    }
    
    private void Init_WarriorVFXs()
    {
        _bulletsByWarrior = new GameObject[(int)EWarrior.MAX];
        _vfxTexturesByWarrior = new Texture2D[(int)EWarrior.MAX][];
        for (int warriorIdx = 0; warriorIdx < (int)EWarrior.MAX; warriorIdx++)
        {
            string s = "Prefabs/Player/Bullet_" + (EWarrior)warriorIdx;
            _bulletsByWarrior[warriorIdx] = Resources.Load<GameObject>("Prefabs/Player/Bullets/Bullet_" + (EWarrior)warriorIdx);
            _vfxTexturesByWarrior[warriorIdx] = new Texture2D[(int)EPlayerAttackType.MAX];
            for (int attackIdx = 0; attackIdx < (int)EPlayerAttackType.MAX; attackIdx++)
            {
                _vfxTexturesByWarrior[warriorIdx][attackIdx] = Resources.Load<Texture2D>(
                    "Sprites/Player/VFX/" + (EWarrior)warriorIdx + "/" + (EPlayerAttackType)attackIdx);
            }
        }
    }

    private void Init_WarriorData()
    {
        _warriors = new List<SWarrior>((int)EWarrior.MAX);
        string dataPath = Application.dataPath + "/Tables/WarriorsTable.csv";
        Debug.Assert(File.Exists(dataPath));

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

                SWarrior data = new SWarrior();
                data.Names = names;
                data.Effects = new EStatusEffect[] 
                    {(EStatusEffect)Enum.Parse(typeof(EStatusEffect), csv.GetField("Effect_Base")),
                     (EStatusEffect)Enum.Parse(typeof(EStatusEffect), csv.GetField("Effect_Upgraded"))};
                
                _warriors.Add(data);
            }
        }
    }

    private void Init_LegacyData()
    {
        // Initialise list
        _legacies = new Dictionary<int, SLegacyData>();
        _legaciesByWarrior = new List<List<SLegacyData>>();
        for (int i = 0; i < (int)EWarrior.MAX; i++)
        {
            _legaciesByWarrior.Add(new List<SLegacyData>());
        }
        
        // Retrieve data
        string dataPath = Application.dataPath + "/Tables/LegaciesTable.csv";
        Debug.Assert(File.Exists(dataPath));

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
                    AssetName = csv.GetField("AssetName"),
                    Names = names,
                    Descs = descs,
                    IconIndex = int.Parse(csv.GetField("IconIndex")),
                    Warrior = (EWarrior)Enum.Parse(typeof(EWarrior), csv.GetField("Warrior")),
                    Type = (ELegacyType)Enum.Parse(typeof(ELegacyType), csv.GetField("Type")),
                };

                // Prerequisites and Stats
                string prerequisites = csv.GetField("Prerequisites");
                if (prerequisites != "")
                    data.PrerequisiteIDs = Array.ConvertAll(prerequisites.Split('|'), int.Parse);

                _legacies.Add(data.ID, data);
                _legaciesByWarrior[(int)data.Warrior].Add(data);
            }
        }
    }

    private void Init_LegacySOs()
    {
        _activeLegacySOByWarrior = new LegacySO[(int)EWarrior.MAX][];
        _activeLegacySODictionary = new Dictionary<int, LegacySO>((int)EWarrior.MAX * 3); // melee, ranged, dash for each warrior
        
        // Create Assets
        // Values uninitialised
        for (int warrior = 0; warrior < (int)EWarrior.MAX; warrior++)
        {
            _activeLegacySOByWarrior[warrior] = new LegacySO[3];
            string basePath = "Assets/Resources/Legacies/" + (EWarrior)warrior + "/";
            foreach (var legacy in _legaciesByWarrior[warrior])
            {
                // Do not create scriptable object asset for passive legacies
                if (legacy.Type == ELegacyType.Passive) continue;
                LegacySO legacyAsset = null;
#if UNITY_EDITOR
                // Otherwise, create asset if file does not exist
                string assetPath = basePath + legacy.AssetName + ".asset";
                if (!File.Exists(assetPath))
                {
                    legacyAsset = (LegacySO)ScriptableObject.CreateInstance("Legacy_" + legacy.Type);
                    AssetDatabase.CreateAsset(legacyAsset, assetPath);
                }
#endif
                // Add asset to array
                if (legacyAsset == null)
                    legacyAsset = Resources.Load<LegacySO>("Legacies/"  + (EWarrior)warrior + "/" + legacy.AssetName);
                legacyAsset.Warrior = legacy.Warrior;
                _activeLegacySOByWarrior[warrior][(int)legacy.Type] = legacyAsset;
                _activeLegacySODictionary.Add(legacy.ID, legacyAsset);
            }
        }
#if UNITY_EDITOR
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
        var legacyData = _legacies[legacyID];
        
        // TODO Handle passive legacy
        if (legacyData.Type == ELegacyType.Passive) return; 
        
        // Find and bind legacy SO
        var legacyAsset = _activeLegacySODictionary[legacyID];
        _playerDamageDealer.AttackBases[(int)legacyData.Type].BindActiveLegacy(legacyAsset);

        // Update VFX
        UpdateAttackVFX(legacyData.Warrior, legacyData.Type);
        
        // TODO Update UI
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

    public Texture2D GetWarriorVFXTexture(EWarrior warrior, EPlayerAttackType attack)
    {
        return _vfxTexturesByWarrior[(int)warrior][(int)attack];
    }

    public EStatusEffect GetWarriorStatusEffect(EWarrior warrior, int level)
    {
        return _warriors[(int)warrior].Effects[level];
    }
    
    public void UpdateAttackVFX(EWarrior warrior, ELegacyType attackType)
    { 
        // try get warrior-specific VFX
        switch (attackType)
        {
            case ELegacyType.Melee:
                {
                    var attackBase = (AttackBase_Melee)_playerDamageDealer.AttackBases[(int)attackType];
                    
                    // Base VFX
                    attackBase.VFXObject.GetComponent<ParticleSystemRenderer>()
                        .material.mainTexture = GetWarriorVFXTexture(warrior, EPlayerAttackType.Melee_Base);
                    
                    // Combo VFX
                    attackBase.VFXObjCombo.GetComponent<ParticleSystemRenderer>()
                        .material.mainTexture = GetWarriorVFXTexture(warrior, EPlayerAttackType.Melee_Combo);
                }
                break;
            case ELegacyType.Ranged:
                {
                    var attackBase = (AttackBase_Ranged)_playerDamageDealer.AttackBases[(int)attackType];
                    
                    // Base VFX
                    attackBase.VFXObject.GetComponent<ParticleSystemRenderer>()
                        .material.mainTexture = GetWarriorVFXTexture(warrior, EPlayerAttackType.Ranged);
                    var main = attackBase.VFXObject.GetComponent<ParticleSystem>().main;
                    if (warrior == EWarrior.Vernon)
                    {
                        main.startSize = 6.5f;
                        attackBase.VFXObject.transform.localPosition = new Vector3(0.04f, 0.945f, 0);
                    }
                    else
                    {
                        main.startSize = 4f;
                        attackBase.VFXObject.transform.localPosition = new Vector3(-0.02f, 0.9f, 0);
                    }
                    
                    // bullet
                    attackBase.SetBullet(_bulletsByWarrior[(int)warrior]);
                }
                break;
            case ELegacyType.Dash:
                {
                    // Base VFX
                    _playerDamageDealer.AttackBases[(int)attackType]
                        .VFXObject.GetComponent<ParticleSystemRenderer>()
                        .material.mainTexture = GetWarriorVFXTexture(warrior, EPlayerAttackType.Dash);
                }
                break;
        }
    }
}
