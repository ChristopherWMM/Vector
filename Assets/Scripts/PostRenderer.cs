using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PostRenderer : MonoBehaviour
{
    public Material _material;
    public bool distortion;
    public bool shift;

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // TV noise
        //_material.SetFloat("_OffsetNoiseX", Random.Range(0f, 0.6f));
        //float offsetNoise = _material.GetFloat("_OffsetNoiseY");
        //_material.SetFloat("_OffsetNoiseY", offsetNoise + Random.Range(-0.03f, 0.03f));

        if (shift)
        {
            if (Random.Range(0, 10) == 1)
            {
                _material.SetFloat("_OffsetPosY", Random.Range(-0.05f, 0.05f));
            }
        }
        else
        {
            // Vertical shift
            float offsetPosY = _material.GetFloat("_OffsetPosY");
            if (offsetPosY > 0.0f)
            {
                _material.SetFloat("_OffsetPosY", offsetPosY - Random.Range(0f, offsetPosY));
            }
            else if (offsetPosY < 0.0f)
            {
                _material.SetFloat("_OffsetPosY", offsetPosY + Random.Range(0f, -offsetPosY));
            }
        }

        // Channel color shift
        float offsetColor = _material.GetFloat("_OffsetColor");
        if (offsetColor > 0f)
        {
            _material.SetFloat("_OffsetColor", offsetColor - 0.001f);
        }
        //else if (Random.Range(0, 200) == 1)
        //{
        //    _material.SetFloat("_OffsetColor", Random.Range(0f, 0.05f));
        //}
        else if (_material.GetFloat("_OffsetColor") < 0)
        {
            _material.SetFloat("_OffsetColor", 0);
        }

        if (distortion)
        { 
            // Distortion
            if (Random.Range(0, 2) == 1)
            {
                _material.SetFloat("_OffsetDistortion", Random.Range(1f, 1000f));
            }
        }
        else
        {
                _material.SetFloat("_OffsetDistortion", 10000f);
        }
        
        Graphics.Blit(source, destination, _material);
    }
}