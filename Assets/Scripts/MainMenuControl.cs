using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public class MainMenuControl : MonoBehaviour {
    // lựa chọn menu chính
    [SerializeField]
    GameObject MainMenuUI, AchivementUI, SettingUI, HowToPlayUI1, ClearDatabut, ClearDataConfirm, AfterClearData;
    // hiển thị| menu chính| thành tựu | cài đặt  | xóa dữ liệu |   xác nhận xóa  | xác nhận đã xóa | ngăn người chơi bấm loạn
    public GameObject TestFieldBut;
    [SerializeField] Animator SceneAnimated, MusicAnimate;
    AchivementInsert achivementInsert;
    SoundManager sounds;
    [SerializeField] float OnClickDelayed = 0.1f; // thời gian chờ sau khi bấm chuột
    void Start(){
        TestFieldBut.SetActive(false); // Test field
        sounds = GetComponent<SoundManager>(); // lấy pref
        achivementInsert = GetComponent<AchivementInsert>();
        MainMenuUI.SetActive(true);
        AchivementUI.SetActive(false);
        SettingUI.SetActive(false);
        ClearDataConfirm.SetActive(false);
        AfterClearData.SetActive(false);
        HowToPlayUI1.SetActive(false);
        achivementInsert.CreateList();
        SceneAnimated.GetComponent<Image>().enabled = false;
        SceneAnimated.SetTrigger("FadeOutBlank");
        GetComponent<MouseCursorUnlock>().UnLock();
    }
    public void Story(){
        sounds.Play("OnClick1");
        PlayerPrefs.SetInt("CurMode", 0); // đặt chế độ
        PlayerPrefs.Save();
        SceneAnimated.enabled = true;
        SceneAnimated.GetComponent<Image>().enabled = true;
        MusicAnimate.SetTrigger("FadeOut");
        SceneAnimated.SetTrigger("MenuFadeOut");
        Invoke(nameof(Story1), 4f);
    }
    void Story1(){
        SceneManager.LoadScene(1);
    }
    public void Survival(){
        PlayerPrefs.SetInt("CurMode", 1); // đặt chế độ
        PlayerPrefs.Save();
        sounds.Play("OnClick1");
        SceneAnimated.enabled = true;
        SceneAnimated.GetComponent<Image>().enabled = true;
        MusicAnimate.SetTrigger("FadeOut");
        SceneAnimated.SetTrigger("MenuFadeOut");
        Invoke(nameof(Survival1), 4f);
    }
    void Survival1(){
        SceneManager.LoadScene(Random.Range(5,7));
    }
    public void Achivement(){
        sounds.Play("OnClick1");
        Invoke(nameof(Achivement1), OnClickDelayed);
    }
    void Achivement1(){
        MainMenuUI.SetActive(false);
        AchivementUI.SetActive(true);
    }
    public void HowToPlayBut(){
        sounds.Play("OnClick1");
        Invoke(nameof(HowToPlayBut1), OnClickDelayed/2);
    }
    void HowToPlayBut1(){
        MainMenuUI.SetActive(false);
        HowToPlayUI1.SetActive(true);
    }
    public void Setting(){
        sounds.Play("OnClick1");
        Invoke(nameof(Setting1), OnClickDelayed);
    }
    void Setting1(){
        AfterClearData.SetActive(false);
        MainMenuUI.SetActive(false);
        SettingUI.SetActive(true);
    }
    public void ClearDataBut(){
        sounds.Play("OnClick1");
        ClearDataConfirm.SetActive(true);
        ClearDatabut.SetActive(false);  
    }
    public void ConfirmClearData(){
        sounds.Play("OnClick1");
        // tiến hành đặt lại dữ liệu
        for (int i = 0; i < achivementInsert.AchiveList.Length; i++) PlayerPrefs.SetInt("Achivement " + i, 0);
        PlayerPrefs.Save(); // lưu lại
        achivementInsert.RefreshList(); // làm mới hiển thị
        ClearDataConfirm.SetActive(false);
        ClearDatabut.SetActive(true);
        AfterClearData.SetActive(true);
    }
    public void DeclineClearData(){
        sounds.Play("OnClick1");
        ClearDataConfirm.SetActive(false);
        ClearDatabut.SetActive(true);
    }
    public void Return(){
        sounds.Play("OnClick1");
        Invoke(nameof(Return1), OnClickDelayed/2);
    }
    void Return1(){
        MainMenuUI.SetActive(true);
        AchivementUI.SetActive(false);
        SettingUI.SetActive(false);
        HowToPlayUI1.SetActive(false);
    }
    public void QuitGame(){
        sounds.Play("OnClick2");
        SceneAnimated.GetComponent<Image>().enabled = true;
        SceneAnimated.SetTrigger("FadeInBlank");
        MusicAnimate.SetTrigger("FadeOut");
        Invoke(nameof(QuitGame1), 2.5f);
    }
    void QuitGame1(){
        Application.Quit();
    }
    public void TestField(){
        SceneAnimated.SetTrigger("TestFieldLoad");
        Invoke(nameof(TestField1), 5f);
    }
    void TestField1(){
        PlayerPrefs.SetInt("CurMode", 2);
        PlayerPrefs.Save();
        SceneManager.LoadScene(7);
    }
}