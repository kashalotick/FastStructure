using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace FastStructure.Content;

// Open/close with /structmenu
// Assumes these exist in the mod: StructSelection, StructureWand, KeepMarkerItem,
// KeepMarkerVisibility (from the keepvis snippet), StructureSaver, StructureReader.

public class StructureMenuSystem : ModSystem
{
    /// <summary>Structure name waiting for a click in the world, or null.</summary>
    static string _pending;

    public static (int w, int h) PendingSize { get; private set; }

    /// <summary>Structure name waiting for a click in the world, or null.</summary>
    public static string Pending
    {
        get => _pending;
        set
        {
            _pending = value;
            PendingSize = (0, 0);
            if (value == null) return;

            try
            {
                using var fs = File.OpenRead(Path.Combine(Dir, value + ".bin"));
                var (w, h) = StructureReader.GetSize(fs);
                PendingSize = (w, h);
            }
            catch (Exception)
            {
                // unreadable file: no preview, Place will report the error
            }
        }
    }

    public static bool MouseOverMenu => IsOpen && _state.MouseOver;
    static UserInterface _ui;
    static StructureMenuState _state;

    public static string Dir => Path.Combine(Main.SavePath, "SavedStructures");
    public static bool IsOpen => _ui?.CurrentState != null;

    public override void Load()
    {
        if (Main.dedServ) return;

        _ui = new UserInterface();
        _state = new StructureMenuState();
        _state.Activate();
    }

    public override void Unload()
    {
        _ui = null;
        _state = null;
        Pending = null;
    }

    public static void Toggle()
    {
        if (_ui == null) return;

        if (IsOpen)
        {
            _ui.SetState(null);
        }
        else
        {
            _ui.SetState(_state);
            _state.Refresh();
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (IsOpen) _ui.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Mouse Text"));
        if (index == -1) return;

        layers.Insert(index, new LegacyGameInterfaceLayer("FastStructure: Menu", () =>
        {
            if (IsOpen) _ui.Draw(Main.spriteBatch, new GameTime());
            return true;
        }, InterfaceScaleType.UI));
    }
}

// "Place" arms a placement: the next left click in the world puts the structure
// with its top left corner on the tile under the cursor. Right click cancels.
public class StructurePlacePlayer : ModPlayer
{
    public override void PreUpdate()
    {
        if (Player.whoAmI != Main.myPlayer || StructureMenuSystem.Pending == null) return;

        if (Main.mouseRight && Main.mouseRightRelease)
        {
            StructureMenuSystem.Pending = null;
            Main.NewText("Placement cancelled.");
            return;
        }

        // mouseInterface is true while the cursor is over the menu or any other UI
        if (!Main.mouseLeft || !Main.mouseLeftRelease || Player.mouseInterface) return;

        Point o = Main.MouseWorld.ToTileCoordinates();
        string path = Path.Combine(StructureMenuSystem.Dir, StructureMenuSystem.Pending + ".bin");

        try
        {
            using var fs = File.OpenRead(path);
            StructureReader.Place(fs, o.X, o.Y);
            Main.NewText($"Placed {StructureMenuSystem.Pending} at {o.X}, {o.Y}", Color.LimeGreen);
        }
        catch (Exception e)
        {
            Main.NewText("Place failed: " + e.Message, Color.Red);
        }

        StructureMenuSystem.Pending = null;
    }
}

internal class DraggablePanel : UIPanel
{
    bool _dragging;
    Vector2 _startMouse, _startPos;

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        // events bubble up from children, so ignore clicks that landed on a button/list/text box
        if (evt.Target != this) return;

        _dragging = true;
        _startMouse = Main.MouseScreen;
        _startPos = new Vector2(Left.Pixels, Top.Pixels);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_dragging && !Main.mouseLeft) _dragging = false;

        float left = Left.Pixels, top = Top.Pixels;

        if (_dragging)
        {
            Vector2 delta = (Main.MouseScreen - _startMouse) / Main.UIScale;
            left = _startPos.X + delta.X;
            top = _startPos.Y + delta.Y;
        }

        // keep the panel on screen (also fixes it after a resolution change)
        float maxLeft = Math.Max(0, Main.screenWidth / Main.UIScale - Width.Pixels);
        float maxTop = Math.Max(0, Main.screenHeight / Main.UIScale - Height.Pixels);
        left = MathHelper.Clamp(left, 0, maxLeft);
        top = MathHelper.Clamp(top, 0, maxTop);

        if (left != Left.Pixels || top != Top.Pixels)
        {
            Left.Set(left, 0f);
            Top.Set(top, 0f);
            Recalculate();
        }
    }
}

public class StructureMenuState : UIState
{
    UIPanel _panel;
    UIList _list;
    NameBox _name;
    UIText _selection, _status;
    UITextPanel<string> _visButton;

    public bool MouseOver => _panel != null && _panel.ContainsPoint(Main.MouseScreen);

