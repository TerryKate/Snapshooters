using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridUI : MonoBehaviour
{
    public float uIFadeDuration = 3f;
    public float uIShowDuration = 0.8f;

    private CanvasGroup turnUI;
    private CanvasGroup pauseUI;
    private CanvasGroup unitsUI;
    private TextMeshProUGUI textTurn;
    private TextMeshProUGUI textTimer;
    private TextMeshProUGUI textLevel;

    private Coroutine routine;
    private float timerTurn = -1;

    public void Setup(string levelName)
    {       
        turnUI = GetComponent<Transform>().Find("TurnUI").GetComponent<CanvasGroup>();
        pauseUI = GetComponent<Transform>().Find("PauseGameUI").GetComponent<CanvasGroup>();        
        unitsUI = GetComponent<Transform>().Find("UnitsUI").GetComponent<CanvasGroup>();
        textTurn = turnUI.GetComponent<Transform>().Find("TextTurn").GetComponent<TextMeshProUGUI>();
        textTimer = GetComponent<Transform>().Find("TimerText").GetComponent<TextMeshProUGUI>();
        textLevel = GetComponent<Transform>().Find("LevelText").GetComponent<TextMeshProUGUI>();

        textTurn.text = "Player Turn";
        textLevel.text = levelName;

        turnUI.gameObject.SetActive(false);
        pauseUI.gameObject.SetActive(false);
        unitsUI.gameObject.SetActive(false);

        HideGridUI();
    }

    public void EndTurn(bool isPlayerTurn)
    {
        timerTurn = GridManager.Instance.turnDuration + .99f;
        ShowTurnText(isPlayerTurn ? "Player Turn" : " Angry Bot Turn");
        HideDetails();
    }

    public void EndGame(bool isPlayerWin)
    {
        HideGridUI();
        HideDetails();

        textTimer.text = "";

        pauseUI.GetComponent<Transform>().Find("TitleText").GetComponent<TextMeshProUGUI>().text = isPlayerWin ? "Victory" : "Defeat";

        Button btnCancel = GetButton(pauseUI, "CancelButton");
        btnCancel.onClick.RemoveAllListeners();
        GetButtonText(btnCancel).text = isPlayerWin ? "Quit" : "Surrender";
        btnCancel.onClick.AddListener(OnSurrender);

        Button btnResume = GetButton(pauseUI, "ResumeButton");
        btnResume.onClick.RemoveAllListeners();
        btnResume.gameObject.SetActive(isPlayerWin);
        GetButtonText(btnResume).text = "Restart";
        btnResume.onClick.AddListener(OnRestart);

        Button btnConfirm = GetButton(pauseUI, "ConfirmButton");
        btnConfirm.onClick.RemoveAllListeners();
        GetButtonText(btnConfirm).text = isPlayerWin ? "Next" : "Restart";

        if (isPlayerWin) 
            btnConfirm.onClick.AddListener(OnNext);
        else
            btnConfirm.onClick.AddListener(OnRestart);

        routine = StartCoroutine(ShowUI(pauseUI, true));
    }

    public void PauseGame()
    {
        HideGridUI();

        pauseUI.GetComponent<Transform>().Find("TitleText").GetComponent<TextMeshProUGUI>().text = "Paused";

        Button btnCancel = GetButton(pauseUI, "CancelButton");
        btnCancel.onClick.RemoveAllListeners();
        GetButtonText(btnCancel).text = "Surrender";
        btnCancel.onClick.AddListener(OnSurrender);

        Button btnResume = GetButton(pauseUI, "ResumeButton");
        btnResume.onClick.RemoveAllListeners();
        GetButtonText(btnResume).text = "Resume";
        btnResume.onClick.AddListener(OnPauseCancel);
        btnResume.gameObject.SetActive(true);

        Button btnConfirm = GetButton(pauseUI, "ConfirmButton");
        btnConfirm.onClick.RemoveAllListeners();
        GetButtonText(btnConfirm).text = "Restart";
        btnConfirm.onClick.AddListener(OnRestart);

        routine = StartCoroutine(ShowUI(pauseUI, true));
    }

    private Button GetButton(CanvasGroup item, string id)
    {
        return item.GetComponent<Transform>().Find(id).GetComponent<Button>();
    }

    private TextMeshProUGUI GetButtonText(Button item)
    {
        return item.GetComponent<Transform>().Find("Text").GetComponent<TextMeshProUGUI>();
    }

    private void OnPauseCancel()
    {        
        routine = StartCoroutine(ShowUI(pauseUI, false));
    }

    private void OnRestart()
    {
        GridManager.Instance.ReloadLevel();
    }

    private void OnSurrender()
    {
        SceneManager.LoadScene(0);
    }

    public void OnNext()
    {
        GridManager.Instance.SetNextLevel();
    }

    private void ShowTurnText(string str)
    {
        textTurn.text = str;
        routine = StartCoroutine(ShowUI(turnUI, true, true));
    }

    IEnumerator ShowUI(CanvasGroup group, bool active = true, bool yoyo = false)
    {
        float time = 0f;
        float from = active ? 0 : 1;
        float to = active ? 1 : 0;

        if(active)
        {
            group.gameObject.SetActive(active);
        }

        var curve = new AnimationCurve(new Keyframe[]
        {
            new Keyframe(0f, from),
            new Keyframe(1f, to)
        });

        while(time < 1f)
        {
            group.alpha = curve.Evaluate(time);
            time += (uIFadeDuration * Time.deltaTime);
            yield return null;
        }

        // Ensure the alpha is set to the target;
        group.alpha = curve.Evaluate(1f);
        

        if (yoyo)
        {
            yield return new WaitForSeconds(uIShowDuration);
            routine = StartCoroutine(ShowUI(turnUI, !active));
        }    
        else
        {
            group.gameObject.SetActive(active);
        }
    }

    public void HideGridUI()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }
        
        turnUI.alpha = 0;
        pauseUI.alpha = 0;
        turnUI.gameObject.SetActive(false);
        pauseUI.gameObject.SetActive(false);
    }

    public bool GamePaused()
    {
        return pauseUI.alpha !=0;
    }

    public bool ShowingTurn()
    {
        return turnUI.alpha != 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(pauseUI.alpha != 0 && GridManager.Instance.isGameRunning)
            {
                OnPauseCancel();
            }
        }

        if ((int)timerTurn > 0 && !GamePaused())
        {
            textTimer.text = ((int)timerTurn).ToString();
            timerTurn -= Time.deltaTime;
        }
        else if ((int)timerTurn == 0)
        {
            GridManager.Instance.EndTurn();
        }
    }

    public void ShowDetails(GridUnit gUnit)
    {
        CanvasGroup attackerUI;
        CanvasGroup defenderUI;
        CanvasGroup updatingUI;

        TextMeshProUGUI textName;
        TextMeshProUGUI textDamage;
        TextMeshProUGUI textHealth;

        attackerUI = unitsUI.gameObject.GetComponent<Transform>().Find("AttackerUI").GetComponent<CanvasGroup>();
        defenderUI = unitsUI.gameObject.GetComponent<Transform>().Find("DefenderUI").GetComponent<CanvasGroup>();

        attackerUI.gameObject.SetActive(!gUnit.isEnemy);
        defenderUI.gameObject.SetActive(gUnit.isEnemy);

        updatingUI = !gUnit.isEnemy ? attackerUI : defenderUI;

        textName = updatingUI.GetComponent<Transform>().Find("NameText").GetComponent<TextMeshProUGUI>();
        textDamage  = updatingUI.GetComponent<Transform>().Find("DamageText").GetComponent<TextMeshProUGUI>();
        textHealth = updatingUI.GetComponent<Transform>().Find("HealthText").GetComponent<TextMeshProUGUI>();

        textName.text = gUnit.unitName;
        textDamage.text = gUnit.damage.ToString();
        textHealth.text = gUnit.CurrentHealth.ToString() + "/" + gUnit.health.ToString();

        unitsUI.gameObject.SetActive(true); 
    }

    public void HideDetails()
    {
        unitsUI.gameObject.SetActive(false);
    }
}
