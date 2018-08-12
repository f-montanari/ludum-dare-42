using UnityEngine;
using UnityEngine.UI;

public class UIUpdater : MonoBehaviour
{
    public Text txtMoney;
    public Text txtCurrentJobProgress;
    public Text txtCurrentJobTime;
    public GameObject hintPanel;
    public Text hintText;

    public Transform camSE;
    public Transform camSW;
    public Transform camNE;
    public Transform camNW;

    private float countdown = 0f;
    private void Start()
    {
        HideHint();
    }

    // Update is called once per frame
    void Update ()
    {
        txtMoney.text = "Money: $" + GameManager.Instance.Money.ToString("######.00");
        GameManager.Instance.Update(Time.deltaTime);
        if (GameManager.Instance.CurrentJobTime >= GameManager.Instance.CurrentJobTotalTime)
        {
            Debug.LogWarning("Lost the game!");
            ShowHint("You lost!");
            countdown += Time.deltaTime;
            if(countdown >= 5f)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        txtCurrentJobProgress.text = "Progress: " + GameManager.Instance.CurrentJobProgress + " / " + GameManager.Instance.CurrentJobTotal;
        txtCurrentJobTime.text = "Time elapsed: " + GameManager.Instance.CurrentJobTime.ToString("###") + " / " + GameManager.Instance.CurrentJobTotalTime;
    }

    public void ShowHint(string Text)
    {
        hintPanel.SetActive(true);
        hintText.text = Text;
    }

    public void HideHint()
    {
        hintPanel.SetActive(false);
        hintText.text = "";
    }

    public void CameraSE()
    {
        FindObjectOfType<Camera>().gameObject.transform.SetPositionAndRotation(camSE.position, camSE.rotation);
    }
    public void CameraSW()
    {
        FindObjectOfType<Camera>().gameObject.transform.SetPositionAndRotation(camSW.position, camSW.rotation);
    }
    public void CameraNE()
    {
        FindObjectOfType<Camera>().gameObject.transform.SetPositionAndRotation(camNE.position, camNE.rotation);
    }
    public void CameraNW()
    {
        FindObjectOfType<Camera>().gameObject.transform.SetPositionAndRotation(camNW.position, camNW.rotation);
    }
}
