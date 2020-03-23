﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Threading.Tasks;

namespace nUpdate.Administration.PluginBase
{
    public sealed class PluginLoader<TPlugin> where TPlugin : IPluginBase
    {
        private bool _pluginsLoaded;
        private readonly string _pluginDirectory;
        private IEnumerable<Lazy<TPlugin>> _plugins;
        // TODO: Check, if plugin is supported
        [ImportMany]
        public IEnumerable<Lazy<TPlugin>> Plugins
        {
            get
            {
                if (!_pluginsLoaded)
                    throw new InvalidOperationException("Plugins must be loaded before using them.");
                return _plugins;
            }
            set => _plugins = value;
        }

        public PluginLoader(string pluginDirectory)
        {
            if (string.IsNullOrWhiteSpace(pluginDirectory))
                throw new ArgumentException(nameof(pluginDirectory));
            _pluginDirectory = pluginDirectory;
        }

        public Task Load()
        {
            if (_pluginsLoaded)
                throw new InvalidOperationException("Plugins have already been loaded.");

            var aggregateCatalog = new AggregateCatalog();
            aggregateCatalog.Catalogs.Add(new DirectoryCatalog(_pluginDirectory));
            aggregateCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetEntryAssembly()));
            var container = new CompositionContainer(aggregateCatalog);
            container.ComposeParts(this);

            _pluginsLoaded = true;
            return Task.CompletedTask;
        }
    }
}