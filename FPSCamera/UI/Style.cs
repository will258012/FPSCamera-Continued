namespace FPSCamera.UI
{
    using CSkyLColor = CSkyL.UI.Style.CSkyLColor;
    using SkyUI = CSkyL.UI;

    public static class Style
    {
        public static readonly SkyUI.Style basic = new SkyUI.Style
        {
            namePrefix = "FPS_",
            textColor = CSkyLColor.RGB(221, 220, 250),
            color = CSkyLColor.RGBA(162, 160, 240, 250),
            bgColor = CSkyLColor.RGBA(51, 50, 120, 250),
            colorDisabled = CSkyLColor.RGBA(42, 40, 80, 220),
            textColorDisabled = CSkyLColor.RGB(122, 120, 140),
            scale = 1f,
            padding = 15
        };
    }
}
