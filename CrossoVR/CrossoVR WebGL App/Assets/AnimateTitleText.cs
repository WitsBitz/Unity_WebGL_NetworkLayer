using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class AnimateTitleText : MonoBehaviour {

	private float sinTimer;
	private float colorTimer;
	private TextMeshProUGUI textMesh;
	private float originalFontSize;
	public Color[] colors;
         
     public int currentIndex = 0;
     private int nextIndex;
     
     public float changeColourTime = 2.0f;
     
     private float lastChange = 0.0f;
     
     void Start() 
	 {
         if (colors == null || colors.Length < 2)
             Debug.Log ("Need to setup colors array in inspector");
         
         nextIndex = (currentIndex + 1) % colors.Length;    
     }

	void Awake()
	{
		textMesh = GetComponent<TextMeshProUGUI>();
		originalFontSize = textMesh.fontSize;
	}
	
	void Update()
	{
		sinTimer += Time.deltaTime * 2;
		colorTimer += Time.deltaTime;

		textMesh.fontSize = originalFontSize + Mathf.Abs(Mathf.Sin(sinTimer)) / 4 * originalFontSize;

		if (colorTimer > changeColourTime) 
		{
		currentIndex = (currentIndex + 1) % colors.Length;
		nextIndex = (currentIndex + 1) % colors.Length;
		colorTimer = 0.0f;
		}
	    textMesh.color = Color.Lerp (colors[currentIndex], colors[nextIndex], colorTimer / changeColourTime );
	}	
}
