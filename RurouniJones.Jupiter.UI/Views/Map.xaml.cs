﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using MapControl;
using MapControl.Caching;
using RurouniJones.Jupiter.UI.ViewModels;

namespace RurouniJones.Jupiter.UI.Views
{
    public partial class Map : UserControl
    {
        static Map()
        {
            try
            {
                ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "DCS.Jupiter");

                TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public Map()
        {
            InitializeComponent();

            if (TileImageLoader.Cache is ImageFileCache cache)
            {
                Loaded += async (s, e) =>
                {
                    await Task.Delay(2000);
                    await cache.Clean();
                };
            }
        }

        /// <summary>
        /// Update the Mouse Position based on mouse movement.
        ///
        /// There must be a better way to do this using bindings but I cannot figure out how so use this for the moment.
        /// This is also the method used in the XAML.MapControl sample WPF application so maybe this is currently the only way.
        /// </summary>
        /// <param name="sender">The Sender which is always the MapControl</param>
        /// <param name="e">The Mouse Event Argument</param>
        private void MainMap_OnMouseMove(object sender, MouseEventArgs e)
        {
            ((MapViewModel) DataContext).MouseLocation = ((MapControl.Map) sender).ViewToLocation(e.GetPosition(MainMap)); 
        }
    }
}