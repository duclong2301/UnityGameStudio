using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CubeMergeArenaUiBuilder
{
    private const string RootFolder = "Assets/Prefabs";
    private const string UiFolder = "Assets/Prefabs/UI";
    private const string MenuFolder = "Assets/Prefabs/UI/CubeMergeArena";

    private static readonly Color32 BlueA = new Color32(0, 103, 235, 255);
    private static readonly Color32 BlueB = new Color32(0, 54, 174, 255);
    private static readonly Color32 BlueC = new Color32(0, 168, 255, 255);
    private static readonly Color32 Yellow = new Color32(255, 201, 15, 255);
    private static readonly Color32 Orange = new Color32(255, 130, 0, 255);
    private static readonly Color32 Green = new Color32(88, 210, 26, 255);
    private static readonly Color32 Purple = new Color32(121, 48, 229, 255);
    private static readonly Color32 White = new Color32(255, 255, 255, 255);
    private static readonly Color32 Cyan = new Color32(75, 235, 255, 255);

    [MenuItem("Tools/Cube Merge Arena/Rebuild Main Menu UI")]
    public static void BuildFromMenu()
    {
        Debug.Log(Build());
    }

    public static string Build()
    {
        var report = new StringBuilder();
        EnsureFolders();
        CreateReusablePrefabs(report);
        CreateSceneUi(report);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return report.ToString();
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder(RootFolder, "UI");
        EnsureFolder(UiFolder, "CubeMergeArena");
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static GameObject GO(string name, Transform parent = null)
    {
        var g = new GameObject(name, typeof(RectTransform));
        if (parent != null) g.transform.SetParent(parent, false);
        return g;
    }

    private static RectTransform RT(GameObject g)
    {
        return g.GetComponent<RectTransform>();
    }

    private static void Rect(GameObject g, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var r = RT(g);
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.pivot = pivot;
        r.sizeDelta = size;
        r.anchoredPosition = pos;
        r.localScale = Vector3.one;
        r.localRotation = Quaternion.identity;
    }

    private static void Stretch(GameObject g)
    {
        Rect(g, Vector2.zero, Vector2.one, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
    }

    private static Image Img(GameObject g, Color color)
    {
        var img = g.GetComponent<Image>();
        if (img == null) img = g.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static Image ButtonImg(GameObject g, Color color)
    {
        var img = Img(g, color);
        img.raycastTarget = true;
        if (g.GetComponent<Button>() == null)
        {
            var b = g.AddComponent<Button>();
            var c = b.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1f, 1f, 1f, .92f);
            c.pressedColor = new Color(.82f, .82f, .82f, 1f);
            b.colors = c;
        }
        return img;
    }

    private static TextMeshProUGUI Txt(GameObject g, string text, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = true)
    {
        var t = g.GetComponent<TextMeshProUGUI>();
        if (t == null) t = g.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        if (bold) t.fontStyle = FontStyles.Bold;
        return t;
    }

    private static void Shadow(GameObject g, Vector2 dist, Color color)
    {
        var s = g.GetComponent<Shadow>();
        if (s == null) s = g.AddComponent<Shadow>();
        s.effectDistance = dist;
        s.effectColor = color;
    }

    private static void Outline(GameObject g, Color color, Vector2 dist)
    {
        var o = g.GetComponent<Outline>();
        if (o == null) o = g.AddComponent<Outline>();
        o.effectColor = color;
        o.effectDistance = dist;
    }

    private static GameObject PanelBase(string name, Vector2 size, Color fill)
    {
        var root = GO(name);
        Rect(root, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), size, Vector2.zero);
        Img(root, new Color(0, 0, 0, .25f));
        Shadow(root, new Vector2(0, -8), new Color(0, 0, 0, .45f));

        var panel = GO("Panel", root.transform);
        Stretch(panel);
        Img(panel, fill);

        var rim = GO("Rim", panel.transform);
        Rect(rim, Vector2.zero, Vector2.one, new Vector2(.5f, .5f), new Vector2(-12, -12), Vector2.zero);
        Img(rim, new Color(.05f, .48f, 1f, .32f));
        return root;
    }

    private static string SavePrefab(GameObject g, string name, StringBuilder report)
    {
        string path = MenuFolder + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(g, path);
        Object.DestroyImmediate(g);
        report.AppendLine("Prefab: " + path);
        return path;
    }

    private static void CreateReusablePrefabs(StringBuilder report)
    {
        CreateResourceCounter(report);
        CreateSquareIconButton(report);
        CreatePlayerProfile(report);
        CreateBestScorePanel(report);
        CreateSeasonPanel(report);
        CreateBottomNavButton(report);
        CreateCubeTile(report);
        CreateMainLogo(report);
        CreatePlayButton(report);
        CreatePopupSetting(report);
        CreateUIWin(report);
    }

    private static void CreateResourceCounter(StringBuilder report)
    {
        var root = PanelBase("ResourceCounter", new Vector2(315, 84), new Color32(0, 72, 188, 245));
        var icon = GO("Icon", root.transform); Rect(icon, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(66, 66), new Vector2(48, 0)); Img(icon, new Color32(255, 179, 0, 255)); Outline(icon, new Color32(255, 238, 76, 255), new Vector2(3, -3));
        var iconText = GO("IconLabel", icon.transform); Stretch(iconText); Txt(iconText, "$", 46, new Color32(255, 244, 118, 255)); Outline(iconText, new Color32(179, 82, 0, 255), new Vector2(2, -2));
        var amount = GO("Amount", root.transform); Rect(amount, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), new Vector2(-130, 72), new Vector2(20, 0)); Txt(amount, "12,450", 35, White, TextAlignmentOptions.MidlineLeft); Shadow(amount, new Vector2(2, -3), new Color(0, 0, 0, .45f));
        var plus = GO("PlusButton", root.transform); Rect(plus, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), new Vector2(58, 58), new Vector2(-43, 0)); ButtonImg(plus, Green);
        var ptxt = GO("Label", plus.transform); Stretch(ptxt); Txt(ptxt, "+", 45, White); Outline(ptxt, new Color32(24, 120, 14, 255), new Vector2(2, -2));
        SavePrefab(root, "ResourceCounter", report);
    }

    private static void CreateSquareIconButton(StringBuilder report)
    {
        var root = PanelBase("SquareIconButton", new Vector2(96, 96), Green);
        ButtonImg(root.transform.Find("Panel").gameObject, Green);
        var label = GO("IconLabel", root.transform); Stretch(label); Txt(label, "SFX", 24, White); Outline(label, new Color32(34, 108, 11, 255), new Vector2(2, -2));
        SavePrefab(root, "SquareIconButton", report);
    }

    private static void CreatePlayerProfile(StringBuilder report)
    {
        var root = PanelBase("PlayerProfile", new Vector2(370, 122), new Color32(0, 63, 178, 235));
        var avatar = GO("Avatar", root.transform); Rect(avatar, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(92, 92), new Vector2(60, 0)); Img(avatar, Green); Outline(avatar, new Color32(151, 255, 92, 255), new Vector2(4, -4));
        var face = GO("Face", avatar.transform); Stretch(face); Txt(face, ":)", 35, new Color32(12, 42, 29, 255));
        var name = GO("PlayerName", root.transform); Rect(name, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(0, .5f), new Vector2(-140, 52), new Vector2(138, 23)); Txt(name, "player7391", 30, White, TextAlignmentOptions.Left); Shadow(name, new Vector2(2, -2), new Color(0, 0, 0, .45f));
        var trophy = GO("Trophy", root.transform); Rect(trophy, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(42, 42), new Vector2(151, -28)); Txt(trophy, "T", 34, Yellow); Outline(trophy, new Color32(190, 98, 0, 255), new Vector2(2, -2));
        var score = GO("TrophyScore", root.transform); Rect(score, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(0, .5f), new Vector2(-210, 46), new Vector2(185, -28)); Txt(score, "1,235", 30, Yellow, TextAlignmentOptions.Left); Shadow(score, new Vector2(2, -2), new Color(0, 0, 0, .45f));
        SavePrefab(root, "PlayerProfile", report);
    }

    private static void CreateBestScorePanel(StringBuilder report)
    {
        var root = PanelBase("BestScorePanel", new Vector2(330, 210), new Color32(0, 62, 170, 230));
        var crown = GO("CrownIcon", root.transform); Rect(crown, new Vector2(0, 1), new Vector2(0, 1), new Vector2(.5f, .5f), new Vector2(58, 46), new Vector2(55, -45)); Txt(crown, "W", 32, Yellow); Outline(crown, new Color32(174, 85, 0, 255), new Vector2(2, -2));
        var title = GO("Title", root.transform); Rect(title, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, .5f), new Vector2(-95, 54), new Vector2(90, -45)); Txt(title, "BEST SCORE", 26, White, TextAlignmentOptions.Left); Shadow(title, new Vector2(2, -2), new Color(0, 0, 0, .45f));
        var score = GO("Score", root.transform); Rect(score, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(280, 72), new Vector2(0, 10)); Txt(score, "5,240", 54, Yellow); Outline(score, new Color32(154, 91, 0, 255), new Vector2(2, -3));
        var line = GO("Divider", root.transform); Rect(line, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), new Vector2(-52, 2), new Vector2(0, -50)); Img(line, new Color32(89, 194, 255, 165));
        var sub = GO("Subtitle", root.transform); Rect(sub, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(230, 48), new Vector2(0, 36)); Txt(sub, "T  Top 7%", 25, Cyan); Shadow(sub, new Vector2(2, -2), new Color(0, 0, 0, .45f));
        SavePrefab(root, "BestScorePanel", report);
    }

    private static void CreateSeasonPanel(StringBuilder report)
    {
        var root = PanelBase("SeasonPanel", new Vector2(390, 210), new Color32(0, 62, 170, 230));
        var header = GO("Header", root.transform); Rect(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(.5f, 1), new Vector2(0, 58), Vector2.zero); Img(header, Purple);
        var htxt = GO("SeasonTitle", header.transform); Rect(htxt, new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(0, .5f), new Vector2(220, 46), new Vector2(35, 0)); Txt(htxt, "SEASON 1", 27, White, TextAlignmentOptions.Left); Shadow(htxt, new Vector2(2, -2), new Color(0, 0, 0, .45f));
        var timer = GO("Timer", header.transform); Rect(timer, new Vector2(.5f, .5f), new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(170, 46), new Vector2(-30, 0)); Txt(timer, "6d 12h", 24, White, TextAlignmentOptions.Right); Shadow(timer, new Vector2(2, -2), new Color(0, 0, 0, .45f));
        var badge = GO("Badge", root.transform); Rect(badge, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(92, 92), new Vector2(78, -25)); Img(badge, new Color32(192, 100, 28, 255)); Outline(badge, new Color32(255, 201, 70, 255), new Vector2(3, -3));
        var star = GO("Star", badge.transform); Stretch(star); Txt(star, "*", 58, Yellow); Outline(star, new Color32(131, 67, 19, 255), new Vector2(2, -2));
        var rank = GO("Rank", root.transform); Rect(rank, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(0, .5f), new Vector2(-160, 54), new Vector2(150, 2)); Txt(rank, "BRONZE II", 33, White, TextAlignmentOptions.Left); Shadow(rank, new Vector2(2, -2), new Color(0, 0, 0, .45f));
        var barBg = GO("ProgressBarBackground", root.transform); Rect(barBg, new Vector2(0, 0), new Vector2(1, 0), new Vector2(.5f, .5f), new Vector2(-105, 36), new Vector2(42, 42)); Img(barBg, new Color32(0, 19, 86, 255));
        var bar = GO("ProgressBarFill", barBg.transform); Rect(bar, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, .5f), new Vector2(178, 0), Vector2.zero); Img(bar, BlueC);
        var bt = GO("ProgressLabel", barBg.transform); Stretch(bt); Txt(bt, "450/700", 21, White); Shadow(bt, new Vector2(1, -2), new Color(0, 0, 0, .45f));
        SavePrefab(root, "SeasonPanel", report);
    }

    private static void CreateBottomNavButton(StringBuilder report)
    {
        var root = PanelBase("BottomNavButton", new Vector2(230, 130), new Color32(0, 117, 234, 240));
        ButtonImg(root.transform.Find("Panel").gameObject, new Color32(0, 117, 234, 240));
        var icon = GO("Icon", root.transform); Rect(icon, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(92, 66), new Vector2(0, -48)); Img(icon, new Color32(255, 73, 55, 255));
        var label = GO("Label", root.transform); Rect(label, new Vector2(0, 0), new Vector2(1, 0), new Vector2(.5f, .5f), new Vector2(-20, 42), new Vector2(0, 30)); Txt(label, "SHOP", 26, White); Outline(label, new Color32(0, 53, 147, 255), new Vector2(2, -2));
        SavePrefab(root, "BottomNavButton", report);
    }

    private static void CreateCubeTile(StringBuilder report)
    {
        var root = GO("CubeTile"); Rect(root, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(126, 126), Vector2.zero);
        var shadow = GO("Shadow", root.transform); Rect(shadow, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(102, 102), new Vector2(10, -14)); Img(shadow, new Color(0, 0, 0, .25f));
        var side = GO("SideShade", root.transform); Rect(side, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(102, 102), new Vector2(12, -10)); Img(side, new Color32(0, 75, 170, 255));
        var face = GO("Face", root.transform); Rect(face, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(102, 102), Vector2.zero); Img(face, BlueC); Outline(face, new Color32(92, 219, 255, 255), new Vector2(3, -3));
        var num = GO("Number", face.transform); Stretch(num); Txt(num, "4", 42, White); Outline(num, new Color32(0, 65, 176, 255), new Vector2(2, -2));
        root.transform.localRotation = Quaternion.Euler(0, 0, 12);
        SavePrefab(root, "CubeTile", report);
    }

    private static void CreateMainLogo(StringBuilder report)
    {
        var root = GO("MainLogo"); Rect(root, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(620, 330), Vector2.zero);
        var back = GO("BackPlate", root.transform); Rect(back, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(560, 240), new Vector2(0, -2)); Img(back, new Color32(0, 73, 185, 190)); Shadow(back, new Vector2(0, -8), new Color(0, 0, 0, .4f));
        var cube = GO("CubeText", root.transform); Rect(cube, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(540, 120), new Vector2(0, -95)); Txt(cube, "CUBE", 92, BlueC); Outline(cube, new Color32(0, 54, 170, 255), new Vector2(4, -6)); Shadow(cube, new Vector2(0, -6), new Color(0, 0, 0, .35f));
        var merge = GO("MergeText", root.transform); Rect(merge, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(610, 130), new Vector2(0, -10)); Txt(merge, "MERGE", 92, Yellow); Outline(merge, Orange, new Vector2(4, -6)); Shadow(merge, new Vector2(0, -7), new Color(0, 0, 0, .35f));
        var arenaPlate = GO("ArenaPlate", root.transform); Rect(arenaPlate, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(500, 80), new Vector2(0, 62)); Img(arenaPlate, new Color32(0, 104, 220, 255)); Outline(arenaPlate, new Color32(86, 195, 255, 255), new Vector2(2, -2));
        var arena = GO("ArenaText", arenaPlate.transform); Stretch(arena); Txt(arena, "ARENA", 54, White); Shadow(arena, new Vector2(2, -4), new Color(0, 0, 0, .45f));
        SavePrefab(root, "MainLogo", report);
    }

    private static void CreatePlayButton(StringBuilder report)
    {
        var root = PanelBase("PlayButton", new Vector2(570, 170), Yellow);
        ButtonImg(root.transform.Find("Panel").gameObject, Yellow);
        var label = GO("PlayLabel", root.transform); Stretch(label); Txt(label, "PLAY", 82, White); Outline(label, Orange, new Vector2(3, -5)); Shadow(label, new Vector2(0, -7), new Color(0, 0, 0, .35f));
        SavePrefab(root, "PlayButton", report);
    }

    private static void CreatePopupSetting(StringBuilder report)
    {
        var root = GO("PopupSetting");
        Stretch(root);
        root.AddComponent<CanvasGroup>();

        var dim = GO("DimOverlay", root.transform);
        Stretch(dim);
        var dimImage = Img(dim, new Color(0, 10f / 255f, 35f / 255f, .72f));
        dimImage.raycastTarget = true;

        var panel = PanelBase("SettingsPanel", new Vector2(800, 700), new Color32(0, 98, 244, 255));
        panel.transform.SetParent(root.transform, false);
        Rect(panel, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(800, 700), Vector2.zero);

        var title = GO("Title", panel.transform);
        Rect(title, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(430, 92), new Vector2(0, -62));
        Txt(title, "SETTINGS", 58, White);
        Outline(title, new Color32(0, 45, 150, 255), new Vector2(3, -4));
        Shadow(title, new Vector2(0, -5), new Color(0, 0, 0, .45f));

        var close = PanelBase("CloseButton", new Vector2(72, 72), new Color32(241, 52, 42, 255));
        close.transform.SetParent(panel.transform, false);
        Rect(close, new Vector2(1, 1), new Vector2(1, 1), new Vector2(.5f, .5f), new Vector2(72, 72), new Vector2(-56, -56));
        ButtonImg(close.transform.Find("Panel").gameObject, new Color32(241, 52, 42, 255));
        var closeLabel = GO("Label", close.transform);
        Stretch(closeLabel);
        Txt(closeLabel, "X", 46, White);
        Outline(closeLabel, new Color32(166, 38, 29, 255), new Vector2(2, -2));

        var content = PanelBase("SettingsContent", new Vector2(705, 470), new Color32(0, 68, 181, 245));
        content.transform.SetParent(panel.transform, false);
        Rect(content, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(705, 470), new Vector2(0, -20));

        CreateSettingRow(content.transform, "MusicRow", "M", "Music", true, -72);
        CreateSettingRow(content.transform, "SoundRow", "S", "Sound", true, -145);
        CreateSettingRow(content.transform, "VibrationRow", "V", "Vibration", true, -218);
        CreateSettingRow(content.transform, "NotificationsRow", "N", "Notifications", true, -291);
        CreateSettingRow(content.transform, "LanguageRow", "G", "Language", false, -364);

        var langBox = PanelBase("LanguageDropdown", new Vector2(235, 58), new Color32(14, 88, 203, 255));
        langBox.transform.SetParent(content.transform.Find("LanguageRow"), false);
        Rect(langBox, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), new Vector2(235, 58), new Vector2(-145, 0));
        var langText = GO("Label", langBox.transform);
        Rect(langText, new Vector2(0, 0), new Vector2(1, 1), new Vector2(.5f, .5f), new Vector2(-72, 0), new Vector2(-18, 0));
        Txt(langText, "English", 26, White);
        Shadow(langText, new Vector2(1, -2), new Color(0, 0, 0, .4f));
        var arrow = GO("Arrow", langBox.transform);
        Rect(arrow, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), new Vector2(42, 42), new Vector2(-34, 0));
        Txt(arrow, "v", 28, White);

        CreateFooterButton(content.transform, "PrivacyPolicyButton", "P", "Privacy Policy", new Vector2(-230, -430));
        CreateFooterButton(content.transform, "TermsButton", "T", "Terms", new Vector2(0, -430));
        CreateFooterButton(content.transform, "SupportButton", "H", "Support", new Vector2(230, -430));

        var closeBottom = PanelBase("CloseBottomButton", new Vector2(290, 88), new Color32(0, 132, 255, 255));
        closeBottom.transform.SetParent(panel.transform, false);
        Rect(closeBottom, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(290, 88), new Vector2(-155, 70));
        ButtonImg(closeBottom.transform.Find("Panel").gameObject, new Color32(0, 132, 255, 255));
        var cbLabel = GO("Label", closeBottom.transform);
        Stretch(cbLabel);
        Txt(cbLabel, "CLOSE", 40, White);
        Outline(cbLabel, new Color32(0, 48, 153, 255), new Vector2(2, -3));

        var reset = PanelBase("ResetButton", new Vector2(290, 88), Yellow);
        reset.transform.SetParent(panel.transform, false);
        Rect(reset, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(290, 88), new Vector2(155, 70));
        ButtonImg(reset.transform.Find("Panel").gameObject, Yellow);
        var resetLabel = GO("Label", reset.transform);
        Stretch(resetLabel);
        Txt(resetLabel, "RESET", 40, White);
        Outline(resetLabel, Orange, new Vector2(2, -3));

        SavePrefab(root, "PopupSetting", report);
    }

    private static void CreateSettingRow(Transform parent, string name, string icon, string label, bool toggle, float y)
    {
        var row = PanelBase(name, new Vector2(670, 68), new Color32(0, 73, 188, 245));
        row.transform.SetParent(parent, false);
        Rect(row, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(670, 68), new Vector2(0, y));

        var iconGo = GO("Icon", row.transform);
        Rect(iconGo, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(70, 54), new Vector2(55, 0));
        Txt(iconGo, icon, 34, White);
        Shadow(iconGo, new Vector2(1, -2), new Color(0, 0, 0, .35f));

        var labelGo = GO("Label", row.transform);
        Rect(labelGo, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(0, .5f), new Vector2(-260, 58), new Vector2(105, 0));
        Txt(labelGo, label, 29, White, TextAlignmentOptions.Left);
        Shadow(labelGo, new Vector2(1, -2), new Color(0, 0, 0, .35f));

        if (toggle)
        {
            var switchGo = PanelBase("ToggleOn", new Vector2(128, 56), Green);
            switchGo.transform.SetParent(row.transform, false);
            Rect(switchGo, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), new Vector2(128, 56), new Vector2(-74, 0));
            ButtonImg(switchGo.transform.Find("Panel").gameObject, Green);
            var on = GO("OnLabel", switchGo.transform);
            Rect(on, new Vector2(0, 0), new Vector2(1, 1), new Vector2(.5f, .5f), new Vector2(-52, 0), new Vector2(-18, 0));
            Txt(on, "ON", 24, White);
            Shadow(on, new Vector2(1, -2), new Color(0, 0, 0, .35f));
            var knob = GO("Knob", switchGo.transform);
            Rect(knob, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), new Vector2(48, 48), new Vector2(-31, 0));
            Img(knob, new Color32(235, 243, 255, 255));
            Outline(knob, new Color32(165, 178, 205, 255), new Vector2(1, -1));
        }
    }

    private static void CreateFooterButton(Transform parent, string name, string icon, string label, Vector2 pos)
    {
        var button = PanelBase(name, new Vector2(220, 62), new Color32(18, 112, 225, 255));
        button.transform.SetParent(parent, false);
        Rect(button, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(220, 62), pos);
        ButtonImg(button.transform.Find("Panel").gameObject, new Color32(18, 112, 225, 255));
        var iconGo = GO("Icon", button.transform);
        Rect(iconGo, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(48, 44), new Vector2(42, 0));
        Txt(iconGo, icon, 26, White);
        var labelGo = GO("Label", button.transform);
        Rect(labelGo, new Vector2(0, 0), new Vector2(1, 1), new Vector2(.5f, .5f), new Vector2(-70, 0), new Vector2(30, 0));
        Txt(labelGo, label, 20, White);
        Shadow(labelGo, new Vector2(1, -2), new Color(0, 0, 0, .35f));
    }

    private static void CreateUIWin(StringBuilder report)
    {
        var root = GO("UIWin");
        Stretch(root);

        var celebrationLayer = GO("CelebrationLayer", root.transform); Stretch(celebrationLayer);
        var titleLayer = GO("TitleLayer", root.transform); Stretch(titleLayer);
        var characterLayer = GO("CharacterLayer", root.transform); Stretch(characterLayer);
        var rewardsLayer = GO("RewardsLayer", root.transform); Stretch(rewardsLayer);
        var statsLayer = GO("StatsLayer", root.transform); Stretch(statsLayer);
        var actionsLayer = GO("ActionsLayer", root.transform); Stretch(actionsLayer);

        CreateConfetti(celebrationLayer.transform);

        var logo = AddPrefab("MainLogo", titleLayer.transform, "GameLogoSmall", new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(360, 185), new Vector2(0, -12));
        logo.transform.localScale = Vector3.one;

        CreateCrown(titleLayer.transform, new Vector2(0, -218));

        var banner = PanelBase("WinTitleBanner", new Vector2(925, 150), new Color32(0, 105, 228, 255));
        banner.transform.SetParent(titleLayer.transform, false);
        Rect(banner, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(925, 150), new Vector2(0, -305));
        var winText = GO("Title", banner.transform);
        Stretch(winText);
        Txt(winText, "YOU WIN!", 96, Yellow);
        Outline(winText, Orange, new Vector2(4, -7));
        Shadow(winText, new Vector2(0, -8), new Color(0, 0, 0, .45f));

        var placeRibbon = PanelBase("PlaceRibbon", new Vector2(650, 82), new Color32(0, 139, 255, 255));
        placeRibbon.transform.SetParent(titleLayer.transform, false);
        Rect(placeRibbon, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(650, 82), new Vector2(0, -405));
        var placeText = GO("Label", placeRibbon.transform);
        Stretch(placeText);
        Txt(placeText, "*  1st Place  *", 48, White);
        Outline(placeText, new Color32(0, 55, 170, 255), new Vector2(2, -3));
        placeText.GetComponent<TextMeshProUGUI>().text = "<color=#FFD21A>*</color> <color=#FFD21A>1st</color> Place <color=#FFD21A>*</color>";

        CreateMascot(characterLayer.transform);
        CreateWinCube(characterLayer.transform, "Cube_2", "2", new Color32(255, 73, 55, 255), new Vector2(-535, -260), 12);
        CreateWinCube(characterLayer.transform, "Cube_4", "4", BlueC, new Vector2(-340, -232), -11);
        CreateWinCube(characterLayer.transform, "Cube_8", "8", Green, new Vector2(-185, -275), 7);
        CreateWinCube(characterLayer.transform, "Cube_32", "32", new Color32(255, 123, 0, 255), new Vector2(230, -275), -8);
        CreateWinCube(characterLayer.transform, "Cube_128", "128", new Color32(238, 174, 105, 255), new Vector2(405, -250), 8);
        CreateWinCube(characterLayer.transform, "Cube_256", "256", Yellow, new Vector2(565, -262), 12);

        CreateRewardsPanel(rewardsLayer.transform);
        CreateStatsBar(statsLayer.transform);
        CreateWinActions(actionsLayer.transform);

        SavePrefab(root, "UIWin", report);
    }

    private static void CreateConfetti(Transform parent)
    {
        Color32[] colors =
        {
            new Color32(255, 55, 78, 255),
            new Color32(255, 212, 28, 255),
            new Color32(46, 220, 64, 255),
            new Color32(50, 190, 255, 255),
            new Color32(236, 64, 255, 255)
        };

        Vector2[] positions =
        {
            new Vector2(-560, -65), new Vector2(-430, -120), new Vector2(-250, -45), new Vector2(260, -62),
            new Vector2(410, -135), new Vector2(560, -78), new Vector2(-625, -330), new Vector2(625, -320),
            new Vector2(-490, -500), new Vector2(485, -500), new Vector2(-330, -610), new Vector2(335, -610)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            var piece = GO("Confetti_" + (i + 1), parent);
            Rect(piece, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(i % 3 == 0 ? 38 : 24, i % 3 == 0 ? 18 : 24), positions[i]);
            Img(piece, colors[i % colors.Length]);
            piece.transform.localRotation = Quaternion.Euler(0, 0, (i * 31) % 70 - 35);
        }
    }

    private static void CreateCrown(Transform parent, Vector2 pos)
    {
        var crown = GO("Crown", parent);
        Rect(crown, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(330, 130), pos);

        var baseGo = GO("Base", crown.transform);
        Rect(baseGo, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(295, 48), new Vector2(0, 30));
        Img(baseGo, Yellow);
        Outline(baseGo, Orange, new Vector2(3, -3));

        for (int i = 0; i < 5; i++)
        {
            float x = -130 + i * 65;
            float height = i == 2 ? 94 : 74;
            var spike = GO("Spike_" + i, crown.transform);
            Rect(spike, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(46, height), new Vector2(x, 44));
            Img(spike, Yellow);
            Outline(spike, Orange, new Vector2(2, -2));
            spike.transform.localRotation = Quaternion.Euler(0, 0, (i - 2) * -10);

            var jewel = GO("Jewel_" + i, crown.transform);
            Rect(jewel, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(34, 34), new Vector2(x, 112 + (i == 2 ? 22 : 0)));
            Img(jewel, i == 2 ? BlueC : Yellow);
            Outline(jewel, Orange, new Vector2(2, -2));
        }
    }

    private static void CreateMascot(Transform parent)
    {
        var mascot = GO("WinnerMascot", parent);
        Rect(mascot, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(250, 210), new Vector2(0, -158));

        var body = PanelBase("Body", new Vector2(178, 155), BlueC);
        body.transform.SetParent(mascot.transform, false);
        Rect(body, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(178, 155), new Vector2(0, -18));
        Outline(body.transform.Find("Panel").gameObject, new Color32(72, 219, 255, 255), new Vector2(3, -3));

        var leftArm = GO("LeftArm", mascot.transform);
        Rect(leftArm, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(52, 112), new Vector2(-120, 30));
        Img(leftArm, new Color32(0, 92, 220, 255));
        leftArm.transform.localRotation = Quaternion.Euler(0, 0, -35);
        var rightArm = GO("RightArm", mascot.transform);
        Rect(rightArm, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(52, 112), new Vector2(120, 30));
        Img(rightArm, new Color32(0, 92, 220, 255));
        rightArm.transform.localRotation = Quaternion.Euler(0, 0, 35);

        var face = GO("Face", mascot.transform);
        Rect(face, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(150, 95), new Vector2(0, -14));
        Txt(face, ":D", 62, new Color32(18, 35, 64, 255));
    }

    private static void CreateWinCube(Transform parent, string name, string number, Color color, Vector2 pos, float rotation)
    {
        var cube = AddPrefab("CubeTile", parent, name, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(132, 132), pos);
        cube.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        cube.transform.Find("Face").GetComponent<Image>().color = color;
        cube.transform.Find("Face/Number").GetComponent<TextMeshProUGUI>().text = number;
    }

    private static void CreateRewardsPanel(Transform parent)
    {
        var panel = PanelBase("RewardsPanel", new Vector2(905, 138), new Color32(0, 74, 189, 245));
        panel.transform.SetParent(parent, false);
        Rect(panel, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(905, 138), new Vector2(0, 220));

        CreateRewardItem(panel.transform, "CoinsReward", "$", "COINS", "+500", Yellow, new Vector2(-300, 0));
        CreateRewardItem(panel.transform, "GemsReward", "D", "GEMS", "+20", new Color32(213, 70, 255, 255), Vector2.zero);
        CreateRewardItem(panel.transform, "TrophiesReward", "T", "TROPHIES", "+35", Yellow, new Vector2(300, 0));

        for (int i = 0; i < 2; i++)
        {
            var line = GO("Divider_" + i, panel.transform);
            Rect(line, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(3, 96), new Vector2(i == 0 ? -150 : 150, 0));
            Img(line, new Color32(74, 196, 255, 145));
        }
    }

    private static void CreateRewardItem(Transform parent, string name, string icon, string label, string amount, Color iconColor, Vector2 pos)
    {
        var item = GO(name, parent);
        Rect(item, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(275, 118), pos);

        var iconGo = GO("Icon", item.transform);
        Rect(iconGo, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(105, 105), new Vector2(60, 0));
        Img(iconGo, iconColor);
        Outline(iconGo, icon == "D" ? new Color32(255, 167, 255, 255) : Orange, new Vector2(3, -3));
        var iconText = GO("IconLabel", iconGo.transform);
        Stretch(iconText);
        Txt(iconText, icon, 48, White);
        Outline(iconText, new Color32(110, 65, 0, 255), new Vector2(2, -2));

        var labelGo = GO("Label", item.transform);
        Rect(labelGo, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, .5f), new Vector2(-118, 45), new Vector2(126, -34));
        Txt(labelGo, label, 24, White, TextAlignmentOptions.Left);
        Shadow(labelGo, new Vector2(1, -2), new Color(0, 0, 0, .35f));

        var amountGo = GO("Amount", item.transform);
        Rect(amountGo, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, .5f), new Vector2(-118, 62), new Vector2(126, 42));
        Txt(amountGo, amount, 47, icon == "D" ? new Color32(255, 118, 255, 255) : Yellow, TextAlignmentOptions.Left);
        Outline(amountGo, icon == "D" ? new Color32(111, 31, 145, 255) : new Color32(128, 76, 0, 255), new Vector2(2, -3));
    }

    private static void CreateStatsBar(Transform parent)
    {
        var bar = PanelBase("StatsBar", new Vector2(905, 76), new Color32(0, 68, 181, 245));
        bar.transform.SetParent(parent, false);
        Rect(bar, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(905, 76), new Vector2(0, 105));

        CreateStatItem(bar.transform, "ScoreStat", "O", "Score", "1024", new Vector2(-225, 0));
        CreateStatItem(bar.transform, "DefeatedStat", "X", "Defeated", "7 opponents", new Vector2(225, 0));

        var divider = GO("Divider", bar.transform);
        Rect(divider, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(3, 50), Vector2.zero);
        Img(divider, new Color32(245, 250, 255, 165));
    }

    private static void CreateStatItem(Transform parent, string name, string icon, string label, string value, Vector2 pos)
    {
        var item = GO(name, parent);
        Rect(item, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(395, 60), pos);
        var iconGo = GO("Icon", item.transform);
        Rect(iconGo, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(56, 56), new Vector2(42, 0));
        Txt(iconGo, icon, 38, White);
        Outline(iconGo, new Color32(0, 56, 160, 255), new Vector2(2, -2));
        var text = GO("Text", item.transform);
        Rect(text, new Vector2(0, 0), new Vector2(1, 1), new Vector2(.5f, .5f), new Vector2(-95, 0), new Vector2(56, 0));
        Txt(text, label + " <color=#FFD21A>" + value + "</color>", 29, White);
        Shadow(text, new Vector2(1, -2), new Color(0, 0, 0, .35f));
    }

    private static void CreateWinActions(Transform parent)
    {
        CreateActionButton(parent, "HomeButton", "H", "HOME", new Color32(0, 142, 255, 255), new Vector2(-365, 42), new Vector2(250, 115));
        CreateActionButton(parent, "X2RewardButton", ">", "X2 REWARD", Yellow, new Vector2(0, 42), new Vector2(365, 115));
        CreateActionButton(parent, "ClaimButton", "", "CLAIM", Green, new Vector2(375, 42), new Vector2(300, 115));
    }

    private static void CreateActionButton(Transform parent, string name, string icon, string label, Color color, Vector2 pos, Vector2 size)
    {
        var button = PanelBase(name, size, color);
        button.transform.SetParent(parent, false);
        Rect(button, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), size, pos);
        ButtonImg(button.transform.Find("Panel").gameObject, color);

        if (!string.IsNullOrEmpty(icon))
        {
            var iconGo = GO("Icon", button.transform);
            Rect(iconGo, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(76, 76), new Vector2(64, 0));
            Txt(iconGo, icon, 42, White);
            Outline(iconGo, new Color32(0, 55, 165, 255), new Vector2(2, -3));
        }

        var labelGo = GO("Label", button.transform);
        Rect(labelGo, Vector2.zero, Vector2.one, new Vector2(.5f, .5f), new Vector2(string.IsNullOrEmpty(icon) ? -20 : -95, 0), new Vector2(string.IsNullOrEmpty(icon) ? 0 : 42, 0));
        Txt(labelGo, label, label.Length > 8 ? 38 : 44, White);
        Outline(labelGo, color == Green ? new Color32(31, 124, 12, 255) : color == Yellow ? Orange : new Color32(0, 55, 165, 255), new Vector2(2, -4));
        Shadow(labelGo, new Vector2(1, -3), new Color(0, 0, 0, .35f));
    }

    private static GameObject AddPrefab(string prefab, Transform parent, string name, Vector2 amin, Vector2 amax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(MenuFolder + "/" + prefab + ".prefab");
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        inst.name = name;
        Rect(inst, amin, amax, pivot, size, pos);
        return inst;
    }

    private static void CreateSceneUi(StringBuilder report)
    {
        var old = GameObject.Find("CubeMergeArenaCanvas");
        if (old != null) Object.DestroyImmediate(old);

        var canvasGO = new GameObject("CubeMergeArenaCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Stretch(canvasGO);

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        var safe = GO("SafeArea", canvasGO.transform); Stretch(safe); safe.AddComponent<SafeAreaFitter>();
        var uiHome = GO("UIHome", safe.transform); Stretch(uiHome);
        var winLayer = GO("WinLayer", safe.transform); Stretch(winLayer);
        var popupLayer = GO("PopupLayer", safe.transform); Stretch(popupLayer);
        string[] layerNames = { "BackgroundLayer", "DecorativeLayer", "HeaderLayer", "LogoLayer", "PrimaryActionLayer", "PanelsLayer", "NavigationLayer", "OverlayLayer" };
        var layers = new Dictionary<string, Transform>();
        foreach (var layerName in layerNames)
        {
            var layer = GO(layerName, uiHome.transform);
            Stretch(layer);
            layers[layerName] = layer.transform;
        }

        var bg = GO("BlueBackground", layers["BackgroundLayer"]); Stretch(bg); Img(bg, new Color32(0, 91, 231, 255));
        var glow = GO("CenterGlow", layers["BackgroundLayer"]); Rect(glow, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(900, 520), new Vector2(0, -35)); Img(glow, new Color32(35, 180, 255, 88));

        var sound = AddPrefab("SquareIconButton", layers["HeaderLayer"], "AudioButton", new Vector2(0, 1), new Vector2(0, 1), new Vector2(.5f, .5f), new Vector2(96, 96), new Vector2(78, -72));
        sound.transform.Find("IconLabel").GetComponent<TextMeshProUGUI>().text = "SFX";
        AddPrefab("PlayerProfile", layers["HeaderLayer"], "PlayerProfile", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(370, 122), new Vector2(145, -22));
        var coins = AddPrefab("ResourceCounter", layers["HeaderLayer"], "CoinsCounter", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(315, 84), new Vector2(-455, -24));
        coins.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = "12,450";
        var gems = AddPrefab("ResourceCounter", layers["HeaderLayer"], "GemsCounter", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(285, 84), new Vector2(-155, -24));
        gems.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = "890";
        gems.transform.Find("Icon/IconLabel").GetComponent<TextMeshProUGUI>().text = "D";
        gems.transform.Find("Icon").GetComponent<Image>().color = new Color32(207, 63, 255, 255);
        var settings = AddPrefab("SquareIconButton", layers["HeaderLayer"], "SettingsButton", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(96, 96), new Vector2(-35, -24));
        settings.transform.Find("IconLabel").GetComponent<TextMeshProUGUI>().text = "SET";
        settings.transform.Find("Panel").GetComponent<Image>().color = BlueC;

        AddPrefab("MainLogo", layers["LogoLayer"], "GameLogo", new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(620, 330), new Vector2(0, -125));
        AddPrefab("PlayButton", layers["PrimaryActionLayer"], "PlayButton", new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(570, 170), new Vector2(0, -235));
        AddPrefab("BestScorePanel", layers["PanelsLayer"], "BestScorePanel", new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(330, 210), new Vector2(65, 145));
        AddPrefab("SeasonPanel", layers["PanelsLayer"], "SeasonPanel", new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(390, 210), new Vector2(-70, 145));

        var nav1 = AddPrefab("BottomNavButton", layers["NavigationLayer"], "SkinsButton", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(230, 130), new Vector2(-285, 22));
        nav1.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = "SKINS";
        nav1.transform.Find("Icon").GetComponent<Image>().color = new Color32(97, 213, 255, 255);
        var nav2 = AddPrefab("BottomNavButton", layers["NavigationLayer"], "ShopButton", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(230, 130), new Vector2(0, 22));
        nav2.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = "SHOP";
        var nav3 = AddPrefab("BottomNavButton", layers["NavigationLayer"], "LeaderboardButton", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(255, 130), new Vector2(295, 22));
        nav3.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = "LEADERBOARD";
        nav3.transform.Find("Icon").GetComponent<Image>().color = Yellow;

        var c4 = AddPrefab("CubeTile", layers["DecorativeLayer"], "Cube_4_Blue", new Vector2(0, 1), new Vector2(0, 1), new Vector2(.5f, .5f), new Vector2(126, 126), new Vector2(245, -250));
        c4.transform.Find("Face/Number").GetComponent<TextMeshProUGUI>().text = "4";
        var c8 = AddPrefab("CubeTile", layers["DecorativeLayer"], "Cube_8_Green", new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(126, 126), new Vector2(340, 20));
        c8.transform.Find("Face").GetComponent<Image>().color = Green;
        c8.transform.Find("Face/Number").GetComponent<TextMeshProUGUI>().text = "8";
        var c2 = AddPrefab("CubeTile", layers["DecorativeLayer"], "Cube_2_Red", new Vector2(1, 1), new Vector2(1, 1), new Vector2(.5f, .5f), new Vector2(126, 126), new Vector2(-260, -250));
        c2.transform.Find("Face").GetComponent<Image>().color = new Color32(255, 73, 55, 255);
        c2.transform.Find("Face/Number").GetComponent<TextMeshProUGUI>().text = "2";
        var c32 = AddPrefab("CubeTile", layers["DecorativeLayer"], "Cube_32_Yellow", new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(.5f, .5f), new Vector2(126, 126), new Vector2(-205, -10));
        c32.transform.Find("Face").GetComponent<Image>().color = Yellow;
        c32.transform.Find("Face/Number").GetComponent<TextMeshProUGUI>().text = "32";

        PrefabUtility.SaveAsPrefabAsset(uiHome, MenuFolder + "/UIHome.prefab");
        report.AppendLine("Prefab: " + MenuFolder + "/UIHome.prefab");

        var uiWin = AddPrefab("UIWin", winLayer.transform, "UIWin", Vector2.zero, Vector2.one, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
        uiHome.SetActive(false);
        uiWin.SetActive(true);

        var popup = AddPrefab("PopupSetting", popupLayer.transform, "PopupSetting", Vector2.zero, Vector2.one, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
        popup.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(canvasGO, MenuFolder + "/CubeMergeArenaMainMenu.prefab");
        report.AppendLine("Prefab: " + MenuFolder + "/CubeMergeArenaMainMenu.prefab");
        report.AppendLine("Scene: " + SceneManager.GetActiveScene().path);
        report.AppendLine("Canvas: CubeMergeArenaCanvas");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}

