using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour { // dùng cho cả 3 chế độ
    [SerializeField] GameObject[] UIMode; // UI khi kết thúc
    [SerializeField] Text WaveDP, TimeDP, GoldDP, HealthDP, StaDP;
    [SerializeField] GameObject UIBlock; // chặn bấm liên tục
    [SerializeField] Animator UIAnimated;
    [SerializeField] SoundManager Sounds;
    void Start(){
        UIShow();
    }
    void UIShow(){ // hiển thị UI
        Block();
        for (int i = 0; i < UIMode.Length; i++){
            if (i == PlayerPrefs.GetInt("CurMode")){
                // set final infor
                if (PlayerPrefs.GetInt("CurMode") == 1){
                    if (PlayerPrefs.GetInt("CL") == 0) WaveDP.text = "Bạn đã trụ được: " + PlayerPrefs.GetInt("WaveDP") + " đợt";
                    else WaveDP.text = "You survived: " + PlayerPrefs.GetInt("WaveDP") + " waves";
                    TimeDP.text = PlayerPrefs.GetString("PlayTime");
                    GoldDP.text = PlayerPrefs.GetInt("GoldDP").ToString();
                    HealthDP.text = PlayerPrefs.GetInt("HealthDP").ToString();
                    StaDP.text = PlayerPrefs.GetInt("StaDP").ToString();
                }
                UIMode[i].SetActive(true);
                UIAnimated.SetTrigger("UITrigger " + i);
            } else UIMode[i].SetActive(false);
        }
    }
    void Block(){
        UIBlock.SetActive(true);
    }
    void UnBlock(){
        UIBlock.SetActive(false);
    }
    public void ButStoryRestart(){
        UIAnimated.SetTrigger("FadeInBlank");
        Sounds.Play("Clicking");
        StartCoroutine(SceneChange(1));
    }
    public void ButSurvivalRestart(){
        UIAnimated.SetTrigger("FadeInBlank");
        Sounds.Play("Clicking");
        StartCoroutine(SceneChange(7));
    }
    public void ButMainMenu(){
        UIAnimated.SetTrigger("FadeInBlank");
        Sounds.Play("Clicking");
        StartCoroutine(SceneChange(0));
    }
    IEnumerator SceneChange(int SceneIndext){
        Block();
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(SceneIndext);
    }
}