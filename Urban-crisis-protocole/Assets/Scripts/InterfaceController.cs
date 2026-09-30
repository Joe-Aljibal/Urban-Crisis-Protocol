#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class InterfaceController : MonoBehaviour
{
    public void Quitter()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

}