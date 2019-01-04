using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DrawGraphOnImage : MonoBehaviour
{

    public RawImage image;
    private Texture2D texture;
    private Color32[] resetColorArray;

    float testX = 0f;

    // Start is called before the first frame update
    void Start()
    {
        texture = new Texture2D(512, 512);

        // Reset all pixels color to transparent
        Color32 resetColor = new Color32(255, 255, 255, 255);
        resetColorArray = texture.GetPixels32();

        for (int i = 0; i < resetColorArray.Length; i++)
        {
            resetColorArray[i] = resetColor;
        }

        texture.SetPixels32(resetColorArray);

        image.texture = texture;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void clear()
    {
        texture.SetPixels32(resetColorArray);
        texture.Apply();
    }

    public void drawPoint(float inX, float inY, Color color)
    {
        // Calculate bounds for drawing
        float widthUpperbound = texture.width * 0.8f;
        float widthLowerbound = texture.width * 0.1f;
        float heightUpperbound = texture.height * 0.8f;
        float heightLowerbound = texture.height * 0.1f;

        // Apply bounds and Convert to int
        int x = Mathf.RoundToInt((inX * widthUpperbound) + widthLowerbound);
        int y = Mathf.RoundToInt((inY * heightUpperbound) + heightLowerbound);

        // Draw
        for (int i = -5; i < 5; i++ )
        {
            for (int j = -5; j < 5; j++)
            {
                texture.SetPixel(x + i, y + j, color);

            }
        }

        texture.Apply();
    }
}
