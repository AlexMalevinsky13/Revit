using System.Runtime.Serialization;

namespace Revit.FamilyEditor.Models
{
    /// <summary>
    /// Описание параметра семейства
    /// </summary>
    [DataContract]
    internal class ParameterData
    {
        /// <summary>Имя параметра</summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>Значение в миллиметрах</summary>
        [DataMember]
        public double Value { get; set; }

        /// <summary>Тип параметра (Length)</summary>
        [DataMember]
        public string Type { get; set; } = "Length";
    }
}