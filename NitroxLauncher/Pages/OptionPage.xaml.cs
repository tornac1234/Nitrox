using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NitroxLauncher.Models;
using NitroxLauncher.Models.Utils;
using NitroxModel.Discovery;

namespace NitroxLauncher.Pages
{
    public partial class OptionPage : PageBase
    {
        public Platform GamePlatform => LauncherLogic.Config.SubnauticaPlatform;
        public string PathToSubnautica => LauncherLogic.Config.SubnauticaPath;
        public string SubnauticaLaunchArguments => LauncherLogic.Config.SubnauticaLaunchArguments;

        public OptionPage()
        {
            InitializeComponent();

            ArgumentsTextbox.Text = SubnauticaLaunchArguments;
            if (SubnauticaLaunchArguments != LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS)
            {
                ResetButton.Visibility = Visibility.Visible;
            }

            Loaded += (s, e) =>
            {
                LauncherLogic.Config.PropertyChanged += OnLogicPropertyChanged;
                OnLogicPropertyChanged(null, null);
            };

            Unloaded += (s, e) =>
            {
                LauncherLogic.Config.PropertyChanged -= OnLogicPropertyChanged;
            };
        }

        private async void OnChangePath_Click(object sender, RoutedEventArgs e)
        {
            string selectedDirectory;

            // Don't use FolderBrowserDialog because its UI sucks. See: https://stackoverflow.com/a/31082
            using (CommonOpenFileDialog dialog = new()
            {
                Multiselect = false,
                InitialDirectory = PathToSubnautica,
                EnsurePathExists = true,
                IsFolderPicker = true,
                Title = "Select Subnautica installation directory"
            })
            {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                selectedDirectory = Path.GetFullPath(dialog.FileName);
            }

            if (!GameInstallationFinder.IsSubnauticaDirectory(selectedDirectory))
            {
                LauncherNotifier.Error("Invalid subnautica directory");
                return;
            }

            if (selectedDirectory != PathToSubnautica)
            {
                await LauncherLogic.Instance.SetTargetedSubnauticaPath(selectedDirectory);
                LauncherNotifier.Success("Applied changes");
            }
        }

        private void OnChangeArguments_Click(object sender, RoutedEventArgs e)
        {
            if (ArgumentsTextbox.Text == SubnauticaLaunchArguments)
            {
                return;
            }

            ResetButton.Visibility = SubnauticaLaunchArguments == LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS ? Visibility.Visible : Visibility.Hidden;
            ArgumentsTextbox.Text = LauncherLogic.Config.SubnauticaLaunchArguments = ArgumentsTextbox.Text.Trim();
            LauncherNotifier.Success("Applied changes");
        }

        private void OnResetArguments_Click(object sender, RoutedEventArgs e)
        {
            if (SubnauticaLaunchArguments != LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS)
            {
                ArgumentsTextbox.Text = LauncherLogic.Config.SubnauticaLaunchArguments = LauncherConfig.DEFAULT_LAUNCH_ARGUMENTS;
                ResetButton.Visibility = Visibility.Hidden;
                LauncherNotifier.Success("Applied changes");
                return;
            }
        }

        private void ForceModsCompat_Click(object sender, RoutedEventArgs e)
        {
            int patchedMods = 0;
            if (!QModHelper.IsQModInstalled(LauncherLogic.Config.SubnauticaPath))
            {
                LauncherNotifier.Error("QModManager is not installed, didn't force any compatibility");
                return;
            }
           
            foreach ((string, Dictionary<string, object>) qMod in GetQMods())
            {
                if (!qMod.Item2.ContainsKey("NitroxCompat"))
                {
                    qMod.Item2.Add("NitroxCompat", true);
                    patchedMods++;
                }

                File.WriteAllText(qMod.Item1, JsonConvert.SerializeObject(qMod.Item2));
            }

            LauncherNotifier.Success($"Forced {patchedMods} mods compatibility");
        }

        private void RemoveModsCompat_Click(object sender, RoutedEventArgs e)
        {
            int patchedMods = 0;
            if (!QModHelper.IsQModInstalled(LauncherLogic.Config.SubnauticaPath))
            {
                LauncherNotifier.Error("QModManager is not installed, didn't force any compatibility");
                return;
            }

            foreach ((string, Dictionary<string, object>) qMod in GetQMods())
            {
                if (qMod.Item2.Remove("NitroxCompat"))
                {
                    patchedMods++;
                    File.WriteAllText(qMod.Item1, JsonConvert.SerializeObject(qMod.Item2));
                }
            }

            LauncherNotifier.Success($"Removed compatibility for {patchedMods} mods");
        }

        private List<(string, Dictionary<string, object>)> GetQMods()
        {
            List<(string, Dictionary<string, object>)> qMods = new();
            string qModsPath = Path.Combine(LauncherLogic.Config.SubnauticaPath, "QMods");
            string[] files = Directory.GetDirectories(qModsPath);
            foreach (string file in files)
            {
                string modFile = Path.Combine(file, "mod.json");
                if (File.Exists(modFile))
                {
                    using StreamReader r = new(modFile);
                    string json = r.ReadToEnd();
                    Dictionary<string, object> modObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    r.Close();
                    qMods.Add((modFile, modObject));
                }
            }
            return qMods;
        }

        private void OnLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged(nameof(PathToSubnautica));
            OnPropertyChanged(nameof(GamePlatform));
        }
    }
}
