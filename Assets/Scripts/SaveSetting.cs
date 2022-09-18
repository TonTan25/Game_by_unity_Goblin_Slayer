using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
public class SaveSetting : MonoBehaviour
{
    // lấy dữ liệu trong quá trình chơi
    [SerializeField] GameObject PauseBoard; // bảng tạm dừng chơi
    [SerializeField] bool Menu, Pausing; // xác định là màn hình chính, điều kiện tạm dừng
    [SerializeField] Slider MuiscSlider, AmbeianceSlider, EffectSlider, MouseSlider;
    //   chỉnh âm lượng    | nhạc nền  |   môi trường   |   hiệu ứng  |
    [SerializeField] Dropdown languageDrop; // ngôn ngữ
    [SerializeField] AudioMixer mixer; // âm thanh tổng
    [SerializeField] CamsControl camsControl; // di chuyển máy quay
    [SerializeField] SoundManager sounds;
    // âm thanh sẽ chỉnh thẳng từ PlayerPrefs > set value thay vì PlayerPrefs > float > set value
    // toàn màn hình và ngôn ngữ sẽ điều chỉnh và lưu thẳng vào dữ liệu thay vì xác nhận lưu
    void Start(){ // chỉ hoạt động khi để ở Start
        if (Menu){ // xem là ở menu không
            PauseBoard = null;
            MouseSlider = null;
            languageDrop.value = PlayerPrefs.GetInt("CL");
            MuiscSlider.value = PlayerPrefs.GetFloat("Mval");
            mixer.SetFloat("MusicVol", Mathf.Log10(PlayerPrefs.GetFloat("Mval")) * 20);
        } else {
            MuiscSlider = null;
            languageDrop = null;
            Pausing = false;
            PauseBoard.SetActive(false);
            languageDrop = null;
        }
        MouseSlider.value = PlayerPrefs.GetFloat("MouseVal");
        camsControl.MouseSensitiv = PlayerPrefs.GetFloat("MouseVal") * 3f;
        // hiển thị số liệu đã lưu
        AmbeianceSlider.value = PlayerPrefs.GetFloat("Aval");
        EffectSlider.value = PlayerPrefs.GetFloat("Eval");
        // đặt số liệu đã lưu
        mixer.SetFloat("AmbeVol", Mathf.Log10(PlayerPrefs.GetFloat("Aval")) * 20);
        mixer.SetFloat("EffectVol", Mathf.Log10(PlayerPrefs.GetFloat("Eval")) * 20);
    }
    void LateUpdate(){
        if (!Menu) PauseAction();
    }
    void PauseAction(){
        if (Input.GetKeyDown(KeyCode.Escape) && !Pausing){ // tạm dừng
            Pausing = true;
            camsControl.enabled = false;
            PauseBoard.SetActive(true);
            Time.timeScale = 0.0001f;
            GetComponent<MouseCursorUnlock>().UnLock();
        } else if(Input.GetKeyDown(KeyCode.Escape) && Pausing){ // tiếp tục
            Pausing = false;
            camsControl.enabled = true;
            PauseBoard.SetActive(false);
            Time.timeScale = 1f;
            GetComponent<MouseCursorUnlock>().Lock();
            PlayerPrefs.Save();
        }
    }
    public void Restart(){
        sounds.Play("OnClick1");
        Time.timeScale = 1f;
        Invoke(nameof(Restart1), 0.2f);
        PlayerPrefs.Save();
    }
    public void Restart1(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // chơi lại màn hiện tại
    }
    public void Return(){
        sounds.Play("OnClick1");
        Time.timeScale = 1f;
        Invoke(nameof(Return1), 0.1f);
        PlayerPrefs.Save();
    }
    public void Return1(){
        camsControl.enabled = true;
        Pausing = false;
        PauseBoard.SetActive(false);
        GetComponent<MouseCursorUnlock>().Lock();
        PlayerPrefs.Save();
    }
    public void MainMenu(){
        sounds.Play("OnClick1");
        Time.timeScale = 1f;
        PlayerPrefs.Save();
        Invoke(nameof(MainMenu1), 0.1f);
    }
    void MainMenu1(){
        SceneManager.LoadScene(0); // quay về màn hình chính
        
    }
    public void EffectTest(){
        sounds.Play("OnClick1");
    }
    // hiệu chỉnh sẽ lưu trực tiếp
    public void LanguageChange(int LanguageIndex){ // language set
        sounds.Play("OnClick1");
        PlayerPrefs.SetInt("CL", LanguageIndex);
        // không biết là nó có hoạt động hay không.....
        // sẽ thay đổi toàn bộ ngôn ngữ trực tiếp
        LanguageText[] TextChanges = FindObjectsOfType<LanguageText>(true); 
        for (int i = 0; i < TextChanges.Length; i++){
            TextChanges[i].RefreshLanguage();
        }
    }
    public void MusicAdjust(float MVal){ // điều chỉnh âm nhạc
        PlayerPrefs.SetFloat("Mval", MVal);
        mixer.SetFloat("MusicVol", Mathf.Log10(MVal) * 20);
        PlayerPrefs.Save();
    }
    public void AmbeianceAdjust(float AVal){ // điều chỉnh âm môi trường
        PlayerPrefs.SetFloat("Aval", AVal);
        mixer.SetFloat("AmbeVol", Mathf.Log10(AVal) * 20);
        PlayerPrefs.Save();
    }
    public void EffectAdjust(float EVal){ // điều chỉnh hiệu ứng
        PlayerPrefs.SetFloat("Eval", EVal);
        mixer.SetFloat("EffectVol", Mathf.Log10(EVal) * 20);
        PlayerPrefs.Save();
    }
    public void MouseAdjust(float MoVal){ // điều chỉnh tốc độ chuột
        PlayerPrefs.SetFloat("MouseVal", MoVal);
        camsControl.MouseSensitiv = MoVal * 3f;
        PlayerPrefs.Save();
    }
}