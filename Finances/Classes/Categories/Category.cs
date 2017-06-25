using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Finances.Classes.Categories
{
    /// <summary>Represents a category of transactions.</summary>
    public class Category : INotifyPropertyChanged
    {
        private string _name;
        private List<MinorCategory> _minorCategories = new List<MinorCategory>();

        #region Properties

        /// <summary>Name of major category</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged("Name"); }
        }

        /// <summary>List of minor categories related to the major category</summary>
        public List<MinorCategory> MinorCategories
        {
            get => _minorCategories;
            private set { _minorCategories = value; OnPropertyChanged("SubCategories"); }
        }

        #endregion Properties

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        internal void Sort()
        {
            if (MinorCategories.Count > 0)
                MinorCategories = MinorCategories.OrderBy(cat => cat.Name).ToList();
        }

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
        public Category(string name, List<MinorCategory> minorCategories)
        {
            Name = name;
            MinorCategories = minorCategories;
        }

        #endregion Constructors
    }
}