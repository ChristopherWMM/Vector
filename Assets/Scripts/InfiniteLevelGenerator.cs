using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class InfiniteLevelGenerator : MonoBehaviour {

    public GameObject level;
    public GameObject[] obstacles;
    public GameObject finish;

    public GameObject oldLevel;

    private GameObject newLevel;
    private static int levelWidth = 100;
    public static int levelDifficulty = 3;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag.Equals("Player")) {
            if (oldLevel != null) {
                Destroy(oldLevel.gameObject);
            }

            Vector3 position = new Vector3(transform.root.position.x + levelWidth, transform.root.position.y, transform.root.position.z);
            Quaternion rotation = transform.rotation;

            GameObject newLevel = Instantiate(level, position, rotation);
            newLevel.name = "Level";
            newLevel.transform.Find("Triggers").GetComponentInChildren<InfiniteLevelGenerator>().oldLevel = transform.root.gameObject;

            for (int x = 0; x < newLevel.transform.Find("Obstacles").transform.childCount; x++) {
                Destroy(newLevel.transform.Find("Obstacles").transform.GetChild(x).gameObject);
            }

            if (levelDifficulty > 7)
            {
                // Do nothing.
            }
            else if (levelDifficulty == 7)
            {
                int offset = levelWidth / 5;

                Vector3 finishPosition = new Vector3(transform.root.position.x + offset, transform.root.position.y, transform.root.position.z);
                Quaternion finishRotation = transform.rotation;

                GameObject finishObject = Instantiate(finish, finishPosition, finishRotation);
                finishObject.name = "Finish";
                finishObject.transform.SetParent(newLevel.transform.Find("Obstacles"));

                int flip = (Random.Range(0, 2) * 2) - 1;

                Vector3 newScale = finish.transform.localScale;
                newScale.y *= flip;
                finishObject.transform.localScale = newScale;
            }
            else
            {
                for (int x = 0; x < levelDifficulty; x++)
                {
                    int offset = ((levelWidth / levelDifficulty) * x);

                    Vector3 obstaclePosition = new Vector3(transform.root.position.x + offset, transform.root.position.y, transform.root.position.z);
                    Quaternion obstacleRotation = transform.rotation;

                    int index = Random.Range(0, obstacles.Length);

                    GameObject obstacle = Instantiate(obstacles[index], obstaclePosition, obstacleRotation);
                    obstacle.name = "" + x;
                    obstacle.transform.SetParent(newLevel.transform.Find("Obstacles"));

                    int flip = (Random.Range(0, 2) * 2) - 1;

                    Vector3 newScale = obstacle.transform.localScale;
                    newScale.y *= flip;
                    obstacle.transform.localScale = newScale;
                }
            }
            

            levelDifficulty++;
        }
    }
}
