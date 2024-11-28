// Copyright (c) Eiveo GmbH. All rights reserved.

using System.Numerics;
using Microsoft.Xna.Framework.Input;
using RemnantOfTheAbyss.Input.Enums;

namespace RemnantOfTheAbyss.Input;

/// <summary>Input abstraction for better usability.</summary>
public class InputManager
{
    private readonly GamePadState[] _gamePadLast;
    private readonly GamePadState[] _gamePadCurrent;

    private KeyboardState _keyboardLast;
    private KeyboardState _keyboardCurrent;
    private MouseState _mouseLast;
    private MouseState _mouseCurrent;

    /// <summary>Initializes a new instance of the <see cref="InputManager"/> class.</summary>
    public InputManager()
    {
        _keyboardLast = _keyboardCurrent = Keyboard.GetState();

        _mouseLast = _mouseCurrent = Mouse.GetState();

        _gamePadLast = new GamePadState[GamePad.MaximumGamePadCount];
        _gamePadCurrent = new GamePadState[GamePad.MaximumGamePadCount];

        for (var i = 0; i < GamePad.MaximumGamePadCount; i++)
            _gamePadLast[i] = _gamePadCurrent[i] = GamePad.GetState(i);
    }

    /// <summary>Gets the mouse position.</summary>
    public Vector2 MousePosition => new(_mouseCurrent.X, _mouseCurrent.Y);

    /// <summary>Gets the mouse delta.</summary>
    public Vector2 MouseDelta => new(_mouseCurrent.X - _mouseLast.X, _mouseCurrent.Y - _mouseLast.Y);

    /// <summary>Gets the mouse wheel.</summary>
    public Vector2 MouseWheel => new(_mouseCurrent.HorizontalScrollWheelValue - _mouseLast.HorizontalScrollWheelValue, _mouseCurrent.ScrollWheelValue - _mouseLast.ScrollWheelValue);

    /// <summary>Checks whether a specific keyboard key is pressed in this frame.</summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns><c>true</c> if the keyboard key is pressed in this frame; otherwise, <c>false</c>.</returns>
    public bool IsPressed(Keys key)
    {
        return _keyboardCurrent.IsKeyDown(key);
    }

    /// <summary>Checks whether a specific keyboard key was pressed in the last frame.</summary>
    /// <param name="key">The keyboard key to check.</param>
    /// <returns><c>true</c> if the keyboard key was pressed in the last frame; otherwise, <c>false</c>.</returns>
    public bool WasPressed(Keys key)
    {
        return _keyboardLast.IsKeyDown(key);
    }

