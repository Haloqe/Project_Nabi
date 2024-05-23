using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class Monologue : MonoBehaviour
{
    /*public GameObject MonologuePanel;
    public TextMeshProUGUI MonologueText;
    public GameObject parentObject;
   
    private int index;

    public Vector3 offset;
    public float wordSpeed;
    public bool playerIsClose;
    Camera uiCamera;*/

    public string[] MonologueVariant;
    public Transform chatTransform;
    public GameObject chatboxPrefab;

   /* private void Start()
    {
        var temp = gameObject.GetComponentInParent<Transform>().position;
        Debug.Log(temp);
        uiCamera = GameObject.Find("UI Camera").GetComponent<Camera>();

        var position = uiCamera.WorldToScreenPoint(temp + offset);
        MonologuePanel.transform.position = position;

    }*/

    void Display()
    {
        GameObject go = Instantiate(chatboxPrefab);
        go.GetComponent<ChatSystem>().OnDialogue(MonologueVariant, chatTransform);
        /*MonologuePanel.SetActive(true);
        var element = MonologueVariant[UnityEngine.Random.Range(0, MonologueVariant.Length)];
        MonologueText.text = element.ToString();*/
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("hello");

        if (collision.CompareTag("Player"))
        {
            
            Display();
        }
    }

   /* void OnTriggerExit2D(Collider2D collision)
    {
        
    }*/
}
