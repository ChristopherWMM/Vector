using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Tobii.Gaming;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public int movementSpeed;
    public int startSpeed;
    public int forwardSpeed;

    public int currentCombo = 0;
    public int yourBestCombo = 0;
    public float time = 0;
    public string timeString = "";

    public string currentColor;

    public GameObject gameOver;
    public GameObject countDown;

    private Rigidbody2D rigidbody;
    private TrailRenderer trail;
    public Color red;
    public Color blue;
    public Transform baseSprite;
    public Transform outline;
    public Transform outline2;
    private float outlineSpeed;
    private SpriteRenderer outlineSprite;
    private bool started = false;
    private bool finished = false;
    public  bool safe = false;
    private bool dead = false;

    private GazePoint _lastGazePoint = GazePoint.Invalid;
    public float VisualizationDistance = 10f;

    private int bestCombo;
    private float bestTime;
    private string bestTimeString = "99.99";
    private IEnumerator coroutine;

    public AudioSource audio;
    public AudioClip good1;
    public AudioClip good2;
    public AudioClip good3;
    public AudioClip bad;
    public AudioClip wallHit;

    public Color break1;

    void Start()
    {
        bestCombo = PlayerPrefs.GetInt("Best Combo", 0);
        bestTime = PlayerPrefs.GetFloat("Best Time", 99.99f);
        bestTimeString = string.Format("{0:00.00}", bestTime);

        rigidbody = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        outlineSprite = outline.gameObject.GetComponent<SpriteRenderer>();
        audio = transform.GetComponentInChildren<AudioSource>();
    }

    private void Update()
    {
        baseSprite.position = transform.position;
        outline.position = transform.position;
        outline2.position = transform.position;

        if (!started && Input.GetKeyDown(KeyCode.Space))
        {
            Camera.main.GetComponent<PostRenderer>()._material.SetFloat("_OffsetColor", 0.01f);

            started = true;
            forwardSpeed = startSpeed;
            outlineSpeed = forwardSpeed;
            StartCoroutine(Countdown());
        }
        else if (started && !finished)
        {
            time += Time.deltaTime;
            time = Mathf.Clamp(time, 0, Mathf.Infinity);
            timeString = string.Format("{0:00.00}", time);

            GameObject.Find("Time").GetComponent<Text>().text = timeString;
        }

        if (Input.GetKey(KeyCode.R))
        {
            Respawn();
        }
    }

    void FixedUpdate()
    {
        if (!currentColor.Equals(""))
        {
            outline.gameObject.GetComponent<Rigidbody2D>().AddTorque(Vector3.forward.z * (Time.deltaTime * outlineSpeed));
            outline2.gameObject.GetComponent<Rigidbody2D>().AddTorque(Vector3.forward.z * (Time.deltaTime * outlineSpeed));

            baseSprite.gameObject.GetComponent<Rigidbody2D>().AddTorque(Vector3.forward.z * (Time.deltaTime * outlineSpeed));
        }

        float[] samples = new float[2048];

        Camera.main.GetComponent<AudioSource>().GetOutputData(samples, 0);
        float data = 0;

        for (int x = 0; x < samples.Length; x++)
        {
            data += Mathf.Abs(samples[x]) / 4;
        }

        outline.localScale = new Vector3((data * 0.007f) + 0.75f, (data * 0.007f) + 0.75f, 0);
        outline2.localScale = new Vector3((data/2 * 0.007f) + 0.5f, (data/2 * 0.007f) + 0.5f, 0);

        if (started && !finished)
        {
            float rawInput = Input.GetAxis("Vertical");

            if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) || TobiiAPI.GetGazePoint().IsRecent() && !dead)
            {
                float verticalMovement = rawInput * movementSpeed;
                GazePoint gazePoint = TobiiAPI.GetGazePoint();
                
                if (gazePoint.Timestamp > (_lastGazePoint.Timestamp + float.Epsilon))
                {
                    Vector3 gazeOnScreen = gazePoint.Screen;
                    gazeOnScreen += (transform.forward * VisualizationDistance);

                    if ((TobiiAPI.GetFocusedObject() != null)) {
                        Vector3 grid = new Vector3(transform.position.x, TobiiAPI.GetFocusedObject().transform.position.y, transform.position.z);
                        verticalMovement = Vector3.MoveTowards(transform.position, grid, 0.25f).y;
                        transform.position = new Vector3(transform.position.x, (float) Math.Round(verticalMovement, 2));
                    }

                    //verticalMovement = 0;

                    _lastGazePoint = gazePoint;
                }

                Vector2 force = new Vector2(forwardSpeed, verticalMovement);

                rigidbody.AddForce(force);
            }
                
            else if (Input.GetAxisRaw("Vertical") < 1)
            {
                rawInput = Input.GetAxis("Vertical") / 6;
                float verticalMovement = -rawInput * movementSpeed;

                Vector2 force = new Vector2(forwardSpeed, verticalMovement);

                rigidbody.AddForce(force);
            }
        }
        else if (started && finished)
        {
            float verticalMovement = movementSpeed;

            Vector3 grid = new Vector3(transform.position.x, 0, transform.position.z);
            verticalMovement = Vector3.MoveTowards(transform.position, grid, 0.25f).y;
            transform.position = new Vector3(transform.position.x, (float)Math.Round(verticalMovement, 2));

            Vector2 force = new Vector2(forwardSpeed, verticalMovement);

            rigidbody.AddForce(force);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Equals("Obstacle") && !dead && !safe)
        {
            Sprite incomingSprite = other.gameObject.GetComponentInChildren<SpriteRenderer>().sprite;
            
            if (incomingSprite.name.Equals(currentColor))
            {
                String message = "";
                int x = UnityEngine.Random.Range(0, 4);

                if (x == 0) message = "Oh No!";
                else if (x == 1) message = "Try Again";
                else if (x == 2) message = "Uh oh!";
                else if (x == 3) message = "Ouch";

                StartCoroutine(DisplayMessage(message));

                audio.clip = bad;
                audio.pitch += UnityEngine.Random.Range(-0.05f, 0f);
                audio.Play();

                StartCoroutine(TempShake());

                outlineSpeed = -outlineSpeed;
                forwardSpeed = startSpeed;
                currentCombo = 0;

                //EnableGameOver();
            }
            else
            {
                currentCombo++;

                String message = "";

                if (currentCombo > 17) message = currentCombo + " : Perfect!";
                else if (currentCombo > 11) message = currentCombo + " : Amazing!";
                else if (currentCombo > 7) message = currentCombo + " : Excellent!";
                else if (currentCombo > 3) message = currentCombo + " : Great!";
                else if (currentCombo > -1) message = currentCombo + " : Good!";

                StartCoroutine(DisplayMessage(message));

                int x = UnityEngine.Random.Range(0, 5);
                AudioClip clip = good1;

                if (currentCombo > 7) clip = good3;
                else if (currentCombo > 3) clip = good2;
                else if (currentCombo > -1) clip = good1;

                audio.clip = clip;
                audio.pitch = 1f + (currentCombo / 10);
                audio.Play();

                audio.pitch = 1f;

                Camera.main.GetComponent<PostRenderer>()._material.SetFloat("_OffsetColor", (currentCombo * 0.001f) + 0.015f);

                currentColor = incomingSprite.name;

                if (other.gameObject.GetComponentInChildren<SpriteRenderer>().sprite.name.Equals("Red"))
                 StartCoroutine(ColorSpriteSwap(red, outlineSprite.material.GetColor("_Color")));
                 else if (other.gameObject.GetComponentInChildren<SpriteRenderer>().sprite.name.Equals("Blue"))
                StartCoroutine(ColorSpriteSwap(blue, outlineSprite.material.GetColor("_Color")));

                if (other.gameObject.GetComponentInChildren<SpriteRenderer>().sprite.name.Equals("Red"))
                    StartCoroutine(ColorTrailSwap(red, trail.startColor));
                else if (other.gameObject.GetComponentInChildren<SpriteRenderer>().sprite.name.Equals("Blue"))
                    StartCoroutine(ColorTrailSwap(blue, trail.startColor));

                if (currentCombo > yourBestCombo)
                {
                    yourBestCombo = currentCombo;
                }

                if (currentCombo > bestCombo)
                {
                    PlayerPrefs.SetInt("Best Combo", currentCombo);
                    bestCombo = currentCombo;
                }

                forwardSpeed += 10;
            }

            StartCoroutine(ColorObjectSwap(other.transform.GetComponentInChildren<SpriteRenderer>(), break1, other.transform.GetComponentInChildren<SpriteRenderer>().material.GetColor("_Color")));
            StartCoroutine(TempDisable());
        }
        else if (other.tag.Equals("Finish") && !finished)
        {
            finished = true;
            EnableGameOver();

            if (time < bestTime)
            {
                PlayerPrefs.SetFloat("Best Time", time);
                bestTime = time;
            }

            bestTimeString = string.Format("{0:00.00}", bestTime);

            GameObject.Find("YourCombo").GetComponent<Text>().text = "" + yourBestCombo;
            GameObject.Find("YourTime").GetComponent<Text>().text = timeString;
            GameObject.Find("BestTime").GetComponent<Text>().text = bestTimeString;
            GameObject.Find("BestCombo").GetComponent<Text>().text = "" + bestCombo;

            GameObject.Find("TimeText").SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag.Equals("Wall"))
        {
            audio.clip = wallHit;
            audio.Play();
        }
            
    }

    IEnumerator AudioStop()
    {
        float time = 0;
        float pitchChange = 1f * Time.deltaTime;
        AudioSource audio = Camera.main.GetComponent<AudioSource>();

        while (time < 5f && audio.pitch > 0)
        {
            time += Time.deltaTime;
            audio.pitch = audio.pitch - pitchChange;
            audio.volume = audio.volume - pitchChange;

            yield return null;
        }
    }

    IEnumerator AudioStart()
    {
        float time = 0;
        float pitchChange = 1f * Time.deltaTime;
        AudioSource audio = Camera.main.GetComponent<AudioSource>();

        while (time < 5f && audio.pitch < 1)
        {
            time += Time.deltaTime;
            audio.pitch = audio.pitch + pitchChange;
            audio.volume = audio.volume + pitchChange;

            yield return null;
        }
    }

    IEnumerator Countdown()
    {
        float time = 4;
        Text text = countDown.GetComponent<Text>();
        Vector3 originalScale = text.transform.localScale;

        while (time > 1)
        {
            time--;
            text.text = "" + time;

            int temp = 0;

            while (temp < 5)
            {
                temp++;
                text.transform.localScale *= 1.15f;

                yield return new WaitForSeconds(1 / 5);
            }

            Camera.main.GetComponent<PostRenderer>()._material.SetFloat("_OffsetColor", 0.01f);
            text.transform.localScale = originalScale;
            yield return new WaitForSeconds(1);
        }

        text.text = "";
    }

    IEnumerator DisplayMessage(String message)
    {
        Text text = countDown.GetComponent<Text>();
        Vector3 originalScale = text.transform.localScale;

        text.text = message;

        int temp = 1;

        while (temp < 5)
        {
            temp++;
            text.transform.localScale *= 1.1f;

            yield return new WaitForSeconds(1 / 5);
        }

        yield return new WaitForSeconds(0.5f);

        text.transform.localScale = originalScale;

        text.text = "";
    }

    IEnumerator ColorTrailSwap(Color newColor, Color oldColor)
    {
        float time = 0;

        while (time < 1)
        {
            time += 0.02f;
            trail.startColor = Color.Lerp(oldColor, newColor, time);
            yield return new WaitForSeconds(1 / 5);
        }
    }

    IEnumerator ColorSpriteSwap(Color newColor, Color oldColor)
    {
        float time = 0;
        Color current;

        while (time < 1)
        {
            time += 0.02f;
            current = Color.Lerp(oldColor, newColor, time);
            outlineSprite.material.SetColor("_Color", current);
            yield return new WaitForSeconds(1 / 5);
        }
    }

    IEnumerator ColorObjectSwap(SpriteRenderer sprite, Color color1, Color oldColor)
    {
        float time = 0;

        while (time < 1)
        {
            time += 0.15f;
            sprite.material.SetColor("_Color", Color.Lerp(oldColor, color1, time));
            yield return new WaitForSeconds(1 / 5);
        }
    }

    IEnumerator TempDisable()
    {
        safe = true;

        yield return new WaitForSeconds(0.5f);

        safe = false;
    }

    IEnumerator TempShake()
    {
        Camera.main.GetComponent<PostRenderer>().shift = true;
        Camera.main.GetComponent<PostRenderer>().distortion = true;

        yield return new WaitForSeconds(0.5f);

        Camera.main.GetComponent<PostRenderer>().shift = false;
        Camera.main.GetComponent<PostRenderer>().distortion = false;
    }

    public void EnableGameOver()
    {
        gameOver.SetActive(true);
    }

    public void Respawn()
    {
        InfiniteLevelGenerator.levelDifficulty = 3;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}