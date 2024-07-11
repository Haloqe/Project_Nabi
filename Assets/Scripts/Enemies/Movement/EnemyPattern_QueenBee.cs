using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class EnemyPattern_QueenBee : EnemyPattern
{
    [SerializeField] private GameObject _dustParticle;
    private BossHealthBar _bossHealthBar;
    private SpriteRenderer _spriteRenderer;
    private GameObject _attackHitbox;
    private GameObject _defaultParticleObject;
    private float _maxHealth;
    private bool _isBouncing = true;
    private bool _beesAreCommanded;
    private bool _isInAttackSequence;
    private bool _justFinishedAttack = true;
    private Object _spawnVFXPrefab;
    private GameObject _bombObject;
    private EnemyManager _enemyManager;
    private Vector3[] _bombPositions = new Vector3[13];

    private float _leftMostPosition = -28f;
    private float _rightMostPosition = 4f;
    private float _bottomMostPosition = -11f;
    private float _topMostPosition = -1f;

    [SerializeField] private AudioSource[] _audioSources;
    [SerializeField] private AudioClip _battleCryAudio;
    [SerializeField] private AudioClip _bodySlamTelegraphAudio;
    [SerializeField] private AudioClip _bodySlamSequence;
    [SerializeField] private AudioClip _poisonTelegraphAudio;
    [SerializeField] private AudioClip _poisonExplosionAudio;
    [SerializeField] private AudioClip _dyingAudio;
    [SerializeField] private AudioClip _deathEnd;
    
    // Damage information
    private AttackInfo _contactAttackInfo;
    private AttackInfo _bodySlamAttackInfo;

    // Cutscenes
    private PlayableDirector _encounterTimeline;
    private PlayableDirector _deathTimeline;
    private bool _isInCutscene = true;
    
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");

    private void Awake()
    {
        MoveType = EEnemyMoveType.QueenBee;
        _spawnVFXPrefab = Resources.Load("Prefabs/Effects/SpawnPoofVFX");
        _bombObject = Resources.Load<GameObject>("Prefabs/Enemies/Spawns/QueenBee_bomb");
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _attackHitbox = GameObject.Find("AttackHitbox");
        _defaultParticleObject = GameObject.Find("defaultVFX");
        Init();
    }

    public override void Init()
    {
        base.Init();
        StartCoroutine(Bounce());
        _enemyManager = EnemyManager.Instance;
        _bossHealthBar = Instantiate(Resources.Load<GameObject>("Prefabs/UI/InGame/BossHealthUI"),
            Vector3.zero, Quaternion.identity).GetComponentInChildren<BossHealthBar>();
        _maxHealth = _enemyBase.EnemyData.MaxHealth;
        _encounterTimeline = GameObject.Find("Encounter Timeline").GetComponent<PlayableDirector>();
        _deathTimeline = GameObject.Find("Death Timeline").GetComponent<PlayableDirector>();

        float gap = 3f;
        for (int i = 0; i < _bombPositions.Length; i++)
        {
            _bombPositions[i] = new Vector3(_leftMostPosition + gap * i, _bottomMostPosition, 0);
        }
        
        StartCoroutine(StartEncounterTimeline());
    }

    private void Start()
    {
        _contactAttackInfo = _enemyBase.DamageInfo;
        _bodySlamAttackInfo = new AttackInfo()
        {
            Damage = new DamageInfo(EDamageType.Base, 40, 0),
            StatusEffects = new List<StatusEffectInfo>(),
        };
    }

    private IEnumerator StartEncounterTimeline()
    {
        Light2D playerLight = _player.GetComponentInChildren<Light2D>();
        playerLight.enabled = false;
        _attackHitbox.SetActive(false);
        _defaultParticleObject.SetActive(false);
        _spriteRenderer.enabled = false;
        SpriteRenderer playerRenderer = _player.GetComponent<SpriteRenderer>();
        playerRenderer.enabled = false;
        _encounterTimeline.Play();
        
        yield return new WaitForSeconds(10f);
        
        playerRenderer.enabled = true;
        _defaultParticleObject.SetActive(true);
        _spriteRenderer.enabled = true;
        _attackHitbox.SetActive(true);
        playerLight.enabled = true;
        PlayerController.Instance.EnablePlayerInput();
        _isInCutscene = false;
    }
    
    private void PlayAudio(int audioSourceIdx, AudioClip audioClip, float pitchRange = 0f, float volume = 1f)
    {
        _audioSources[audioSourceIdx].pitch = Random.Range(1 - pitchRange, 1 + pitchRange);
        _audioSources[audioSourceIdx].volume = volume;
        _audioSources[audioSourceIdx].PlayOneShot(audioClip);
    }

    public override void Attack()
    {
        if (_isInAttackSequence || _isInCutscene) return;
        
        if (_justFinishedAttack)
        {
            StartCoroutine(Idle());
            return;
        }
        
        EnemyPattern_Bee[] allBees = FindObjectsOfType<EnemyPattern_Bee>();
        bool lessThanTwoBees = allBees.Length <= 1;
        if (lessThanTwoBees)
        {
            StartCoroutine(SpawnMinions(Random.Range(3, 5)));
            return;
        }
        
        if (_enemyBase.Health <= _maxHealth * 0.5f && !_beesAreCommanded)
        {
            StartCoroutine(BattleCry());
            return;
        }
        
        StartCoroutine(Generate3RandomAttacks());
    }
    
    private IEnumerator Generate3RandomAttacks()
    {
        _isInAttackSequence = true;
        
        for (int i = 0; i <= 2; i++)
        {
            switch (Random.Range(0, 2))
            {
                case 0:
                yield return BodySlam();
                break;

                case 1:
                yield return PoisonBomb();
                break;
            }
        }

        _justFinishedAttack = true;
        _isInAttackSequence = false;
    }

    private IEnumerator MoveToPosition(Vector3 destination, float speed, bool facingTarget = true)
    {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, _leftMostPosition, _rightMostPosition),
            Mathf.Clamp(transform.position.y, _bottomMostPosition, _topMostPosition), 0f);
        Vector3 clampedDestination = 
            new Vector3(Mathf.Clamp(destination.x, _leftMostPosition, _rightMostPosition), 
                Mathf.Clamp(destination.y, _bottomMostPosition, _topMostPosition), 0);
        Vector3 moveDirection = (clampedDestination - transform.position).normalized;
        while (!IsCloseEnough(gameObject, clampedDestination)
               && transform.position.x >= _leftMostPosition
               && transform.position.x <= _rightMostPosition
               && transform.position.y >= _bottomMostPosition
               && transform.position.y <= _topMostPosition)
        {
            _rigidBody.velocity = moveDirection * speed;
            if (facingTarget) FlipEnemyTowardsTarget();
            else FlipEnemyTowardsMovement();
            yield return null;
        }
    }

    private IEnumerator MoveToAttackPosition()
    {
        int directionFacing = 1;
        if (_player.transform.position.x > transform.position.x) 
            directionFacing *= -1;
        Vector3 position = _player.transform.position + 
                           new Vector3(directionFacing * 10f, 2f, 0);
        yield return MoveToPosition(position, MoveSpeed);
    }

    private bool IsCloseEnough(GameObject obj, Vector3 pos)
    {
        if (!(Vector3.Distance(obj.transform.position, pos) < 0.3f))
            return false;
        obj.transform.position = pos;
        return true;
    }
    
    private IEnumerator Idle()
    {
        _isInAttackSequence = true;
        _animator.SetBool(IsAttacking, false);
        
        int directionFacing = 1;
        if (_player.transform.position.x > transform.position.x)
            directionFacing *= -1;
        Vector3 idlePosition = _player.transform.position + 
                               new Vector3(directionFacing * 7f, 2f, 0);
        yield return MoveToPosition(idlePosition, MoveSpeed);
        yield return new WaitForSeconds(3f);
        
        _justFinishedAttack = false;
        _isInAttackSequence = false;
    }

    private IEnumerator Bounce()
    {
        float speed = 3f;
        int direction = 1;
        while (_isBouncing)
        {
            var force = new Vector2(0f, direction * speed * 0.5f);
            _rigidBody.AddForce(force, ForceMode2D.Impulse);
            yield return new WaitForSeconds(3f / speed);
            direction *= -1;
        }
    }

    private IEnumerator SpawnMinions(int spawnAmount)
    {
        _isInAttackSequence = true;
        
        yield return MoveToAttackPosition();
        yield return new WaitForSeconds(1f);
        PlayAudio(0, _battleCryAudio, 0.15f);
        _animator.SetBool(IsAttacking, true);
        _animator.SetInteger(AttackIndex, 1);
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, false);
        
        Vector3 spawnLocation = _player.transform.position;
        for (int i = 0; i < spawnAmount; i++)
        {
            spawnLocation += new Vector3(Random.Range(0, 3f), Random.Range(0, 3f), 0);
            _enemyManager.SpawnEnemy(2, spawnLocation, true)
                .GetComponent<EnemyPattern_Bee>().SendQueenSpawnedInfo();
            Instantiate(_spawnVFXPrefab, spawnLocation, Quaternion.identity);
        }

        _justFinishedAttack = true;
        _isInAttackSequence = false;
    }

    private IEnumerator PoisonBomb()
    {
        yield return MoveToAttackPosition();
        yield return new WaitForSeconds(1f);
        PlayAudio(0, _poisonTelegraphAudio, 0.15f);
        _animator.SetBool(id: IsAttacking, true);
        _animator.SetInteger(AttackIndex, 3);
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, false);

        foreach (Vector3 position in _bombPositions)
        {
            Instantiate(_bombObject, position, Quaternion.identity);
        }

        yield return new WaitForSeconds(1f);
        PlayAudio(1, _poisonExplosionAudio, 0.1f);
    }

    private IEnumerator BodySlam()
    {
        _isBouncing = false;
        
        // TODO: 이거 정확하게 돌진 시작하는 부분에서 인포 업데이트 해주라! 끝날때도 마찬가지.
        _enemyBase.UpdateAttackInfo(_bodySlamAttackInfo);
        
        int directionFacing = 1;
        if (_player.transform.position.x > transform.position.x) directionFacing *= -1;
        Vector3 startPosition = _player.transform.position + new Vector3(directionFacing * 7f, 0, 0);
        Vector3 finalPosition = startPosition - new Vector3(directionFacing * 5f, 0, 0);
        
        yield return MoveToPosition(startPosition, MoveSpeed * 1.3f);
        _rigidBody.velocity = new Vector2(0f, 0f);
        PlayAudio(0, _bodySlamTelegraphAudio, 0.1f);
        _animator.SetBool(IsAttacking, true);
        _animator.SetInteger(AttackIndex, 2);
        
        _enemyBase.UpdateAttackInfo(_contactAttackInfo);
        yield return new WaitForSeconds(0.8f);
        
        float dashSpeed = MoveSpeed * 5f;
        _dustParticle.SetActive(true);
        _dustParticle.transform.localScale = new Vector3(-directionFacing, 1, 1);
        PlayAudio(1, _bodySlamSequence, 0.1f);
        _enemyBase.UpdateAttackInfo(_bodySlamAttackInfo);
        yield return MoveToPosition(finalPosition, dashSpeed, false);

        _animator.SetBool(IsAttacking, false);
        _enemyBase.UpdateAttackInfo(_contactAttackInfo);
        yield return new WaitForSeconds(2f);

        _isBouncing = true;
        StartCoroutine(Bounce());
    }

    private IEnumerator BattleCry()
    {
        _isInAttackSequence = true;
        
        yield return MoveToAttackPosition();
        yield return new WaitForSeconds(1f);
        PlayAudio(0, _battleCryAudio, 0.15f);
        _animator.SetBool(IsAttacking, true);
        _animator.SetInteger(AttackIndex, 1);
        yield return new WaitForSeconds(1f);
        _animator.SetBool(IsAttacking, false);
        
        EnemyPattern_Bee[] allBeesScript = FindObjectsOfType<EnemyPattern_Bee>();
        GameObject[] allBees = new GameObject[allBeesScript.Length];
        for (int i = 0; i < allBeesScript.Length; i++)
        {
            allBees[i] = allBeesScript[i].gameObject;
        }

        int attackOrDefense = Random.Range(0, 2);
        _beesAreCommanded = true;

        switch (attackOrDefense)
        {
            case 0:
            foreach (GameObject b in allBees)
            {
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                EnemyPattern beeMovement = b.GetComponent<EnemyPattern>();
                GameObject attackCommandVFX = b.transform.Find("AttackCommandVFX").gameObject;
                beeMovement.ChangeSpeedByPercentage(1.3f);
                beeBase.ChangeAttackSpeedByPercentage(1.3f);
                attackCommandVFX.SetActive(true);
            }
            break;

            case 1:
            foreach(GameObject b in allBees)
            {
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                GameObject defenseCommandVFX = b.transform.Find("DefenseCommandVFX").gameObject;
                beeBase.ChangeArmourByPercentage(1.3f);
                defenseCommandVFX.SetActive(true);
            }
            break;
        }

        _justFinishedAttack = true;
        _isInAttackSequence = false;
        
        yield return new WaitForSeconds(15f);
        _beesAreCommanded = false;

        switch (attackOrDefense)
        {
            case 0:
            foreach (GameObject b in allBees)
            {
                if (b == null) continue;
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                EnemyPattern beeMovement = b.GetComponent<EnemyPattern>();
                GameObject attackCommandVFX = b.transform.Find("AttackCommandVFX").gameObject;
                
                beeMovement.ResetMoveSpeed();
                beeBase.ResetAttackSpeed();
                attackCommandVFX.SetActive(false);
            }
            break;

            case 1:
            foreach(GameObject b in allBees)
            {
                if (b == null) continue;
                EnemyBase beeBase = b.GetComponent<EnemyBase>();
                GameObject defenseCommandVFX = b.transform.Find("DefenseCommandVFX").gameObject;
                beeBase.ResetArmour();
                defenseCommandVFX.SetActive(false);
            }
            break;
        }
    }
    
    public override void OnTakeDamage(float damage, float maxHealth)
    {
        _bossHealthBar.OnBossHPChanged(damage / maxHealth);
    }

    public override void OnDeath()
    {
        _bossHealthBar.transform.root.gameObject.SetActive(false);
        _isInCutscene = true;
        StopAllCoroutines();
        transform.Find("VFXs").gameObject.SetActive(false);
        StartCoroutine(OnDeathCoroutine());
        EnemyPattern_Bee[] allBeesScript = FindObjectsOfType<EnemyPattern_Bee>();
        foreach (var b in allBeesScript)
        {
            b.gameObject.GetComponent<EnemyBase>().Die();
        }
        CameraManager.Instance.AllVirtualCameras[5].GetComponent<TestCameraShake>().OnMidBossDeath(8f);
    }

    private IEnumerator OnDeathCoroutine()
    {
        Vector3 deathPosition = new Vector3(-9f, -4.5f, 0);
        yield return MoveToPosition(deathPosition, MoveSpeed * 5, false);
        GameObject.Find("QueenBee Mannequin").transform.position = deathPosition;
        _attackHitbox.SetActive(false);
        _defaultParticleObject.SetActive(false);
        _spriteRenderer.enabled = false;
        
        _deathTimeline.Play();
        _audioSources[1].loop = true;
        _audioSources[1].clip = _dyingAudio; 
        _audioSources[1].Play();
        
        yield return new WaitForSeconds(8f);

        _audioSources[1].Stop();
        FindObjectOfType<Portal>(true).gameObject.SetActive(true);
    }
    
    public override bool PlayerIsInAttackRange()
    {
        return true;
    }

    public override bool PlayerIsInDetectRange()
    {
        return false;
    }
}
