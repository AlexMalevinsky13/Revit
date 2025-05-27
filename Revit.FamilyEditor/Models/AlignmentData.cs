using System.Runtime.Serialization;

namespace Revit.FamilyEditor.Models
{
    /// <summary>
    /// Информация о выравнивании или центрировании (EQ)
    /// </summary>
    [DataContract]
    internal class AlignmentData
    {
        /// <summary>Направление: вертикальное или горизонтальное</summary>
        [DataMember]
        public string Direction { get; set; }

        /// <summary>Нужно ли применять EQ</summary>
        [DataMember]
        public bool Equalize { get; set; } = true;
    }
}
