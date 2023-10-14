using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

namespace Managers
{
    public class DataManager : Singleton<DataManager>
    {
        private PlayerAbilityManager _playerAbilityManager;

        // 한군데서 path 관리하는게 편할듯하여 datamanager -> init(path)식으로 짰는데
        // 하위 manager에서 관리하는식으로 변경할지도
        // 클래스 명 자체를 xxxTable로 하고 데이터만 저장하도록하여 path도 자동화하는 방안도 고민중
        string basePath = Application.dataPath + "/Tables/";

        protected override void Awake()
        {
            base.Awake();   // singleton

            // Awake에서 호출하는게 위험하지 않은지 확인 필요 (relative order of initialisations differ)
            _playerAbilityManager = GetComponent<PlayerAbilityManager>();
        }
        
        private void Start()
        {
            _playerAbilityManager.Init(basePath + "PlayerAbilitiesTable.csv");
        }
    }
}