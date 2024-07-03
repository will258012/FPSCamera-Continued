namespace FPSCamera
{
    using ICities;

    public sealed class ThreadingExtension : ThreadingExtensionBase
    {
        public static Controller Controller;
        public override void OnAfterSimulationFrame()
        {
            base.OnAfterSimulationFrame();
            Controller?.SimulationFrame();
        }
    } // end class
}
