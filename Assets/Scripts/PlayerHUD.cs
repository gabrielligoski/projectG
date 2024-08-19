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
    private Room.RoomType selectedRoomType = Room.RoomType.none;

    private void Start()
    {
        var rooms = Enumerable.Range(0, Enum.GetNames(typeof(Room.RoomType)).Length).ToList();
        rooms.RemoveAt(0);

        var root = doc.rootVisualElement;
        root.styleSheets.Add(css);

        var roomsShop = new ScrollView(ScrollViewMode.Horizontal);
        roomsShop.mouseWheelScrollSize = 10000;
        roomsShop.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        roomsShop.AddToClassList("rooms-shop");

        root.Add(roomsShop);

        rooms.ForEach((room) =>
        {
            var roomButton = new Button();
            //roomButton.pickingMode = PickingMode.Ignore;
            string roomName = Enum.GetName(typeof(Room.RoomType), room);
            roomButton.text = roomName;
            roomButton.AddToClassList("room");
            roomButton.RegisterCallback<MouseUpEvent>((evt) => selectRoomType(roomButton, (Room.RoomType)room, evt));
            roomsShop.Add(roomButton);
            roomsButtons.Add(roomButton);
        });

    }

    public void selectRoomType(Button roomButton, Room.RoomType room, MouseUpEvent evt)
    {
        roomsButtons.ForEach((button) => button.RemoveFromClassList("room-selected"));
        if (room == selectedRoomType) { 
            room = Room.RoomType.none;
        } 
        else
        {
            roomButton.AddToClassList("room-selected");
        }
        selectedRoomType = room;
        PlayerController.roomToInstance = room;
        evt.StopPropagation();


    }

}
