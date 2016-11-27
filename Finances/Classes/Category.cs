﻿using System.Collections.Generic;
using System.ComponentModel;

namespace Finances
{
    internal class Category : INotifyPropertyChanged
    {
        private string _name;
        private List<string> _minorCategories = new List<string>();

        #region Properties

        /// <summary>Name of major category</summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        /// <summary>List of minor categories related to the major category</summary>
        public List<string> MinorCategories
        {
            get { return _minorCategories; }
            set { _minorCategories = value; OnPropertyChanged("SubCategories"); }
        }

        #endregion Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        public override string ToString()
        {
            return Name;
        }

        #region Constructors

        /// <summary>Initializes a default instance of Category.</summary>
        public Category()
        {
        }

        /// <summary>Initializes an instance of Category by assigning Properties.</summary>
        /// <param name="name">Name of major category</param>
        /// <param name="minorCategories">List of minor categories related to the major category</param>
        public Category(string name, List<string> minorCategories)
        {
            Name = name;
            MinorCategories = minorCategories;
        }

        #endregion Constructors
    }
}