    /// <summary>Checks whether a specific mouse button was just pressed in this frame.</summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns><c>true</c> if the mouse button was pressed in this frame; otherwise, <c>false</c>.</returns>
    public bool IsPressed(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => _mouseCurrent.LeftButton == ButtonState.Pressed,
            MouseButton.Middle => _mouseCurrent.MiddleButton == ButtonState.Pressed,
            MouseButton.Right => _mouseCurrent.RightButton == ButtonState.Pressed,
            MouseButton.Extra1 => _mouseCurrent.XButton1 == ButtonState.Pressed,
            MouseButton.Extra2 => _mouseCurrent.XButton2 == ButtonState.Pressed,
            _ => false,
        };
    }

    /// <summary>Checks whether a specific mouse button was pressed in the last frame.</summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns><c>true</c> if the mouse button was pressed in the last frame; otherwise, <c>false</c>.</returns>
    public bool WasPressed(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => _mouseLast.LeftButton == ButtonState.Pressed,
            MouseButton.Middle => _mouseLast.MiddleButton == ButtonState.Pressed,
            MouseButton.Right => _mouseLast.RightButton == ButtonState.Pressed,
            MouseButton.Extra1 => _mouseLast.XButton1 == ButtonState.Pressed,
            MouseButton.Extra2 => _mouseLast.XButton2 == ButtonState.Pressed,
            _ => false,
        };
    }

    /// <summary>Checks whether a specific gamepad button is pressed in this frame.</summary>
    /// <param name="button">The gamepad button to check.</param>
    /// <param name="gamePadIndex">The index of the gamepad to check.</param>
    /// <returns><c>true</c> if the gamepad button is pressed in this frame; otherwise, <c>false</c>.</returns>
    public bool IsPressed(Buttons button, int gamePadIndex = 0)
    {
        return _gamePadCurrent[gamePadIndex].IsButtonDown(button);
    }

    /// <summary>Checks whether a specific gamepad button was pressed in the last frame.</summary>
    /// <param name="button">The gamepad button to check.</param>
    /// <param name="gamePadIndex">The index of the gamepad to check.</param>
    /// <returns><c>true</c> if the gamepad button was pressed in the last frame; otherwise, <c>false</c>.</returns>
    public bool WasPressed(Buttons button, int gamePadIndex = 0)
    {
        return _gamePadLast[gamePadIndex].IsButtonDown(button);
    }

    /// <summary>Checks whether a specific gamepad digital pad button is pressed in this frame.</summary>
    /// <param name="gamePadDigitalPad">The gamepad digital pad button to check.</param>
    /// <param name="gamePadIndex">The index of the gamepad to check.</param>
    /// <returns><c>true</c> if the gamepad digital pad button is pressed in this frame; otherwise, <c>false</c>.</returns>
    public bool IsPressed(GamePadDigitalPad gamePadDigitalPad, int gamePadIndex = 0)
    {
        var dpad = _gamePadCurrent[gamePadIndex].DPad;

        return gamePadDigitalPad switch
        {
            GamePadDigitalPad.Up => dpad.Up == ButtonState.Pressed,
            GamePadDigitalPad.Down => dpad.Down == ButtonState.Pressed,
            GamePadDigitalPad.Left => dpad.Left == ButtonState.Pressed,
            GamePadDigitalPad.Right => dpad.Right == ButtonState.Pressed,
            _ => false,
        };
    }

    /// <summary>Checks whether a specific gamepad digital pad button was pressed in the last frame.</summary>
    /// <param name="gamePadDigitalPad">The gamepad digital pad button to check.</param>
    /// <param name="gamePadIndex">The index of the gamepad to check.</param>
    /// <returns><c>true</c> if the gamepad digital pad button was pressed in the last frame; otherwise, <c>false</c>.</returns>
    public bool WasPressed(GamePadDigitalPad gamePadDigitalPad, int gamePadIndex = 0)
    {
        var dpad = _gamePadLast[gamePadIndex].DPad;

        return gamePadDigitalPad switch
        {
            GamePadDigitalPad.Up => dpad.Up == ButtonState.Pressed,
            GamePadDigitalPad.Down => dpad.Down == ButtonState.Pressed,
            GamePadDigitalPad.Left => dpad.Left == ButtonState.Pressed,
            GamePadDigitalPad.Right => dpad.Right == ButtonState.Pressed,
            _ => false,
        };
    }

    /// <summary>Gets the gamepad thumbstick.</summary>
    /// <param name="gamePadThumbStick">The gamepad thumbstick to get.</param>
    /// <param name="gamePadIndex">The index of the gamepad to get.</param>
    /// <returns>The gamepad thumbstick.</returns>
    public Vector2 GetGamePadThumbStick(GamePadThumbStick gamePadThumbStick, int gamePadIndex = 0)
    {
        var thumbSticks = _gamePadCurrent[gamePadIndex].ThumbSticks;

        return gamePadThumbStick switch
        {
            GamePadThumbStick.Left => thumbSticks.Left.ToNumerics(),
            GamePadThumbStick.Right => thumbSticks.Right.ToNumerics(),
            _ => Vector2.Zero,
        };
    }

    /// <summary>Gets the gamepad trigger.</summary>
    /// <param name="gamePadTrigger">The gamepad trigger to get.</param>
    /// <param name="gamePadIndex">The index of the gamepad to get.</param>
    /// <returns>The gamepad trigger.</returns>
    public Vector2 GetGamePadTrigger(GamePadTrigger gamePadTrigger, int gamePadIndex = 0)
    {
        var triggers = _gamePadCurrent[gamePadIndex].Triggers;

        return gamePadTrigger switch
        {
            GamePadTrigger.Left => new Vector2(triggers.Left, 0),
            GamePadTrigger.Right => new Vector2(triggers.Right, 0),
            _ => Vector2.Zero,
        };
    }

    /// <summary>Updates the inputs.</summary>
    public void Update()
    {
        _keyboardLast = _keyboardCurrent;
        _keyboardCurrent = Keyboard.GetState();

        _mouseLast = _mouseCurrent;
        _mouseCurrent = Mouse.GetState();

        for (var i = 0; i < GamePad.MaximumGamePadCount; i++)
        {
            _gamePadLast[i] = _gamePadCurrent[i];
            _gamePadCurrent[i] = GamePad.GetState(i);
        }
    }
}
