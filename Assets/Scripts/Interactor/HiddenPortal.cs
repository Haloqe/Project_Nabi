using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HiddenPortal : MonoBehaviour
{
    [SerializeField] private float spawnChance = 0.08f;
    [SerializeField] private Light2D portalLight2D;
    [SerializeField] private Collider2D portalCollider2D; 
    [SerializeField] private SpriteRenderer portalSpriteRenderer; 
    public float SpawnChance => spawnChance;
    private Coroutine _activeRevealCoroutine;
    private Coroutine _activeHideCoroutine;
    private int _playerInteractingPartsCount;
    
    private void Awake()
    {
        portalCollider2D.GetComponent<Portal>()._isHidden = true;
        portalCollider2D.enabled = false;
        portalSpriteRenderer.color = new Color(1, 1, 1, 0);
        portalLight2D.intensity = 0;
    }

    public bool ShouldSpawn()
    {
        return Random.value <= spawnChance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (++_playerInteractingPartsCount > 1) return;
        if (_activeHideCoroutine != null)
            StopCoroutine(_activeHideCoroutine);
        
        _activeRevealCoroutine = StartCoroutine(RevealCoroutine());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (--_playerInteractingPartsCount > 0) return;
        if (_activeRevealCoroutine != null)
            StopCoroutine(_activeRevealCoroutine);

        _activeHideCoroutine = StartCoroutine(HideCoroutine());
    }

    private IEnumerator RevealCoroutine()
    {
        // Gradually reveal portal
        float alpha = portalSpriteRenderer.color.a;
        while (alpha < 0.7f)
        {
            alpha += Time.unscaledDeltaTime * 0.4f;
            var portalColour = portalSpriteRenderer.color;
            portalSpriteRenderer.color = new Color(portalColour.r, portalColour.g, portalColour.b, alpha);
            portalLight2D.intensity = alpha;
            yield return null;
        }

        // Activate portal
        portalCollider2D.enabled = true;

        // Reveal the rest
        while (alpha < 1f)
        {
            alpha += Time.unscaledDeltaTime * 0.4f;
            var portalColour = portalSpriteRenderer.color;
            portalSpriteRenderer.color = new Color(portalColour.r, portalColour.g, portalColour.b, alpha);
            portalLight2D.intensity = alpha;
            yield return null;
        }
        
        _activeRevealCoroutine = null;
    }

    private IEnumerator HideCoroutine()
    {
        // Deactivate portal initially
        portalCollider2D.enabled = false;
        
        // Gradually hide portal
        var portalColour = portalSpriteRenderer.color;
        while (portalColour.a > 0)
        {
            portalColour.a -= Time.deltaTime * 1.2f;
            portalSpriteRenderer.color = portalColour;
            portalLight2D.intensity -= Time.deltaTime * 1.2f;
            yield return null;
        }
        _activeHideCoroutine = null;
    }
}