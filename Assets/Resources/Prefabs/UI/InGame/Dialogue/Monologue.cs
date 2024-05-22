using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class Monologue : MonoBehaviour
{
    public GameObject MonologuePanel;
    public TextMeshProUGUI MonologueText;
    public GameObject parentObject;
    public string[] MonologueVariant;
    private int index;

    public Vector3 offset;
    public float wordSpeed;
    public bool playerIsClose;
    Camera uiCamera;

    private void Start()
    {
        var temp = gameObject.GetComponentInParent<Transform>().position;
        Debug.Log(temp);
        uiCamera = GameObject.Find("UI Camera").GetComponent<Camera>();
        
        var position = uiCamera.WorldToScreenPoint(temp + offset);
        MonologuePanel.transform.position = position;

    }

    void Display()
    {
        MonologuePanel.SetActive(true);
        var element = MonologueVariant[UnityEngine.Random.Range(0, MonologueVariant.Length)];
        MonologueText.text = element.ToString();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("hello");
        MonologuePanel.SetActive(true);
        if (collision.CompareTag("Player"))
        {
            InvokeRepeating("Display", 3.0f, 3.0f);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        MonologuePanel.SetActive(false);
    }
}
