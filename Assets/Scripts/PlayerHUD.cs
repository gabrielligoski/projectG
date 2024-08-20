using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private UIDocument doc;
    [SerializeField] private StyleSheet css;
    private List<Button> roomsButtons = new List<Button>();
    private List<StyleBackground> roomsButtonsImgsGray = new List<StyleBackground>();
    private List<StyleBackground> roomsButtonsImgsGreen = new List<StyleBackground>();
    private Room.RoomType selectedRoomType = Room.RoomType.none;

    private List<Room.RoomType> rooms = new List<Room.RoomType>()
    {
        Room.RoomType.empty,
        Room.RoomType.orc_spawner,
        Room.RoomType.lizardman_spawner,
        Room.RoomType.werewolf_spawner,
        Room.RoomType.skeleton_spawner,
        Room.RoomType.spike_trap,
        Room.RoomType.bomb_trap
    };

    private void Start()
    {
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
            string roomName = Enum.GetName(typeof(Room.RoomType), room);
            if (roomName.Equals("empty"))
            {
                var btnPickaxeImg = new StyleBackground(Resources.Load<Texture2D>($"{roomName}"));
                var btnPickaxeImgBlue = new StyleBackground(Resources.Load<Texture2D>($"{roomName}_blue"));
                roomButton.style.backgroundImage = btnPickaxeImg;
                roomButton.AddToClassList("room");
                roomButton.RegisterCallback<MouseUpEvent>((evt) => selectRoomType(roomButton, room, evt, btnPickaxeImgBlue));
                roomsShop.Add(roomButton);
                roomsButtons.Add(roomButton);
                roomsButtonsImgsGray.Add(btnPickaxeImg);
                roomsButtonsImgsGreen.Add(btnPickaxeImg);
                return;
            }
            var btnImgGray = new StyleBackground(Resources.Load<Texture2D>($"{roomName}_gray"));
            var btnImgGreen = new StyleBackground(Resources.Load<Texture2D>($"{roomName}_green"));
            var btnImgBlue = new StyleBackground(Resources.Load<Texture2D>($"{roomName}_blue"));
            roomButton.style.backgroundImage = btnImgGray;
            roomButton.AddToClassList("room");
            roomButton.RegisterCallback<MouseUpEvent>((evt) => selectRoomType(roomButton, room, evt, btnImgBlue));
            roomsShop.Add(roomButton);
            roomsButtons.Add(roomButton);
            roomsButtonsImgsGray.Add(btnImgGray);
            roomsButtonsImgsGreen.Add(btnImgGreen);
        });

    }

    public void selectRoomType(Button roomButton, Room.RoomType room, MouseUpEvent evt, StyleBackground selectedStyle)
    {
        for (int i = 0; i < roomsButtons.Count; i++)
            roomsButtons[i].style.backgroundImage = roomsButtonsImgsGray[i];

        if (room == selectedRoomType)
            room = Room.RoomType.none;
        else
            roomButton.style.backgroundImage = selectedStyle;

        selectedRoomType = room;
        PlayerController.roomToInstance = room;
        evt.StopPropagation();


    }

}
