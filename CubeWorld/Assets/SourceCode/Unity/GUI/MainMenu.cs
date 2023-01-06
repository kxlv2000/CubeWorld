using UnityEngine;
using System.Collections;
using CubeWorld.Gameplay;
using CubeWorld.Configuration;
using System;
using System.IO;
public class MainMenu
{
    public string [,] users =new string[10,3];

    public string [,] ownership = new string[10,3];

    private int usercount=0;
    private int currentuser=0;
    public double money=100;
    public double sizePrice=0;
    public double terrainPrice=0;
    public double totalPrice=0;
    public string input="1";
    private string username="Test user";
    private string password="Password";

    
    private GameManagerUnity gameManagerUnity;

    public MainMenu(GameManagerUnity gameManagerUnity)
    {
        this.gameManagerUnity = gameManagerUnity;
    }

    private string path = @"user.csv";
    private string path1 = @"ownership.csv";

    public void Write()
    {

        File.Delete(path);
        File.Create(path).Close();
        File.Delete(path1);
        File.Create(path1).Close();
        StreamWriter stream = new StreamWriter(path);
        StreamWriter stream1 = new StreamWriter(path1);

        for (int i = 0; i < 10; i++)
        {
            stream.WriteLine(users[i,0]+","+users[i,1]+","+users[i,2]);
            stream1.WriteLine(ownership[i,0]+","+ownership[i,1]+","+ownership[i,2]);
        }

        stream.Close();
        stream.Dispose();
        stream1.Close();
        stream1.Dispose();
    }

    public void Read()
    {
        if (!File.Exists(path)|!File.Exists(path1))
            return;
        string line, line1;
        StreamReader stream = new StreamReader(path);
        StreamReader stream1 = new StreamReader(path1);
        for (int i = 0; i < 10; i++)
        {
            line=stream.ReadLine();
            line1=stream1.ReadLine();
            users[i,0]=line.Split(',')[0];
            users[i,1]=line.Split(',')[1];
            users[i,2]=line.Split(',')[2];
            ownership[i,0]=line1.Split(',')[0];
            ownership[i,1]=line1.Split(',')[1];
            ownership[i,2]=line1.Split(',')[2];
        }
        stream.Close();
        stream.Dispose();
        stream1.Close();
        stream1.Dispose();
    }
    public enum MainMenuState
    {
        LOGIN,
        NORMAL,
        GENERATOR,
        OPTIONS,
        JOIN_MULTIPLAYER,
#if !UNITY_WEBPLAYER
        LOAD,
        SAVE,
#endif
        ABOUT,
        TOPUP
    }

    public MainMenuState state = MainMenuState.LOGIN;

    public void Draw()
    {
        MenuSystem.useKeyboard = false;

        switch (state)
        {
            case MainMenuState.LOGIN:
                DrawMenuLogin();
                break;

            case MainMenuState.NORMAL:
                DrawMenuNormal();
                break;

            case MainMenuState.GENERATOR:
                DrawGenerator();
                break;

            case MainMenuState.OPTIONS:
                DrawOptions();
                break;

            case MainMenuState.ABOUT:
                DrawMenuAbout();
                break;

            case MainMenuState.TOPUP:
                DrawMenuTopup();
                break;

            case MainMenuState.JOIN_MULTIPLAYER:
                DrawJoinMultiplayer();
                break;

#if !UNITY_WEBPLAYER
            case MainMenuState.LOAD:
                DrawMenuLoadSave(true);
                break;

            case MainMenuState.SAVE:
                DrawMenuLoadSave(false);
                break;
#endif
        }
    }

