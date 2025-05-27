using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Revit.FamilyEditor.Models
{
    /// <summary>
    /// Описание экструзии: контур и параметр глубины
    /// </summary>
    [DataContract]
    internal class ExtrusionData
    {
        /// <summary>Список точек двумерного контура</summary>
        [DataMember]
        public List<Point2D> ProfilePoints { get; set; }

        /// <summary>Имя параметра</summary>
        [DataMember]
        public string DepthParameter { get; set; }
    }
}