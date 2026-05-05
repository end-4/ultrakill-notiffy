using System;
using System.IO;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;

namespace Notiffy {
    public class ConfigManager {
        public static PluginConfigurator? config;

        public static FloatSliderField defaultTimeout;

        public static void Initialize() {
            config = PluginConfigurator.Create("Notiffy", NotiffyPlugin.PluginGUID);
            string iconPath = Path.Combine(NotiffyPlugin.workingDir, "icon.png");
            if (File.Exists(iconPath)) config.SetIconWithURL(iconPath);

            new ConfigHeader(config.rootPanel, "", 10);
            new ConfigHeader(config.rootPanel, "-- NOTIFICATION SYSTEM --", 24);
            defaultTimeout = new FloatSliderField(config.rootPanel, "Default timeout (secs)",
                "defaultTimeout", Tuple.Create(1f, 20f), 6);
        }
    }
}
