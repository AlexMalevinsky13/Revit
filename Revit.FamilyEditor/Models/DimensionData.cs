using System.Runtime.Serialization;

namespace Revit.FamilyEditor.Models
{
    /// <summary>
    /// Данные о размерной связи между двумя точками и параметром
    /// </summary>
    [DataContract]
    internal class DimensionData
    {
        /// <summary>Начальная точка размера</summary>
        [DataMember]
        public Point2D Start { get; set; }

        /// <summary>Конечная точка размера</summary>
        [DataMember]
        public Point2D End { get; set; }

        /// <summary>Имя параметра, к которому привязан размер</summary>
        [DataMember]
        public string Label { get; set; }
    }
}