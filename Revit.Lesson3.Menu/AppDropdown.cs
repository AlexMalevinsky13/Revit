using Autodesk.Revit.UI;
using System.Configuration.Assemblies;

namespace Revit.Lesson3.Menu
{
    public class AppDropdown : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;

        public Result OnStartup(UIControlledApplication app)
        {
            const string tabName = "Моё семейство";
            const string panelName = "Генерация Выпадашка";
            string path = typeof(AppDropdown).Assembly.Location;

            try 
            { 
                app.CreateRibbonTab(tabName); 
            } 
            catch 
            { 
            }

            RibbonPanel panel = app.CreateRibbonPanel(tabName, panelName);

            PulldownButtonData dropdownData = new PulldownButtonData("FamilyDropdown", "Генератор")
            {
                ToolTip = "Операции с семейством"
            };

            PulldownButton dropdown = panel.AddItem(dropdownData) as PulldownButton;
            new PushButtonData(
                "ExportBtn",
                "Экспорт",
                path,
                "Revit.Lesson3.Menu.ExportCommand"
            );

            new PushButtonData(
                "ImportBtn",
                "Импорт",
                path,
                "Revit.Lesson3.Menu.ImportCommand"
            );

            return Result.Succeeded;
        }
    }
}
