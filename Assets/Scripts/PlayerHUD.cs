using System;
using System.Collections.Generic;
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
    private VisualElement gameOverScreen;
    private Label scoreText;

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
        CreateGameOverScreen(root);

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

    private void CreateRoomMenu(VisualElement root)
    {
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

            var cost = UnityEngine.Random.Range(10, 10000);
            try
            {
                cost = gameMaster.map.getTileByType(room).GetComponent<Room>().cost;
            }
            catch (Exception ex)
            {
                //ignore
            };
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

    private void CreateGameOverScreen(VisualElement root)
    {
        gameOverScreen = new VisualElement();
        gameOverScreen.AddToClassList("game-over-screen");
        var gameOverText = new Label();
        gameOverText.AddToClassList("game-over-text");
        gameOverText.text = "Game Over";
        gameOverScreen.Add(gameOverText);
        scoreText = new Label();
        scoreText.AddToClassList("score-text");
        scoreText.text = "";
        gameOverScreen.Add(scoreText);
        var restartButton = new Button();
        restartButton.AddToClassList("restart-button");
        restartButton.text = "Restart";
        restartButton.RegisterCallback<MouseUpEvent>((evt) => gameMaster.RestartGame());
        gameOverScreen.Add(restartButton);
        root.Add(gameOverScreen);
    }
    public void UpdateLifeBar(float life, float maxLife)
    {
        coreLifeBar.value = life;
        coreLifeBar.highValue = maxLife;
        coreLifeBar.title = $"{life}/{maxLife}";
    }

    public void UpdateExpBar(float exp)
    {
        coreExpBar.value = exp * 100;
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

    public void showGameOverScreen()
    {
        scoreText.text = $"Rank: {rankIconText.text} Score: {gameMaster.xp}";
        gameOverScreen.style.display = DisplayStyle.Flex;
    }

    public void hideGameOverScreen()
    {
        gameOverScreen.style.display = DisplayStyle.None;
    }

}
