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
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<Level> listLevel = LevelUtils.GetLevels(commandData);

            // Этот блок кода нужно будет вынести в функционал выбора через пользовательский интерфейс
            Level level1 = listLevel
                .Where(x => x.Name.Equals("Уровень 1"))
                .FirstOrDefault();
            Level level2 = listLevel
                .Where(x => x.Name.Equals("Уровень 2"))
                .FirstOrDefault();          

            List<XYZ> points = GetPoits(doc);
            List<Wall> walls = new List<Wall>();
            
            Transaction ts = new Transaction(doc, "Построение стен");
            {
                ts.Start();
                for (int i=0; i<4; i++)
                {
                    Line line = Line.CreateBound(points[i], points[i + 1]);
                    Wall wall = Wall.Create(doc, line, level1.Id, false);
                    walls.Add(wall);
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
                }
                ts.Commit();
            }
            return Result.Succeeded;
        }
        
        // Метод для определения координат отрезков по которым будут строиться стены.
        public List<XYZ> GetPoits (Document doc)
        {
            //Хотелось бы вынести отсюда значение длины и ширины сооружения в пользовательский интерфейс
            double width = UnitUtils.ConvertToInternalUnits(10000, DisplayUnitType.DUT_MILLIMETERS);
            double depth = UnitUtils.ConvertToInternalUnits(10000, DisplayUnitType.DUT_MILLIMETERS);
            double dx = width / 2;
            double dy = width / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));
            
            return points;
        }
    }
}


//double width = UnitUtils.ConvertToInternalUnits(10000, DisplayUnitType.DUT_MILLIMETERS);
//double depth = UnitUtils.ConvertToInternalUnits(10000, DisplayUnitType.DUT_MILLIMETERS);
//double dx = width / 2;
//double dy = width / 2;

//List<XYZ> points = new List<XYZ>();
//points.Add(new XYZ(-dx, -dy, 0));
//points.Add(new XYZ( dx, -dy, 0));
//points.Add(new XYZ( dx,  dy, 0));
//points.Add(new XYZ(-dx,  dy, 0));
//points.Add(new XYZ(-dx, -dy, 0));
