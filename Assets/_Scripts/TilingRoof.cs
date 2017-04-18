using UnityEngine;

namespace ProceduralSceneGenerator
{
    public class TilingRoof : MonoBehaviour
    {
        public float scaleToTiles;
        private Material mat;

        void LateUpdate()
        {
            float scaleX = 1;
            float scaleY = 1;

            scaleY = (transform.lossyScale.y + transform.lossyScale.z);
            scaleX = transform.lossyScale.x;

            mat = GetComponent<Renderer>().material;
            mat.SetTextureScale("_MainTex",
                new Vector2(scaleX * scaleToTiles, scaleY * scaleToTiles));

        }
    }
}