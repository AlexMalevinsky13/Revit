using Autodesk.Revit.UI;

namespace Revit.Lesson3.Menu
{
    public class AppClassic : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;

        public Result OnStartup(UIControlledApplication app)
        {
            const string tabName = "Моё семейство";
            const string panelName = "Генерация Классика";
            string path = typeof(AppClassic).Assembly.Location;

            try 
            { 
                app.CreateRibbonTab(tabName); 
            } 
            catch 
            { 
            }
            
            RibbonPanel panel = app.CreateRibbonPanel(tabName, panelName);

            PushButtonData exportBtn = new PushButtonData(
                "ExportBtn", "Экспорт",
                path, "Revit.Lesson3.Menu.ExportCommand")
            {
                ToolTip = "Экспорт семейства в JSON"
            };

            PushButtonData importBtn = new PushButtonData(
                "ImportBtn", "Импорт",
                path, "Revit.Lesson3.Menu.ImportCommand")
            {
                ToolTip = "Импорт семейства из JSON"
            };

            panel.AddItem(exportBtn);
            panel.AddItem(importBtn);

            return Result.Succeeded;
        }
    }
}
