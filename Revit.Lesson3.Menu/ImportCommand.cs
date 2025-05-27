using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit.Lesson3.Menu
{
    [Transaction(TransactionMode.Manual)]
    public class ImportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            FamilyParameterHelper.AddWidthParameter(doc);
            TaskDialog.Show("Импорт", "Параметр 'w' добавлен");
            return Result.Succeeded;
        }
    }
}