    public override void OnInitialize()
    {
        _panel = new DraggablePanel();
        _panel.Left.Set(40, 0);
        _panel.Top.Set(120, 0);
        _panel.Width.Set(420, 0);
        _panel.Height.Set(500, 0);
        _panel.SetPadding(10);
        Append(_panel);

        var title = new UIText("Structures", 1f);
        title.Top.Set(4, 0);
        _panel.Append(title);

        var close = Button("X", 40, () => StructureMenuSystem.Toggle());
        close.Left.Set(-40, 1f);
        _panel.Append(close);

        // tools row
        var wand = Button("Wand", 90, () => Give(ModContent.ItemType<StructureWand>(), 1));
        wand.Left.Set(0, 0);
        wand.Top.Set(34, 0);
        _panel.Append(wand);

        var markers = Button("Markers", 100, () => Give(ModContent.ItemType<KeepMarkerItem>(), 999));
        markers.Left.Set(96, 0);
        markers.Top.Set(34, 0);
        _panel.Append(markers);

        _visButton = Button("", 90, ToggleKeepVisibility);
        _visButton.Left.Set(202, 0);
        _visButton.Top.Set(34, 0);
        UpdateVisText();
        _panel.Append(_visButton);

        var folder = Button("Folder", 90, () =>
        {
            Directory.CreateDirectory(StructureMenuSystem.Dir);
            Utils.OpenFolder(StructureMenuSystem.Dir);
        });
        folder.Left.Set(298, 0);
        folder.Top.Set(34, 0);
        _panel.Append(folder);

        // save row
        _name = new NameBox();
        _name.Left.Set(0, 0);
        _name.Top.Set(74, 0);
        _name.Width.Set(300, 0);
        _name.Height.Set(32, 0);
        _panel.Append(_name);

        var save = Button("Save", 90, Save);
        save.Left.Set(306, 0);
        save.Top.Set(74, 0);
        _panel.Append(save);

        _selection = new UIText("", 0.8f);
        _selection.Top.Set(112, 0);
        _panel.Append(_selection);

        // list
        _list = new UIList { ListPadding = 4f };
        _list.Top.Set(136, 0);
        _list.Width.Set(-25, 1f);
        _list.Height.Set(300, 0);
        _panel.Append(_list);

        var scroll = new UIScrollbar();
        scroll.Top.Set(136, 0);
        scroll.Height.Set(300, 0);
        scroll.HAlign = 1f;
        _panel.Append(scroll);
        _list.SetScrollbar(scroll);

        // bottom
        var refresh = Button("Refresh", 100, Refresh);
        refresh.Left.Set(0, 0);
        refresh.Top.Set(444, 0);
        _panel.Append(refresh);

        _status = new UIText("", 0.8f);
        _status.Left.Set(110, 0);
        _status.Top.Set(454, 0);
        _panel.Append(_status);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (MouseOver) Main.LocalPlayer.mouseInterface = true;

        if (StructSelection.HasP1 && StructSelection.HasP2)
        {
            Point a = StructSelection.P1, b = StructSelection.P2;
            int w = Math.Abs(a.X - b.X) + 1, h = Math.Abs(a.Y - b.Y) + 1;
            _selection.SetText($"Selection: {w}x{h}, origin {Math.Min(a.X, b.X)}, {Math.Min(a.Y, b.Y)}");
        }
        else
        {
            _selection.SetText("Selection: set P1 and P2 with the wand");
        }

        _status.SetText(StructureMenuSystem.Pending == null
            ? ""
            : $"Placing {StructureMenuSystem.Pending}: LMB in world, RMB cancels");
    }