    public void DrawPause()
    {
        MenuSystem.useKeyboard = false;

        switch (state)
        {
            case MainMenuState.NORMAL:
                DrawMenuPause();
                break;

            case MainMenuState.OPTIONS:
                DrawOptions();
                break;

#if !UNITY_WEBPLAYER
            case MainMenuState.LOAD:
                DrawMenuLoadSave(true);
                break;

            case MainMenuState.SAVE:
                DrawMenuLoadSave(false);
                break;
#endif
        }
    }

#if !UNITY_WEBPLAYER
    void DrawMenuLoadSave(bool load)
    {
        Read();
        if (load)
            MenuSystem.BeginMenu("Load");
        else
            MenuSystem.BeginMenu("Save");

        for (int i = 0; i < 5; i++)
        {
            System.DateTime fileDateTime = WorldManagerUnity.GetWorldFileInfo(i);

            if (fileDateTime != System.DateTime.MinValue)
            {
                string prefix;
                if (load)
                    prefix = "Load World ";
                else
                    prefix = "Overwrite World";

                MenuSystem.Button(prefix + (i + 1).ToString() +" ("+ ownership[i,1]+ ") [ " + fileDateTime.ToString() + " ]", delegate()
                {
                    if (load)
                    {
                        Read();
                        if (ownership[i,1]==username){
                            gameManagerUnity.worldManagerUnity.LoadWorld(i);
                            state = MainMenuState.NORMAL;
                        }
                        else Debug.Log("not your world");
                    }
                    else
                    {
                        if (ownership[i,1]==username){
                            ownership[i,0]=i.ToString();
                            ownership[i,1]=username;
                            ownership[i,2]=totalPrice.ToString();
                            gameManagerUnity.worldManagerUnity.SaveWorld(i);
                            state = MainMenuState.NORMAL;
                            gameManagerUnity.ReturnToMainMenu();
                            Write();
                        }
                        else Debug.Log("not your world");
                    }
                }
                );
            }
            else
            {
                MenuSystem.Button("-- Empty Slot --", delegate()
                {
                    if (load == false)
                    {
                        ownership[i,0]=i.ToString();
                        ownership[i,1]=username;
                        ownership[i,2]=totalPrice.ToString();
                        gameManagerUnity.worldManagerUnity.SaveWorld(i);
                        state = MainMenuState.NORMAL;
                        gameManagerUnity.ReturnToMainMenu();
                        Write();
                    }
                }
                );
            }
        }

        MenuSystem.LastButton("Return", delegate()
        {
            state = MainMenuState.NORMAL;
        }
        );

        MenuSystem.EndMenu();
    }
#endif

    private CubeWorld.Configuration.Config lastConfig; 

    void DrawMenuPause()
    {
        MenuSystem.BeginMenu("Pause");

        // if (lastConfig != null)
        // {
        //     MenuSystem.Button("Re-create Random World", delegate()
        //     {
        //         gameManagerUnity.worldManagerUnity.CreateRandomWorld(lastConfig);
        //     }
        //     );
        // }

#if !UNITY_WEBPLAYER
        MenuSystem.Button("Save and exit", delegate()
        {
            state = MainMenuState.SAVE;
        }
        );
#endif

        MenuSystem.Button("Options", delegate()
        {
            state = MainMenuState.OPTIONS;
        }
        );

        MenuSystem.LastButton("Return", delegate()
        {
            gameManagerUnity.Unpause();
        }
        );

        MenuSystem.EndMenu();
    }

    void DrawMenuNormal()
    {
        MenuSystem.BeginMenu("Hello "+users[currentuser,0]+" (Current money: "+users[currentuser,2]+" BTC)");

        MenuSystem.Button("Buy a New Land", delegate()
        {
            state = MainMenuState.GENERATOR;
        }
        );

#if !UNITY_WEBPLAYER
        MenuSystem.Button("Go To Your Lands", delegate()
        {
            state = MainMenuState.LOAD;
        }
        );
#endif

        MenuSystem.Button("Visit Other's Lands Online", delegate()
        {
            state = MainMenuState.JOIN_MULTIPLAYER;
        }
        );


        MenuSystem.Button("Options", delegate()
        {
            state = MainMenuState.OPTIONS;
        }
        );

        MenuSystem.Button("Topup", delegate()
        {
            state = MainMenuState.TOPUP;
        }
        );

        MenuSystem.Button("Logout", delegate()
        {
            state = MainMenuState.LOGIN;
        }
        );



        if (Application.platform != RuntimePlatform.WebGLPlayer && Application.isEditor == false)
        {
            MenuSystem.LastButton("Exit", delegate()
            {
                Application.Quit();
            }
            );
        }

        MenuSystem.EndMenu();
    }

