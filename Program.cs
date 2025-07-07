// SPDX-License-Identifier: MIT
// The authors below grant copyright rights under the MIT license:
// Copyright (c) 2019-2025 Nick Klingensmith
// Copyright (c) 2023-2025 Qualcomm Technologies, Inc.

using StereoKit;
using StereoKit.Framework;
using System;

class Program
{
    // This is the starting scene, and can be overridden by passing
    // -start <testname> via the CLI.
    static string startTest = "welcome";

    // The base settings we use for this test app. Some of these, like mode,
    // are overridden, particularly when running tests.
    static SKSettings settings = new SKSettings
    {
        appName = "StereoKit C#",
        blendPreference = DisplayBlend.AnyTransparent,
        mode = AppMode.XR,
    };

    static Mesh floorMesh;
    static Material floorMat;
    static Pose windowDemoPose = new Pose(-0.7f, 0, -0.3f, Quat.LookDir(1, 0, 1));
    static Sprite powerButton;
    static SceneType sceneCategory = SceneType.Programs;

    public static bool WindowDemoShow = false;

    static void Main(string[] args)
    {
        // CLI arguments
        bool headless = ParamPresent(args, "-headless");
        MenuSort.IsTesting = ParamPresent(args, "-test");
        MenuSort.MakeScreenshots = !ParamPresent(args, "-noscreens");
        MenuSort.ScreenshotRoot = ParamVal(args, "-screenfolder", "../../../docs/img/screenshots");
        MenuSort.GltfFolders = ParamVal(args, "-gltf", null); // "C:\\Tools\\glTF-Sample-Models-master\\2.0";
        MenuSort.GltfScreenshotRoot = ParamVal(args, "-gltfscreenfolder", null);
        MenuSort.TestSingle = ParamPresent(args, "-start");
        startTest = ParamVal(args, "-start", startTest);

        if (MenuSort.IsTesting)
        {
            settings.mode = headless ? AppMode.Offscreen : AppMode.Simulator;
            settings.standbyMode = StandbyMode.None;
        }

        // OpenXR extensions need added before SK.Initialize, so does
        // LogWindow for early log registration!
        SK.AddStepper<PassthroughFBExt>();
        //SK.AddStepper<Win32PerformanceCounterExt>();

        // Initialize StereoKit
        if (!SK.Initialize(settings))
            Environment.Exit(1);

        Init();

        SK.Run(Step, MenuSort.Shutdown);
    }

    static void Init()
    {
        floorMat = new Material("Shaders/floor_shader.hlsl");
        floorMat.Transparency = Transparency.Blend;
        floorMat.QueueOffset = -11;
        floorMat["radius"] = new Vec4(5, 10, 0, 0);

        floorMesh = Mesh.GeneratePlane(V.XY(40, 40), Vec3.Up, Vec3.Forward);
        powerButton = Sprite.FromTex(Tex.FromFile("power.png"));

        MenuSort.FindScenes();
        MenuSort.SetClassActive(startTest);
        MenuSort.Initialize();

        if (MenuSort.IsTesting)
        {
            UI.EnableFarInteract = false;
            Time.Scale = 0;
            WindowDemoShow = false;
        }
        else
        {
            WindowDemoShow = true;
        }
    }

    //////////////////////

    static void Step()
    {
        if (Input.Key(Key.Esc).IsJustActive())
            SK.Quit();

        /// :CodeSample: Projection Renderer.Projection
        /// ### Toggling the projection mode
        /// Only in flatscreen apps, there is the option to change the main
        /// camera's projection mode between perspective and orthographic.
        if (SK.ActiveDisplayMode == DisplayMode.Flatscreen &&
            Input.Key(Key.P).IsJustActive())
        {
            Renderer.Projection = Renderer.Projection == Projection.Perspective
                ? Projection.Ortho
                : Projection.Perspective;
        }
        /// :End:

        // If we can't see the world, we'll draw a floor!
        if (Device.DisplayBlend == DisplayBlend.Opaque)
            floorMesh.Draw(floorMat, World.HasBounds ? World.BoundsPose.ToMatrix() : Matrix.T(0, -1.5f, 0), Color.White);

        CheckFocus();
        MenuSort.Update();
        WindowDemoStep();
    }

    static void WindowDemoStep()
    {
        // Skip the window if we're in test mode
        if (!WindowDemoShow) return;

        // Make a window for demo selection
        UI.WindowBegin("Demos", ref windowDemoPose, new Vec2(50 * U.cm, 0));

        // Display the different categories of tests we have available
        for (int i = 0; i < (int)SceneType.MAX; i++)
        {
            SceneType category = (SceneType)i;
            if (UI.Radio(category.ToString(), category == sceneCategory))
                sceneCategory = category;
            UI.SameLine();
        }
        // Display a quit button on the far right side
        Vec2 exitSize = new Vec2(0.06f, 0);
        UI.HSpace(UI.LayoutRemaining.x - exitSize.x);
        if (UI.ButtonImg("Exit", powerButton, UIBtnLayout.Left, exitSize))
            SK.Quit();

        UI.HSeparator();

        // Now display a nice, lined-up collection of buttons for each
        // demo/test in the current category.
        int start = 0;
        float currWidthTotal = 0;
        UISettings uiSettings = UI.Settings;
        TextStyle style = UI.TextStyle;
        float windowWidth = UI.LayoutRemaining.x;
        for (int i = 0; i < MenuSort.Count(sceneCategory); i++)
        {
            float width = Text.Size(MenuSort.GetClassName(sceneCategory, i), style).x + uiSettings.padding * 2;
            if (currWidthTotal + (width + uiSettings.gutter) > windowWidth)
            {
                float inflate = (windowWidth - (currWidthTotal - uiSettings.gutter + 0.0001f)) / (i - start);
                for (int t = start; t < i; t++)
                {
                    string name = MenuSort.GetClassName(sceneCategory, t);
                    float currWidth = Text.Size(name, style).x + uiSettings.padding * 2 + inflate;
                    if (UI.Radio(name, MenuSort.IsActive(sceneCategory, t), null, null, UIBtnLayout.None, new Vec2(currWidth, 0)))
                        MenuSort.SetClassActive(sceneCategory, t);
                    UI.SameLine();
                }
                start = i;
            }
            if (start == i)
                currWidthTotal = 0;
            currWidthTotal += width + uiSettings.gutter;
        }
        for (int t = start; t < MenuSort.Count(sceneCategory); t++)
        {
            string name = MenuSort.GetClassName(sceneCategory, t);
            float currWidth = Text.Size(name, style).x + uiSettings.padding * 2;
            if (UI.Radio(name, MenuSort.IsActive(sceneCategory, t), null, null, UIBtnLayout.None, new Vec2(currWidth, 0)))
                MenuSort.SetClassActive(sceneCategory, t);
            UI.SameLine();
        }

        UI.WindowEnd();
    }
    /// :CodeSample: AppFocus SK.AppFocus
    /// ### Checking for changes in application focus
    static AppFocus lastFocus = AppFocus.Hidden;
    static void CheckFocus()
    {
        if (lastFocus != SK.AppFocus)
        {
            lastFocus = SK.AppFocus;
            Log.Info($"App focus changed to: {lastFocus}");
        }
    }
    /// :End:

    static bool ParamPresent(string[] args, string param)
        => Array.IndexOf(args, param) != -1;
    static string ParamVal(string[] args, string param, string defaultVal)
    {
        int index = Array.IndexOf(args, param);
        return (index == -1 || index + 1 >= args.Length)
            ? defaultVal
            : args[index + 1];
    }
}