using Tobii.Gaming;
using UnityEngine;

public class GazeDetection : MonoBehaviour {
    public GameObject[] gameObjects;

    void Update()
    {
        if (TobiiAPI.GetGazePoint().IsRecent())
        {
            ShowGraphic(true);
        }
        else
        {
            ShowGraphic(false);
        }
    }

    private void ShowGraphic(bool isVisible)
    {
        foreach (GameObject obj in gameObjects)
        {
            obj.SetActive(isVisible);
        }
    }
}
