using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ClockworkSpawner : Interactor
{
    public bool isFixedSpawn;
    private EWarrior _warrior;

    private void Awake()
    {
        PlayerEvents.Spawned += Init;
    }

    private void Init()
    {
        // Choose a random warrior
        int warrior = Random.Range(0, (int)EWarrior.MAX);
        _warrior = (EWarrior)warrior;
        
        // Change visual to match the warrior
        var psMain = GetComponent<ParticleSystem>().main;
        var color = psMain.startColor;
        color.colorMax = Define.WarriorMainColours[warrior];
        psMain.startColor = color;
    }
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        if (_isInteracting) return;
        _isInteracting = true;

        StartCoroutine(InteractCoroutine());
    }

    private IEnumerator InteractCoroutine()
    {
        // Interaction effect
        var ps = GetComponent<ParticleSystem>();
        var psMain = ps.main;
        var velocity = ps.velocityOverLifetime;
        psMain.loop = false;
        psMain.startLifetime = 0.8f;
        velocity.enabled = true;
        yield return new WaitForSeconds(0.8f);
        
        // Spawn a clockwork
        var clockwork = Instantiate(PlayerAttackManager.Instance.ClockworkPrefab, 
            new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), quaternion.identity);
        clockwork.GetComponent<Clockwork>().warrior = _warrior;
        var spriteRenderer = clockwork.GetComponent<SpriteRenderer>();
        var startColour = spriteRenderer.color;
        spriteRenderer.color = new Color(startColour.r, startColour.g, startColour.b, 0.0f); 
        while (spriteRenderer && spriteRenderer.color.a < 1.0f)
        {
            spriteRenderer.color = new Color(startColour.r, startColour.g, startColour.b, spriteRenderer.color.a + 0.006f);
            yield return null;
        }
        
        // Destroy this spawner
        Destroy(this);
    }

    private void OnDestroy()
    {
        PlayerEvents.Spawned -= Init;
    }
}