using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTD_SingleMapRenderer.BaseClasses
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public BaseViewModel()
        {

        }

        #region ********** INotifyPropertyChanged **********
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void RaisePropertyChanged(string prop)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

    }
}
