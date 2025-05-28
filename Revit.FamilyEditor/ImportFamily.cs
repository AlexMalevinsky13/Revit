using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Microsoft.Win32;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.FamilyEditor.Models;

namespace Revit.FamilyEditor
{
    [Transaction(TransactionMode.Manual)]
    public class ImportFamily : IExternalCommand
    {
        const string Error = "Ошибка";
        const string Succeeded = "Успех";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;

            var openJson = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Выберите JSON-файл с данными семейства"
            };
            if (openJson.ShowDialog() != true)
                return Result.Cancelled;

            FamilyData data = FamilyDataSerializer.Deserialize(openJson.FileName);

            var openTemplate = new OpenFileDialog
            {
                Filter = "RFT Files (*.rft)|*.rft",
                Title = "Выберите шаблон семейства"
            };
            if (openTemplate.ShowDialog() != true)
                return Result.Cancelled;

            Document doc = uiApp.Application.NewFamilyDocument(openTemplate.FileName);
            if (doc == null || !doc.IsFamilyDocument)
            {
                TaskDialog.Show(Error, "Не удалось создать семейство.");
                return Result.Failed;
            }

            using (Transaction t = new Transaction(doc, "Импорт семейства"))
            {
                try
                {
                    t.Start();

                    CreateParameters(doc, data);
                    var (extrusion, edgeReferences) = CreateExtrusion(doc, data);
                    BindExtrusionDepth(doc, data, extrusion);
                    CreateDimensionsAndEQ(doc, edgeReferences);

                    doc.Regenerate();
                    doc.AutoJoinElements();

                    t.Commit();
                }
                catch (Exception ex)
                {
                    if (t.GetStatus() == TransactionStatus.Started)
                        t.RollBack();

                    TaskDialog.Show(Error, $"Ошибка при создании семейства: {ex.Message}");
                    return Result.Failed;
                }
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "Сохранить семейство",
                Filter = "Revit Family (*.rfa)|*.rfa",
                DefaultExt = "rfa",
                FileName = "ИмпортированноеСемейство"
            };

            if (saveDialog.ShowDialog() != true)
            {
                TaskDialog.Show("Информация", "Семейство создано, но не сохранено. Вы можете сохранить его вручную.");
                return Result.Succeeded;
            }

            string savePath = saveDialog.FileName;
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(savePath);

            SaveAsOptions saveOptions = new SaveAsOptions { OverwriteExistingFile = true };
            doc.SaveAs(modelPath, saveOptions);
            doc.Close(false);

            uiApp.OpenAndActivateDocument(savePath);

