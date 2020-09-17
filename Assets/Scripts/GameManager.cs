using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{   
    private float timer = 8;
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Text centreText, timeFade, scoreFade, hiScore, letterScore,letterTime;
    private Camera main;

    public GameObject theBall, floor, wall, rotate, clock, points;
    private List<GameObject> arenaObjects = new List<GameObject>();

    private CellManager path;
    private AudioManager _audio;
    private Ball player;
    public Material[] mazeMaterials;
    private List< Vector3> cellPoints;
    private List<Vector3> floorPositions = new List<Vector3>();
    private List<Maze> mazes = new List<Maze>();
    private List<Vector3> pathToFinish = new List<Vector3>();
    private Cell holeCell;

    private float defaultY = 0;
    private int defaultPlayerSpeed = 3;
    private float fallDownIndex;
    private int level = 0;
    private int mazeColIndex;
    private float timeToFall;
    public int mazeX, mazeZ;
    private bool pauseTimer, isDead, inMotion;
    private int score;
    private int playSession;
    private float timeDifficultlySetting = 1.5f;
    private int stage = 0;

    void Start()
    {
        path = GetComponent<CellManager>();
        _audio = FindObjectOfType<AudioManager>();
        timeText.text = timer.ToString();
        main = FindObjectOfType<Camera>();
        fallDownIndex = 8;
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = (int)0f;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        OnPlay();
    }

    public void OnPlay()
    {
        playSession = 1;
        OnSpawn();
        PlayerPosition(holeCell.point);
        player = GameObject.FindWithTag("Player").GetComponent<Ball>();      
    }

    void DefaultPlayer()
    {
        player.speed = defaultPlayerSpeed;
        player.GetRigidbody().isKinematic = false;
        player.GetRigidbody().useGravity = true;
        player.transform.position = PlayerPosition(holeCell.point);
        if (isDead)
            player.gameObject.SetActive(true);
        player.GetRigidbody().velocity = Vector3.zero;
    }

    void ClearArena()
    {
        foreach (GameObject g in arenaObjects)
            Destroy(g);
    }

    public void OnRestart() //resetting counters/ maze variables
    {
        StopAllCoroutines();
        path.ClearPaths();
        ClearArena();
        playSession++;
        stage = 0;
        level = 0;
        mazeX = 3;
        mazeZ = 3;
        StartCoroutine("ResetScore");
        timer = 8; 
        pauseTimer = false;
        StartCoroutine("Timer");
        fallDownIndex = 8;
        defaultY = 0;
        mazeColIndex = 0;
        floorPositions.Clear();
        pathToFinish.Clear();
        cellPoints.Clear();
        mazes.Clear();
        OnSpawn();
        DefaultPlayer();        
        main.transform.position = FirstCameraPos();
        centreText.text = "";
        hiScore.text = "";
        _audio.Stop("CountDown");
        OnStageOne();
    }

    void SpawnItem(GameObject obj, List<Vector3> positions) //spawing an item randomally from the given list
    {
        int rand = UnityEngine.Random.Range(0, positions.Count);
        Vector3 pos = positions[rand];
        positions.Remove(pos);
        GameObject Iobj =  Instantiate(obj, pos, obj.transform.rotation);
        arenaObjects.Add(Iobj);
    }

    public void DisableText()
    {
        timeText.enabled = false;
        scoreText.enabled = false;
        letterScore.enabled = false;
        letterTime.enabled = false;
    }
    public void EnableText()
    {
        timeText.enabled = true;
        scoreText.enabled = true;
        letterTime.enabled = true;
        letterScore.enabled = true;
    }

    void GameOver() //on game over
    {
        player.speed = 0;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        centreText.color = new Color(1, 1, 1, 1);
        centreText.text = "GAME OVER";
        player.gameObject.SetActive(false);
        isDead = true;
        if (score > PlayerPrefs.GetInt("High", 0)) //storing the high score
        {
            PlayerPrefs.SetInt("High", score);
            hiScore.text = "NEW HIGH SCORE \n\n";
        }
        string s = "Mazes Completed: " + (level - 1).ToString() + "\n" + "Stage: " + stage.ToString() + "\n\n";
        hiScore.text += s + "CURRENT HIGH SCORE: " + PlayerPrefs.GetInt("High");
    }

    public Vector3 FirstCameraPos() //the inital camera position
    {
        Vector3 pos = path.GetMazeCentre(floorPositions[level - 1], mazeX, mazeZ);
        return pos;
    }    

    public void OnStageOne()
    {
        //butcherd
        string s = "STAGE  " + stage.ToString();
        StartCoroutine(FadeText(3.5f, centreText, 0, s));
    }

    public void SetDifficultly(Maze maze) //setting when the maze becomes more difficult
    {
        maze.wall = wall;
        if (level % 6 == 0) //might change back to 8 or 4
        {
            MeshRenderer mesh = wall.GetComponent<MeshRenderer>();
            stage++;
            mazeX++;
            if (mazeX == mazeZ + 2) //altering the dimensions
                mazeZ++;
            if (stage == 6)
                mazeColIndex = 0;
            mesh.sharedMaterial = mazeMaterials[mazeColIndex]; //changing the maze colour

            if (level != 0)
            {
                if (timeDifficultlySetting > 1)
                    timeDifficultlySetting -= .125f; //changing the time back recieved upon completing a maze
                string s = "STAGE " + stage.ToString();
                StartCoroutine(FadeText(3.5f, centreText, 0f, s));
            }
            mazeColIndex++;
        }
    }

    void SpawnLevel(float y) //when the player completes a maze, this function is called
    {
        Maze m = gameObject.AddComponent<Maze>(); //creating a new maze component
        timeToFall = Mathf.Sqrt(fallDownIndex / -(Physics.gravity.y / 2)); //simulating gravity 
        SetDifficultly(m); 
        m.xSize = mazeX; //intialising variables 
        m.zSize = mazeZ;
        m.yStart = y;
        m.CreateWalls();
        arenaObjects.Add(m.wallHolder);
        List<GameObject> activeWalls = m.activeWalls;
        path.InitialiseCells(m.cells);
        cellPoints = m.centrePoints;
        mazes.Add(m);

        if (level == 0) //first instance
        {
            holeCell = path.GetRandomCell();
            floorPositions.Add(holeCell.point);
            path.GetPath(holeCell, holeCell.point, activeWalls, pathToFinish, mazeX);
        }
        else
        {
            path.ClearPaths();
            Destroy(gameObject.GetComponent<Maze>());
            holeCell = path.RandomFurthest(floorPositions[level - 1], cellPoints); //where the hole will spawn
            floorPositions.Add(holeCell.point); 
            StartCoroutine(MoveCamera(timeToFall + 1f)); //moving the camera with a second offset
            StartCoroutine(OnDescent(holeCell.point)); //moving the player to the new maze, simulating physics
            StartCoroutine("DestroyWalls"); //removing the previous maze
            StartCoroutine(UpdateScore(mazeZ + mazeX + pathToFinish.Count)); //updating the score
            path.GetPath(holeCell, holeCell.point, activeWalls, pathToFinish, mazeX); //finding the path to finish the new maze
            StartCoroutine(UpdateTime(mazeX + mazeZ + ((float)pathToFinish.Count / 10) - timer/timeDifficultlySetting)); //updating the time remaining based on an algorthim
        }
        AccessPlayer(holeCell.point); //cheching to see if any items can be spawned into the new maze
        GameObject tempFloor = Instantiate(floor, floorPositions[level] - Vector3.up/7, Quaternion.identity);//spawing in the maze floor
        arenaObjects.Add(tempFloor);
        level++;
    }

    void AccessPlayer(Vector3 hole) //probably needs more testing as to when to spawn items
    {
        if(pathToFinish.Count > 25) //time object
        {
            SpawnItem(clock, pathToFinish);
        }       
        if(timer > 8 && level > 0)
            SpawnItem(points, pathToFinish); //points 
    }

    public float TimeToAdd()
    {       
        return stage;
    }

    Vector3 PlayerPosition(Vector3 pos)
    {
        Cell playerPos = path.GetFurthestCell(pos);
        //playerPos.point.y = 1;
        if(playSession == 1)
            Instantiate(theBall, playerPos.point, Quaternion.identity);
        return playerPos.point;
    }

    public bool OnPauseGame()
    {
        player.speed = 0;
        _audio.PauseAll();
        if (pauseTimer)
            return true;
        else
            pauseTimer = true;
        return false;
    }

    public void OnResumeGame()
    {
        if (!inMotion)
            pauseTimer = false;          
        player.speed = defaultPlayerSpeed; //for now
        _audio.ResumeAll();
    }

    public void OnSpawn()
    {
        fallDownIndex += .2f;
        defaultY -= fallDownIndex;
        SpawnLevel(defaultY);
    }

    public IEnumerator MoveCamera(float time) //moving the camara through the maze hole
    {
        Vector3 centre = path.GetMazeCentre(floorPositions[level], mazeX, mazeZ);
        Vector3 start = main.transform.position;
        float fDist = Vector3.Distance(start, floorPositions[level - 1]);
        float sDist = Vector3.Distance(floorPositions[level - 1], centre);
        float totalDistance = fDist + sDist;
        float fp = fDist / totalDistance;
        float sp = sDist / totalDistance; //percentage values
        float t1 = time * fp; //same speeds for each coroutine
        float t2 = time * sp;
        yield return StartCoroutine(Move(main.transform, floorPositions[level - 1], t1));
        yield return StartCoroutine(Move(main.transform, centre, t2));
    }


    IEnumerator ResetScore()
    {
        int tempScore = score;
        StartCoroutine(FadeText(1f, scoreFade, 0, ""));
        int incre = 1;
        while (score > 0)
        {
            scoreText.text = score.ToString("0000");
            score -= incre;
            incre++;
            yield return new WaitForEndOfFrame();
        }
        score = 0;
        scoreText.text = score.ToString("0000");
    }

    public IEnumerator OnDescent(Vector3 holePosition) // coroutine to move the player/ball
    {
        if (player != null)
        {
            inMotion = true;
            pauseTimer = true;
            Cell playerCell = path.GetFurthestCell(holePosition);
            player.speed = 0;
            player.GetRigidbody().velocity = Vector3.zero;
            player.GetRigidbody().useGravity = false;
            player.GetRigidbody().isKinematic = true;
            yield return StartCoroutine(Move(player.transform, playerCell.point, timeToFall + .5f));
            player.GetRigidbody().useGravity = true;
            player.GetRigidbody().isKinematic = false;
            player.speed = defaultPlayerSpeed;
            pauseTimer = false;
            inMotion = false;
        }
    }

    IEnumerator DestroyWalls()
    {
        GameObject holder = mazes[level - 1].wallHolder;
        yield return new WaitForSeconds(timeToFall); //destroying the previous maze walls
        Destroy(holder);
    }

    public IEnumerator UpdateScore(int amount) //updartng the score
    {
        StartCoroutine(FadeText(1f, scoreFade, amount, ""));
        int total = score + amount;
        while (score != total)
        {
            score += 1;
            scoreText.text = score.ToString("0000");
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeText(float dur, Text text, float value, string str)
    {
        text.text = str;
        if(value != 0)
            text.text = "+" + Mathf.Round(value).ToString();
        text.color = new Color(text.color.r, text.color.g, text.color.r, 1f);
        while (text.color.a > 0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.r, text.color.a - Time.deltaTime / dur);
            yield return null;
        }
    }

    public IEnumerator UpdateTime(float amount)
    {
        StartCoroutine(FadeText(1f, timeFade, amount, ""));
        float total = timer + amount;
        while (timer != total)
        {
            timer += 1;
            timeText.text = Mathf.Round(timer).ToString("0:00");
            if (timer >= total)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator Move(Transform fromPos, Vector3 toPos, float dur)
    {
        float i = 0;
        Vector3 startPos = fromPos.position;
        while(i < dur)
        {
            i += Time.deltaTime;
            if (fromPos != null)
            {
                fromPos.position = Vector3.Lerp(startPos, toPos, i / dur);
                yield return null;
            }           
        }
    }

    IEnumerator RotateAnObject(GameObject obj, Vector3 axis, float angle, float dur)
    {
        float elapsed = 0;
        float rotated = 0;
        while(elapsed < dur)
        {
            float step = angle / dur * Time.deltaTime;
            obj.transform.RotateAround(obj.transform.position, axis, step);
            elapsed += Time.deltaTime;
            rotated += step;
            yield return null;
        }
        obj.transform.RotateAround(obj.transform.position, axis, angle - rotated);
    }

    public IEnumerator RotateCamera() //not in use , possible game mechanic
    {
        float deg = UnityEngine.Random.Range(-90, 90);
        yield return StartCoroutine(RotateAnObject(main.gameObject, Vector3.up, deg, 3f));
        yield return StartCoroutine(RotateAnObject(main.gameObject, Vector3.up, -deg, 3f));
    }

    public IEnumerator AlterPlayerSpeed(float newSpeed) //possible game mechanic 
    {
        player.speed = newSpeed;
        yield return new WaitForSeconds(3);
        player.speed = defaultPlayerSpeed;
    }

    public IEnumerator Timer() //timer coutdown per play session
    {
        bool play = true;

        while (timer > 0)
        {
            while (!pauseTimer)
            {
                timer -= Time.deltaTime;
                timeText.text = Mathf.Round(timer).ToString("0:00");
                if (timer <= 0)
                    pauseTimer = true;
                if(timer <= 3f && play)
                {
                    _audio.Play("CountDown");
                    play = false;
                }
                if(!play && timer > 4f)
                {
                    _audio.Stop("CountDown");
                    play = true;
                }
                yield return null;
            }
            yield return null;
        }
        pauseTimer = true;
        timer = 0;
        GameOver();
    }

   
}
