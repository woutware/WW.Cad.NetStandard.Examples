using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;

using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Tables;
using WW.Drawing.Printing;
using WW.Math;
using WW.Math.Geometry;

namespace PdfExportExample {
    // This class demonstrates how to export an AutoCAD file to PDF (both model space and paper space layouts).
    public class PdfExporterExample {
        // Exports an AutoCAD file to PDF. For each layout a page in the PDF file is created.
        public static void ExportToPdf(string filename) {
            DxfModel model = CadReader.Read(filename);
            ExportToPdf(model);
        }

        // Exports an AutoCAD file to PDF. For each layout a page in the PDF file is created.
        public static void ExportToPdf(DxfModel model) {
            string filename = Path.GetFileName(model.Filename);
            string dir = Path.GetDirectoryName(model.Filename);
            string filenameNoExt = Path.GetFileNameWithoutExtension(filename);
            // as PDF
            using (FileStream stream = File.Create(Path.Combine(dir, filenameNoExt + ".pdf"))) {
                PdfExporter pdfExporter;
                GraphicsConfig config;
                CreatePdfExporter(stream, out pdfExporter, out config);

                foreach (DxfLayout layout in model.OrderedLayouts) {
                    AddLayoutToPdfExporter(pdfExporter, config, model, null, layout);
                }
                pdfExporter.EndDocument();
            }
        }

        // Exports the specified layout of an AutoCAD file to PDF.
        public static void ExportToPdf(DxfModel model, DxfLayout layout) {
            string filename = Path.GetFileName(model.Filename);
            string dir = Path.GetDirectoryName(model.Filename);
            string filenameNoExt = Path.GetFileNameWithoutExtension(filename);
            // as PDF
            using (FileStream stream = File.Create(Path.Combine(dir, filenameNoExt + "-" + layout.Name + ".pdf"))) {
                PdfExporter pdfExporter;
                GraphicsConfig config;
                CreatePdfExporter(stream, out pdfExporter, out config);

                AddLayoutToPdfExporter(pdfExporter, config, model, null, layout);

                pdfExporter.EndDocument();
            }
        }

        // For each layout, add a page to the PDF file.
        // Optionally specify a modelView (for model space only).
        // Optionally specify a layout.
        public static void AddLayoutToPdfExporter(
            PdfExporter pdfExporter, GraphicsConfig config, DxfModel model, DxfView modelView, DxfLayout layout
        ) {
            Bounds3D bounds;
            const float defaultMargin = 0.5f;
            float margin = 0f;
            PaperSize paperSize = null;
            bool useModelView = false;
            bool emptyLayout = false;
            if (layout == null || !layout.PaperSpace) {
                // Model space.
                BoundsCalculator boundsCalculator = new BoundsCalculator();
                boundsCalculator.GetBounds(model);
                bounds = boundsCalculator.Bounds;
                if (bounds.Initialized) {
                    if (bounds.Delta.X > bounds.Delta.Y) {
                        paperSize = PaperSizes.GetPaperSize(PaperKind.A4Rotated);
                    } else {
                        paperSize = PaperSizes.GetPaperSize(PaperKind.A4);
                    }
                } else {
                    emptyLayout = true;
                }
                margin = defaultMargin;
                useModelView = modelView != null;
            } else {
                // Paper space layout.
                Bounds2D plotAreaBounds = layout.GetPlotAreaBounds();
                bounds = new Bounds3D();
                emptyLayout = !plotAreaBounds.Initialized;
                if (plotAreaBounds.Initialized) {
                    bounds.Update((Point3D)plotAreaBounds.Min);
                    bounds.Update((Point3D)plotAreaBounds.Max);

                    if (layout.PlotArea == PlotArea.LayoutInformation) {
                        switch (layout.PlotPaperUnits) {
                            case PlotPaperUnits.Millimeters:
                                paperSize = new PaperSize(Guid.NewGuid().ToString(), (int)(plotAreaBounds.Delta.X * 100d / 25.4d), (int)(plotAreaBounds.Delta.Y * 100d / 25.4d));
                                break;
                            case PlotPaperUnits.Inches:
                                paperSize = new PaperSize(Guid.NewGuid().ToString(), (int)(plotAreaBounds.Delta.X * 100d), (int)(plotAreaBounds.Delta.Y * 100d));
                                break;
                            case PlotPaperUnits.Pixels:
                                // No physical paper units. Fall back to fitting layout into a known paper size.
                                break;
                        }
                    }

                    if (paperSize == null) {
                        if (bounds.Delta.X > bounds.Delta.Y) {
                            paperSize = PaperSizes.GetPaperSize(PaperKind.A4Rotated);
                        } else {
                            paperSize = PaperSizes.GetPaperSize(PaperKind.A4);
                        }
                        margin = defaultMargin;
                    }
                }
            }

            if (!emptyLayout) {
                // Lengths in inches.
                float pageWidthInInches = paperSize.Width / 100f;
                float pageHeightInInches = paperSize.Height / 100f;

                double scaleFactor;
                Matrix4D to2DTransform;
                if (useModelView) {
                    to2DTransform = modelView.GetMappingTransform(
                        new Rectangle2D(
                            margin * PdfExporter.InchToPixel,
                            margin * PdfExporter.InchToPixel,
                            (pageWidthInInches - margin) * PdfExporter.InchToPixel,
                            (pageHeightInInches - margin) * PdfExporter.InchToPixel),
                        false);
                    scaleFactor = double.NaN; // Not needed for model space.
                } else {
                    to2DTransform = DxfUtil.GetScaleTransform(
                        bounds.Corner1,
                        bounds.Corner2,
                        new Point3D(bounds.Center.X, bounds.Corner2.Y, 0d),
                        new Point3D(new Vector3D(margin, margin, 0d) * PdfExporter.InchToPixel),
                        new Point3D(new Vector3D(pageWidthInInches - margin, pageHeightInInches - margin, 0d) * PdfExporter.InchToPixel),
                        new Point3D(new Vector3D(pageWidthInInches / 2d, pageHeightInInches - margin, 0d) * PdfExporter.InchToPixel),
                        out scaleFactor
                        );
                }
                if (layout == null || !layout.PaperSpace) {
                    pdfExporter.DrawPage(model, config, to2DTransform, paperSize);
                } else {
                    pdfExporter.DrawPage(model, config, to2DTransform, scaleFactor, layout, null, paperSize);
                }
            }
        }

        private static void CreatePdfExporter(FileStream stream, out PdfExporter pdfExporter, out GraphicsConfig config) {
            pdfExporter = new PdfExporter(stream);
            pdfExporter.EmbedFonts = true;

            config = (GraphicsConfig)GraphicsConfig.WhiteBackgroundCorrectForBackColor.Clone();
            config.DisplayLineTypeElementShapes = true;
            config.TryDrawingTextAsText = true;
        }
    }
}