            TaskDialog.Show(Succeeded, "Семейство успешно импортировано");
            return Result.Succeeded;
        }

        private static void CreateParameters(Document doc, FamilyData data)
        {
            FamilyManager fm = doc.FamilyManager;

            foreach (var p in data.Parameters)
            {
                if (fm.get_Parameter(p.Name) != null)
                    continue;

                var param = fm.AddParameter(
                    p.Name,
                    GroupTypeId.Constraints,
                    SpecTypeId.Length,
                    false
                );

                if (fm.CurrentType != null && param != null)
                {
                    double internalVal = UnitUtils.ConvertToInternalUnits(p.Value, UnitTypeId.Millimeters);
                    fm.Set(param, internalVal);
                }
            }
        }

        private static (Extrusion extrusion, List<Reference> edgeReferences) CreateExtrusion(Document doc, FamilyData data)
        {
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
            SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

            var points = data.Extrusion.ProfilePoints;
            var profile = new CurveArray();
            var references = new List<Reference>();

            for (int i = 0; i < points.Count; i++)
            {
                int next = (i + 1) % points.Count;

                XYZ start = new XYZ(points[i].X / 304.8, points[i].Y / 304.8, 0);
                XYZ end = new XYZ(points[next].X / 304.8, points[next].Y / 304.8, 0);

                Line line = Line.CreateBound(start, end);
                ModelCurve curve = doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                profile.Append(line);
                references.Add(curve.GeometryCurve.Reference);
            }

            CurveArrArray curveArrArray = new CurveArrArray();
            curveArrArray.Append(profile);

            double depth = data.Parameters
                .FirstOrDefault(p => p.Name == data.Extrusion.DepthParameter)?.Value / 304.8 ?? 0.5;

            Extrusion extrusion = doc.FamilyCreate.NewExtrusion(true, curveArrArray, sketchPlane, depth);
            return (extrusion, references);
        }

        private static void BindExtrusionDepth(Document doc, FamilyData data, Extrusion extrusion)
        {
            FamilyManager fm = doc.FamilyManager;
            FamilyParameter param = fm.get_Parameter(data.Extrusion.DepthParameter);
            if (param != null)
            {
                Parameter extDepthParam = extrusion.get_Parameter(BuiltInParameter.EXTRUSION_END_PARAM);
                if (extDepthParam != null)
                {
                    fm.SetFormula(param, param.Definition.Name);
                }
            }
        }

        private static void CreateDimensionsAndEQ(Document doc, List<Reference> references)
        {
            if (references == null || references.Count < 4)
                return;

            View view = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .FirstOrDefault(v => v.ViewType == ViewType.FloorPlan && !v.IsTemplate);

            if (view == null)
                throw new InvalidOperationException("Не найден план этажа для размеров.");

            FamilyManager fm = doc.FamilyManager;
            FamilyParameter wParam = fm.get_Parameter("w");
            if (wParam == null)
            {
                wParam = fm.AddParameter("w", GroupTypeId.Geometry, SpecTypeId.Length, false);
            }

            var lines = references
                .Select(r => new
                {
                    Reference = r,
                    Line = doc.GetElement(r.ElementId).GetGeometryObjectFromReference(r) as Line
                })
                .Where(x => x.Line != null)
                .ToList();

            var horizontal = lines
                .Where(x =>
                    x.Line.GetEndPoint(0).Y == x.Line.GetEndPoint(1).Y &&
                    x.Line.GetEndPoint(0).X != x.Line.GetEndPoint(1).X)
                .OrderBy(x => Math.Min(x.Line.GetEndPoint(0).X, x.Line.GetEndPoint(1).X))
                .ToList();

            var vertical = lines
                .Where(x =>
                    x.Line.GetEndPoint(0).X == x.Line.GetEndPoint(1).X &&
                    x.Line.GetEndPoint(0).Y != x.Line.GetEndPoint(1).Y)
                .OrderBy(x => Math.Min(x.Line.GetEndPoint(0).Y, x.Line.GetEndPoint(1).Y))
                .ToList();

            double offset = UnitUtils.ConvertToInternalUnits(500, UnitTypeId.Millimeters); // больше отступ

            if (horizontal.Count >= 2)
            {
                Reference left = horizontal.First().Reference;
                Reference right = horizontal.Last().Reference;

                ReferenceArray array = new ReferenceArray();
                array.Append(left);
                array.Append(right);

                XYZ p1 = horizontal.First().Line.GetEndPoint(0);
                XYZ p2 = horizontal.Last().Line.GetEndPoint(0);
                XYZ mid = (p1 + p2) / 2;

                XYZ dir = XYZ.BasisY;
                XYZ normal = XYZ.BasisX;
                XYZ shiftedMid = mid + normal * offset;

                Line dimLine = Line.CreateBound(shiftedMid - dir * 1000, shiftedMid + dir * 1000);

                Dimension dim = doc.FamilyCreate.NewDimension(view, dimLine, array);
                if (dim != null)
                {
                    dim.FamilyLabel = wParam;
                }
            }

            if (vertical.Count >= 2)
            {
                Reference bottom = vertical.First().Reference;
                Reference top = vertical.Last().Reference;

                ReferenceArray array = new ReferenceArray();
                array.Append(bottom);
                array.Append(top);

                XYZ p1 = vertical.First().Line.GetEndPoint(0);
                XYZ p2 = vertical.Last().Line.GetEndPoint(0);
                XYZ mid = (p1 + p2) / 2;

                XYZ dir = XYZ.BasisX;
                XYZ normal = XYZ.BasisY;
                XYZ shiftedMid = mid - normal * offset;

                Line dimLine = Line.CreateBound(shiftedMid - dir * 1000, shiftedMid + dir * 1000);

                Dimension dim = doc.FamilyCreate.NewDimension(view, dimLine, array);
                if (dim != null && dim.Segments != null && dim.Segments.Size > 1)
                {
                    dim.AreSegmentsEqual = true;
                }
            }
        }
    }
}
