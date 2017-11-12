using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DTD_SingleMapRenderer.BaseClasses;

namespace _7DTD_SingleMapRenderer.Tools.RegionViewer
{
    public class ChunkViewModel : BaseViewModel
    {
        private string m_Name;

        public string Name
        {
            get { return m_Name; }
            set
            {
                if (m_Name != value)
                {
                    m_Name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        private int m_X;

        public int X
        {
            get { return m_X; }
            set
            {
                if (m_X != value)
                {
                    m_X = value;
                    RaisePropertyChanged("X");
                }
            }
        }

        private int m_Y;

        public int Y
        {
            get { return m_Y; }
            set
            {
                if (m_Y != value)
                {
                    m_Y = value;
                    RaisePropertyChanged("Y");
                }
            }
        }

        private RegionFileViewModel m_Region;

        public RegionFileViewModel Region
        {
            get { return m_Region; }
            set
            {
                if (m_Region != value)
                {
                    m_Region = value;
                    RaisePropertyChanged("Region");
                }
            }
        }


        public ChunkViewModel(RegionFileViewModel region, int x, int y)
        {
            this.Region = region;
            this.X = x;
            this.Y = y;
            this.Name = x + " / " + y;
        }

    }
}