    public const string CubeworldWebServerServerList = "https://sunny-state-368610.et.r.appspot.com/list";
    public const string CubeworldWebServerServerRegister = "https://sunny-state-368610.et.r.appspot.com/register?owner={owner}&description={description}&port={port}";

    private WWW wwwRequest;
    private string[] servers;

    void DrawJoinMultiplayer()
    {
        MenuSystem.BeginMenu("Join Multiplayer");

        if (wwwRequest == null && servers == null)
            wwwRequest = new WWW(CubeworldWebServerServerList);

        if (servers == null && wwwRequest != null && wwwRequest.isDone)
            servers = wwwRequest.text.Split(';');
        servers[0]="localhost,9999,fede,xyz";
        if (wwwRequest != null && wwwRequest.isDone)
        {
            foreach (string s in servers)
            {
                string[] ss = s.Split(',');

                if (ss.Length >= 2)
                {
                    MenuSystem.Button("Join [" + ss[0] + ":" + ss[1] + "]", delegate()
                    {
                        gameManagerUnity.worldManagerUnity.JoinMultiplayerGame(ss[0], System.Int32.Parse(ss[1]));

                        availableConfigurations = null;

                        wwwRequest = null;
                        servers = null;

                        state = MainMenuState.NORMAL;
                    }
                    );
                }
            }

            MenuSystem.Button("Refresh List", delegate()
            {
                wwwRequest = null;
                servers = null;
            }
            );
        }
        else
        {
            MenuSystem.TextField("Waiting data from server..");
        }

        MenuSystem.LastButton("Back", delegate()
        {
            wwwRequest = null;
            servers = null;
            state = MainMenuState.NORMAL;
        }
        );

        MenuSystem.EndMenu();
    }

    void DrawOptions()
    {
        MenuSystem.BeginMenu("Options");

        MenuSystem.Button("Draw Distance: " + CubeWorldPlayerPreferences.farClipPlanes[CubeWorldPlayerPreferences.viewDistance], delegate()
        {
            CubeWorldPlayerPreferences.viewDistance = (CubeWorldPlayerPreferences.viewDistance + 1) % CubeWorldPlayerPreferences.farClipPlanes.Length;

            if (gameManagerUnity.playerUnity)
                gameManagerUnity.playerUnity.mainCamera.farClipPlane = CubeWorldPlayerPreferences.farClipPlanes[CubeWorldPlayerPreferences.viewDistance];
        }
        );

        MenuSystem.Button("Show Help: " + CubeWorldPlayerPreferences.showHelp, delegate()
        {
            CubeWorldPlayerPreferences.showHelp = !CubeWorldPlayerPreferences.showHelp;
        }
        );

        MenuSystem.Button("Show FPS: " + CubeWorldPlayerPreferences.showFPS, delegate()
        {
            CubeWorldPlayerPreferences.showFPS = !CubeWorldPlayerPreferences.showFPS;
        }
        );

        MenuSystem.Button("Show Engine Stats: " + CubeWorldPlayerPreferences.showEngineStats, delegate()
        {
            CubeWorldPlayerPreferences.showEngineStats = !CubeWorldPlayerPreferences.showEngineStats;
        }
        );

        MenuSystem.Button("Visible Strategy: " + System.Enum.GetName(typeof(SectorManagerUnity.VisibleStrategy), CubeWorldPlayerPreferences.visibleStrategy), delegate()
        {
            if (System.Enum.IsDefined(typeof(SectorManagerUnity.VisibleStrategy), (int)CubeWorldPlayerPreferences.visibleStrategy + 1))
            {
                CubeWorldPlayerPreferences.visibleStrategy = CubeWorldPlayerPreferences.visibleStrategy + 1;
            }
            else
            {
                CubeWorldPlayerPreferences.visibleStrategy = 0;
            }
        }
        );

        MenuSystem.LastButton("Back", delegate()
        {
            CubeWorldPlayerPreferences.StorePreferences();

            gameManagerUnity.PreferencesUpdated();

            state = MainMenuState.NORMAL;
        }
        );

        MenuSystem.EndMenu();
    }

