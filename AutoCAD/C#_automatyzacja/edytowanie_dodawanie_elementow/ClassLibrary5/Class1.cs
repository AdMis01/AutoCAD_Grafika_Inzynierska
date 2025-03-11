using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Windows;
using VB = Microsoft.VisualBasic;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Globalization;
using System.Xml.Linq;

namespace adam
{
    public class Class1
    {
        [CommandMethod("Mojtekst")]
        public void Mojtekst()
        {
            int liczba = Application.DocumentManager.Count;
            Application.ShowAlertDialog("Liczba otwartych dokumentow: " + liczba.ToString());
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Pierwszy komunikat");
            //Document dok = Application.DocumentManager.Open("D:\\Rysunek.dwg");

        }
        [CommandMethod("pobieraniepunktu")]
        public void Pobieraniepunktu()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions kom = new PromptPointOptions("wskaz punkt");
            PromptPointResult rez = ed.GetPoint(kom);

            Point3d punkt = new Point3d();
            if (rez.Status == PromptStatus.OK)
            {
                punkt = rez.Value;
                ed.WriteMessage("Wspolrzedna X " + punkt.X.ToString() + "\tWspolrzedna Y " + punkt.Y.ToString() + "\tWspolrzedna Z " + punkt.Z.ToString());

            }
            PromptPointOptions kom2 = new PromptPointOptions(" wskaz drugi punkt ");
            kom2.UseBasePoint = true;
            kom2.BasePoint = punkt;
            PromptPointResult rez2 = ed.GetPoint(kom2);
            Point3d punkt2 = rez2.Value;
            ed.WriteMessage(" Wspolrzedna X drugiego punktu " + punkt2.X.ToString());
            Application.ShowAlertDialog("Odleglosc " + punkt2.DistanceTo(punkt).ToString());


        }
        [CommandMethod("atrybut_tw")]

