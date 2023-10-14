using Enums.PlayerEnums;
using Structs.PlayerStructs;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

namespace Managers
{
    public class PlayerAbilityManager : Singleton<PlayerAbilityManager>
    {
        private PlayerInput _playerInput;
        private Dictionary<int, SAbilityData> _abilities;
        private Dictionary<int, SAbilityData>[] _abilitiesByMetals;

        protected override void Awake()
        {
            base.Awake();   // singleton

            _playerInput = GetComponent<PlayerInput>();
            _abilities = new Dictionary<int, SAbilityData>();
            _abilitiesByMetals = new[]
            {
                new Dictionary<int, SAbilityData>(),    // Copper
                new Dictionary<int, SAbilityData>(),    // Gold
            };
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
                        Name_EN = csv.GetField("Name_EN"),
                        Name_KO = csv.GetField("Name_KO"),
                        Des_EN = csv.GetField("Description_EN"),
                        Des_KO = csv.GetField("Description_KO"),
                        IconPath = csv.GetField("Icon"),
                        Type = (EAbilityType)Enum.Parse(typeof(EAbilityType), csv.GetField("Type")),
                        MetalType = (EAbilityMetalType)Enum.Parse(typeof(EAbilityMetalType), csv.GetField("MetalType")),
                        CoolDownTime = float.Parse(csv.GetField("CoolDownTime")),
                        LifeTime = float.Parse(csv.GetField("LifeTime"))
                    };
         
                    int id = int.Parse(csv.GetField("Id"));

                    _abilities.Add(id, data);
                    _abilitiesByMetals[(int)data.MetalType].Add(id, data);
                    Debug.Log("Ability [" + data.Name_EN + "][" + id + "] added to the dictionary.");
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
            // NOTE 현재 active나 passive가 아닌 경우는 고려하지 않음
            foreach (var ability in _abilities)
            {
                string temp = string.Empty;
                string ability_name = String.Concat(ability.Value.Name_EN.Where(c => !Char.IsWhiteSpace(c)));
                
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
            }
        }

        // TODO
        public void BindAbility(int keyIdx, int abilityIdx)
        {
            string actionName = "Cast_" + keyIdx;
            InputAction action = _playerInput.actions[actionName];
            action.ApplyBindingOverride(new InputBinding(
                //{overrideInteractions = }
            ));
        }
    }
}