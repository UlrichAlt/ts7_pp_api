using System;

namespace GeislingerPp
{
    public class GeislingerAddin : TopSolid.Kernel.TX.AddIns.AddIn
    {
        public override string[] Description
        {
            get
            {
                return new string[] { "Der tolle PP von Geislinger" };
            }
        }

        public override string Manufacturer
        {
            get
            {
                return "Geislinger";
            }
        }

        public override string Name
        {
            get
            {
                return "Geislinger PP";
            }
        }

        public override Guid[] RequiredAddIns
        {
            get
            {
                return new Guid[]
                {
                    new System.Guid("E47E7476-59EB-46c4-B689-03861952882A"),	// TopSolid.Cad.Design.AddIn
					new System.Guid("E367426C-9F1E-46d3-9733-1380703D133A")		// TopSolid.Cam.NC.MillTurn.AddIn
				};
            }
        }

        public override void EndSession()
        {
        }

        public override void InitializeSession()
        {
        }

        public override void StartSession()
        {
        }
    }
}
