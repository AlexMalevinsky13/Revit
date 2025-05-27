using Autodesk.Revit.UI;

namespace Revit.FamilyEditor
{
    public class App : IExternalApplication
    {
        const string TabName = "Генератор семейств";
        const string PanelName = "Инструменты";

        public Result OnStartup(UIControlledApplication app)
        {
            try 
            { 
                app.CreateRibbonTab(TabName); 
            } 
            catch 
            {
                TaskDialog.Show("Revit API", $"Не удалось создать вкладку {TabName}");
                return Result.Failed;
            }

            CreateRibbonPanel(app);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;

        private static void CreateRibbonPanel(UIControlledApplication app)
        {
            string assemblyPath = typeof(App).Assembly.Location;

            var panel = app.CreateRibbonPanel(TabName, PanelName);

            AddButtons(panel, assemblyPath);
        }

        private static void AddButtons(RibbonPanel panel, string assemblyPath)
        {
            var exportBtn = new PushButtonData(
                "ExportBtn",
                "Экспорт",
                assemblyPath,
                "Revit.FamilyEditor.ExportCommand"
            );

            var importBtn = new PushButtonData(
                "ImportBtn",
                "Импорт",
                assemblyPath,
                "Revit.FamilyEditor.ImportCommand"
            );

            panel.AddItem(exportBtn);
            panel.AddItem(importBtn);
        }
    }
}
