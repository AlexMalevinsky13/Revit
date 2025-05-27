using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Microsoft.Win32;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.FamilyEditor.Models;
using System.IO;

namespace Revit.FamilyEditor
{
    [Transaction(TransactionMode.Manual)]
    internal class ImportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Выберите JSON-файл семейства",
                Filter = "JSON files (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dialog.ShowDialog() != true)
            {
                return Result.Cancelled;
            }

            string filePath = dialog.FileName;
            if (!File.Exists(filePath))
            {
                TaskDialog.Show("FamilyEditor", "Файл не найден: " + filePath);
                return Result.Failed;
            }

            FamilyData data = FamilyDataSerializer.Deserialize(filePath);
            if (data == null || data.Extrusion == null || data.Extrusion.ProfilePoints == null)
            {
                TaskDialog.Show("FamilyEditor", "Данные JSON некорректны или отсутствуют.");
                return Result.Failed;
            }

            Document doc = uiApp.Application.NewFamilyDocument(@"C:\ProgramData\Autodesk\RVT 2024\Family Templates\Russian\Метрическая система.rft");
            if (doc == null)
            {
                TaskDialog.Show("FamilyEditor", "Не удалось создать семейство.");
                return Result.Failed;
            }

            using (Transaction t = new Transaction(doc, "Импорт семейства"))
            {
                t.Start();

                SketchPlane plane = SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero));

                CurveArray curveArray = new CurveArray();
                List<Point2D> pts = data.Extrusion.ProfilePoints;
                for (int i = 0; i < pts.Count - 1; i++)
                {
                    XYZ start = new XYZ(pts[i].X / 304.8, pts[i].Y / 304.8, 0);
                    XYZ end = new XYZ(pts[i + 1].X / 304.8, pts[i + 1].Y / 304.8, 0);
                    Line line = Line.CreateBound(start, end);
                    curveArray.Append(line);
                }

                Extrusion extrusion = doc.FamilyCreate.NewExtrusion(true, curveArray, plane, 500.0 / 304.8);

                t.Commit();
            }

            uiApp.ActiveUIDocument.RefreshActiveView();
            TaskDialog.Show("FamilyEditor", "Импорт завершён.");

            return Result.Succeeded;
        }
    }
}