    private AvailableConfigurations availableConfigurations;
    private int currentSizeOffset = 0;
    private int currentGeneratorOffset = 0;
    private int currentDayInfoOffset = 0;
    private int currentGameplayOffset = 0;
#if !UNITY_WEBPLAYER
    private bool multiplayer = false;
#endif

    void DrawGenerator()
    {
   
        if (availableConfigurations == null)
        {
            availableConfigurations = GameManagerUnity.LoadConfiguration();
            currentDayInfoOffset = 0;
            currentGeneratorOffset = 0;
            currentSizeOffset = 0;
            currentGameplayOffset = 2;
        }
        switch(currentSizeOffset){
            case 0:sizePrice=4;break;
            case 1:sizePrice=2;break;
            case 2:sizePrice=1;break;
            case 3:sizePrice=8;break;
        }
        switch(currentGeneratorOffset){
            case 0:terrainPrice=4;break;
            case 1:terrainPrice=2;break;
            case 2:terrainPrice=6;break;
            case 3:terrainPrice=4;break;
            case 4:terrainPrice=3;break;
            case 5:terrainPrice=1;break;
            case 6:terrainPrice=6;break;
            case 7:terrainPrice=8;break;
            case 8:terrainPrice=10;break;
        }
        MenuSystem.BeginMenu("Buy a New Land (Current money: "+users[currentuser,2]+" BTC)");

        // MenuSystem.Button("Gameplay: " + GameplayFactory.AvailableGameplays[currentGameplayOffset].name, delegate()
        // {
        //     currentGameplayOffset = (currentGameplayOffset + 1) % GameplayFactory.AvailableGameplays.Length;
        // }
        // );

        MenuSystem.Button("World Size: " + availableConfigurations.worldSizes[currentSizeOffset].name+" (Price: +"+sizePrice+")", delegate()
        {
            currentSizeOffset = (currentSizeOffset + 1) % availableConfigurations.worldSizes.Length;
        }
        );

        if (GameplayFactory.AvailableGameplays[currentGameplayOffset].hasCustomGenerator == false)
        {
            MenuSystem.Button(availableConfigurations.worldGenerators[currentGeneratorOffset].name+ " (Price: +"+terrainPrice+")", delegate()
            {
                currentGeneratorOffset = (currentGeneratorOffset + 1) % availableConfigurations.worldGenerators.Length;
            }
            );
        }

        MenuSystem.Button("Day Length: " + availableConfigurations.dayInfos[currentDayInfoOffset].name, delegate()
        {
            currentDayInfoOffset = (currentDayInfoOffset + 1) % availableConfigurations.dayInfos.Length;
        }
        );

#if !UNITY_WEBPLAYER
        MenuSystem.Button("Host Multiplayer: " + (multiplayer ? "Yes" : "No") , delegate()
        {
            multiplayer = !multiplayer;
        }
        );
#endif
        MenuSystem.Button("Back", delegate() { state = MainMenuState.NORMAL; });

        totalPrice=sizePrice+terrainPrice;
        MenuSystem.LastButton("Generate! (Total Price:"+totalPrice+" BTC)", delegate()
        {

            if (totalPrice>(Double.Parse(users[currentuser,2]))){
                return;
            }
            users[currentuser,2] = (double.Parse(users[currentuser,2])-totalPrice).ToString();

            Write();
            lastConfig = new CubeWorld.Configuration.Config();
            lastConfig.tileDefinitions = availableConfigurations.tileDefinitions;
			lastConfig.itemDefinitions = availableConfigurations.itemDefinitions;
			lastConfig.avatarDefinitions = availableConfigurations.avatarDefinitions;
            lastConfig.dayInfo = availableConfigurations.dayInfos[currentDayInfoOffset];
            lastConfig.worldGenerator = availableConfigurations.worldGenerators[currentGeneratorOffset];
            lastConfig.worldSize = availableConfigurations.worldSizes[currentSizeOffset];
            lastConfig.extraMaterials = availableConfigurations.extraMaterials;
            lastConfig.gameplay = GameplayFactory.AvailableGameplays[currentGameplayOffset];

#if !UNITY_WEBPLAYER
            if (multiplayer)
            {
                MultiplayerServerGameplay multiplayerServerGameplay = new MultiplayerServerGameplay(lastConfig.gameplay.gameplay, true);

                GameplayDefinition g = new GameplayDefinition("", "", multiplayerServerGameplay, false);

                lastConfig.gameplay = g;

                gameManagerUnity.RegisterInWebServer();
            }
#endif

            gameManagerUnity.worldManagerUnity.CreateRandomWorld(lastConfig);

            availableConfigurations = null;

            state = MainMenuState.NORMAL;
        }
        );

        MenuSystem.EndMenu();
    }

