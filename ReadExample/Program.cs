using System;

using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Math;

namespace ReadExample {        
    class Program {
        static void Main(string[] args) {
            // For this application to work you will need a trial license.
            // The MyAppKeyPair.snk linked in the project is not present in the repository, 
            // you should generate your own strong name key and keep it private.
            //
            // 1) You can generate a strong name key with the following command in the Visual Studio command prompt:
            //     sn -k MyKeyPair.snk
            //
            // 2) The next step is to extract the public key file from the strong name key (which is a key pair):
            //     sn -p MyKeyPair.snk MyPublicKey.snk
            //
            // 3) Display the public key token for the public key: 	
            //     sn -t MyPublicKey.snk
            //
            // 4) Go to the project properties Singing tab, and check the "Sign the assembly" checkbox, 
            //    and choose the strong name key you created.
            //
            // 5) Register and get your trial license from https://www.woutware.com/SoftwareLicenses.
            //    Enter your strong name key public key token that you got at step 3.
            WW.WWLicense.SetLicense("<license string>");

            CreateAndWriteCadDrawing();
            ReadCadDrawing();

            Console.WriteLine("Press enter.");
            Console.ReadLine();
        }

        // Create a very simple AutoCAD drawing.
        private static void CreateAndWriteCadDrawing() {
            DxfModel model = new DxfModel();

            model.Entities.Add(new DxfLine(new Point2D(1, 0), new Point2D(3, 2)));
            model.Entities.Add(
                new DxfDimension.Aligned(model.CurrentDimensionStyle) {
                    DimensionLineLocation = new Point3D(2, 4, 0),
                    ExtensionLine1StartPoint = new Point3D(1, 0, 0),
                    ExtensionLine2StartPoint = new Point3D(3, 2, 0)
                }
            );

            DwgWriter.Write(@"test.dwg", model);
        }

        // Read the AutoCAD drawing that was just created.
        private static void ReadCadDrawing() {
            DxfModel model = CadReader.Read("test.dwg");
            Console.WriteLine(model.Entities.Count);
        }
    }
}
