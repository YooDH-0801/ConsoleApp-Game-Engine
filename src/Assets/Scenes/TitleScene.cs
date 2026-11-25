using ConsoleApp_GameEngine;

public sealed class TitleScene : Scene
{
    public TitleScene() : base("Title") { }

    private int _selectedNum = 1;

    public int SelectedNum
    {
        get => _selectedNum;
        set
        {
            _selectedNum = Math.Clamp(value, 0, 1);
        }
    }

    public override void OnEnter()
    {
        // 타이틀 씬 진입 시 필요한 초기화나 안내 메시지 리셋
    }

    public override void Update()
    {
        if (Input.GetKeyDown(ConsoleKey.D1))
        {
            SceneManager.LoadScene("Play");
        }
        else if (Input.GetKeyDown(ConsoleKey.D2))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(ConsoleKey.UpArrow))
        {
            SelectedNum++;
        }
        else if (Input.GetKeyDown(ConsoleKey.DownArrow))
        {
            SelectedNum--;
        }

        if (Input.GetKeyDown(ConsoleKey.Enter))
        {
            if (SelectedNum == 0)
            {
                Application.Quit();
            }
            else if (SelectedNum == 1)
            {
                SceneManager.LoadScene("Play");
            }
        }
    }

    public override void Render()
    {
        Screen.Clear(' ', ConsoleColor.Gray, ConsoleColor.Black);

        string borderTopBottom = "+" + new string('-', 126) + "+";
        string emptyInner = new string(' ', 126);

        // 1. 외곽 테두리 그리기 (DarkGray)
        Screen.DrawText(0, 0, borderTopBottom, ConsoleColor.DarkGray);
        for (int y = 1; y < 47; y++)
        {
            Screen.DrawText(0, y, "|", ConsoleColor.DarkGray);
            Screen.DrawText(1, y, emptyInner);
            Screen.DrawText(127, y, "|", ConsoleColor.DarkGray);
        }
        Screen.DrawText(0, 47, borderTopBottom, ConsoleColor.DarkGray);

        // 3. 메인 타이틀 로고 (Yellow)
        string[] logo =
        {
                @"_________  _______           _________      _______  _______  _______ ",
                @"\__   __/ (  ____ \|\     /|\__   __/     (  ____ )(  ____ )(  ____ \",
                @"   ) (    | (    \/( \   / )   ) (        | (    )|| (    )|| (    \/",
                @"   | |    | (__     \ (_) /    | |        | (____)|| (____)|| |      ",
                @"   | |    |  __)     ) _ (     | |        |     __)|  _____)| | ____ ",
                @"   | |    | (       / ( ) \    | |        | (\ (   | (      | | \_  )",
                @"   | |    | (____/\( /   \ )   | |        | ) \ \__| )      | (___) |",
                @"   )_(    (_______/|/     \|   )_(        |/   \__/|/       (_______)"
            };

        for (int i = 0; i < logo.Length; i++)
        {
            Screen.DrawText(30, 10 + i, logo[i], ConsoleColor.Yellow);
        }

        // 4. 서브 타이틀 (Gray)
        Screen.DrawText(53, 20, ":  C O N S O L E   A P P  :", ConsoleColor.Gray);

        // 5. 메뉴 영역 (White / Green)
        Screen.DrawText(58, 32, "===================", ConsoleColor.DarkGray);
        Screen.DrawText(62, 34, "1. 게 임  시 작", _selectedNum == 1 ? ConsoleColor.Green : ConsoleColor.White);
        Screen.DrawText(62, 36, "2. 게 임  종 료", _selectedNum == 0 ? ConsoleColor.Green : ConsoleColor.White);
        Screen.DrawText(58, 38, "===================", ConsoleColor.DarkGray);

        // 6. 하단 안내 및 크레딧 (DarkGray)
        Screen.DrawText(49, 43, "방 향 키(↑/↓) 또 는  숫 자 키 를 눌 러  선 택 하 세 요.", ConsoleColor.Gray);
        Screen.DrawText(3, 46, "Ver 1.0.0", ConsoleColor.DarkGray);
        Screen.DrawText(102, 46, "(C) 2026 TextRPG Studio", ConsoleColor.DarkGray); ;

        Screen.Present();
    }
}