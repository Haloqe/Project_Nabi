using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEditor;

public class PlayerAttackManager : Singleton<PlayerAttackManager>
{
    private PlayerController _playerController;
    private PlayerInput _playerInput;
    private PlayerDamageDealer _playerDamageDealer;
    private PlayerDamageReceiver _playerDamageReceiver;
    private SWarrior[] _warriors;
    
    // All legacies
    private Dictionary<int, SLegacyData> _legacies;
    private List<List<SLegacyData>> _legaciesByWarrior;
    private Dictionary<int, LegacySO> _legacySODictionary;
    
    // Collected legacies
    private HashSet<int> _collectedLegacyIDs;
    private List<int> _collectedPassiveIDs;
    private Dictionary<int, ELegacyPreservation> _collectedLegacyPreservations;
    private int[] _boundActiveLegacyIDs;
    
    // Attack visual related
    private Texture2D[][] _vfxTexturesByWarrior;
    private GameObject[] _bulletsByWarrior;
    
    // UI related
    private Sprite[] _legacyIconSpriteSheet;
    private Image[] _activeLegacyIcons = new Image[4];
    private Image[] _activeLegacyOverlays = new Image[4];
    private Transform _passiveLegacyGroup;
    private GameObject _passiveLegacySlotPrefab;
    private LegacySlotUI[] _activeLegacySlots = new LegacySlotUI[4];

    protected override void Awake()
    {
        base.Awake();
        if (_toBeDestroyed) return;
        _legacyIconSpriteSheet = Resources.LoadAll<Sprite>("Sprites/Icons/Abilities/PlayerAbilitySpritesheet");
        _collectedLegacyIDs = new HashSet<int>();
        _collectedPassiveIDs = new List<int>();
        _collectedLegacyPreservations = new Dictionary<int, ELegacyPreservation>();
        _boundActiveLegacyIDs = new int[] {-1,-1,-1};
        _passiveLegacySlotPrefab = Utility.LoadGameObjectFromPath("Prefabs/UI/InGame/PassiveLegacySlot");
        GameEvents.restarted += OnRestarted;
    }
    
    private void OnRestarted()
    {
        _collectedLegacyIDs = new HashSet<int>();
        _collectedPassiveIDs = new List<int>();
        _collectedLegacyPreservations = new Dictionary<int, ELegacyPreservation>();
        _boundActiveLegacyIDs = new int[] {-1,-1,-1};
    }

    public void InitInGameVariables()
    {
        _playerController = PlayerController.Instance;
        _playerInput = FindObjectOfType<PlayerInput>();
        _playerDamageDealer = FindObjectOfType<PlayerDamageDealer>();
        _playerDamageReceiver = FindObjectOfType<PlayerDamageReceiver>();

        // UI 
        var combatCanvas = UIManager.Instance.inGameCombatUI;
        var activeLegacyGroup = combatCanvas.transform.Find("ActiveLayoutGroup").transform;
        for (int i = 0; i < 4; i++)
        {
             var slot = activeLegacyGroup.Find("Slot_" + i);
             _activeLegacyIcons[i] = slot.Find("AbilityIcon").GetComponent<Image>();
             _activeLegacyOverlays[i] = slot.Find("Overlay").GetComponent<Image>();
             _activeLegacySlots[i] = slot.AddComponent<LegacySlotUI>();
        }
        _passiveLegacyGroup = combatCanvas.transform.Find("PassiveLayoutGroup").transform;
    }
    
    public void Init()
    {
        //Init_WarriorData();
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
        _warriors = new SWarrior[(int)EWarrior.MAX];
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

                EWarrior warrior = Enum.Parse<EWarrior>(data.Names[(int)ELocalisation.ENG].Replace(" ", ""));
                _warriors[(int)warrior] = data;
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
                    Names = names,
                    Descs = descs,
                    IconIndex = int.Parse(csv.GetField("IconIndex")),
                    Warrior = (EWarrior)Enum.Parse(typeof(EWarrior), csv.GetField("Warrior")),
                    Type = (ELegacyType)Enum.Parse(typeof(ELegacyType), csv.GetField("Type")),
                };

                // Prerequisites and Stats
                string prerequisites = csv.GetField("Prerequisites");
                data.PrerequisiteIDs = prerequisites == "" ? Array.Empty<int>() : Array.ConvertAll(prerequisites.Split(','), int.Parse);
                
