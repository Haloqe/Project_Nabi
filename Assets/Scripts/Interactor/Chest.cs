using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : Interactor
{
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject soulShardPrefab;
    [SerializeField] private GameObject clockworkPrefab;
    private int _rewardLevel;
    private Animator _animator;
    public bool IsOpen { get; private set; }
    private readonly static int Open = Animator.StringToHash("Open");
    private readonly static int Close = Animator.StringToHash("Close");

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        if (IsInteracting || IsOpen) return;
        IsInteracting = true;
        IsOpen = true;
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
                    DropGold(20, 50);
                }
                break;
            case 1:
                {
                    // Fixed drop
                    DropGold(100, 150);
                    
                    // Random drop
                    float r = Random.value;
                    if (r <= 0.6f) DropSoulShard(3, 5);   // 60% Soul shard
                    else if (r <= 0.90f) DropArbor();     // 30% Arbor
                    else DropClockwork(isPristine:false); // 10% Clockwork
                }
                break;
            case 2:
                {
                    // Fixed drop
                    DropGold(300, 500);
                    DropSoulShard(3, 5);
                    
                    // Random drop
                    float r = Random.value;
                    if (r <= 0.5f) DropArborScroll();
                    else DropClockwork(isPristine:true);
                }
                break;
        }
    }
    
    private void DropGold(int minRange, int maxRange)
    {
        // Calculate how much gold to drop
        int goldToDrop = Random.Range(minRange, maxRange + 1);
        
        // Drop coins
        UIManager.Instance.DisplayGoldPopUp(goldToDrop);
        for (int i = 0; i < goldToDrop / 5 + 1; i++) 
        {   
            // Instantiate a coin at the enemy's position
            var coin = Instantiate(goldPrefab, transform.position, Quaternion.identity).GetComponent<Gold>();
            coin.value = (i < goldToDrop / 5) ? 5 : goldToDrop % 5;
            coin.SetForce(11f);
        }
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
        
    }

    private void DropClockwork(bool isPristine)
    {
        var clockwork = Instantiate(clockworkPrefab, transform.position + 
            new Vector3(Random.Range(-1.5f, 1.5f),2f, 0), Quaternion.identity).GetComponent<Clockwork>();
        clockwork.warrior = (EWarrior)Random.Range(0, (int)EWarrior.MAX);
        clockwork.isPristineClockwork = isPristine;
    }

    private void DropArborScroll()
    {
        
    }
}
