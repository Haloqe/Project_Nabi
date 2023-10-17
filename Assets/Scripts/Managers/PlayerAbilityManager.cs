using Enums.PlayerEnums;
using Structs.PlayerStructs;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
using Player.Abilities.Base;
using System;
using Player;
using Unity.VisualScripting;
using Player.Abilities;

namespace Managers
{
    public class PlayerAbilityManager : Singleton<PlayerAbilityManager>
    {
        private GameObject _abilityContainer;
        private PlayerInput _playerInput;
        private Dictionary<int, SAbilityData> _abilities;
        private static Dictionary<int, SAbilityData>[] _abilitiesByMetals;
        private ActiveAbilityBase[] _activeAbilities = new ActiveAbilityBase[5];
        private PassiveAbilityBase[] _passiveAbilities = new PassiveAbilityBase[3];
        
        public static Dictionary<int, SAbilityData>[] GetAbilitiesByMetals(EAbilityMetalType metalType)
        {
            return _abilitiesByMetals;
        }

        protected override void Awake()
        {
            base.Awake();   // singleton
            
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
            _abilityContainer = PlayerController.Instance.transform.Find("Abilities").gameObject;
        }

        // csvHelper에 곧장 클래스에 데이터 넣어주는 기능도 있지만 여기선 직접 넣는 방법 사용
        // NOTE THERE IS NO ERROR CHECKING OF THE DATA !!!!
        // The field names and data types must be correct and reasonable
        public void Init(string dataPath)
        {
            Debug.Log("Initialising Player Abilities");
            Debug.Assert(dataPath != null && File.Exists(dataPath));

            using (var reader = new StreamReader(dataPath))
            using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //var records = new List<Foo>();
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    SAbilityData data = new SAbilityData
                    {
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
         
                    _abilities.Add(data.Id, data);
                    _abilitiesByMetals[(int)data.MetalType].Add(data.Id, data);
                    Debug.Log("Ability [" + data.Name_EN + "][" + data.Id + "] added to the dictionary.");
                }
            }

            CreateAbilityScripts();
        }

        // TODO 배포시 첫 실행에서만 하도록 변경 (현재는 매 플레이 실행)
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
            Type t = Type.GetType("Player.Abilities." + _abilities[abilityIdx].ClassName);
            _activeAbilities[actionIdx] = (ActiveAbilityBase)_abilityContainer.AddComponent(t);

            // Set variables of the ability
            _activeAbilities[actionIdx]._data = _abilities[abilityIdx];
            _activeAbilities[actionIdx].Init();

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
            Type abilityType = Type.GetType("Player.Abilities." + 
                _abilities[_activeAbilities[actionIdx]._data.Id].ClassName);
            Destroy(_abilityContainer.GetComponent(abilityType));
            _activeAbilities[actionIdx] = null;
        }
    }
}