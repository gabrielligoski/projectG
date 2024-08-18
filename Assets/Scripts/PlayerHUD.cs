using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private UIDocument doc;
    [SerializeField] private StyleSheet css;
    private List<Button> roomsButtons = new List<Button>();

    private void Start()
    {
        var rooms = Enumerable.Range(0, Enum.GetNames(typeof(Room.RoomType)).Length).ToList();

        var root = doc.rootVisualElement;
        root.styleSheets.Add(css);

        var roomsShop = new ScrollView(ScrollViewMode.Horizontal);
        roomsShop.mouseWheelScrollSize = 10000;
        roomsShop.AddToClassList("rooms-shop");

        root.Add(roomsShop);

        rooms.ForEach((room) =>
        {
            var roomButton = new Button();
            string roomName = Enum.GetName(typeof(Room.RoomType), room);
            roomButton.text = roomName;
            roomButton.AddToClassList("room");
            roomButton.RegisterCallback<MouseUpEvent>((evt) => selectRoomType(roomButton, (Room.RoomType)room));
            roomsShop.Add(roomButton);
            roomsButtons.Add(roomButton);
        });

    }

    public void selectRoomType(Button roomButton, Room.RoomType room )
    {
        roomsButtons.ForEach((button) => button.RemoveFromClassList("room-selected"));
        roomButton.AddToClassList("room-selected");
        PlayerController.roomToInstance = room;
    }

}
