using UnityEngine;
using UnityEngine.UI;

public class RoomHandler : MonoBehaviour
{
    [SerializeField]
    private Outline shader;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toggleOutline(bool toggle) {
        if (toggle)
        {
            shader.outlineOnTop = true;
            shader.OutlineWidth = 10;
        } else
        {
            shader.outlineOnTop = false;
            shader.OutlineWidth = 0;
        }
    }
}
