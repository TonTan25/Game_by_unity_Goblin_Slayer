using UnityEngine;
public class MouseCursorUnlock : MonoBehaviour
{
    public void UnLock(){
        Cursor.lockState = CursorLockMode.None; //lock the mouse at center of screen
        Cursor.visible = true; // hide the mouse when gaming
    }
    public void Lock(){
        Cursor.lockState = CursorLockMode.Locked; //lock the mouse at center of screen
        Cursor.visible = false; // hide the mouse when gaming
    }
}