                _legacies.Add(data.ID, data);
                _legaciesByWarrior[(int)data.Warrior].Add(data);
            }
        }
    }

    private void Init_LegacySOs()
    {
        _legacySODictionary = new Dictionary<int, LegacySO>();
        
        // Create scriptable object assets with values uninitialised
        for (int warrior = 0; warrior < (int)EWarrior.MAX; warrior++)
        {
            string basePath = "Assets/Resources/Legacies/" + (EWarrior)warrior + "/";
            foreach (var legacy in _legaciesByWarrior[warrior])
            {
                LegacySO legacyAsset = null;
                string assetPath = basePath + legacy.Names[(int)ELocalisation.KOR] + ".asset";
#if UNITY_EDITOR
                // Create scriptable object asset if file does not exist
                if (!File.Exists(assetPath))
                {
                    string className = legacy.Type == ELegacyType.Passive ? "PassiveLegacySO" : "Legacy_" + legacy.Type;
                    legacyAsset = (LegacySO)ScriptableObject.CreateInstance(className);
                    legacyAsset.SetWarrior(legacy.Warrior);
                    AssetDatabase.CreateAsset(legacyAsset, assetPath);
                }
#endif
                // Add asset to array
                if (legacyAsset == null)
                    legacyAsset = Resources.Load<LegacySO>("Legacies/"  + (EWarrior)warrior + "/" + legacy.Names[(int)ELocalisation.KOR]);

                // Save statistics in Define.cs?
                if (legacy.Type == ELegacyType.Passive)
                {
                    var asset = (PassiveLegacySO)legacyAsset;
                    if (asset.SavedInDefine)
                    {
                        var fieldInfo = typeof(Define).GetField($"{asset.BuffType}Stats");
                        if (fieldInfo != null) fieldInfo.SetValue(null, asset.Stats);
                    }
                }
                
                legacyAsset.SetWarrior((EWarrior)warrior);
                _legacySODictionary.Add(legacy.ID, legacyAsset);
            }
        }
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
    }

