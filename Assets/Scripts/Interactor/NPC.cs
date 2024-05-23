using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public GameObject interlocutorNamePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI interlocutorNameText;
    public string[] dialogue;
    private int index;

    public float wordSpeed;
    public bool playerIsClose;

    // Update is called once per frame
    private void Start()
    {
       
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)&& playerIsClose)
        {
            if (dialoguePanel.activeInHierarchy)
            {
                Reset();
            }
            else
            {
                dialoguePanel.SetActive(true);
                StartCoroutine(Typing());
            }
        }
    }
    public void NextLine()
    {
        if(index < dialogue.Length - 1) {
            index++;
            dialogueText.text = "";
            StartCoroutine(Typing());
        }
        else
        {
            Reset();
        }
    }

    public void Reset()
    {
        dialogueText.text = "";
        index = 0;
        dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }
    IEnumerator Typing()
    {
        foreach(char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = false;
            Reset();
        }
    }
}
