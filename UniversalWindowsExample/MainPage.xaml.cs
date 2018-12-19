using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using SixLabors.Primitives;

using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.Drawing.NetCore;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Math;
using WW.Cad.Drawing.Uwp;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniversalWindowsExample {
    /// <summary>
    /// This is a basic UWP application displaying an AutoCAD drawing.
    /// First setup up your trial license in <see cref="App.App()"/>.
    /// </summary>
    public sealed partial class MainPage : Page {
        private DxfModel model;
        private NetCoreGraphics3D cadGraphics;
        private Bounds3D bounds;
        private EventDelayer resizeEventDelayer;
        private DispatcherTimer resizeEventTimer;

        public MainPage() {
            this.InitializeComponent();

            resizeEventTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            resizeEventTimer.Tick += (sender, e) => UpdateImageControlSource();
            resizeEventDelayer = new EventDelayer(TimeSpan.FromMilliseconds(100));
            resizeEventDelayer.DelayedEvent += (sender, e) => UpdateImageControlSource();
        }

        private async void openAutoCadFileMenuItem_Click(object sender, RoutedEventArgs e) {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add(".dwg");
            openPicker.FileTypeFilter.Add(".dxf");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null) {
                Stream inStream = await file.OpenStreamForReadAsync();
                model = DwgReader.Read(inStream);

                var graphicsConfig = GraphicsConfig.AcadLikeWithBlackBackground;
                cadGraphics = new NetCoreGraphics3D(graphicsConfig, ImageRenderOptions.Default.GraphicsOptions);
                cadGraphics.CreateDrawables(model);
                bounds = new Bounds3D();
                cadGraphics.BoundingBox(bounds);

                resizeEventDelayer.StopStart();
            }
        }

        private void imageControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            resizeEventDelayer.StopStart();
        }

        private void UpdateImageControlSource() {
            var bitmap = CreateBitmap();
            if (bitmap != null) {
                imageControl.Source = CreateBitmap();
            }
        }

        private unsafe WriteableBitmap CreateBitmap() {
            WriteableBitmap writeableBitmap = null;
            FrameworkElement frameworkElement = mainGrid;
            if (
                model != null && 
                !double.IsNaN(frameworkElement.ActualWidth) && 
                !double.IsNaN(frameworkElement.ActualHeight) && 
                frameworkElement.ActualWidth != 0 && 
                frameworkElement.ActualHeight != 0
            ) {
                int pixelWidth = (int)frameworkElement.ActualWidth;
                int pixelHeight = (int)frameworkElement.ActualHeight;

                SixLabors.Primitives.Size size = new SixLabors.Primitives.Size(pixelWidth, pixelHeight);
                Matrix4D transform = DxfUtil.GetScaleTransform(
                    bounds.Min,
                    bounds.Max,
                    bounds.Center,
                    new Point3D(0, size.Height, 0),
                    new Point3D(size.Width, 0, 0),
                    new Point3D(0.5d * size.Width, 0.5d * size.Height, 0)
                );

                writeableBitmap = new WriteableBitmap(pixelWidth, pixelHeight);
                cadGraphics.Draw(writeableBitmap, new Rectangle(0, 0, size.Width, size.Height), transform);
            }
            return writeableBitmap;
        }
    }
}
