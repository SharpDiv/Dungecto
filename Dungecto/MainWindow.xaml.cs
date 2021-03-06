﻿using Dungecto.Common;
using Dungecto.Properties;
using Dungecto.View;
using Dungecto.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Dungecto
{
    using System.Globalization;

    /// <summary> Interaction logic for MainWindow.xaml </summary>
    public partial class MainWindow : MetroWindow 
    {
        /// <summary> Initializes a new instance of the MainWindow class. </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            TextVersion.Text = String.Format(CultureInfo.InvariantCulture, "{0} {1}", Resource.Version, AppInfo.GetProductVersion());
            Copyright.Text = AppInfo.GetLegalCopyright();

            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        /// <summary> Open if closed/close if open main menu</summary>
        private void OpenCloseMainMenu(object sender, RoutedEventArgs e)
        {
            MainMenuFlyout.IsOpen = !MainMenuFlyout.IsOpen;
        }

        /// <summary> Open if closed/close if open map properties</summary>
        private void OpenCloseMapProperties(object sender, RoutedEventArgs e)
        {
            MapPropertiesFlyout.IsOpen = !MapPropertiesFlyout.IsOpen;

            if(TilePropertiesFlyout.IsOpen)
            {
                TilePropertiesFlyout.IsOpen = false;
            }
        }

        /// <summary> Open if closed/close if open selected tile properties</summary>
        private void OpenCloseTileProperties(object sender, RoutedEventArgs e)
        {
            TilePropertiesFlyout.IsOpen = !TilePropertiesFlyout.IsOpen;

            if(MapPropertiesFlyout.IsOpen)
            {
                MapPropertiesFlyout.IsOpen = false;
            }
        }

        /// <summary> Click on "Remove tile" context menu. Close tile properties panel</summary>
        private void ClickOnMenuItemRemoveTile(object sender, RoutedEventArgs e)
        {
            TilePropertiesFlyout.IsOpen = false;
        }

        /// <summary>Show About panel, hide main menu</summary>
        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            AboutFlyout.IsOpen = true;
            MainMenuFlyout.IsOpen = false;
        }

        /// <summary> Start browser and go to e.Uri.AbsoluteUri </summary>
        private void ClickOnHyperlink(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        /// <summary> Scroll map with mouse wheel </summary>
        /// <param name="sender"> It has to be <seealso cref="ScrollViewer"/></param>
        private void MapPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollView = sender as ScrollViewer;
            if (scrollView == null) { return; }

            scrollView.ScrollToVerticalOffset(scrollView.VerticalOffset - e.Delta);
            e.Handled = true;
        }

//TODO: dirty trick

        /// <summary>Left click on toolbox. Begin dragging item from toolbox</summary>
        /// <param name="sender">Some kind of FrameworkElement with <seealso cref="Model.Tile"/> as DataContext</param>
        private void ToolboxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var contex = (sender as FrameworkElement).DataContext;
            if (contex == null) { return; }

            var desc = contex as Model.Tile;
            if (desc == null) { return; }

            var dragObj = new DataObject("{MapTile}", desc.Clone());
            DragDrop.DoDragDrop(this, dragObj, DragDropEffects.Copy);
        }

        /// <summary> Drop on canvas </summary>
        private void CanvasDrop(object sender, DragEventArgs e)
        {
            if (e == null || e.Data == null) { return; }

            var dropData = e.Data.GetData("{MapTile}");
            if (dropData == null) { return; }

            var dropTile = dropData as Model.Tile;
            if (dropTile == null) { return; }

            var uielement = sender as UIElement;
            if (uielement == null) { return; }

            dropTile.X = Convert.ToInt32(e.GetPosition(uielement).X);
            dropTile.Y = Convert.ToInt32(e.GetPosition(uielement).Y);

            //TODO: fix it
            (DataContext as MainViewModel).Map.Tiles.Add(dropTile);
        }

//TODO: dirty trick

        /// <summary> Canvas with tiles </summary>
        /// <remarks> I need this to export tiles to image file</remarks>
        private MapCanvas _canvas = null;
        
        /// <summary> After listview load </summary>
        /// <param name="sender"><seealso cref="MapCanvas"/></param>
        /// <param name="e"></param>
        private void ListViewTemplateLoaded(object sender, RoutedEventArgs e)
        {
            _canvas = sender as MapCanvas;

            if(_canvas == null)
            {
                throw new InvalidCastException("Map canvas is not initialized");
            }
        }

//TODO: dirty trick

        /// <summary> Export <see cref="_canvas"/> to png</summary>
        private void ExportMap(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel).SelectedTile = null;

            var file = Dialogs.ShowSaveDialog("", ".png");

            if (!string.IsNullOrEmpty(file))
            {
                Exporter.ToPng(_canvas, file);
            }
        }

//TODO: dirty trick

        /// <summary>Click on tile, makes it <c>Selected</c></summary>
        /// <param name="sender">Some kind of <see cref="FrameworkElement"/> with <seealso cref="Model.Tile"/> in DataContext</param>
        private void ThumbMover_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var felement = sender as FrameworkElement;
            if (felement == null) { return; }

            var mtile = felement.DataContext as Model.Tile;

            (DataContext as MainViewModel).SelectedTile = mtile;
        }

//TODO: dirty trick
        private void MapCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var context = DataContext as MainViewModel;

            switch (context.EditorMode)
            {
                case EditorMode.Filler: { context.AddFiller(e.GetPosition(_canvas)); break; }

                //TODO: Change to context.DelFiller
                case EditorMode.Eraser: { context.AddFiller(e.GetPosition(_canvas)); break; }

                case EditorMode.ColorPicker: { context.GetFillerColorAt(e.GetPosition(_canvas)); break; }
            }

        }

//TODO: dirty trick
        private void ToolboxPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

    }
}
