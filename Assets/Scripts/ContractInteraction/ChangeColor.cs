using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{   
    public float timer = 0.0f; 
    public SpriteRenderer spriteRenderer;
    public Color newColor;
    
    // Start is called before the first frame update
    void Start()
    {   
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
		if (timer >= 1.5f)//change the float value here to change how long it takes to switch.
		{
			// pick a random color
			newColor = new Color( Random.value, Random.value, Random.value, 1.0f );
			// apply it on current object's material
			spriteRenderer.color = newColor;
			timer = 0;
		}
 
    }
}
