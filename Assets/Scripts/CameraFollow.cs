using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public GameObject player;

    void Update () {
        Vector3 position = new Vector3(player.transform.position.x + 6.5f, 0, -10);

        transform.position = position;
	}
}
