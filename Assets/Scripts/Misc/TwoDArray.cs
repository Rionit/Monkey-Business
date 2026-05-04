using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("MonkeyBusiness.Misc.Editor")]
namespace MonkeyBusiness.Misc
{
    /// <summary>
    /// A simple implementation of a 2D array. Internally uses a 1D array of size Width * Height.
    /// </summary>
    [Serializable]
    public class TwoDArray<T>
    {   
        [SerializeField]
        internal T[] _data;

        /// <summary>
        /// Width of the array.
        /// </summary>
        [field:SerializeField]
        public int Width { get; private set; }
        
        [field:SerializeField]
        public int Height { get; private set; }

        public TwoDArray(int height, int width)
        {
            Debug.Log("Calling constructor !");
            Width = width;
            Height = height;
            _data = new T[width * height];
        }

        public TwoDArray(T[,] initialData)
        {
            Height = initialData.GetLength(0);
            Width = initialData.GetLength(1);
            _data = new T[Width * Height];
            for(int i = 0; i < Height; i++)
            {
                for(int j = 0; j < Width; j++)
                {
                    _data[i * Width + j] = initialData[i, j];
                }
            }
        }

        public T this[int row, int col]
        {
            get
            {
                if(row < 0 || col < 0 || row >= Height || col >= Width)
                {
                    throw new IndexOutOfRangeException($"Index out of range: ({row}, {col}). Array size is ({Height}, {Width}).");
                }
                return _data[row * Width + col];
            }
            set
            {
                if(row < 0 || col < 0 || row >= Height || col >= Width)
                {
                    throw new IndexOutOfRangeException($"Index out of range: ({row}, {col}). Array size is ({Height}, {Width}).");
                }
                _data[row * Width + col] = value;
            }
        }

    }
}
