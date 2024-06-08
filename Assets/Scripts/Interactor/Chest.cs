using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : Interactor
{
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject soulShardPrefab;
    [SerializeField] private GameObject clockworkPrefab;
    [SerializeField] private GameObject superfoodPrefab;
    private int _rewardLevel;
    private Animator _animator;
    private bool _isOpen;
    private readonly static int Open = Animator.StringToHash("Open");

    protected override void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
    }
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        if (IsInteracting || _isOpen) return;
        IsInteracting = true;
        _isOpen = true;
        if (_animator == null) DropRewards();
        else _animator.SetTrigger(Open);
    }

    public void ResetReward(int roomLevel)
    {
        _rewardLevel = roomLevel;
        gameObject.SetActive(false);
    }
    
    public void DropRewards()
    {
        switch (_rewardLevel)
        {
            case 0:
                {
                    // Fixed drop
                    DropGold(30, 40);
                }
                break;
            case 1:
                {
                    // Fixed drop
                    DropGold(100, 120);
                    
                    // Random drop
                    if (Random.value <= 0.4f) DropSoulShard(1,3);   // 60% Soul shard
                }
                break;
            case 2:
                {
                    // Fixed drop
                    DropGold(300, 350);
                    DropSoulShard(3, 4);
                    
                    // Random drop
                    float r = Random.value;
                    DropSuperfood();
                    if (r <= 0.5f) DropArbor();                           // 50% Arbor
                    else if (r <= 0.8f) DropSuperfood();                         // 30% Superfood
                    else if (r <= 0.95f) DropClockwork(isPristine:false);   // 15% Clockwork
                    else DropClockwork(isPristine:true);                    // 5% Pristine clockwork
                }
                break;
        }
    }
    
    private void DropGold(int minRange, int maxRange)
    {
        // Calculate how much gold to drop
        int goldToDrop = Random.Range(minRange, maxRange + 1);
        
        // Set force depending on reward level
        float goldForce = _rewardLevel switch
        {
            0 => 6f,
            1 => 8f,
            2 => 11f,
        };

        // Drop coins
        for (int i = 0; i < goldToDrop / 5 + 1; i++) 
        {   
            var coin = Instantiate(goldPrefab, transform.position, Quaternion.identity).GetComponent<Gold>();
            coin.value = (i < goldToDrop / 5) ? 5 : goldToDrop % 5;
            coin.SetForce(goldForce);
        }
        
        // Display UI
        UIManager.Instance.DisplayGoldPopUp(goldToDrop);
    }

    private void DropSoulShard(int minRange, int maxRange)
    {
        int amountToDrop = Random.Range(minRange, maxRange + 1);
        float angleStep = 180f / Mathf.Max(1, amountToDrop - 1);
        float currentAngle = amountToDrop == 1 ? 0 : -90f;

        for (int i = 0; i < amountToDrop; i++)
        {
            GameObject soulShard = Instantiate(soulShardPrefab, transform.position + new Vector3(0,0.1f, 0), Quaternion.identity);
            Rigidbody2D rb = soulShard.GetComponent<Rigidbody2D>();
            Vector2 direction = new Vector2(Mathf.Sin(currentAngle * Mathf.Deg2Rad), Mathf.Abs(Mathf.Sin(currentAngle * Mathf.Deg2Rad)));
            rb.AddForce(direction * 5f, ForceMode2D.Impulse);
            currentAngle += angleStep;
        }
    }

    private void DropArbor()
    {
        GameObject arbor = Instantiate(EnemyManager.Instance.GetRandomArbor(), transform.position, Quaternion.identity);
        var randDir = Random.onUnitSphere;
        randDir = new Vector3(randDir.x, Mathf.Abs(randDir.y), 0);
        arbor.GetComponent<Rigidbody2D>().AddForce(randDir * Random.Range(7,10), ForceMode2D.Impulse);
        arbor.GetComponent<Arbor>().tensionController = UIManager.Instance.TensionController;
    }

    private void DropClockwork(bool isPristine)
    {
        var clockwork = Instantiate(clockworkPrefab, transform.position + 
            new Vector3(Random.Range(-1.5f, 1.5f),2f, 0), Quaternion.identity).GetComponent<Clockwork>();
        clockwork.warrior = (EWarrior)Random.Range(0, (int)EWarrior.MAX);
        clockwork.isPristineClockwork = isPristine;
    }

    private void DropSuperfood()
    {
        GameObject superfood = Instantiate(superfoodPrefab, transform.position + new Vector3(0,0.1f, 0), Quaternion.identity);
        Rigidbody2D rb = superfood.GetComponent<Rigidbody2D>();
        var randDir = Random.onUnitSphere;
        randDir = new Vector3(randDir.x, Mathf.Abs(randDir.y), 0);
        rb.AddForce(randDir * Random.Range(5, 10), ForceMode2D.Impulse);
    }
}
