﻿using System.Drawing;
using System.Threading;

namespace Game;

public interface IInput
{
    void KeyDown(int key);

    void KeyUp(int key);

    int PressRandom(int key, int milliseconds);

    int PressRandom(int key, int milliseconds, CancellationToken ct);

    void PressFixed(int key, int milliseconds, CancellationToken ct);

    void SetCursorPos(Point p);

    void RightClick(Point p);

    void LeftClick(Point p);

    void SendText(string text);

    void SetClipboard(string text);

    void PasteFromClipboard();
}
