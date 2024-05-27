using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public GameObject gameItem;
    public Player player;
    public Weapon weapon;
    public Enemy boss1;
    public Enemy boss2;
    public float playTime;

    public AudioSource loseSound;
    public AudioSource winSound;
    public AudioSource backgroundSound;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;
    public Text maxScoreTxt;
    public Text scoreTxt;
    public Text playTimeTxt;
    public Text playerHealthTxt;
    public Text playerAmmoTxt;
    public Text playerCoinTxt;
    public Text reloadTxt;
    public Text playerDieTxt;
    public Text playerWinTxt;
    public Text timeOutTxt;

    public RectTransform bossHealthBar1;
    public RectTransform bossHealthBar2;
    public Text curScoreText;
    public Text bestText;
    public float limitTime;
    bool isDie;
    bool istimeOut;
    bool isWin;
    bool isBattle;

    void Awake()
    {
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));

        if (PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);

        isDie= false;
        istimeOut=false;
        isWin=false;
        isBattle = false;
    }

    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);

        gameItem.SetActive(true);

        boss1.gameObject.SetActive(true);
        boss2.gameObject.SetActive(true);
        backgroundSound.Play();
        isBattle = true;
    }

    public void GameOver()
    {
        isBattle = false;
        gamePanel.SetActive(false);
        overPanel.SetActive(true);

        player.gameObject.SetActive(false);

        gameItem.SetActive(false);

        boss1.gameObject.SetActive(false);
        boss2.gameObject.SetActive(false);

        if (isWin)
        {
            int endScore = player.score + player.coin + (int)Mathf.Round(limitTime);
            curScoreText.text = string.Format("{0:n0}", endScore);

            int maxScore = PlayerPrefs.GetInt("MaxScore");
            if (endScore > maxScore)
            {
                bestText.gameObject.SetActive(true);
                PlayerPrefs.SetInt("MaxScore", endScore);
            }
        }
        else
        {
            curScoreText.text = string.Format("{0:n0}", 0);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (isBattle && limitTime > 0 && !isDie && !isWin && !istimeOut)
        {
            limitTime -= Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        scoreTxt.text = string.Format("{0:n0}", player.score);

        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour * 3600) / 60);
        int second = (int)(playTime % 60);

        playTimeTxt.text = string.Format("{0:n0}", Mathf.Round(limitTime));

        playerHealthTxt.text = player.health + " / " + player.maxHealth;
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);
        playerAmmoTxt.text = player.weapon.curAmmo + " / " + player.weapon.maxAmmo;

        bossHealthBar1.localScale = new Vector3((float)boss1.curHealth / boss1.maxHealth, 1, 1);
        bossHealthBar2.localScale = new Vector3((float)boss2.curHealth / boss2.maxHealth, 1, 1);

        if(weapon.curAmmo == 0)
        {
            reloadTxt.gameObject.SetActive(true);
        }
        else
        {
            reloadTxt.gameObject.SetActive(false);
        }

        if(player.health <= 0 && !isWin && !istimeOut && !isDie)
        {
            backgroundSound.Stop();
            loseSound.Play();
            isDie = true;
            playerDieTxt.gameObject.SetActive(true);
        }

        if(boss1.curHealth <=0 && boss2.curHealth <= 0 && !istimeOut && !isDie && !isWin)
        {
            backgroundSound.Stop();
            winSound.Play();
            isWin= true;
            playerWinTxt.gameObject.SetActive(true);
            Invoke("GameOver", 5f);
        }

        if (Mathf.Round(limitTime) == 0 && !isDie && !isWin && !istimeOut)
        {
            backgroundSound.Stop();
            loseSound.Play();
            istimeOut = true;
            timeOutTxt.gameObject.SetActive(true);
            Invoke("GameOver", 3f);
        }
    }
}
