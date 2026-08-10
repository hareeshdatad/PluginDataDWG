using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using static System.Net.Mime.MediaTypeNames;

namespace AutocadDLLPLugin
{
    public class Class1
    {
        [CommandMethod("TESTDWG")]
        public void TestDwg()
        {
            var doc =
                Application.DocumentManager.MdiActiveDocument;

            doc.Editor.WriteMessage(
                "\nAutoCAD .NET plugin is working!");
        }
    }
}