using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Revit.FamilyEditor.Models
{
    /// <summary>
    /// Корневой объект, содержит описание семейства
    /// </summary>
    [DataContract]
    internal class FamilyData
    {
        /// <summary>Список параметров (w, p)</summary>
        [DataMember]
        public List<ParameterData> Parameters { get; set; }

        /// <summary>Данные экструзии (контур и глубина)</summary>
        [DataMember]
        public ExtrusionData Extrusion { get; set; }

        /// <summary>Размерные связи, связанные с параметрами</summary>
        [DataMember]
        public List<DimensionData> Dimensions { get; set; }

        /// <summary>Выравнивания и EQ</summary>
        [DataMember]
        public List<AlignmentData> Alignments { get; set; }
    }
}