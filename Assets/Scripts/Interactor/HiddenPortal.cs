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
        if (++_playerInteractingPartsCount > 1) return;
        
        if (_activeHideCoroutine != null)
            StopCoroutine(_activeHideCoroutine);
        
        _activeRevealCoroutine = StartCoroutine(RevealCoroutine());
        Debug.Log("Start revealing");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (--_playerInteractingPartsCount > 0) return;
        
        if (_activeRevealCoroutine != null)
            StopCoroutine(_activeRevealCoroutine);

        _activeHideCoroutine = StartCoroutine(HideCoroutine());
        Debug.Log("End revealing");
    }

    private IEnumerator RevealCoroutine()
    {
        // Gradually reveal portal
        float alpha = _portalSpriteRenderer.color.a;
        while (alpha < 0.8f)
        {
            alpha += Time.unscaledDeltaTime * 0.4f;
            var portalColour = _portalSpriteRenderer.color;
            _portalSpriteRenderer.color = new Color(portalColour.r, portalColour.g, portalColour.b, alpha);
            yield return null;
        }

        // Activate portal
        _portalCollider2D.enabled = true;

        // Reveal the rest
        while (alpha < 1f)
        {
            alpha += Time.unscaledDeltaTime * 0.4f;
            var portalColour = _portalSpriteRenderer.color;
            _portalSpriteRenderer.color = new Color(portalColour.r, portalColour.g, portalColour.b, alpha);
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
        while (portalColour.a > 0)
        {
            portalColour.a -= Time.deltaTime * 1.2f;
            _portalSpriteRenderer.color = portalColour;
            yield return null;
        }
        _activeHideCoroutine = null;
    }
}