    public void Refresh()
    {
        _list.Clear();

        if (!Directory.Exists(StructureMenuSystem.Dir)) return;

        foreach (string file in Directory.GetFiles(StructureMenuSystem.Dir, "*.bin")
                     .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            _list.Add(MakeRow(file));
    }

    UIPanel MakeRow(string file)
    {
        string name = Path.GetFileNameWithoutExtension(file);

        string size;
        try
        {
            using var fs = File.OpenRead(file);
            var (w, h) = StructureReader.GetSize(fs);
            size = $"{w}x{h}";
        }
        catch (Exception)
        {
            size = "bad file";
        }

        var row = new UIPanel();
        row.SetPadding(0);
        row.Width.Set(0, 1f);
        row.Height.Set(40, 0);

        var label = new UIText($"{name}  ({size})", 0.85f);
        label.Left.Set(10, 0);
        label.Top.Set(11, 0);
        row.Append(label);

        var place = Button("Place", 70, () =>
        {
            StructureMenuSystem.Pending = name;
            Main.NewText($"Click in the world to place {name}. RMB cancels.", Color.Yellow);
        });
        place.Left.Set(-76, 1f);
        place.Top.Set(4, 0);
        row.Append(place);

        return row;
    }

    void Save()
    {
        string name = _name.Text.Trim();
        if (name.Length == 0)
        {
            Main.NewText("Enter a name first.", Color.Red);
            return;
        }

        if (!StructSelection.HasP1 || !StructSelection.HasP2)
        {
            Main.NewText("Set P1 and P2 first.", Color.Red);
            return;
        }

        Point a = StructSelection.P1, b = StructSelection.P2;
        int x0 = Math.Min(a.X, b.X), y0 = Math.Min(a.Y, b.Y);
        int w = Math.Abs(a.X - b.X) + 1, h = Math.Abs(a.Y - b.Y) + 1;

        Directory.CreateDirectory(StructureMenuSystem.Dir);
        string path = Path.Combine(StructureMenuSystem.Dir, name + ".bin");

        bool existed = File.Exists(path);
        if (existed) File.Copy(path, path + ".bak", true);

        StructureSaver.Save(path, x0, y0, w, h);
        Main.NewText($"Saved {name} ({w}x{h})" + (existed ? ", previous version kept as .bak" : ""), Color.LimeGreen);

        Refresh();
    }

    static void Give(int itemType, int stack)
    {
        Player p = Main.LocalPlayer;
        p.QuickSpawnItem(p.GetSource_Misc("StructureMenu"), itemType, stack);
    }

    void ToggleKeepVisibility()
    {
        KeepMarkerVisibility.Visible = !KeepMarkerVisibility.Visible;
        UpdateVisText();
    }

    void UpdateVisText() =>
        _visButton.SetText($"Keep: {(KeepMarkerVisibility.Visible ? "on" : "off")}");

    static UITextPanel<string> Button(string text, float width, Action onClick)
    {
        var b = new UITextPanel<string>(text, 0.8f);
        b.Width.Set(width, 0);
        b.Height.Set(32, 0);
        b.OnLeftClick += (_, _) => onClick();
        b.OnMouseOver += (_, _) => b.BackgroundColor = new Color(73, 94, 171);
        b.OnMouseOut += (_, _) => b.BackgroundColor = new Color(63, 82, 151) * 0.7f;
        return b;
    }
}

// Minimal text box: click to focus, Enter/Esc/click elsewhere to unfocus.
internal class NameBox : UIPanel
{
    public string Text = "";
    public bool Focused;

    readonly UIText _label = new("", 0.9f);

    public NameBox()
    {
        SetPadding(0);
        _label.Left.Set(10, 0);
        _label.Top.Set(8, 0);
        Append(_label);
    }

    public override void LeftClick(UIMouseEvent evt)
    {
        Focused = true;
        Main.clrInput();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (IsMouseHovering) Main.LocalPlayer.mouseInterface = true;
        if (Focused && Main.mouseLeft && !IsMouseHovering) Focused = false;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (Focused)
        {
            PlayerInput.WritingText = true;
            Main.instance.HandleIME();

            char[] invalid = Path.GetInvalidFileNameChars();
            string s = new(Main.GetInputText(Text).Where(c => !invalid.Contains(c)).ToArray());
            Text = s.Length > 32 ? s[..32] : s;

            if (Main.inputTextEnter || Main.inputTextEscape) Focused = false;
        }

        bool blink = Focused && (int)(Main.GlobalTimeWrappedHourly * 2) % 2 == 0;
        _label.SetText(Text.Length == 0 && !Focused ? "name..." : Text + (blink ? "|" : ""));

        base.DrawSelf(spriteBatch);
    }
}

public class StructurePreviewDraw : ModSystem
{
    public static void DrawOverlay (SpriteBatch sb)
    {
        string name = StructureMenuSystem.Pending;
        if (name == null || StructureMenuSystem.MouseOverMenu) return;

        var (w, h) = StructureMenuSystem.PendingSize;
        if (w <= 0 || h <= 0) return;

        Point o = Main.MouseWorld.ToTileCoordinates();
        bool outside = o.X < 0 || o.Y < 0 || o.X + w > Main.maxTilesX || o.Y + h > Main.maxTilesY;
        Color c = outside ? Color.Red : Color.LimeGreen;

        Rectangle frame = new(o.X * 16 - (int)Main.screenPosition.X, o.Y * 16 - (int)Main.screenPosition.Y, w * 16, h * 16);

        StructureSelectionDraw.Fill(sb, frame, c * 0.15f);
        StructureSelectionDraw.Outline(sb, frame, 2, c * 0.9f);
        StructureSelectionDraw.Fill(sb, new Rectangle(frame.X, frame.Y, 16, 16), c * 0.5f); // top left = origin
        Utils.DrawBorderString(sb, $"{name}  {w}x{h}", new Vector2(frame.X, frame.Y - 22), c, 0.9f);
    }
}

public class WorldOverlaySystem : ModSystem
{
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Inventory"));
        if (index == -1) return;

        layers.Insert(index, new LegacyGameInterfaceLayer("FastStructure: World Overlay", () =>
        {
            StructureSelectionDraw.DrawOverlay(Main.spriteBatch);
            StructurePreviewDraw.DrawOverlay(Main.spriteBatch);
            return true;
        }, InterfaceScaleType.Game));
    }
}