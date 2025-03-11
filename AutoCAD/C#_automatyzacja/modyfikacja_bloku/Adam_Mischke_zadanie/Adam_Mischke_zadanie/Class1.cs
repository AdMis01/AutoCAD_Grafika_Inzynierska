using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using System.Data.SqlClient;
using static Autodesk.AutoCAD.Internal.WSUtils;
using System.Data;

namespace ClassLibrary2
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
        [CommandMethod("tAtry")]
        
        public void tAtry()
        {
            Document dok = Application.DocumentManager.MdiActiveDocument;
            PromptSelectionOptions koms = new PromptSelectionOptions();
            koms.SingleOnly = false;

            TypedValue[] fi = new TypedValue[1];

            TypedValue dane = new TypedValue(0, "INSERT");

            fi[0] = dane;
            SelectionFilter fil = new SelectionFilter(fi);
            PromptSelectionResult wyb = dok.Editor.GetSelection(koms, fil);
            SelectionSet wybor;
            if(wyb.Status == PromptStatus.OK)
            {
                wybor = wyb.Value;
                List<string> tag_a = new List<string>();
                Database bazad = dok.Database;
                using (Transaction tran = bazad.TransactionManager.StartTransaction())
                {
                    string atrb = "";
                    foreach(SelectedObject sob in wybor)
                    {
                        BlockReference blokw = sob.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;
                        AttributeCollection atr_t = blokw.AttributeCollection;

                        foreach(ObjectId ff in atr_t)
                        {
                            AttributeReference atrww = ff.GetObject(OpenMode.ForRead) as AttributeReference;
                            atrb += atrww.Tag + "\t" + atrww.TextString + "\n";
                            tag_a.Add(atrww.Tag);
                        }
                    }
                    var l_mw = tag_a.Distinct();
                    Application.ShowAlertDialog(atrb);
                    string ww_l = "";

                    foreach(var naz in l_mw)
                    {
                        ww_l += naz.ToString() + "\n";
                    }
                    Application.ShowAlertDialog(ww_l);

                    PromptStringOptions komss = new PromptStringOptions("podaj nazwe atrybutu do zmiany\n" + ww_l);
                    PromptResult rezultat = dok.Editor.GetString(komss);

                    if(rezultat.Status == PromptStatus.OK)
                    {
                        foreach(SelectedObject sob in wybor)
                        {
                            BlockReference blokw_1 = sob.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;
                            AttributeCollection atr_1 = blokw_1.AttributeCollection;
                            foreach(ObjectId aatt in atr_1)
                            {
                                AttributeReference atrww_1 = aatt.GetObject(OpenMode.ForRead) as AttributeReference;
                                if(atrww_1.Tag == rezultat.StringResult)
                                {
                                    atrww_1.UpgradeOpen();
                                    PromptResult kom3 = dok.Editor.GetString("podaj nowa wartosc atrybutu");

                                    if(kom3.Status == PromptStatus.OK)
                                    {
                                        atrww_1.TextString = kom3.StringResult;
                                    }
                                    else
                                    {
                                        atrww_1.TextString += "\tnumer 12";
                                    }
                                }
                            }
                        }
                    }
                    tran.Commit();
                }
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
                foreach(ObjectId blok in bt)
                {
                    BlockTableRecord btr = blok.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    if(btr.Name.Substring(0,1)!= "*" & btr.HasAttributeDefinitions == true)
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

                foreach(string ss in naz_bl)
                {
                    kom1.Keywords.Add(ss);
                }
                PromptResult wynik = ed.GetKeywords(kom1);
                Application.ShowAlertDialog(wynik.StringResult);
                if(wynik.Status == PromptStatus.OK)
                {
                    BlockTableRecord blok_zm = bt[wynik.StringResult].GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    foreach(ObjectId bb in blok_zm)
                    {
                        Entity en1 = bb.GetObject(OpenMode.ForRead) as Entity;
                        if (en1.GetType()== typeof(AttributeCollection)){
                            AttributeDefinition atdef = (AttributeDefinition)en1;

                            atdef.UpgradeOpen();

                            atdef.ColorIndex = 1;
                        }
                    }
                    tran.Commit();
                }
            }
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

            if(wynik.Status == PromptStatus.OK)
            {
                using(Transaction tran = baza.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tran.GetObject(baza.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    if (bt.Has(wynik.StringResult))
                    {

                        //var ggg = bt[wynik.StringResult];
                        BlockTableRecord bt2 = new BlockTableRecord();
                        foreach(ObjectId id in bt)
                        {
                            BlockTableRecord btr3 = id.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                            if(btr3.Name == wynik.StringResult)
                            {
                                bt2 = btr3;
                            }
                        }
                        bt2.UpgradeOpen();

                        string atr = "nazwa atrybutów\n";
                        foreach(ObjectId ob8 in bt2)
                        {
                            Entity ob10 = ob8.GetObject(OpenMode.ForRead) as Entity;
                            if(ob10.GetType()== typeof(AttributeDefinition))
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

                        if(wyn.Status == PromptStatus.OK)
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

                        if(rez.Status == PromptStatus.OK)
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
                        if(rez3.Status == PromptStatus.OK)
                        {
                            Entity obw = tran.GetObject(rez3.ObjectId, OpenMode.ForRead) as Entity;
                            Entity obw1 = obw.Clone() as Entity;
                            blok1.AppendEntity(obw1);
                            tran.AddNewlyCreatedDBObject(obw1,true);
                        }

                        PromptSelectionOptions kom6 = new PromptSelectionOptions();
                        kom6.SingleOnly = true;
                        kom6.MessageForAdding = "\nwskaz atrybut";
                        PromptSelectionResult wynik6 = ed.GetSelection(kom6);

                        if(wynik6.Status == PromptStatus.OK)
                        {
                            SelectionSet wynik7 = wynik6.Value;
                            Entity obiekt1 = tran.GetObject(wynik7[0].ObjectId, OpenMode.ForWrite) as Entity;
                            if(obiekt1.GetType() == typeof(AttributeDefinition))
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
                        if(rez4.Status == PromptStatus.OK)
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
                        if(rez5.Status == PromptStatus.OK)
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

        [CommandMethod("pobieranie_lancucha")]
        public void pobieranie_lancucha()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptStringOptions kom = new PromptStringOptions("Podaj nazwe");
            kom.AllowSpaces = true;
            PromptResult wynik = ed.GetString(kom);

            if (wynik.Status == PromptStatus.OK)
            {
                Application.ShowAlertDialog(wynik.StringResult);
            }

        }
        [CommandMethod("odleglosc")]
        public void odleglosc()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptDistanceOptions dys = new PromptDistanceOptions("Podaj Dystans");
            PromptDoubleResult wynik = ed.GetDistance(dys);
            Application.ShowAlertDialog("Dystans: " + wynik.ToString());
            if (wynik.Status == PromptStatus.OK)
            {
                Application.ShowAlertDialog(wynik.StringResult);
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
        [CommandMethod("StworzOkrag")]
        public void stworzOkrag()
        {

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions kom = new PromptPointOptions("wskaz punkt");
            PromptPointResult rez = ed.GetPoint(kom);

            Point3d punkt = new Point3d();

            PromptPointOptions kom2 = new PromptPointOptions(" wskaz drugi punkt ");

            kom2.BasePoint = punkt;
            PromptPointResult rez2 = ed.GetPoint(kom2);
            Point3d punkt2 = rez2.Value;
            Circle okr = new Circle();
            okr.Radius = punkt.DistanceTo(punkt2);
            okr.Center = punkt;

        }
        [CommandMethod("bloki")]
        public void rozpoznaj_bloki()
        {
            Database baza1 = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tran = baza1.TransactionManager.StartTransaction())
            {
                BlockTable bt = tran.GetObject(baza1.BlockTableId, OpenMode.ForRead) as BlockTable;
                Application.ShowAlertDialog("Typ obiektu to : * " + bt.GetType().ToString());
                string info = "";
                foreach (ObjectId btr in bt)
                {
                    BlockTableRecord btr2 = btr.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    info += /*btr2.GetType().ToString() + "\t" +*/ btr2.Name + "\n";

                }
                PromptStringOptions pp = new PromptStringOptions("" + info);
                var ppp = Application.DocumentManager.MdiActiveDocument.Editor.GetString(pp);
                string nazwa_b = "";
                if (ppp.Status == PromptStatus.OK)
                {
                    nazwa_b = ppp.StringResult;
                }
                if (bt.Has(nazwa_b) == true)
                {
                    var xx = bt[nazwa_b];
                    Application.ShowAlertDialog(xx.ToString());
                    BlockTableRecord pppp = xx.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    foreach (ObjectId dd in pppp)
                    {
                        Entity ddd = dd.GetObject(OpenMode.ForRead) as Entity;
                        info += ddd.GetType();
                    }

                }
                Application.ShowAlertDialog(info);
            }

        }
        [CommandMethod("rozp")]

        public void clock()
        {
            Database baza_1 = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tran = baza_1.TransactionManager.StartTransaction())
            {
                BlockTable bt = tran.GetObject(baza_1.BlockTableId, OpenMode.ForRead) as BlockTable;
                Application.ShowAlertDialog("Typ obiektu to : * " + bt.GetType().ToString());
                string info = "";
                foreach (ObjectId btr in bt)
                {
                    BlockTableRecord btr2 = btr.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    info += /*btr2.GetType().ToString() + "\t" + */btr2.Name + "\n";
                }
                PromptStringOptions pp = new PromptStringOptions("" + info);
                var ppp = Application.DocumentManager.MdiActiveDocument.Editor.GetString(pp);
                string nazwa_b = "";
                if (ppp.Status == PromptStatus.OK)
                {
                    nazwa_b = ppp.StringResult;
                }
                if (bt.Has(nazwa_b) == true)
                {
                    foreach (ObjectId btr in bt) ;
                }
                Application.ShowAlertDialog(info);
            }
        }

        [CommandMethod("filtry")]

        public void filtry()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] filtr = new TypedValue[3];
            filtr.SetValue(new TypedValue((int)DxfCode.Start, "Circle"), 0);
            filtr.SetValue(new TypedValue((int)DxfCode.Operator, ">="), 1);
            filtr.SetValue(new TypedValue((int)DxfCode.Real, 20), 2);

            SelectionFilter filtr1 = new SelectionFilter(filtr);
            PromptSelectionResult wynik_0 = ed.SelectAll(filtr1);
            SelectionSet wynik;

            if (wynik_0.Status == PromptStatus.OK)
            {
                wynik = wynik_0.Value;
                Application.ShowAlertDialog("Liczba spełniająca to  kryterium to: " + wynik.Count.ToString());
            }
        }
        [CommandMethod("sql")]
        public void sql()
        {
            SqlConnection pol = new SqlConnection();
            pol.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ACAD;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            pol.Open();
            Application.ShowAlertDialog(pol.State.ToString());

            SqlCommand com = new SqlCommand();
            com.Connection = pol;
            com.CommandType = CommandType.Text;
            com.CommandText = "Select * From Prostok";
            SqlDataReader wynik = com.ExecuteReader();

            System.Data.DataTable t1 = new System.Data.DataTable();
            t1.Load(wynik);
            Application.ShowAlertDialog(t1.Rows.Count.ToString());
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tran = db.TransactionManager.StartTransaction())
            {

                Point3d p1 = new Point3d();

                var z1 = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                Line l1 = new Line();
                l1.StartPoint = new Point3d();
            }
            pol.Close();



        }
        [CommandMethod("tworzenieBloku")]

        public void tworzenieBloku() {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database baz = ed.Document.Database;

            using (Transaction tran = baz.TransactionManager.StartTransaction())
            {

                BlockTable bt = tran.GetObject(baz.BlockTableId, OpenMode.ForRead) as BlockTable;
                string info = "";
                foreach (ObjectId btr in bt)
                {
                    BlockTableRecord btr2 = btr.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    info += btr2.Name + "\n";
                }
            }

        }



        [CommandMethod("rozbijanieB")]

        public void rozbijanieB()
        {
            Document dok = Application.DocumentManager.MdiActiveDocument;

            Database baza = dok.Database;

            PromptPointOptions kom = new PromptPointOptions("wskaz punkt");
            PromptPointResult rez = dok.Editor.GetPoint(kom);
            Point3d pkt1 = new Point3d();

            if (rez.Status == PromptStatus.OK)
            {
                pkt1 = rez.Value;
            }

            PromptPointOptions kom1 = new PromptPointOptions("wskaz punkt");
            kom1.BasePoint = pkt1;
            kom1.UseBasePoint = true;
            kom.UseDashedLine = true;
            PromptPointResult rez1 = dok.Editor.GetPoint(kom1);
            Point3d pkt2 = new Point3d();

            if (rez1.Status == PromptStatus.OK)
            {
                pkt2 = rez1.Value;
            }

            TypedValue[] fill = new TypedValue[4];
            fill.SetValue(new TypedValue((int)DxfCode.Operator,"<or"),0);
            fill.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 1);
            fill.SetValue(new TypedValue((int)DxfCode.Start, "Lwpolyline"), 2);
            fill.SetValue(new TypedValue((int)DxfCode.Operator,"or>"),3);

            SelectionFilter filter1 = new SelectionFilter(fill);

            PromptSelectionResult rez2 = dok.Editor.SelectWindow(pkt1,pkt2, filter1);
            SelectionSet wybor;
            if(rez2.Status == PromptStatus.OK )
            {

                wybor = rez2.Value;

                Application.ShowAlertDialog(wybor.Count.ToString());

                using (Transaction tran = baza.TransactionManager.StartTransaction())
                {
                    BlockTableRecord b1 = tran.GetObject(baza.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                    DBObjectCollection zbior = new DBObjectCollection();
                    foreach(SelectedObject ob in wybor)
                    {
                        Entity blok1 = tran.GetObject(ob.ObjectId, OpenMode.ForWrite) as Entity;
                        blok1.Explode(zbior);
                        blok1.Erase();
                    }


                    foreach(Entity ob1 in zbior)
                    {
                        ob1.ColorIndex = 2;
                        b1.AppendEntity(ob1);
                        tran.AddNewlyCreatedDBObject(ob1, true);
                    }
                    tran.Commit();
                }
            }

        }

        [CommandMethod("CreatingABlock")]
        public void CreatingABlock()
        {
            // Get the current database and start a transaction
            Database acCurDb;
            acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (!acBlkTbl.Has("CircleBlock2"))
                {
                    using (BlockTableRecord acBlkTblRec = new BlockTableRecord())
                    {
                        acBlkTblRec.Name = "CircleBlock";

                        // Set the insertion point for the block
                        acBlkTblRec.Origin = new Point3d(0, 0, 0);

                        // Add a circle to the block
                        using (Circle acCirc = new Circle())
                        {
                            acCirc.Center = new Point3d(0, 0, 0);
                            acCirc.Radius = 2;

                            acBlkTblRec.AppendEntity(acCirc);

                            acBlkTbl.UpgradeOpen();
                            acBlkTbl.Add(acBlkTblRec);
                            acTrans.AddNewlyCreatedDBObject(acBlkTblRec, true);
                        }
                    }
                }

                // Save the new object to the database
                acTrans.Commit();

                // Dispose of the transaction
            }
        }

        [CommandMethod("znajomi")]
        public void znajomi()
        {
            string pol_1 = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ACAD;Integrated Security=True";
            SqlDataAdapter ff = new SqlDataAdapter("Select * from znajomi", pol_1);
            DataSet zasob = new DataSet();
            ff.Fill(zasob, "znajomi");
            var wynik = zasob.Tables[0].Rows[0][1].ToString();
            Database baza_1 = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //Application.ShowAlertDialog(wynik);
            var wynik2 = "";
            foreach (DataRow n in zasob.Tables[0].Rows)
            {
                foreach (System.Data.DataColumn a in zasob.Tables[0].Columns)
                {
                    wynik2 += n[a].ToString() + " ";
                }

            }
            Application.ShowAlertDialog(wynik2);
            zasob.Dispose();
            ff.Dispose();

            MText tt = new MText();
            tt.Contents = wynik2;
            PromptPointOptions kom = new PromptPointOptions("wskaz punkt");
            PromptPointResult rez = ed.GetPoint(kom);
            Point3d punkt = new Point3d();
            if (rez.Status == PromptStatus.OK)
            {
                punkt = rez.Value;


            }
            tt.Location = punkt;
            using (Transaction tran = baza_1.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = baza_1.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(tt);
                tran.AddNewlyCreatedDBObject(tt, true);
                tran.Commit();

            }


        }

    }
}