using Autodesk.AutoCAD.Runtime;
using AutocadDLLPLugin.Services;

namespace AutocadDLLPLugin.Commands;

public class DwgCommands
{
    [CommandMethod("READDWG")]
    public void ReadDwg()
    {
        try
        {
            var reader = new DwgReader();

            reader.ReadCurrentDrawing();
        }
        catch (System.Exception ex)
        {
            Autodesk.AutoCAD.ApplicationServices
                .Application
                .DocumentManager
                .MdiActiveDocument
                .Editor
                .WriteMessage(
                    $"\nERROR: {ex.Message}");
        }
    }
}