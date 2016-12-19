using System;
using TopSolid.Cam.NC.PostProcessors.Bases.Siemens;
using TopSolid.Cam.NC.PostProcessors.Kernel.DB;
using TopSolid.Cam.NC.PostProcessors.Kernel.DB.Items;

namespace GeislingerPp
{
    public class MirEgalKlasse : PostProcessorBaseSiemens
    {
        // hier kommt unser schlauer Code hin
        // Ergänzen immer durch Überschreiben (override) von Methoden

        // steht für ein Wort im Postprozessor
        PPData PROGRAMM_ERSTELLER, WKLISTE_ZEILE;

        // eine Zahlenvariable
        double masch_nummer;

        // Verbindet unsere eigenen PP-Wörter mit der PP-Datenbank
        public override void LoadNewPPData()
        {
            base.LoadNewPPData();
            PROGRAMM_ERSTELLER = AddPPData("PROGRAMM_ERSTELLER");
            WKLISTE_ZEILE = AddPPData("WKLISTE_ZEILE");
        }

        // Einlesen von Konfigurationsinformationen aus der INI-Datei
        public override void LoadParameters()
        {
            base.LoadParameters();
            masch_nummer = GetParameterValue("GEISLINGER", "maschinen_nummer");
        }

        // Erlaubt es, den auszugebenden Wert eines Wortes vor der Ausgabe zu verändern
        public override void LoadDelegateDataText()
        {
            base.LoadDelegateDataText();
            AddDelegateDataTextGenerating("GOTO_Z", GotoZAufmotzen);
            AddDelegateDataTextGenerating("HOME_Z", GotoZAufmotzen);
        }

        private string GotoZAufmotzen(PPData inPPData, string inText)
        {
            if (Math.Abs(inPPData.Value - Math.Round(inPPData.Value)) < 0.001)
                return inText + ".0";
            else
                return inText;
        }

        #region Meine Eventfunktionen

        // hängt unsere Funktionen an bestimmte Ereignisse
        public override void LoadDelegateEvent()
        {
            base.LoadDelegateEvent();
            AddDelegateEventBeforeExecute("MYDEFINE_TOOL_BEGIN", WerkzeugAnfang);
            AddDelegateEventBeforeExecute("DEFINE_ARC", KreisVertauschen);
            AddDelegateEventBeforeExecute("MYDEFINE_PROGRAMME_BEGIN", ProgrammAnfang);
            AddDelegateEventBeforeExecute("MYDEFINE_TOOL_END", SpindelAus);
            AddDelegateEventBeforeExecute("MYDEFINE_PROGRAMME_BEGIN_WKLISTE", WerkzeugListe);
            AddDelegateEventBeforeExecute("MYDEFINE_MATRIX_BEGIN", SeiteAufruf);
            AddDelegateEventBeforeExecute("MYDEFINE_PROGRAMME_END_UPS", ContextUnterprogramme);
        }

        private bool ContextUnterprogramme(string inPPEventName)
        {
            foreach (PPContext cont in ContextList)
                if (cont.IsMatrixTranslated() || cont.IsMatrixRotated())
                {
                    IDENT_SUB_PROG.Value = cont.Id;
                    MATRIX_OX.Value = cont.X;
                    MATRIX_OY.Value = cont.Y;
                    MATRIX_OZ.Value = cont.Z;
                    MATRIX_ANGLE_X.Value = cont.AngleX;
                    MATRIX_ANGLE_Y.Value = cont.AngleY;
                    MATRIX_ANGLE_Z.Value = cont.AngleZ;
                    ExecuteEvent("MYDEFINE_PROGRAMME_END_UP_CONTEXT");
                }
            return true;
        }

        private bool SeiteAufruf(string inPPEventName)
        {
            IDENT_SUB_PROG.Text = CurrentContext.Id.ToString();
            return true;
        }

        private bool WerkzeugListe(string inPPEventName)
        {
            foreach (PPTool tool in ToolList)
            {
                WKLISTE_ZEILE.Text = $"T{tool.PocketNumber}: {tool.GetDataText("Description")} D:{tool.GetDataValue("CuttingDiameter")} L:{tool.Length}";
                ExecuteEvent("MYDEFINE_PROGRAMME_BEGIN_WKLISTE_ZEILE");  // #bloc = MSG,[MYDEFINE_PROGRAMME_BEGIN_WKLISTE_ZEILE]
            }
            return true;
        }

        private bool SpindelAus(string inPPEventName)
        {
            var spindel_jetzt = CurrentOperation.GetDataText("SPINDLE_ROT") ?? "NONE";
            var spindel_nachher = NextOperation?.GetDataText("SPINDLE_ROT") ?? "NONE";
            if (spindel_jetzt != spindel_nachher)
                SPINDLE.Value = 5.0;
            else
                SPINDLE.DataVoid();  // #bloc = CLS,[SPINDLE]
            return true;
        }

        private bool ProgrammAnfang(string inPPEventName)
        {
            PROGRAMM_ERSTELLER.Text = $"Prog.: {Document.GetDataText("Author")} - Masch.-Nr: {masch_nummer}";
            // double anf_distanz = CurrentOperation.GetDataValue("LeadDistance");
            return true;
        }

        private bool KreisVertauschen(string inPPEventName)
        {
            G_INTERPO.Value = 5.0 - G_INTERPO.Value;
            return true;
        }

        private bool WerkzeugAnfang(string inPPEventName)
        {
            // LOADTL.Value = CurrentTool.Length;
            TOOL_COMM.Text = $"{CurrentTool.GetDataText("Description")} D:{CurrentTool.GetDataValue("CuttingDiameter")} L:{CurrentTool.Length}";
            return true;
        }
        #endregion
    }
}
