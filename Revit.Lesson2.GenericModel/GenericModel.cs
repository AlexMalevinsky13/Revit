using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace Revit.Lesson2.GenericModel
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GenericModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Application app = uiApp.Application;

            string templatePath = @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\Russian\Метрическая система, типовая модель.rft";
            Document famDoc = app.NewFamilyDocument(templatePath);

            using (Transaction tx = new Transaction(famDoc, "Создание экструзии"))
            {
                tx.Start();

                double mm = 1.0 / 304.8;
                double width = 500 * mm;
                double height = 300 * mm;
                double depth = 200 * mm;

                CurveArrArray curveArrArray = new CurveArrArray();
                CurveArray rect = new CurveArray();

                rect.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(width, 0, 0)));
                rect.Append(Line.CreateBound(new XYZ(width, 0, 0), new XYZ(width, height, 0)));
                rect.Append(Line.CreateBound(new XYZ(width, height, 0), new XYZ(0, height, 0)));
                rect.Append(Line.CreateBound(new XYZ(0, height, 0), new XYZ(0, 0, 0)));

                curveArrArray.Append(rect);

                SketchPlane sketch = SketchPlane.Create(famDoc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero));
                famDoc.FamilyCreate.NewExtrusion(true, curveArrArray, sketch, depth);

                tx.Commit();
            }

            string savePath = @"C:\Temp\TestFamily.rfa";

            try
            {
                famDoc.SaveAs(savePath, new SaveAsOptions { OverwriteExistingFile = true });
                famDoc.Close(false);

                TaskDialog.Show("Успех", $"Семейство сохранено: {savePath}");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка при сохранении", ex.Message);
                return Result.Failed;
            }

            TaskDialog.Show("Готово", "Экструзия успешно создана.");
            return Result.Succeeded;
        }

        private static UIDocument UIdocFromApp(UIApplication uiApp)
        {
            return new UIDocument(uiApp.Application.Documents
                .Cast<Document>()
                .FirstOrDefault(d => d.IsFamilyDocument && d.Title.Contains("Метрическая система")));
        }
    }
}
