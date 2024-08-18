using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private UIDocument doc;
    [SerializeField] private StyleSheet css;
    [SerializeField] private string[] rooms = new string[3];

    private void Start()
    {
        var root = doc.rootVisualElement;
        root.styleSheets.Add(css);

        var roomsShop = new VisualElement();
        roomsShop.AddToClassList("rooms-shop");

        var roomsShopTitle = new Label();
        roomsShopTitle.text = "Rooms Shop";
        roomsShopTitle.AddToClassList("rooms-shop-title");
        roomsShop.Add(roomsShopTitle);

        root.Add(roomsShop);
        for (int i = 0; i < rooms.Length; i++) { 
            var room = new Button();
            room.text = rooms[i];
            room.AddToClassList("room");
            Debug.Log(rooms[i]);
            room.RegisterCallback<MouseUpEvent, string>(selectRoomType, rooms[i]);
            roomsShop.Add(room);
        }
    }

    public void selectRoomType(MouseUpEvent evt, string texto)
    {
        Debug.Log("Clicou");
        Debug.Log(texto);
    }

}
