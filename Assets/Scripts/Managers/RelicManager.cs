using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelicManager : Singleton<RelicManager>
{
    private Sprite[] _relicSprites;
    private List<SRelicData> _relics;        // 모든 성유물 리스트
    private List<SRelicData> _relicsStore;   // 상점 판매 성유물 리스트
    private List<SRelicData> _relicsField;   // 필드 드랍 성유물 리스트
    private List<bool> _relicsCollected;     // 성유물 획득 여부
    private List<bool> _relicsAppearedInStore; // 상점에 해당 성유물이 등장한 적 있는지 (ability Index 기준)

    protected override void Awake()
    {
        base.Awake();
        _relicSprites = Resources.LoadAll<Sprite>("Sprites/Relics/RelicsTempSpriteSheet");
        _relics = new List<SRelicData>();
        _relicsStore = new List<SRelicData>();
        _relicsField = new List<SRelicData>();
    }

    public void Init(string dataPath)
    {
        Debug.Assert(dataPath != null && File.Exists(dataPath));

        using (var reader = new StreamReader(dataPath))
        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                // Fill relic info
                SRelicData data = new SRelicData
                {
                    AbilityId = int.Parse(csv.GetField("Id")),
                    Name_EN = csv.GetField("Name_EN"),
                    Name_KO = csv.GetField("Name_KO"),
                    Des_EN = csv.GetField("Description_EN"),
                    Des_KO = csv.GetField("Description_KO"),
                    SpriteIndex = int.Parse(csv.GetField("SpriteIndex")),
                    Rarity = (EAbilityRarity)Enum.Parse(typeof(EAbilityRarity), csv.GetField("Rarity")),
                    Locations = Array.ConvertAll(csv.GetField("Locations").Split('|'), int.Parse),
                };

                var methods_str = csv.GetField("ObtainMethod").Split('|');
                foreach (var method_str in methods_str)
                {
                    data.ObtainMethod |= (EAbilityObtainMethod)Enum.Parse(typeof(EAbilityObtainMethod), method_str);
                }

                _relics.Add(data);
                if ((data.ObtainMethod & EAbilityObtainMethod.Field) != 0) _relicsField.Add(data);
                if ((data.ObtainMethod & EAbilityObtainMethod.Store) != 0) _relicsStore.Add(data);                
            }
        }

        int maxId = _relics[_relics.Count - 1].AbilityId + 1;
        _relicsCollected = new List<bool>(maxId);
        _relicsAppearedInStore = new List<bool>(maxId);
        for (int i = 0; i < maxId; i++)
        {
            _relicsCollected.Add(false);
            _relicsAppearedInStore.Add(false);
        }
    }

    /**
     * 필요한 기능:
     *  1. 상점에서 판매할 성유물
     *      상점에서 판매 가능한 성유물 중에서 확률 계산하여 성유물 선택. 등장한 적 있는 스킬은 판매 X
     *      공통: 현재 가지고 있지 않은 스킬
     *      
     *  2. 필드에서 드랍할 성유물
     *      필드에서 드랍 가능한 성유물 중에서 해당 맵에서 획득 가능한 성유물 중 확률 계산하여 선택
     *      공통: 현재 가지고 있지 않은 스킬
     *      
     *  3. 보스에서 드랍할 성유물
     *      각 보스에서 설정 - 현재는 X    
     *      
     *  4. 획득한 적 있는 성유물인지 -> 컬렉션/도전과제용
     */
    public List<SRelicData> RandomChooseRelic_Store(int count)
    {
        List<SRelicData> relicsStoreActive = new List<SRelicData>(); // 현재 상점에서 판매할 성유물 리스트
        if (count == 0) return new List<SRelicData>();

        // 1. 플레이어가 가지고 있거나 상점에 등장한 적 있는 스킬 제외
        var availableRelics = _relicsStore.Where(
            relic => PlayerAbilityManager.Instance.IsAbilityAlreadyBound(relic.AbilityId) == false
            && _relicsAppearedInStore[relic.AbilityId] == false).ToList();

        int num = Math.Min(count, availableRelics.Count);
        Debug.AssertFormat(num != 0, "No available relic to display in store!");
        System.Random random = new System.Random();
        List<SRelicData> availableRelicsWithRarity = new List<SRelicData>();

        while (relicsStoreActive.Count < num)
        {
            // 2. 등급 랜덤 선택
            availableRelicsWithRarity.Clear();
            while (availableRelicsWithRarity.Count == 0)
            {
                var rand = random.NextDouble() * Define.AbilityOccurrenceByRarity[3];
                var d = Array.FindIndex(Define.AbilityOccurrenceByRarity, ability => ability <= rand);

                EAbilityRarity rarity = (EAbilityRarity)Array.FindIndex(Define.AbilityOccurrenceByRarity, ability => ability >= rand);
                availableRelicsWithRarity = availableRelics.Where(relic => relic.Rarity == rarity
                                            && _relicsAppearedInStore[relic.AbilityId] == false).ToList();
            }

            // 3. 스킬 랜덤 선택
            int randIdx = random.Next(0, availableRelicsWithRarity.Count);
            SRelicData relic = availableRelicsWithRarity[randIdx];

            _relicsAppearedInStore[relic.AbilityId] = true;
            relicsStoreActive.Add(relic);
        }

        return relicsStoreActive;
    }

    // 한 맵에 여러개 등장할 경우 핸들링 필요
    public SRelicData RandomChooseRelic_Field()
    {
        // 1. 현 스테이지에서 얻을 수 있는 성유물 
        // Scene name format (temp): InGame_Stage_01_03
        int currStage = int.Parse(SceneManager.GetActiveScene().name.Split('_')[2]);
        var availableRelics = _relicsField.Where(relic => relic.Locations.Contains(currStage)).ToList();

        // 2. 플레이어가 가지고 있는 스킬 제외
        availableRelics = availableRelics.Where(
            relic => PlayerAbilityManager.Instance.IsAbilityAlreadyBound(relic.AbilityId) == false).ToList();

        // 3. 등급 랜덤 선택
        System.Random random = new System.Random();
        List<SRelicData> availableRelicsWithRarity = new List<SRelicData>();
        Debug.AssertFormat(availableRelics.Count != 0, "No available relic to spawn on the field!");
        
        while (availableRelicsWithRarity.Count == 0)
        {
            var rand = random.NextDouble() * Define.AbilityOccurrenceByRarity[3];
            EAbilityRarity rarity = (EAbilityRarity)Array.FindIndex(Define.AbilityOccurrenceByRarity, ability => ability >= rand);
            availableRelicsWithRarity = availableRelics.Where(relic => relic.Rarity == rarity).ToList();
        }

        // 4. 스킬 랜덤 선택
        int randIdx = random.Next(0, availableRelicsWithRarity.Count);
        SRelicData relic = availableRelicsWithRarity[randIdx];

        return relic;
    }

    public Sprite GetSprite(int abilityIdx)
    {
        return _relicSprites[abilityIdx];
    }
}
