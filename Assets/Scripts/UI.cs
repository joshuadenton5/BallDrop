using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameObject pauseScreen;
    private GameManager gameManager;
    private CellManager cManager;
    private AudioManager audioM;
    private bool play;
    public Button pathButton;
    public GameObject playButton;
    public GameObject pathObj;
    public GameObject restartButton;
    public GameObject pauseButton;
    public GameObject quitButton;
    public GameObject controlsButton;
    public GameObject backButton;
    public static bool inGame;
    private int pathUses = 3;
    private Text pathText;
    public Text titleText;
    public Text controlsText, muteText;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioM = FindObjectOfType<AudioManager>();
        cManager = GetComponent<CellManager>();
        pauseScreen.SetActive(false);
        restartButton.SetActive(false);
        pauseButton.SetActive(false);
        backButton.SetActive(false);
        gameManager.DisableText();
        inGame = false;
        pathText = pathButton.GetComponentInChildren<Text>();
        pathText.text = "Show Path\n x" + pathUses.ToString();
        pathObj.SetActive(false);
        titleText.text = "BallDrop";
    }

    IEnumerator Begin()
    {      
        playButton.SetActive(false);
        quitButton.SetActive(false);
        controlsButton.SetActive(false);
        pathObj.SetActive(false);
        titleText.text = "";
        yield return StartCoroutine(gameManager.Move(Camera.main.transform, gameManager.FirstCameraPos(), 1f));
        restartButton.SetActive(true);
        pauseButton.SetActive(true);
        pathObj.SetActive(true);
        inGame = true;
        gameManager.EnableText();
        gameManager.OnStageOne();
        yield return StartCoroutine(gameManager.Timer());
    }

    public void Play()
    {
        StartCoroutine("Begin");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Mute()
    {
        audioM.OnMuteButton(muteText);
    }

    public void Controls()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        controlsButton.SetActive(false);
        backButton.SetActive(true);
        controlsText.text = "Tilt your device to control the movement of the ball.\n";
        controlsText.text += "Reach the hole before the time runs out!\n";
        controlsText.text += "Use the Show Path button if you find yourself stuck\n Enjoy!";
    }

    public void BackToMain()
    {
        playButton.SetActive(true);
        quitButton.SetActive(true);
        controlsButton.SetActive(true);
        backButton.SetActive(false);
        controlsText.text = "";
    }

    public void Restart() //button to restart the ga
    {
        Time.timeScale = 1;
        gameManager.OnRestart();
        StopAllCoroutines();
        pathUses = 3;
        pathText.text = "Show Path\n x" + pathUses.ToString();
        EnablePathButton(true);
    }

    public void ExitGame() //back to main menu
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main");
    }

    public void EnablePathButton(bool enabled)
    {
        pathButton.interactable = enabled;
    }

    public void ShowPath() //button function for the path mechanic 
    {
        pathUses--;        
        if(pathUses >= 0)
            cManager.ShowCurrentPath(true);
        if(pathUses == 0)
        {
            EnablePathButton(false);
            pathUses = 0;
        }
        pathText.text = "Show Path\n x" + pathUses.ToString();
    }

    public void Resume()
    {
        pauseScreen.SetActive(false);
        Time.timeScale = 1;
        gameManager.OnResumeGame();
        EnablePathButton(true);
        restartButton.GetComponent<Button>().interactable = true;
    }



    public void PauseGame() //pasuing the game
    {
        if (!pauseScreen.activeInHierarchy)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
            gameManager.OnPauseGame();
            EnablePathButton(false);
            restartButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            Resume();
        }
    }

    void Update()
    {
        if (inGame)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }        
    }
}
