using System.Collections;
using System.Linq;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
        private Texture2D _renderTexture;
        private Transform _playerTransform;
        public Texture2D brushTexture;
        private Color[] _brushColours;
        
        private void Awake()
        {
                // Create new fog of war overlay texture
                var spriteRenderer = GetComponent<SpriteRenderer>();
                var originalSprite = spriteRenderer.sprite;
                var originalTexture = originalSprite.texture;
                _renderTexture = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, false, true);

                var originalColours = originalTexture.GetPixels();
                _renderTexture.SetPixels(originalColours);
                _renderTexture.Apply();
                
                Sprite sprite = Sprite.Create(_renderTexture, new Rect(0, 0, _renderTexture.width, _renderTexture.height), Vector2.one / 2f, originalSprite.pixelsPerUnit);
                spriteRenderer.sprite = sprite;
                
                // Get pixel data from the brush texture
                _brushColours = brushTexture.GetPixels();

                // Start revealing fow when the player is spawned
                PlayerEvents.Spawned += OnPlayerSpawned;
        }

        private void OnPlayerSpawned()
        {
                _playerTransform = PlayerController.Instance.transform;
                StartCoroutine(RevealFogCoroutine());
        }

        private void OnDestroy()
        {
                PlayerEvents.Spawned -= OnPlayerSpawned;
        }

        private IEnumerator RevealFogCoroutine()
        {
                var wait = new WaitForSecondsRealtime(0.3f);
                while (true)
                {
                        PaintCircle(_playerTransform.position + new Vector3(0,1,0));
                        yield return wait;
                }
        }

        private void PaintCircle(Vector3 worldCenter)
        {
                // Get the local position of the center of the circle
                Vector3 localCenter = transform.InverseTransformPoint(worldCenter) + new Vector3(_renderTexture.width / 2, _renderTexture.height / 2, 0);
                
                // Convert the local position of the center to pixel coordinates
                Vector3 textureScale = transform.localScale;
                int centerX = (int)(localCenter.x / textureScale.x);
                int centerY = (int)(localCenter.y / textureScale.y);

                // Calculate the pixel coordinates of the top-left corner of the brush
                int startX = centerX - brushTexture.width / 2;
                int startY = centerY - brushTexture.height / 2;

                // Create an array to hold the colors for the pixels in the circle
                Color[] renderColours = _renderTexture.GetPixels(startX, startY, brushTexture.width, brushTexture.height);
                
                // Apply the brush to the render texture
                for (int x = 0; x < brushTexture.width; x++)
                {
                        for (int y = 0; y < brushTexture.height; y++)
                        {
                                // Calculate the indices of the pixel in the textures
                                int brushIndex = y * brushTexture.width + x;
                                int renderIndex = y * brushTexture.width + x;

                                // Make sure the index is within the bounds of the renderColours array
                                if (renderIndex >= 0 && renderIndex < renderColours.Length)
                                { 
                                        renderColours[renderIndex] = new Color(0,0,0, Mathf.Min(_brushColours[brushIndex].a, renderColours[renderIndex].a));
                                }
                        }
                }

                // Apply the changes to the texture
                _renderTexture.SetPixels(startX, startY, brushTexture.width, brushTexture.height, renderColours);
                _renderTexture.Apply(false);
        }
}