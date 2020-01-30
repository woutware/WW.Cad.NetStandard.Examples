using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.IO;
using WW.Math;

using WebApplication.Models;
using WW.Cad.Model.Entities;

namespace WebApplication.Controllers {
    public class HomeController : Controller {
        /// <summary>
        /// Creates and displays a simple AutoCAD drawing.
        /// 
        /// Important:
        /// 
        /// The Wout Ware license is initialized in <see cref="Startup.Startup(Microsoft.Extensions.Configuration.IConfiguration)"/>.
        /// The comments there explain how to obtain and setup a trial license.
        /// </summary>
        public IActionResult Index() {
            DxfModel model = CreateAutoCadDrawing();
            Image<Bgra32> bitmap = RenderToBitmap(model);

            using (MemoryStream stream = new MemoryStream()) {
                bitmap.SaveAsJpeg(stream);
                return File(stream.ToArray(), "image/jpeg");
            }
        }

        private static DxfModel CreateAutoCadDrawing() {
            DxfModel model = new DxfModel();
            DxfText text = new DxfText("Welcome to WW.Cad", Point3D.Zero, 1) { Color = EntityColors.Blue };
            model.Entities.Add(text);
            model.Entities.Add(new DxfText("the trial version only supports the orange color", new Point3D(0.2, -0.5, 0), 0.3) { Color = EntityColors.Gray });

            // Make a fancy rectangle with rounded corner around the text.
            var textBounds = text.TextBounds;
            double margin = 1;
            double roundedCornerBulge = 0.5;
            model.Entities.Add(
                new DxfLwPolyline(
                    new DxfLwPolyline.Vertex(textBounds.Min - margin * Vector2D.XAxis, roundedCornerBulge),
                    new DxfLwPolyline.Vertex(textBounds.Min - margin * Vector2D.YAxis),
                    new DxfLwPolyline.Vertex(textBounds.Min + textBounds.Delta.X * Vector2D.XAxis - margin * Vector2D.YAxis, roundedCornerBulge),
                    new DxfLwPolyline.Vertex(textBounds.Min + textBounds.Delta.X * Vector2D.XAxis + margin * Vector2D.XAxis),
                    new DxfLwPolyline.Vertex(textBounds.Max + margin * Vector2D.XAxis, roundedCornerBulge),
                    new DxfLwPolyline.Vertex(textBounds.Max + margin * Vector2D.YAxis),
                    new DxfLwPolyline.Vertex(textBounds.Min + textBounds.Delta.Y * Vector2D.YAxis + margin * Vector2D.YAxis, roundedCornerBulge),
                    new DxfLwPolyline.Vertex(textBounds.Min + textBounds.Delta.Y * Vector2D.YAxis - margin * Vector2D.XAxis)
                ) { Closed = true, Color = EntityColors.Green });
            return model;
        }

        private static Image<Bgra32> RenderToBitmap(DxfModel model) {
            var graphicsConfig = (GraphicsConfig)GraphicsConfig.AcadLikeWithWhiteBackground.Clone();

            BoundsCalculator boundsCalculator = new BoundsCalculator();
            boundsCalculator.GetBounds(model);
            Bounds3D bounds = boundsCalculator.Bounds;

            int size = 2000;
            Matrix4D transform = DxfUtil.GetScaleTransform(
                bounds.Min,
                bounds.Max,
                bounds.Center,
                new Point3D(0, size, 0),
                new Point3D(size, 0, 0),
                new Point3D(0.5d * size, 0.5d * size, 0)
            );

            Memory<Bgra32> memory = new Memory<Bgra32>();
            Image.WrapMemory<Bgra32>(memory, 10, 10);
            Image<Bgra32> bitmap = ImageExporter.CreateBitmap<Bgra32>(
                model, transform,
                graphicsConfig,
                //new GraphicsOptions(false),
                new Size(size, size));
            return bitmap;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
