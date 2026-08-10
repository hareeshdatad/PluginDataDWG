using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using System.Text;
using System.Transactions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutocadDLLPLugin.Services;

public class DwgReader
{
    public void ReadCurrentDrawing()
    {
        Document doc =
            Application.DocumentManager.MdiActiveDocument;

        Database db = doc.Database;

        Editor editor = doc.Editor;

        using Transaction tr =
            db.TransactionManager.StartTransaction();

        var entityCounts =
            new Dictionary<string, int>();

        BlockTable blockTable =
            (BlockTable)tr.GetObject(
                db.BlockTableId,
                OpenMode.ForRead);

        BlockTableRecord modelSpace =
            (BlockTableRecord)tr.GetObject(
                blockTable[BlockTableRecord.ModelSpace],
                OpenMode.ForRead);

        // ============================================================
        // FIRST PASS - COUNT ENTITIES
        // ============================================================

        foreach (ObjectId objectId in modelSpace)
        {
            if (!objectId.ObjectClass.IsDerivedFrom(
                    RXClass.GetClass(typeof(Entity))))
            {
                continue;
            }

            Entity entity =
                tr.GetObject(
                    objectId,
                    OpenMode.ForRead) as Entity;

            if (entity == null)
                continue;

            string entityType =
                entity.GetType().Name;

            if (!entityCounts.ContainsKey(entityType))
            {
                entityCounts[entityType] = 0;
            }

            entityCounts[entityType]++;
        }

        // ============================================================
        // PRINT TOTAL ENTITY COUNTS
        // ============================================================

        editor.WriteMessage(
            "\n\nTotal Entities for current DWG\n");

        editor.WriteMessage(
            "{" + string.Join(
                ", ",
                entityCounts.Select(
                    x => $"'{x.Key}': {x.Value}"))
            + "}");

        // ============================================================
        // SECOND PASS - DETAILED ENTITY DATA
        // ============================================================

        foreach (ObjectId objectId in modelSpace)
        {
            if (!objectId.ObjectClass.IsDerivedFrom(
                    RXClass.GetClass(typeof(Entity))))
            {
                continue;
            }

            Entity entity =
                tr.GetObject(
                    objectId,
                    OpenMode.ForRead) as Entity;

            if (entity == null)
                continue;

            PrintEntity(entity, tr, editor);
        }

        tr.Commit();

        editor.WriteMessage(
            "\n\nExtraction completed.\n");
    }

