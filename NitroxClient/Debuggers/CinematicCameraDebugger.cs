using System.Diagnostics.CodeAnalysis;
using NitroxClient.Debuggers.Drawer;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.Debuggers;

[ExcludeFromCodeCoverage]
public class CinematicCameraDebugger : BaseDebugger
{
    private Vector2 scrollPosition;

    public CinematicCameraDebugger() : base(700, null, KeyCode.K, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
    {
        ActiveTab = AddTab("Points", RenderTabPoints);
        ActiveTab = AddTab("Settings", RenderTabSettings);
    }

    protected override void OnSetSkin(GUISkin skin)
    {
        base.OnSetSkin(skin);

        skin.SetCustomStyle("generated_keypoint",
                            skin.textArea,
                            s =>
                            {
                                s.normal = new GUIStyleState { textColor = Color.yellow };
                            });
    }

    private void RenderTabPoints()
    {
        if (!NitroxCinematicCamera.Instance)
        {
            GUILayout.TextArea("NitroxCinematicCamera is null");
            return;
        }

        using (new GUILayout.VerticalScope("Box"))
        {
            GUILayout.TextArea("Toggle Recording: M\nAdd Keypoint: P\nToggle Playback: O\nYellow keypoints are auto generated");

            using (new GUILayout.HorizontalScope())
            {
                if (NitroxCinematicCamera.Instance.IsRecording)
                {
                    if (GUILayout.Button("Add Point"))
                    {
                        NitroxCinematicCamera.Instance.AddKeyPoint();
                    }

                    if (GUILayout.Button("Clear All"))
                    {
                        NitroxCinematicCamera.Instance.KeyPoints.Clear();
                    }

                    if (GUILayout.Button("Stop Recording"))
                    {
                        NitroxCinematicCamera.Instance.StopRecording();
                    }
                }
                else if (NitroxCinematicCamera.Instance.IsPlaying)
                {
                    if (GUILayout.Button("Stop Playback"))
                    {
                        NitroxCinematicCamera.Instance.StopPlayback();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start Recording"))
                    {
                        NitroxCinematicCamera.Instance.StartRecording();
                    }

                    if (GUILayout.Button("Start Playback"))
                    {
                        NitroxCinematicCamera.Instance.StartPlayback();
                    }
                }
            }

            using (new GUILayout.VerticalScope("Box"))
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500f));

                using (new GUILayout.HorizontalScope("Box"))
                {
                    GUILayout.TextArea("Index", GUILayout.Width(60));
                    GUILayout.TextArea("Time", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
                    GUILayout.TextArea("Position", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
                    GUILayout.TextArea("Rotation", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
                    GUILayout.TextArea("Del", GUILayout.Width(30));
                }

                for (int index = 0; index < NitroxCinematicCamera.Instance.KeyPoints.Count; index++)
                {
                    NitroxCinematicCamera.KeyPoint keyPoint = NitroxCinematicCamera.Instance.KeyPoints[index];
                    using (new GUILayout.HorizontalScope("Box"))
                    {
                        string style = keyPoint.IsGenerated ? "generated_keypoint" : "Box";
                        int newIndex = NitroxGUILayout.IntField(index, 60);

                        keyPoint.Time = NitroxGUILayout.FloatField(keyPoint.Time);
                        GUILayout.TextArea(keyPoint.Position.ToString(), style, GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
                        GUILayout.TextArea(keyPoint.Rotation.eulerAngles.ToString(), style, GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));

                        if (GUILayout.Button("X", GUILayout.Width(30)))
                        {
                            NitroxCinematicCamera.Instance.KeyPoints.RemoveAt(index);
                            index--;
                        }

                        if (newIndex != index && newIndex < NitroxCinematicCamera.Instance.KeyPoints.Count)
                        {
                            (NitroxCinematicCamera.Instance.KeyPoints[newIndex], NitroxCinematicCamera.Instance.KeyPoints[index]) = (NitroxCinematicCamera.Instance.KeyPoints[index], NitroxCinematicCamera.Instance.KeyPoints[newIndex]); // Swap
                        }
                    }
                }

                GUILayout.EndScrollView();
            }
        }
    }

    private void RenderTabSettings()
    {
        if (!NitroxCinematicCamera.Instance)
        {
            GUILayout.TextArea("NitroxCinematicCamera is null");
            return;
        }

        using (new GUILayout.VerticalScope("Box"))
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Is running: {NitroxCinematicCamera.Instance.enabled}", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                if (GUILayout.Button("Toggle", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
                {
                    NitroxCinematicCamera.Instance.enabled = !NitroxCinematicCamera.Instance.enabled;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Show keypoints in world: {NitroxCinematicCamera.Instance.ShowDebugLine}", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                if (GUILayout.Button("Toggle", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
                {
                    NitroxCinematicCamera.Instance.ShowDebugLine = !NitroxCinematicCamera.Instance.ShowDebugLine;
                }
                if (GUILayout.Button("Refresh", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
                {
                    NitroxCinematicCamera.Instance.RefreshDebugLines();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("PlaybackSpeed", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                NitroxCinematicCamera.Instance.PlaybackSpeed = NitroxGUILayout.FloatField(NitroxCinematicCamera.Instance.PlaybackSpeed);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("ColdStartDuration", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                NitroxCinematicCamera.Instance.ColdStartDuration = NitroxGUILayout.FloatField(NitroxCinematicCamera.Instance.ColdStartDuration);
            }

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Show UI"))
                {
                    ShowUI();
                }

                if (GUILayout.Button("Hide UI"))
                {
                    HideUI();
                }
            }
        }
    }

    private void ShowUI()
    {
        GUIController.SetHidePhase(GUIController.HidePhase.None);
        GUIController.main.hidePhase = GUIController.HidePhase.None;
        Player.main.mode = Player.Mode.Normal; // See uGUI_Pings.IsVisibleNow()
    }

    private void HideUI()
    {
        GUIController.SetHidePhase(GUIController.HidePhase.All);
        GUIController.main.hidePhase = GUIController.HidePhase.All;
        Player.main.mode = Player.Mode.Sitting; // See uGUI_Pings.IsVisibleNow()
    }
}
