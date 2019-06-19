using System.Xml;

namespace GeometryFriends.Levels.Shared
{
    interface IXmlSerializable
    {
        XmlNode ToXml(XmlDocument doc);
    }
}