    private void PrintEntity(
        Entity entity,
        Transaction tr,
        Editor editor)
    {
        editor.WriteMessage(
            "\n\n" +
            new string('=', 100));

        editor.WriteMessage(
            $"\nENTITY TYPE : {GetEntityType(entity)}");

        editor.WriteMessage(
            "\n" + new string('=', 100));

        // ------------------------------------------------------------
        // HANDLE
        // ------------------------------------------------------------

        editor.WriteMessage(
            $"\nHandle : {entity.Handle}");

        editor.WriteMessage(
            "\n\nDXF DATA");

        // ------------------------------------------------------------
        // COMMON PROPERTIES
        // ------------------------------------------------------------

        editor.WriteMessage(
            $"\nowner_handle              : {GetOwnerHandle(entity)}");

        editor.WriteMessage(
            $"\nlayer_handle              : {GetLayerHandle(entity)}");

        editor.WriteMessage(
            $"\nlayer                     : {entity.Layer}");

        editor.WriteMessage(
            $"\ncolor_index               : {GetColorIndex(entity)}");

        editor.WriteMessage(
            $"\ntrue_color                : {GetTrueColor(entity)}");

        editor.WriteMessage(
            $"\nlinetype                  : {GetLinetype(entity, tr)}");

        // ------------------------------------------------------------
        // ENTITY-SPECIFIC DATA
        // ------------------------------------------------------------

        switch (entity)
        {
            case Line line:

                editor.WriteMessage(
                    $"\nstart                     : {line.StartPoint}");

                editor.WriteMessage(
                    $"\nend                       : {line.EndPoint}");

                break;

            case Arc arc:

                editor.WriteMessage(
                    $"\ncenter                    : {arc.Center}");

                editor.WriteMessage(
                    $"\nradius                    : {arc.Radius}");

                editor.WriteMessage(
                    $"\nstart_angle               : {arc.StartAngle}");

                editor.WriteMessage(
                    $"\nend_angle                 : {arc.EndAngle}");

                break;

            case Circle circle:

                editor.WriteMessage(
                    $"\ncenter                    : {circle.Center}");

                editor.WriteMessage(
                    $"\nradius                    : {circle.Radius}");

                break;

            case DBText text:

                editor.WriteMessage(
                    $"\ntext                      : {text.TextString}");

                editor.WriteMessage(
                    $"\nposition                  : {text.Position}");

                editor.WriteMessage(
                    $"\nheight                    : {text.Height}");

                editor.WriteMessage(
                    $"\nrotation                  : {text.Rotation}");

                break;

            case MText mtext:

                editor.WriteMessage(
                    $"\ntext                      : {mtext.Text}");

                editor.WriteMessage(
                    $"\nlocation                  : {mtext.Location}");

                editor.WriteMessage(
                    $"\ntext_height               : {mtext.TextHeight}");

                break;

            case Polyline polyline:

                editor.WriteMessage(
                    $"\nvertex_count              : {polyline.NumberOfVertices}");

                editor.WriteMessage(
                    $"\nlength                    : {polyline.Length}");

                break;

            case BlockReference blockReference:

                editor.WriteMessage(
                    $"\nblock_name                : {blockReference.Name}");

                editor.WriteMessage(
                    $"\nposition                  : {blockReference.Position}");

                editor.WriteMessage(
                    $"\nrotation                  : {blockReference.Rotation}");

                editor.WriteMessage(
                    $"\nscale_x                   : {blockReference.ScaleFactors.X}");

                editor.WriteMessage(
                    $"\nscale_y                   : {blockReference.ScaleFactors.Y}");

                editor.WriteMessage(
                    $"\nscale_z                   : {blockReference.ScaleFactors.Z}");

                break;

            case DBPoint point:

                editor.WriteMessage(
                    $"\nposition                  : {point.Position}");

                break;

            case Ellipse ellipse:

                editor.WriteMessage(
                    $"\ncenter                    : {ellipse.Center}");

                editor.WriteMessage(
                    $"\nmajor_radius              : {ellipse.MajorRadius}");

                editor.WriteMessage(
                    $"\nminor_radius              : {ellipse.MinorRadius}");

                break;

            case Solid3d solid3d:

                editor.WriteMessage(
                    $"\nsolid_type                : 3DSOLID");

                break;

            case Dimension dimension:

                editor.WriteMessage(
                    $"\ndimension_text            : {dimension.DimensionText}");

                editor.WriteMessage(
                    $"\nmeasurement               : {dimension.Measurement}");

                break;
        }
    }

    private string GetEntityType(Entity entity)
    {
        return entity.GetType().Name;
    }

    private string GetOwnerHandle(Entity entity)
    {
        if (entity.OwnerId.IsNull)
            return "None";

        return entity.OwnerId.Handle.ToString();
    }

    private string GetLayerHandle(Entity entity)
    {
        if (entity.LayerId.IsNull)
            return "None";

        return entity.LayerId.Handle.ToString();
    }

    private string GetColorIndex(Entity entity)
    {
        try
        {
            return entity.ColorIndex.ToString();
        }
        catch
        {
            return "None";
        }
    }

    private string GetTrueColor(Entity entity)
    {
        try
        {
            if (entity.Color.IsByColor)
            {
                return entity.Color.ColorValue.ToString();
            }

            return "None";
        }
        catch
        {
            return "None";
        }
    }

    private string GetLinetype(
        Entity entity,
        Transaction tr)
    {
        if (entity.LinetypeId.IsNull)
            return "None";

        LinetypeTableRecord linetype =
            tr.GetObject(
                entity.LinetypeId,
                OpenMode.ForRead) as LinetypeTableRecord;

        return linetype?.Name ?? "None";
    }
}