using System;

namespace FPSCamera.Utils;

public class SimulationElapsedTimer
{
    internal TimeSpan Elapsed
    {
        get
        {
            Update();
            return elapsed;
        }
    }

    internal void Reset()
    {
        elapsed = TimeSpan.Zero;
        lastGameTime = SimulationManager.instance.m_currentGameTime;
    }

    private void Update()
    {
        var currentGameTime = SimulationManager.instance.m_currentGameTime;
        if (lastGameTime != default && currentGameTime >= lastGameTime)
            elapsed += currentGameTime - lastGameTime;
        lastGameTime = currentGameTime;
    }

    private TimeSpan elapsed;
    private DateTime lastGameTime;
}
