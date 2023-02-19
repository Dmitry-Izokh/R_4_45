using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R_4_45
{
    public class LevelUtils
    {
        public static List<Level> GetLevels(ExternalCommandData commandData)
        {
        Document doc = commandData.Application.ActiveUIDocument.Document;
        List<Level> listLevel = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .OfType<Level>()
            .ToList();

            return listLevel;
        }
    }
}
