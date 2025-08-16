using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.FamilyEditor.Models;

namespace Revit.FamilyEditor
{
    [Transaction(TransactionMode.Manual)]
    public class ExportFamily : IExternalCommand
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

            try
            {
                var familyData = ExtractFamilyData(doc);
                FamilyDataSerializer.Serialize(familyData, path);
                TaskDialog.Show(Succeeded, $"Семейство экспортировано в {path}");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show(Error, $"Ошибка при экспорте: {ex.Message}");
                return Result.Failed;
            }
        }

        private FamilyData ExtractFamilyData(Document doc)
        {
            var parameters = GetParameters(doc);
            var extrusion = GetExtrusionData(doc);
            var dimensions = GetDimensionData(doc);
            var alignments = GetAlignmentData(doc);

            return new FamilyData
            {
                Parameters = parameters,
                Extrusion = extrusion,
                Dimensions = dimensions,
                Alignments = alignments
            };
        }

        private static List<ParameterData> GetParameters(Document doc)
        {
            var manager = doc.FamilyManager;
            return manager.Parameters
                .Cast<FamilyParameter>()
                .Select(p => new ParameterData
                {
                    Name = p.Definition.Name,
                    Value = manager.CurrentType != null && manager.CurrentType.HasValue(p)
                        ? (double)(manager.CurrentType.AsDouble(p) * 304.8) : 0.0,
                    Type = GetParameterTypeString(p.Definition)
                }).ToList();
        }

        private static string GetParameterTypeString(Definition def)
        {
            try
            {
                ForgeTypeId specId = def.GetDataType();

                if (specId == SpecTypeId.Length) 
                    return "Length";
                
                if (specId == SpecTypeId.Angle) 
                    return "Angle";
                
                if (specId == SpecTypeId.String.Text) 
                    return "Text";

                if (specId == SpecTypeId.String.MultilineText) 
                    return "MultilineText";

                if (specId == SpecTypeId.String.Url) 
                    return "Url";

                if (specId == SpecTypeId.Boolean.YesNo) 
                    return "YesNo";

                if (specId == SpecTypeId.Int.Integer) 
                    return "Integer";

                if (specId == SpecTypeId.Reference.Material) 
                    return "Material";

                return specId.TypeId;
            }
            catch
            {
                return "Length";
            }
        }

        private static ExtrusionData GetExtrusionData(Document doc)
        {
            var extrusion = new FilteredElementCollector(doc)
                .OfClass(typeof(Extrusion))
                .Cast<Extrusion>()
                .FirstOrDefault();

            if (extrusion == null)
                throw new InvalidOperationException("Экструзия не найдена.");

            var profilePoints = new List<Point2D>();
            CurveArray profile = extrusion.Sketch.Profile.get_Item(0);

            foreach (Curve c in profile)
            {
                XYZ pt = c.GetEndPoint(0);
                profilePoints.Add(new Point2D
                {
                    X = pt.X * 304.8,
                    Y = pt.Y * 304.8
                });
            }

            var depthParam = extrusion.get_Parameter(BuiltInParameter.EXTRUSION_END_PARAM);

            return new ExtrusionData
            {
                ProfilePoints = profilePoints,
                DepthParameter = depthParam?.Definition.Name
            };
        }

        private static List<DimensionData> GetDimensionData(Document doc)
        {
            var result = new List<DimensionData>();

            var dims = new FilteredElementCollector(doc)
                .OfClass(typeof(Dimension))
                .Cast<Dimension>()
                .Where(d => d.NumberOfSegments == 1 && d.FamilyLabel != null);

            foreach (var dim in dims)
            {
                try
                {
                    var references = dim.References;
                    if (references == null || references.Size < 2)
                        continue;

                    var pt1 = GetReferencePoint(doc, references.get_Item(0));
                    var pt2 = GetReferencePoint(doc, references.get_Item(1));

                    result.Add(new DimensionData
                    {
                        Start = new Point2D { X = pt1.X * 304.8, Y = pt1.Y * 304.8 },
                        End = new Point2D { X = pt2.X * 304.8, Y = pt2.Y * 304.8 },
                        Label = dim.FamilyLabel.Definition.Name
                    });
                }
                catch
                {
                    continue;
                }
            }

            return result;
        }

        private static List<AlignmentData> GetAlignmentData(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Dimension))
                .Cast<Dimension>()
                .Where(d => d.AreSegmentsEqual)
                .Select(d => new AlignmentData
                {
                    Direction = IsHorizontal(d) ? "Horizontal" : "Vertical",
                    Equalize = true
                })
                .ToList();
        }

        private static XYZ GetReferencePoint(Document doc, Reference reference)
        {
            var el = doc.GetElement(reference.ElementId);

            if (el is FamilyInstance fi && fi.Location is LocationPoint lp)
                return lp.Point;

            if (reference.GlobalPoint != null)
                return reference.GlobalPoint;

            return XYZ.Zero;
        }

        private static bool IsHorizontal(Dimension dim)
        {
            var curve = dim.Curve;
            if (curve == null) 
                return true;
           
            var dir = curve.GetEndPoint(1) - curve.GetEndPoint(0);
            
            return Math.Abs(dir.X) > Math.Abs(dir.Y);
        }
    }
}