    void DrawMenuAbout()
    {
        MenuSystem.BeginMenu("Author");

        GUI.TextArea(new Rect(10, 40 + 30 * 0, 380, 260),
                    "Work In Progress by Federico D'Angelo (lawebdefederico@gmail.com)");

        MenuSystem.LastButton("Back", delegate() { state = MainMenuState.NORMAL; });

        MenuSystem.EndMenu();
    }

    void DrawMenuLogin()
    {
        Read();
        MenuSystem.BeginMenu("Login/Register");

        username = GUI.TextField(new Rect(50, 70 , 300, 20), username, 25);

        password = GUI.PasswordField(new Rect(50, 120 , 300, 20), password,"*"[0], 25);

        MenuSystem.Button("Login", delegate() 
        { 
            for(int i=0;i<10;i++)
            {
                if (users[i,0]==username & users[i,1]==password){

                    state = MainMenuState.NORMAL; 
                    currentuser=i;
                    break;
                }
            }
            Debug.Log("username not found: " + username);
        },3);
        MenuSystem.Button("Register", delegate() 
        { 
            users[usercount,0]=username;
            users[usercount,1]=password;
            users[usercount,2]="0";
            usercount++;
            Debug.Log("username added: " + username);
        },4);
        Write();
        MenuSystem.EndMenu();
    }

    void DrawMenuTopup()
    {
        
        MenuSystem.BeginMenu("Topup (Current money: "+users[currentuser,2]+" BTC)");

        MenuSystem.Button("Topup: 1.0 BTC", delegate()
        {
            users[currentuser,2]=(Double.Parse(users[currentuser,2])+1).ToString();
        });
        MenuSystem.Button("Topup: 10.0 BTC", delegate()
        {
            users[currentuser,2]=(Double.Parse(users[currentuser,2])+10).ToString();
        });
        input=GUI.TextField(new Rect(50, 180 , 300, 20), input, 25);
        MenuSystem.Button("Topup input value", delegate()
        {
            users[currentuser,2]=(Double.Parse(users[currentuser,2])+Double.Parse(input)).ToString();
        },3);

        MenuSystem.LastButton("Back", delegate() { state = MainMenuState.NORMAL; });
        Write();
        MenuSystem.EndMenu();
    }

    public void DrawGeneratingProgress(string description, int progress)
    {
        Rect sbPosition = new Rect(40,
                                    Screen.height / 2 - 20,
                                    Screen.width - 80,
                                    40);

        GUI.HorizontalScrollbar(sbPosition, 0, progress, 0, 100);

        Rect dPosition = new Rect(Screen.width / 2 - 200, sbPosition.yMax + 10, 400, 25);
        GUI.Box(dPosition, description);
    }
}
