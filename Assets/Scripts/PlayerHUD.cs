using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance { get; private set; }
    [SerializeField] private UIDocument doc;
    [SerializeField] private StyleSheet css;
    private List<Button> roomsButtons = new List<Button>();
    private List<StyleBackground> roomsButtonsImgsGray = new List<StyleBackground>();
    private List<StyleBackground> roomsButtonsImgsGreen = new List<StyleBackground>();
    private Room.RoomType selectedRoomType = Room.RoomType.none;
    private ProgressBar coreLifeBar;
    private ProgressBar coreExpBar;
    private ProgressBar coreResourceBar;

    //private string roomsButtonsToCreate = "empty|orc_spawner|lizardman_spawner|werewolf_spawner|skeleton_spawner|spike_trap|bomb_trap";
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
        Instance = this;
        var root = doc.rootVisualElement;
        root.styleSheets.Add(css);
        CreateRoomMenu(root);
        CreateCoreExpBar(root);

        var div = new VisualElement();
        div.AddToClassList("div-header");
        var rankIcon = new VisualElement();
        rankIcon.AddToClassList("rank-icon");
        div.Add(rankIcon);
        var divBar = new VisualElement();
        divBar.AddToClassList("div-bar");
        div.Add(divBar);
        root.Add(div);

        CreateCoreLifeBar(divBar);
        CreateResourceBar(divBar);
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

    private void CreateRoomMenu(VisualElement root) {
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
                var btnPickaxeImg = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Prefabs/UI/icons/{roomName}.png"));
                var btnPickaxeImgBlue = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Prefabs/UI/icons/{roomName}_blue.png"));
                roomButton.style.backgroundImage = btnPickaxeImg;
                roomButton.AddToClassList("room");
                roomButton.RegisterCallback<MouseUpEvent>((evt) => selectRoomType(roomButton, room, evt, btnPickaxeImgBlue));
                roomsShop.Add(roomButton);
                roomsButtons.Add(roomButton);
                roomsButtonsImgsGray.Add(btnPickaxeImg);
                roomsButtonsImgsGreen.Add(btnPickaxeImg);
                return;
            }
            var btnImgGray = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Prefabs/UI/icons/{roomName}_gray.png"));
            var btnImgGreen = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Prefabs/UI/icons/{roomName}_green.png"));
            var btnImgBlue = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Prefabs/UI/icons/{roomName}_blue.png"));
            roomButton.style.backgroundImage = btnImgGray;
            roomButton.AddToClassList("room");
            roomButton.RegisterCallback<MouseUpEvent>((evt) => selectRoomType(roomButton, room, evt, btnImgBlue));
            roomsShop.Add(roomButton);
            roomsButtons.Add(roomButton);
            roomsButtonsImgsGray.Add(btnImgGray);
            roomsButtonsImgsGreen.Add(btnImgGreen);
        });
    }

    private void CreateCoreLifeBar(VisualElement root)
    {
        coreLifeBar = new ProgressBar();
        coreLifeBar.AddToClassList("core-life-bar");
        //coreLifeBar.value = gameMaster.GetComponent<GameMaster>().life;
        root.Add(coreLifeBar);
    }

    private void CreateCoreExpBar(VisualElement root)
    {
        coreExpBar = new ProgressBar();
        coreExpBar.AddToClassList("core-exp-bar");

        root.Add(coreExpBar);
    }

     private void CreateResourceBar(VisualElement root)
    {
        coreResourceBar = new ProgressBar();
        coreResourceBar.AddToClassList("core-resource-bar");

        root.Add(coreResourceBar);
    }

    public void UpdateLifeBar(float life)
    {
        coreLifeBar.value = life;
    }

    public void UpdateExpBar(float exp)
    {
        Debug.Log(exp);
        coreExpBar.value = exp*100;
    }

    public void UpdateResourceBar(int resource, int maxResource)
    {
        coreResourceBar.value = resource;
        coreResourceBar.highValue = maxResource;
    }
}