        public void atrybut_tw()
        {
            Document dok = Application.DocumentManager.MdiActiveDocument;
            Editor ed = dok.Editor;
            Database baza = dok.Database;

            PromptStringOptions kom = new PromptStringOptions("podaj nazwe");
            kom.AllowSpaces = true;
            kom.DefaultValue = "blokmw";

            PromptResult wynik = ed.GetString(kom);

            if (wynik.Status == PromptStatus.OK)
            {
                using (Transaction tran = baza.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tran.GetObject(baza.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    if (bt.Has(wynik.StringResult))
                    {

                        //var ggg = bt[wynik.StringResult];
                        BlockTableRecord bt2 = new BlockTableRecord();
                        foreach (ObjectId id in bt)
                        {
                            BlockTableRecord btr3 = id.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                            if (btr3.Name == wynik.StringResult)
                            {
                                bt2 = btr3;
                            }
                        }
                        bt2.UpgradeOpen();

                        string atr = "nazwa atrybutów\n";
                        foreach (ObjectId ob8 in bt2)
                        {
                            Entity ob10 = ob8.GetObject(OpenMode.ForRead) as Entity;
                            if (ob10.GetType() == typeof(AttributeDefinition))
                            {
                                atr += ((AttributeDefinition)ob10).Tag.ToString() + "\n";
                            }
                        }
                        Application.ShowAlertDialog(atr);

                        TypedValue[] fi = new TypedValue[1];
                        TypedValue dane = new TypedValue(0, "ATTDEF");
                        fi[0] = dane;
                        SelectionFilter fil = new SelectionFilter(fi);
                        PromptSelectionOptions kom3 = new PromptSelectionOptions();
                        kom3.SingleOnly = true;

                        PromptSelectionResult wyn = ed.GetSelection(kom3, fil);

                        if (wyn.Status == PromptStatus.OK)
                        {
                            SelectionSet wybor = wyn.Value;
                            Entity ob4 = wybor[0].ObjectId.GetObject(OpenMode.ForRead) as Entity;
                            Entity ob5 = ob4.Clone() as Entity;
                            AttributeDefinition ob6 = (AttributeDefinition)ob5;
                            ob6.Position = bt2.Origin;
                            ob4.Erase();
                            bt2.AppendEntity(ob6);
                            tran.AddNewlyCreatedDBObject(ob6, true);
                        }

                    }
                    else
                    {
                        BlockTableRecord blok1 = new BlockTableRecord();
                        blok1.Name = wynik.StringResult;

                        PromptPointOptions kom2 = new PromptPointOptions("wskaz punkt bloku");

                        PromptPointResult rez = ed.GetPoint(kom2);

                        if (rez.Status == PromptStatus.OK)
                        {
                            blok1.Origin = rez.Value;
                            blok1.Units = UnitsValue.Millimeters;
                            blok1.Explodable = true;
                        }
                        bt.Add(blok1);
                        tran.AddNewlyCreatedDBObject(blok1, true);


                        Circle ok1 = new Circle();
                        ok1.Center = rez.Value;
                        kom2.Message = "wskaz drugi punkt wyznacznie promienia";
                        kom2.UseBasePoint = true;
                        kom2.BasePoint = rez.Value;

                        PromptPointResult rez1 = ed.GetPoint(kom2);

                        ok1.Radius = rez.Value.DistanceTo(rez1.Value);
                        blok1.AppendEntity(ok1);
                        tran.AddNewlyCreatedDBObject(ok1, true);

                        PromptEntityOptions kom3 = new PromptEntityOptions("wskaz obkiet");
                        PromptEntityResult rez3 = ed.GetEntity(kom3);
                        if (rez3.Status == PromptStatus.OK)
                        {
                            Entity obw = tran.GetObject(rez3.ObjectId, OpenMode.ForRead) as Entity;
                            Entity obw1 = obw.Clone() as Entity;
                            blok1.AppendEntity(obw1);
                            tran.AddNewlyCreatedDBObject(obw1, true);
                        }

                        PromptSelectionOptions kom6 = new PromptSelectionOptions();
                        kom6.SingleOnly = true;
                        kom6.MessageForAdding = "\nwskaz atrybut";
                        PromptSelectionResult wynik6 = ed.GetSelection(kom6);

                        if (wynik6.Status == PromptStatus.OK)
                        {
                            SelectionSet wynik7 = wynik6.Value;
                            Entity obiekt1 = tran.GetObject(wynik7[0].ObjectId, OpenMode.ForWrite) as Entity;
                            if (obiekt1.GetType() == typeof(AttributeDefinition))
                            {
                                Entity obiekt2 = obiekt1.Clone() as Entity;
                                blok1.AppendEntity(obiekt2);
                                tran.AddNewlyCreatedDBObject(obiekt2, true);
                                obiekt1.Erase();
                            }
                        }

                        AttributeDefinition art1 = new AttributeDefinition();
                        double[] tab1 = { 10, 10, 0 };
                        Vector3d wek = new Vector3d(tab1);
                        art1.Position = blok1.Origin.TransformBy(Matrix3d.Displacement(wek));

                        PromptStringOptions kom4 = new PromptStringOptions("podaj etykiete i jednoczesnie komunikat zgloszenowy");
                        kom4.AllowSpaces = true;
                        PromptResult rez4 = ed.GetString(kom4);
                        if (rez4.Status == PromptStatus.OK)
                        {
                            string[] tab2 = rez4.StringResult.Split(new char[] { ' ' }, 2);
                            art1.Prompt = tab2[1];
                            art1.Tag = tab2[0];
                        }
                        art1.Verifiable = true;
                        art1.LockPositionInBlock = true;
                        art1.TextStyleId = baza.Textstyle;

                        PromptIntegerOptions kom5 = new PromptIntegerOptions("podaj wysokosc tekstu");
                        kom5.AllowNegative = false;
                        kom5.AllowZero = false;

                        kom5.DefaultValue = 14;

                        PromptIntegerResult rez5 = ed.GetInteger(kom5);
                        if (rez5.Status == PromptStatus.OK)
                        {
                            art1.Height = rez5.Value;
                        }
                        blok1.AppendEntity(art1);
                        tran.AddNewlyCreatedDBObject(art1, true);
                    }
                    tran.Commit();
                }
            }
        }
        [CommandMethod("slowakluczowe")]
        public void slowakluczowe()
        {
            Document AcDoc = Application.DocumentManager.MdiActiveDocument;
            PromptIntegerOptions pInOpts = new PromptIntegerOptions("");
            pInOpts.Message = "wprowadz rozmiar lub";

            pInOpts.AllowZero = false;
            pInOpts.AllowNegative = false;

            pInOpts.Keywords.Add("Duzy");
            pInOpts.Keywords.Add("Maly");
            pInOpts.Keywords.Add("Typowy");
            pInOpts.Keywords.Default = "Typowy";
            pInOpts.AllowNone = true;
            pInOpts.DefaultValue = 2;
            pInOpts.AppendKeywordsToMessage = true;

            PromptIntegerResult pINRes = AcDoc.Editor.GetInteger(pInOpts);

            if (pINRes.Status == PromptStatus.Keyword)
            {
                Application.ShowAlertDialog("wprowadzono slowo kluczowe: " + pINRes.StringResult);
            }
            else
            {
                Application.ShowAlertDialog("wprowadzono wartosc " + pINRes.Value.ToString());
            }
        }
        [CommandMethod("zmianaDefinicjiAtrybutu")]

        public void zmianaDefinicjiAtrybutu()
        {
            Database baza = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            using (Transaction tran = baza.TransactionManager.StartTransaction())
            {
                string nazwa_bloku = "";
                List<string> naz_bl = new List<string>();
                BlockTable bt = baza.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                foreach (ObjectId blok in bt)
                {
                    BlockTableRecord btr = blok.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    if (btr.Name.Substring(0, 1) != "*" & btr.HasAttributeDefinitions == true)
                    {
                        int liczba = btr.GetBlockReferenceIds(true, true).Count;

                        if (liczba > 0)
                        {
                            nazwa_bloku += btr.Name + "\n";
                            naz_bl.Add(btr.Name);
                        }
                    }
                }
                PromptKeywordOptions kom1 = new PromptKeywordOptions("podaj nazwy kluczowe");
                kom1.AppendKeywordsToMessage = true;

                foreach (string ss in naz_bl)
                {
                    kom1.Keywords.Add(ss);
                }
                PromptResult wynik = ed.GetKeywords(kom1);
                Application.ShowAlertDialog(wynik.StringResult);
                if (wynik.Status == PromptStatus.OK)
                {
                    BlockTableRecord blok_zm = bt[wynik.StringResult].GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    foreach (ObjectId bb in blok_zm)
                    {
                        Entity en1 = bb.GetObject(OpenMode.ForRead) as Entity;
                        if (en1.GetType() == typeof(AttributeCollection))
                        {
                            AttributeDefinition atdef = (AttributeDefinition)en1;

                            atdef.UpgradeOpen();

                            atdef.ColorIndex = 1;
                        }
                    }
                    tran.Commit();
                }
            }
        }
    }
}




