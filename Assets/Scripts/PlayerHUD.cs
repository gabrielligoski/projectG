using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private UIDocument doc;
    [SerializeField] private StyleSheet css;

    private void Start()
    {
        var root = doc.rootVisualElement;
        root.styleSheets.Add(css);

        var roomsShop = new VisualElement();
        roomsShop.AddToClassList("rooms-shop");

        root.Add(roomsShop);

    }


}
