using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMannequin : MonoBehaviour
{
    public int AnimIndex = 0;
    private Animator _animator;
    public Slider Slider;

    // TEMP
    private Rigidbody2D _rb;
    private TrailRenderer _trailRenderer;
    private float _dashStrength = 7;


    void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
        _animator.SetInteger("AnimIndex", AnimIndex);
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _trailRenderer = gameObject.GetComponentInChildren<TrailRenderer>();
        if (AnimIndex == 4)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private void Update()
    {
        _animator.SetFloat("Multiplier", Slider.value);
    }

    private IEnumerator DashCoroutine()
    {
        while (true)
        {
            StartCoroutine(Dash());
            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator Dash()
    {
        Debug.Log("Dash: " + _animator.speed);
        _animator.SetInteger("AnimIndex", 4);
        float prevGravity = _rb.gravityScale;
        _rb.gravityScale = 0f;
        _rb.velocity = new Vector2(-transform.localScale.x * _dashStrength, 0f);
        _trailRenderer.emitting = true;
        yield return new WaitForSeconds(0.35f);

        Debug.Log("Set to idle");
        _animator.SetInteger("AnimIndex", -1);
        _trailRenderer.emitting = false;
        _rb.gravityScale = prevGravity;
        _rb.velocity = new Vector2(0f, 0f);
        transform.localScale = new Vector3(-transform.localScale.x,
            transform.localScale.y, transform.localScale.z);
    }

    public void FireBullet()
    {

    }
}
