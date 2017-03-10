using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ListenInTool.Classes
{
    class CBindDGPatientDetail : INotifyPropertyChanged
    {
        private int m_intIdx;
        public string m_strPatientId;
        public string m_strDataset;
        public string m_strStartDate;
        
        public event PropertyChangedEventHandler PropertyChanged;
        //public event NotifyCollectionChangedEventHandler CollectionChanged;

        //----------------------------------------------------------------------------------------------------
        // CBindDGPatientDetail
        //----------------------------------------------------------------------------------------------------
        public CBindDGPatientDetail(int intI, string strPatientId, string strDataset, string strDate)
        {
            m_intIdx = intI;
            m_strPatientId = strPatientId;
            m_strDataset = strDataset;
            m_strStartDate = strDate;            
        }

        //----------------------------------------------------------------------------------------------------
        // Getters and Setters
        //----------------------------------------------------------------------------------------------------
        #region Properties Getters and Setters
        public int Idx
        {
            get { return this.m_intIdx; }
            set
            {
                this.m_intIdx = value;
                OnPropertyChanged("Idx");
            }
        }

        public string PatientId
        {
            get
            {
                return this.m_strPatientId;
            }
            set
            {
                //this.intIdx = value;
                OnPropertyChanged("PatientId");
            }
        }

        public string Dataset
        {
            get
            {
                return this.m_strDataset;
            }
            set
            {
                //this.intIdx = value;
                OnPropertyChanged("Dataset");
            }
        }

        public string StartDate
        {
            get
            {
                return this.m_strStartDate;
            }
            set
            {
                //this.intIdx = value;
                OnPropertyChanged("StartDate");
            }
        }       

        #endregion

        //----------------------------------------------------------------------------------------------------
        // OnPropertyChanged
        //----------------------------------------------------------------------------------------------------
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
