using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MENU : MonoBehaviour
{
    private UIDocument doc;


    private void Start()
    {
        doc = GetComponent<UIDocument>();
        doc.rootVisualElement.Q<Button>("btn").RegisterCallback<MouseUpEvent>((evt) => SceneManager.LoadScene(0));
    }

}
