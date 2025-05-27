using System.Linq;
using Autodesk.Revit.DB;

namespace Revit.Lesson3.Menu
{
    internal static class FamilyParameterHelper
    {
        public static void AddWidthParameter(Document doc)
        {
            if (!(doc.IsFamilyDocument)) return;

            using (Transaction t = new Transaction(doc, "Add W Parameter"))
            {
                t.Start();

                FamilyManager manager = doc.FamilyManager;

                if (!manager.Parameters.Cast<FamilyParameter>().Any(p => p.Definition.Name == "w"))
                {
                    ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions("w", SpecTypeId.Length)
                    {
                        Visible = true
                    };

                    DefinitionFile defFile = doc.Application.OpenSharedParameterFile();
                    DefinitionGroup group = defFile?.Groups.FirstOrDefault() ?? defFile?.Groups.Create("Default");

                    if (group?.Definitions.Create(opt) is ExternalDefinition wDef)
                    {
                        manager.AddParameter(wDef, GroupTypeId.Constraints, false);
                    }
                }

                t.Commit();
            }
        }
    }
}
