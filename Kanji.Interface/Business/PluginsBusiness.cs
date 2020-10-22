using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Kanji.Interface.Controls;
using Kanji.Interface.Helpers;
using Kanji.Interface.Plugins;
using Kanji.Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kanji.Interface.Business
{
    class PluginsBusiness
    {
        public static PluginsBusiness Instance { get; set; }

        public List<Plugin> Plugins { get; set; }
        private (Type ViewModel, Type View)[] genericSteps = new (Type, Type)[]
        {
            (typeof(ImportViewModel), typeof(ImportSelectionDialog)),
            (typeof(ImportOverviewViewModel), typeof(ImportOverview)),
            (typeof(ImportProgressViewModel), typeof(ImportProgress)),
        };
        public PluginsBusiness()
        {
            var pluginsDir = Path.Combine(ConfigurationHelper.CommonDataDirectoryPath, "Plugins");
            IEnumerable<Assembly> assemblies = new[] { Assembly.GetExecutingAssembly() };
            if (Directory.Exists(pluginsDir))
                assemblies = assemblies.Concat(Directory.GetFiles(pluginsDir, "*.dll").Select(Assembly.LoadFrom));
            
            Plugins = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Plugin)))
                .Select(Activator.CreateInstance).Cast<Plugin>().ToList();
        }
        public IEnumerable<FuncDataTemplate> PluginTemplates =>
            Plugins.SelectMany(p => p.Steps)
                    .Concat(genericSteps)
                    .Select(pair => new FuncDataTemplate(pair.ViewModel, (x, _) => (IControl)Activator.CreateInstance(pair.View)));
    }
}
