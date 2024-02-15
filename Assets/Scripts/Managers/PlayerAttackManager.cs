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
    private PlayerInput _playerInput;
    private PlayerDamageDealer _playerDamageDealer;

    private List<SWarrior> _warriors;
    private Dictionary<int, SLegacyData> _legacies;
    private List<List<SLegacyData>> _legaciesByWarrior;
    
    private LegacySO[][] _activeLegacySOByWarrior; //probs discard later
    private Dictionary<int, LegacySO> _activeLegacySODictionary;
    
    private Texture2D[][] _vfxTexturesByWarrior;
    private GameObject[] _bulletsByWarrior;

    private HashSet<int> _collectedLegacyIDs;
    private int[] _boundActiveLegacyIDs;
    
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
        _boundActiveLegacyIDs = new int[] {-1,-1,-1};
        _passiveLegacySlotPrefab = Utility.LoadGameObjectFromPath("Prefabs/UI/InGame/PassiveLegacySlot");
    }

    public void InitInGameVariables()
    {
        _playerInput = FindObjectOfType<PlayerInput>();
        _playerDamageDealer = FindObjectOfType<PlayerDamageDealer>();

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
                string assetPath = basePath + legacy.Names[(int)ELocalisation.KOR] + ".asset";
                if (!File.Exists(assetPath))
                {
                    legacyAsset = (LegacySO)ScriptableObject.CreateInstance("Legacy_" + legacy.Type);
                    AssetDatabase.CreateAsset(legacyAsset, assetPath);
                }
#endif
                // Add asset to array
                if (legacyAsset == null)
                    legacyAsset = Resources.Load<LegacySO>("Legacies/"  + (EWarrior)warrior + "/" + legacy.Names[(int)ELocalisation.KOR]);
                legacyAsset.Warrior = legacy.Warrior;
                _activeLegacySOByWarrior[warrior][(int)legacy.Type] = legacyAsset;
                _activeLegacySODictionary.Add(legacy.ID, legacyAsset);
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
    
    public bool IsLegacyCollected(int legacyIdx)
    {
        return _collectedLegacyIDs.Contains(legacyIdx);
    }
    
    public void CollectLegacy(int legacyID)
    {
        var legacyData = _legacies[legacyID];
        Debug.Log("CollectLegacy: " + legacyData.Names[1]);
        
        if (legacyData.Type == ELegacyType.Passive)
        {
            // TODO Handle passive legacy
            
            // Update UI
            var slotObj = Instantiate(_passiveLegacySlotPrefab, _passiveLegacyGroup);
            slotObj.AddComponent<LegacySlotUI>().Init(legacyData.Warrior, legacyData.Names, legacyData.Descs);
            slotObj.GetComponent<Image>().sprite = GetLegacyIcon(legacyID);
        }
        else
        {
            // Find and bind legacy SO
            int legacyTypeIdx = (int)legacyData.Type;
            var legacyAsset = _activeLegacySODictionary[legacyID];
            _playerDamageDealer.AttackBases[legacyTypeIdx].BindActiveLegacy(legacyAsset);
            _boundActiveLegacyIDs[legacyTypeIdx] = legacyID;

            // Update VFX
            UpdateAttackVFX(legacyData.Warrior, legacyData.Type);  
            
            // Update UI
            _activeLegacyIcons[legacyTypeIdx].sprite = GetLegacyIcon(legacyID);
            _activeLegacyIcons[legacyTypeIdx].color = Color.white;
            _activeLegacyOverlays[legacyTypeIdx].fillAmount = 0;
            _activeLegacySlots[legacyTypeIdx].Init(legacyData.Warrior, legacyData.Names, legacyData.Descs);
        }
        _collectedLegacyIDs.Add(legacyID);
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
        // TODO prerequisite
        var legacies = _legaciesByWarrior[(int)warrior];
        var bindables = legacies.Where(legacy => !IsLegacyCollected(legacy.ID)).ToList();
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
