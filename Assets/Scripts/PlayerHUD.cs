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
    private GameMaster gameMaster;
    private List<Button> roomsButtons = new List<Button>();
    private List<StyleBackground> roomsButtonsImgsGray = new List<StyleBackground>();
    private List<StyleBackground> roomsButtonsImgsGreen = new List<StyleBackground>();
    private Room.RoomType selectedRoomType = Room.RoomType.none;
    private ProgressBar coreLifeBar;
    private ProgressBar coreExpBar;
    private ProgressBar coreResourceBar;
    private Label rankIconText;

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
        gameMaster = GameMaster.Instance;
        var root = doc.rootVisualElement;
        root.styleSheets.Add(css);
        CreateRoomMenu(root);
        CreateCoreExpBar(root);

        var div = new VisualElement();
        div.AddToClassList("div-header");
        var rankIcon = new VisualElement();
        rankIcon.AddToClassList("rank-icon");
        rankIconText = new Label();
        rankIconText.AddToClassList("rank-icon-text");
        rankIconText.text = "";
        rankIcon.Add(rankIconText);
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
            Debug.Log("Room: " + room);
            var cost = UnityEngine.Random.Range(10, 10000);
            try {
                cost = gameMaster.rooms.Find(r => r.GetComponent<Room>().roomType() == room).GetComponent<Room>().cost;
            } catch (Exception ex) {
                //ignore
            };
            Debug.Log("Room cost" + cost);
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

            //criar um tooltip com o custo que aparece quando passa o mouse por cima
            var tooltip = new Label();
            tooltip.AddToClassList("tooltip");
            tooltip.text = cost.ToString();
            roomButton.Add(tooltip);
            roomButton.RegisterCallback<MouseEnterEvent>((evt) => tooltip.style.display = DisplayStyle.Flex);
            roomButton.RegisterCallback<MouseLeaveEvent>((evt) => tooltip.style.display = DisplayStyle.None);

        });
    }

    private void CreateCoreLifeBar(VisualElement root)
    {
        coreLifeBar = new ProgressBar();
        coreLifeBar.AddToClassList("core-life-bar");
        coreLifeBar.title = "";
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

    public void UpdateLifeBar(float life, float maxLife)
    {
        coreLifeBar.value = life;
        coreLifeBar.highValue = maxLife;
        coreLifeBar.title = $"{life}/{maxLife}";
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
        coreResourceBar.title = $"{resource}/{maxResource}";
    }

    public void UpdateRankIcon(string level)
    {
        rankIconText.text = level;
    }
}
