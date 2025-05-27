using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.FamilyEditor.Models;
using System.IO;

namespace Revit.FamilyEditor
{
    [Transaction(TransactionMode.Manual)]
    public class ExportCommand : IExternalCommand
    {
        const string Error = "Ошибка";
        const string Succeeded = "Успех";
        const string DefaultExt = "json";
        const string DefaultFileName = "family_export.json";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            if (!doc.IsFamilyDocument)
            {
                TaskDialog.Show(Error, "Открытый документ не является семейством.");
                return Result.Failed;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Сохранить семейство как JSON",
                Filter = "JSON Files (*.json)|*.json",
                DefaultExt = DefaultExt,
                FileName = DefaultFileName
            };

            if (dialog.ShowDialog() != true)
                return Result.Cancelled;

            string path = dialog.FileName;

            FamilyManager manager = doc.FamilyManager;
            List<ParameterData> parameters = manager.Parameters
                .Cast<FamilyParameter>()
                .Select(p => new ParameterData
                {
                    Name = p.Definition.Name,
                    Value = manager.CurrentType != null && manager.CurrentType.HasValue(p)
                        ? (double)(manager.CurrentType.AsDouble(p) * 304.8) : 0,
                    Type = "Length"
                }).ToList();

            Extrusion extrusion = new FilteredElementCollector(doc)
                .OfClass(typeof(Extrusion))
                .Cast<Extrusion>()
                .FirstOrDefault();

            if (extrusion == null)
            {
                TaskDialog.Show("FamilyEditor", "Экструзия не найдена.");
                return Result.Failed;
            }

            ExtrusionData extrusionData = new ExtrusionData
            {
                ProfilePoints = new List<Point2D>()
            };
            Options options = new Options { DetailLevel = ViewDetailLevel.Fine };
            GeometryElement geomElement = extrusion.get_Geometry(options);

            CurveArray profile = extrusion.Sketch.Profile.get_Item(0);

            foreach (Curve c in profile)
            {
                try
                {
                    if (c == null)
                        continue;

                    XYZ pt = c.GetEndPoint(0);
                    extrusionData.ProfilePoints.Add(new Point2D
                    {
                        X = pt.X * 304.8,
                        Y = pt.Y * 304.8
                    });
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Ошибка", $"Кривая: {c?.GetType().Name}: {ex.Message}");
                    return Result.Failed;
                }
            }

            Parameter depthParam = extrusion.get_Parameter(BuiltInParameter.EXTRUSION_END_PARAM);
            if (depthParam != null)
            {
                extrusionData.DepthParameter = depthParam.Definition.Name;
            }

            var data = new FamilyData
            {
                Parameters = parameters,
                Extrusion = extrusionData
            };

            try
            {
                FamilyDataSerializer.Serialize(data, path);
                TaskDialog.Show(Succeeded, $"Семейство экспортировано в {path}");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show(Error, $"Не удалось сохранить файл: {ex.Message}");
                return Result.Failed;
            }
        }
    }
}
