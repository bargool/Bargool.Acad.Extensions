// © Andrey Bushman, 2013
// From http://adn-cis.org/forum/index.php?topic=411.0
//Microsoft
using System;

//Autodesk
using cad = Autodesk.AutoCAD.ApplicationServices.Application;
using App = Autodesk.AutoCAD.ApplicationServices;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;

namespace Bargool.Acad.Extensions
{

    public static class LayoutManagerExtensionMethods
    {
        /// <summary>
        /// This is Extension Method for the <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c>
        /// class. It gets the current space in the current Database.
        /// </summary>
        /// <param name="mng">Target <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c>
        /// instance.</param>
        /// <returns>Returns the SpaceEnum value.</returns>            
        public static SpaceEnum GetCurrentSpaceEnum(this Db.LayoutManager mng)
        {
            Db.Database db = cad.DocumentManager.MdiActiveDocument.Database;
            Int16 tilemode = (Int16)cad.GetSystemVariable("TILEMODE");

            if (tilemode == 1)
                return SpaceEnum.Model;

            Int16 cvport = (Int16)cad.GetSystemVariable("CVPORT");
            if (cvport == 1)
                return SpaceEnum.Layout;
            else
                return SpaceEnum.Viewport;
        }

        /// <summary>
        /// This is Extension Method for the <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c>
        /// class. It gets the name of the current space in the current Database.
        /// </summary>
        /// <param name="mng">Target <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c>
        /// instance.</param>
        /// <returns>Returns the name of current space.</returns>
        public static String GetCurrentSpaceName(this Db.LayoutManager mng)
        {
            SpaceEnum space = GetCurrentSpaceEnum(mng);
            Db.Database db = cad.DocumentManager.MdiActiveDocument.Database;
            String modelSpaceLocalizedName = String.Empty;
            using (Db.Transaction tr = db.TransactionManager.StartTransaction())
            {
                Db.BlockTable bt = tr.GetObject(db.BlockTableId, Db.OpenMode.ForRead) as Db.BlockTable;
                Db.BlockTableRecord btr = tr.GetObject(bt[Db.BlockTableRecord.ModelSpace], Db.OpenMode.ForRead)
                        as Db.BlockTableRecord;
                modelSpaceLocalizedName = (tr.GetObject(btr.LayoutId, Db.OpenMode.ForRead) as Db.Layout).LayoutName;
            }
            String result = space == SpaceEnum.Viewport ?
                    "Model" as String : mng.CurrentLayout;
            return result;
        }

        /// <summary>
        /// This is Extension Method for the <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c>
        /// class. It gets the localized name of the Model tab.
        /// </summary>
        /// <param name="mng">Target <c>Autodesk.AutoCAD.DatabaseServices.LayoutManager</c>
        /// instance.</param>
        /// <returns>Returns the name of current space.</returns>
        public static String GetModelTabLocalizedName(this Db.LayoutManager mng)
        {
            Db.Database db = cad.DocumentManager.MdiActiveDocument.Database;
            String modelTabLocalizedName = String.Empty;
            using (Db.Transaction tr = db.TransactionManager.StartTransaction())
            {
                Db.BlockTable bt = tr.GetObject(db.BlockTableId, Db.OpenMode.ForRead) as Db.BlockTable;
                Db.BlockTableRecord btr = tr.GetObject(bt[Db.BlockTableRecord.ModelSpace], Db.OpenMode.ForRead)
                        as Db.BlockTableRecord;
                modelTabLocalizedName = (tr.GetObject(btr.LayoutId, Db.OpenMode.ForRead) as Db.Layout).LayoutName;
            }
            return modelTabLocalizedName;
        }
    }


    /// <summary>
    /// This enum indicates the current space in the current Database.
    /// </summary>
    public enum SpaceEnum
    {
        /// <summary>
        /// The Model space.
        /// </summary>
        Model,
        /// <summary>
        /// The Layout space.
        /// </summary>
        Layout,
        /// <summary>
        /// The Model space through the Layout's viewport.
        /// </summary>
        Viewport
    }
}