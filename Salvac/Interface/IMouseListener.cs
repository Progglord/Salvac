using System;
using OpenTK;
using System.Windows.Forms;

namespace Salvac.Interface
{
    public interface IMouseListener
    {
        int Priority
        { get; }

        bool IsMouseOverListener(Vector2 position);

        void MouseClick(MouseButtons button, Vector2 position);
        void MouseDoubleClick(MouseButtons button, Vector2 position);

        void MouseDrag(MouseButtons button, Vector2 delta, float wheelDelta);
        void MouseMove(Vector2 position, float wheelDelta);

    }
}
