#if DEBUG

using System;
using System.Diagnostics;

namespace Salvac.Interface.Rendering
{

    public static class DebugInfo
    {
        public static int DrawnSectorBackgrounds
        { get; set; }

        public static int DrawnSectorBoundaries
        { get; set; }

        public static bool DrawBoundingBoxes
        { get; set; }

        public static double LastFrameTime
        { get; set; }
    }

}

#endif