#region Ability Collection
    //------------------------------------------------------------------------------------
    public string GetBoundActiveLegacyName(ELegacyType legacyType)
    {
        int id = _boundActiveLegacyIDs[(int)legacyType];
        return id == -1 ? String.Empty : _legacies[id].Names[(int)Define.Localisation];
    }
    
    private bool IsLegacyCollected(int legacyIdx)
    {
        return _collectedLegacyIDs.Contains(legacyIdx);
    }
    
    public void CollectLegacy(int legacyID, ELegacyPreservation preservation)
    {
        var legacyData = _legacies[legacyID];
        int legacyTypeIdx = (int)legacyData.Type;
        var legacyAsset = _legacySODictionary[legacyID];
        Debug.Log("CollectLegacy: [" + preservation + "] " + legacyData.Names[1]);
        
        if (legacyData.Type == ELegacyType.Passive)
        {
            // Find and bind legacy SO
            BindPassiveLegacy(legacyID, (PassiveLegacySO)legacyAsset, preservation);
            _collectedPassiveIDs.Add(legacyID);
            
            // Update UI
            var slotObj = Instantiate(_passiveLegacySlotPrefab, _passiveLegacyGroup);
            slotObj.AddComponent<LegacySlotUI>().Init(legacyData.Warrior, legacyData.Names, legacyData.Descs);
            slotObj.GetComponent<Image>().sprite = GetLegacyIcon(legacyID);
        }
        else
        {
            // Find and bind legacy SO
            CheckRelevantPassiveLegacies((ActiveLegacySO)legacyAsset);
            _boundActiveLegacyIDs[legacyTypeIdx] = legacyID;
            _playerDamageDealer.AttackBases[legacyTypeIdx].BindActiveLegacy((ActiveLegacySO)legacyAsset, preservation);
            
            // Update VFX
            UpdateAttackVFX(legacyData.Warrior, legacyData.Type);  
            
            // Update UI
            _activeLegacyIcons[legacyTypeIdx].sprite = GetLegacyIcon(legacyID);
            _activeLegacyIcons[legacyTypeIdx].color = Color.white;
            _activeLegacyOverlays[legacyTypeIdx].fillAmount = 0;
            _activeLegacySlots[legacyTypeIdx].Init(legacyData.Warrior, legacyData.Names, legacyData.Descs);
        }
        _collectedLegacyIDs.Add(legacyID);
        _collectedLegacyPreservations.Add(legacyID, preservation);
    }

    private void BindPassiveLegacy(int legacyID, PassiveLegacySO legacySO, ELegacyPreservation preservation)
    {
        int preservationIdx = (int)preservation;
        switch (legacySO.BuffType)
        {
            case EBuffType.StatUpgrade:
                _playerController.UpgradeStat(legacyID, legacySO.StatUpgradeData, preservation);
                break;
            
            case EBuffType.SpawnAreaIncrease:
                foreach (var attackBase in _playerDamageDealer.AttackBases)
                {
                    if (attackBase.activeWarrior == legacySO.warrior)
                    {
                        attackBase.UpdateSpawnSize(legacySO.BuffIncreaseAmounts[preservationIdx], legacySO.BuffIncreaseMethod);
                    }
                }
                break;            
            
            case EBuffType.EnemyItemDropRate:
                break;
            
            case EBuffType.BindingSkillUpgrade:
                _playerDamageDealer.UpgradeStatusEffectLevel(legacySO.warrior, preservation, legacySO.Stats);
                break;
            
            case EBuffType.EuphoriaEnemyGoldDropBuff:
            case EBuffType.EuphoriaEcstasyUpgrade:
            case EBuffType.SommerHypHallucination:
            case EBuffType.TurbelaMaxButterfly:
            case EBuffType.TurbelaDoubleSpawn:
            case EBuffType.TurbelaButterflyCrit:
                _playerController.ActivateBuffByName(legacySO.BuffType, preservation);
                break;
        }
    }

    // 새로운 액티브 유산이 바인딩 되었을 경우, 이전에 바인딩 된 패시브에 따라 업그레이드 해야 할 부분이 있는지 확인한다.
    private void CheckRelevantPassiveLegacies(ActiveLegacySO newActiveLegacy)
    {
        foreach (int passiveID in _collectedPassiveIDs)
        {
            var legacySO = (PassiveLegacySO)_legacySODictionary[passiveID];
            if (legacySO.warrior != newActiveLegacy.warrior) continue;
            int passivePreservation = (int)_collectedLegacyPreservations[passiveID]; 
            
            switch (legacySO.BuffType)
            {
                case EBuffType.SpawnAreaIncrease:
                    foreach (var attackBase in _playerDamageDealer.AttackBases)
                    {
                        attackBase.UpdateSpawnSize(legacySO.BuffIncreaseAmounts[passivePreservation], legacySO.BuffIncreaseMethod);
                    }
                    break;
                
                // TODO
            }
        }
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

    public List<SLegacyData> GetBindableLegaciesByWarrior(EWarrior warrior)
    {
        // Function to check if all prerequisites are met
        bool ArePrerequisitesMet(SLegacyData legacyToCollect)
        {
            foreach (var prereq in legacyToCollect.PrerequisiteIDs)
            {
                if (IsLegacyCollected(prereq)) return true;
            }
            return legacyToCollect.PrerequisiteIDs.Length == 0;
        }
        
        // Return all collectable legacies
        var legacies = _legaciesByWarrior[(int)warrior];
        var bindables = legacies.Where(legacy => !IsLegacyCollected(legacy.ID) && ArePrerequisitesMet(legacy)).ToList();
        return bindables;
    }

    public Sprite GetLegacyIcon(int legacyID)
    {
        return _legacyIconSpriteSheet[_legacies[legacyID].IconIndex];
    }
#endregion
    
    public Texture2D GetWarriorVFXTexture(EWarrior warrior, EPlayerAttackType attack)
    {
        return _vfxTexturesByWarrior[(int)warrior][(int)attack];
    }

    public EStatusEffect GetWarriorStatusEffect(EWarrior warrior, int level)
    {
        return Define.StatusEffectByWarrior[(int)warrior, level];
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
                    // if (warrior == EWarrior.Vernon)
                    // {
                    //     main.startSize = 6.5f;
                    //     attackBase.VFXObject.transform.localPosition = new Vector3(0.04f, 0.945f, 0);
                    // }
                    // else
                    // {
                    //     main.startSize = 4f;
                    //     attackBase.VFXObject.transform.localPosition = new Vector3(-0.02f, 0.9f, 0);
                    // }
                    
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
