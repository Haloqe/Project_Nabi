using System.Collections;
using UnityEngine;

public class HiddenPortal : MonoBehaviour
{
    private Collider2D _portalCollider2D;
    private SpriteRenderer _portalSpriteRenderer;
    private Coroutine _activeRevealCoroutine;
    private Coroutine _activeHideCoroutine;
    private int _playerInteractingPartsCount;
    
    private void Awake()
    {
        var portal = transform.Find("Portal");
        portal.GetComponent<Portal>()._isHidden = true;
        _portalCollider2D = portal.GetComponent<Collider2D>();
        _portalSpriteRenderer = portal.GetComponent<SpriteRenderer>();
        _portalCollider2D.enabled = false;
        _portalSpriteRenderer.color = new Color(1, 1, 1, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _playerInteractingPartsCount++;
        if (_playerInteractingPartsCount != 1) return;
        
        if (_activeHideCoroutine != null)
            StopCoroutine(_activeHideCoroutine);
        
        _activeRevealCoroutine = StartCoroutine(RevealCoroutine());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _playerInteractingPartsCount--;
        if (_playerInteractingPartsCount != 0) return;
        
        if (_activeRevealCoroutine != null)
            StopCoroutine(_activeRevealCoroutine);

        _activeHideCoroutine = StartCoroutine(HideCoroutine());
    }

    private IEnumerator RevealCoroutine()
    {
        // Gradually reveal portal
        var portalColour = _portalSpriteRenderer.color;
        while (portalColour.a < 0.9f)
        {
            portalColour.a += Time.unscaledDeltaTime * 0.4f;
            _portalSpriteRenderer.color = portalColour;
            yield return null;
        }
        
        // Activate portal
        _portalCollider2D.enabled = true;
        
        // Reveal the rest
        while (portalColour.a < 1f)
        {
            portalColour.a += Time.unscaledDeltaTime * 0.4f;
            _portalSpriteRenderer.color = portalColour;
            yield return null;
        }
        
        _activeRevealCoroutine = null;
    }

    private IEnumerator HideCoroutine()
    {
        // Deactivate portal initially
        _portalCollider2D.enabled = false;
        
        // Gradually hide portal
        var portalColour = _portalSpriteRenderer.color;
        while (portalColour.a < 0.9f)
        {
            portalColour.a -= Time.unscaledDeltaTime * 1.2f;
            _portalSpriteRenderer.color = portalColour;
            yield return null;
        }
        _activeHideCoroutine = null;
    }
}