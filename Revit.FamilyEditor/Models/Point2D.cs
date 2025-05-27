using System.Runtime.Serialization;

namespace Revit.FamilyEditor.Models
{
    /// <summary>
    /// 2D-точка
    /// </summary>
    [DataContract]
    internal class Point2D
    {
        /// <summary>Координата X в мм</summary>
        [DataMember]
        public double X { get; set; }

        /// <summary>Координата Y в мм</summary>
        [DataMember]
        public double Y { get; set; }
    }
}