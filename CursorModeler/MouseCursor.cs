namespace CursorModeler
{
    public enum MouseCursor
    {
        // Normal pointer arrow
        Arrow = 0,
        // Text cursor
        Text = 1,
        // Vertical resize arrows
        ResizeVertical = 2,
        // Horizontal resize arrows
        ResizeHorizontal = 3,
        // Arrow with a Link badge (for assigning pointers)
        Link = 4,
        // Arrow with small arrows for indicating sliding at number fields
        SlideArrow = 5,
        // Resize up-right for window edges
        ResizeUpRight = 6,
        // Resize up-Left for window edges.
        ResizeUpLeft = 7,
        // Arrow with the move symbol next to it for the sceneview
        MoveArrow = 8,
        // Arrow with the rotate symbol next to it for the sceneview
        RotateArrow = 9,
        // Arrow with the scale symbol next to it for the sceneview
        ScaleArrow = 10,
        // Arrow with the plus symbol next to it
        ArrowPlus = 11,
        // Arrow with the minus symbol next to it
        ArrowMinus = 12,
        // Cursor with a dragging hand for pan
        Pan = 13,
        // Cursor with an eye for orbit
        Orbit = 14,
        // Cursor with a magnifying glass for zoom
        Zoom = 15,
        // Cursor with an eye and stylized arrow keys for FPS navigation
        FPS = 16,
        // The current user defined cursor
        CustomCursor = 17,
        // Split resize up down arrows
        SplitResizeUpDown = 18,
        // Split resize left right arrows
        SplitResizeLeftRight = 19,
        Alias,
        AllScroll,
        Auto,
        Cell,
        ContextMenu,
        ColResize,
        Copy,
        Crosshair,
        E_Resize,
        EW_Resize,
        Grab,
        Grabbing,
        Help,
        Move,
        N_Resize,
        NE_Resize,
        NESW_Resize,
        NS_Resize,
        NW_Resize,
        NWSE_Resize,
        No_Drop,
        None,
        Not_Allowed,
        Pointer,
        Progress,
        Row_Resize,
        S_Resize,
        SE_Resize,
        SW_Resize,
        W_Resize,
        Wait,
        Zoom_In,
        Zoom_Out
    }
}