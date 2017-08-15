using System;
using System.ComponentModel;

namespace Finances.Classes.Categories
{
    /// <summary>Represents a minor category.</summary>
    public class MinorCategory : INotifyPropertyChanged, IEquatable<MinorCategory>
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        #region Override Operators

        private static bool Equals(MinorCategory left, MinorCategory right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left) ^ ReferenceEquals(null, right)) return false;
            return string.Equals(left.Name, right.Name);
        }

        public sealed override bool Equals(object obj) => Equals(this, obj as MinorCategory);

        public bool Equals(MinorCategory otherMinorCategory) => Equals(this, otherMinorCategory);

        public static bool operator ==(MinorCategory left, MinorCategory right) => Equals(left, right);

        public static bool operator !=(MinorCategory left, MinorCategory right) => !Equals(left, right);

        public sealed override int GetHashCode() => base.GetHashCode() ^ 17;

        public sealed override string ToString() => Name;

        #endregion Override Operators

        #region Constructors

        /// <summary>Initializes a default instance of MinorCategory.</summary>
        public MinorCategory()
        {
        }

        /// <summary>Iniitalizes an instance of MinorCategory by assigning its name.</summary>
        /// <param name="name">Name</param>
        public MinorCategory(string name) => Name = name;

        /// <summary>Replaces  instance of MinorCategory with a new instance.</summary>
        /// <param name="otherCategory">MinorCategory to replace  instance</param>
        public MinorCategory(MinorCategory otherCategory) => Name = otherCategory.Name;

        #endregion Constructors
    }
}