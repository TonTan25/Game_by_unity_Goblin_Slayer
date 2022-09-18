using UnityEngine;
using UnityEngine.UI;
public class LanguageText : MonoBehaviour
{
    // thay đổi ngôn ngữ
    public string VText, EText;
    void Awake(){
        RefreshLanguage();
    }
    public void RefreshLanguage(){
        if (PlayerPrefs.GetInt("CL") == 0){
            GetComponent<Text>().text = VText;
        } else if (PlayerPrefs.GetInt("CL") == 1) {
            GetComponent<Text>().text = EText;
        }
    }
}