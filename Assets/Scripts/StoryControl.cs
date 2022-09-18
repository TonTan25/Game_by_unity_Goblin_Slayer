using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class StoryControl : MonoBehaviour{
    [SerializeField] bool StartText;
    [SerializeField] int WaveSet;
    [SerializeField] TextChange[] textChange;
    [SerializeField] Text TextDisplay;
    [SerializeField] Animator Animated;
    [SerializeField] SoundManager sounds;
    [SerializeField] AchiveIGTrack achiveIGTrack;
    SaveSetting saveSetting;
    WaveControl waveControl;
    GameControl gameControl;
    // fading sẽ thêm âm thanh, dồn toàn bộ comp kiểm soát game vào UI
    void Start(){
        // sắp đặt
        achiveIGTrack = GetComponent<AchiveIGTrack>();
        gameControl = GetComponent<GameControl>();
        waveControl = GetComponent<WaveControl>();
        saveSetting = GetComponent<SaveSetting>();
        StartCoroutine(StoryRun()); // chạy chữ
    }
    IEnumerator StoryRun(){
        yield return new WaitForSeconds(4f);
        // dừng bộ đếm phụ
        gameControl.StopAllCoroutines();
        waveControl.StopAllCoroutines();
        saveSetting.enabled = false;
        int CurLine = 0;
        if (StartText){ // chạy chữ
            while (CurLine < textChange.Length){
                if (PlayerPrefs.GetInt("CL") == 1) TextDisplay.text = textChange[CurLine].ELine.ToString();
                else TextDisplay.text = textChange[CurLine].VLine.ToString();
                sounds.Play("TextPop");
                CurLine++;
                yield return new WaitForSeconds(textChange[CurLine].TimeWait);
            }
        }
        TextDisplay.enabled = false;
        gameControl.GameBegin();
        saveSetting.enabled = true;
        while(waveControl.CurWave < WaveSet) yield return null; // theo dõi số đợt còn lại
        waveControl.StopAllCoroutines(); // dừng bộ đếm
        waveControl.WaveDelayedText.text = "";
        yield return new WaitForSeconds(5f); // đợi
    // kiểm tra thành tựu
        int Sceneindext = SceneManager.GetActiveScene().buildIndex;
        if(Sceneindext == 1){
            GetComponent<AchiveIGTrack>().AchivePorgress(0, false, 1);
            yield return new WaitForSeconds(6f);
        } else if(Sceneindext == 3){
            GetComponent<AchiveIGTrack>().AchivePorgress(1, false, 1);
            yield return new WaitForSeconds(6f);
        }
        Animated.SetTrigger("FadeInBlank"); // chuyển màn đen
        gameControl.PlayerControling(false); // tắt di chuyển
        yield return new WaitForSeconds(3f); // đợi
        if (!StartText){ // cho phần kết truyện
            TextDisplay.enabled = true;
            while (CurLine < textChange.Length){
                if (PlayerPrefs.GetInt("CL") == 1) TextDisplay.text = textChange[CurLine].ELine.ToString();
                else TextDisplay.text = textChange[CurLine].VLine.ToString();
                sounds.Play("TextPop");
                CurLine++;
                yield return new WaitForSeconds(textChange[CurLine].TimeWait);
            }
        }
        int CurScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (CurScene == 4) SceneManager.LoadScene(0);
        else SceneManager.LoadScene(CurScene);
    }
    [System.Serializable] public class TextChange{
        public string VLine, ELine;
        public float TimeWait;
    }
}