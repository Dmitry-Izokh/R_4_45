using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
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

            //Хотелось бы вынести отсюда значение длины и ширины сооружения в пользовательский интерфейс
            double widthWall = 10000;
            double depthWall = 10000;
            double hightRoof = 3000;
            List <XYZ> points = GetPoits(doc, widthWall, depthWall);
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
                AddDoor(doc, level1, walls[0]);
                
                AddWindow(doc, level1, walls[1]);
                AddWindow(doc, level1, walls[2]);
                AddWindow(doc, level1, walls[3]);
               
                AddRoof(doc, level2, widthWall, depthWall, hightRoof);
                ts.Commit();
            }
            return Result.Succeeded;
        }

        // Метод для определения координат отрезков по которым будут строиться стены.
        private List<XYZ> GetPoits (Document doc, double widthWall, double depthWall)
        {            
            double width = UnitUtils.ConvertToInternalUnits(widthWall, DisplayUnitType.DUT_MILLIMETERS);
            double depth = UnitUtils.ConvertToInternalUnits(depthWall, DisplayUnitType.DUT_MILLIMETERS);
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

        // Метод создания двери
        private void AddDoor(Document doc, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 х 2134 мм"))
                .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if(!doorType.IsActive)
            {
                doorType.Activate();
            }
            doc.Create.NewFamilyInstance(point, doorType, wall, level1, StructuralType.NonStructural);
        }
        // Метод создания окна
        private void AddWindow(Document doc, Level level1, Wall wall)
        {
            FamilySymbol windowType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 x 1830 мм"))
                .Where(x => x.FamilyName.Equals("M_Неподвижный"))
                .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!windowType.IsActive)
            {
                windowType.Activate();
            }
            doc.Create.NewFamilyInstance(point, windowType, wall, level1, StructuralType.NonStructural);
        }
        // Метод создания кровли
        private Result AddRoof(Document doc, Level level2, double widthWall, double depthWall, double hightRoof)
        {
            // Получить тип крыши по умолчанию
            ElementId id = doc.GetDefaultElementTypeId(ElementTypeGroup.RoofType);
            RoofType roofType = doc.GetElement(id) as RoofType;
            if (roofType == null)
            {
                TaskDialog.Show("Error", "Not RoofType");
                return Result.Failed;
            }
            // Cоздать схему
            double width = UnitUtils.ConvertToInternalUnits(widthWall, DisplayUnitType.DUT_MILLIMETERS);
            double depth = UnitUtils.ConvertToInternalUnits(depthWall, DisplayUnitType.DUT_MILLIMETERS);
            double hight = UnitUtils.ConvertToInternalUnits(hightRoof, DisplayUnitType.DUT_MILLIMETERS);
            double dx = width / 2;
            double dxMidle = width / 2;
            double dy = width / 2;
            double dz = hight;
            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx,      -dy, 0));
            points.Add(new XYZ( dxMidle, -dy, dz));
            points.Add(new XYZ( dx,      -dy, 0));
            
            CurveArray curveArray = new CurveArray();
            curveArray.Append(Line.CreateBound(points[0], points[1]));
            curveArray.Append(Line.CreateBound(points[1], points[2]));
            // Получить высоту текущего вида
            Level roofHost = level2;
            if (roofHost == null)
            {
                TaskDialog.Show("Error", "No es PlainView");
                return Result.Failed;
            }
            ReferencePlane plane = doc.Create.NewReferencePlane(points[0], points[1], points[2], doc.ActiveView);
            doc.Create.NewExtrusionRoof(curveArray, plane, level2, roofType, -dy, dy);
            return Result.Succeeded;
        }
    }
}
            
            

