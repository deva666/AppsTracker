////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Text;
////using System.Threading.Tasks;
////using System.Xml.Linq;

////namespace AppsTracker.Utils
////{
////    internal abstract class SettingsBase
////    {
////        public abstract string RootNodeName
////        {
////            get;
////            protected set;
////        }

////        public virtual XElement CreateXml()
////        {
////            var root = new XElement(RootNodeName);
////            foreach (var prop in this.GetType().GetProperties())
////            {
////                var attr = prop.GetCustomAttributes(typeof(SettingsAttribute), true).FirstOrDefault();
////                if( attr == null)
////                    continue;
////                var subNode = new XElement(((SettingsAttribute)attr).PropertyName);

////            }

////            return root;
////        }

////        public virtual void FromXml(XElement node)
////        {

////        }
////    }
////}



//using Microsoft.VisualBasic;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.Xml.Linq;
//using System.Linq;

//namespace AppsTracker.Utils
//{

//   public class XmlSettingsBase
//   {

//      private string _itemListElementName = "ItemList";

//      private string _itemElementName = "Item";
//      public virtual XElement CreateXml()
//      {
//         List<System.Reflection.PropertyInfo> properties = GetFlaggedProperties();

//         //create root node
//         SettingsRootNodeName settingsrootNodeName = (SettingsRootNodeName)this.GetType().GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(SettingsRootNodeName));
//         string rootName = string.Empty;

//         if ((settingsrootNodeName != null))
//         {
//            if (!string.IsNullOrEmpty(settingsrootNodeName.Name))
//            {
//               rootName = settingsrootNodeName.Name;
//            }
//         }

//         if (string.IsNullOrEmpty(rootName))
//         {
//            rootName = this.GetType().Name;
//         }

//         XElement xRoot = new XElement(rootName);

//         //create sub nodes
//         foreach (System.Reflection.PropertyInfo p in properties)
//         {
//            SettingsAttribute xAttr = (SettingsAttribute)p.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(SettingsAttribute));
//            XElement subNode = new XElement(xAttr.PropertyName);

//            dynamic propValString = GetPropertyValue(p, xAttr);

//            if (propValString == null)
//            {
//               // try for IList types
//               XElement propValList = GetPropertyValues(p, xAttr);
//               if (propValList != null)
//               {
//                  subNode.Add(propValList);
//                  xRoot.Add(subNode);
//               }

//            }
//            else
//            {
//               subNode.Value = propValString;
//               xRoot.Add(subNode);
//            }
//         }

//         return xRoot;
//      }

//      public virtual void FromXml(XElement nd)
//   {
//      if (nd == null)
//         return;

//      List<System.Reflection.PropertyInfo> properties = GetFlaggedProperties();

//      foreach (System.Reflection.PropertyInfo p in properties) {
//         SettingsAttribute xAttr = (SettingsAttribute) p.GetCustomAttributes(true).FirstOrDefault(a=> a.GetType() == typeof(SettingsAttribute);

//         //find value for property
//         XElement node = nd.Element(xAttr.PropertyName);

//         if (node == null) {
//            Debug.WriteLine(string.Format("WARNING: --------- THE GIVEN ELEMENT {0} ISN'T FOUND IN THE XML", xAttr.PropertyName));
//            continue;
//         }

//         p.SetValue(this, GetValueFromXml(p.PropertyType, xAttr, node));
//      }
//   }

//      private object GetValueFromXml(System.Type propertyType, SettingsAttribute attr, XElement xmlNode)
//   {
//      string val = xmlNode.Value;

//      if(propertyType == typeof(string) ){
//            if (string.IsNullOrEmpty(val) & string.IsNullOrEmpty(attr.DefaultValue.ToString()))
//               return "";
//            if (string.IsNullOrEmpty(val))
//               return attr.DefaultValue;

//            return val;
//      }
//         if(propertyType == typeof(bool)){
//            if (string.IsNullOrEmpty(val) & attr.DefaultValue == null)
//               return false;
//            if (string.IsNullOrEmpty(val))
//               return attr.DefaultValue;

//            return Convert.ToBoolean(val);
//         }
//         case typeof(DateTime):
//            if (string.IsNullOrEmpty(val) & attr.DefaultValue == null)
//               return (new DateTime()).ToString("s");
//            if (string.IsNullOrEmpty(val))
//               return attr.DefaultValue;

//            return DateTime.Parse(val);
//         case typeof(TimeSpan):
//            if (string.IsNullOrEmpty(val) & attr.DefaultValue == null)
//               return new TimeSpan();
//            if (string.IsNullOrEmpty(val))
//               return attr.DefaultValue;

//            return TimeSpan.Parse(val);
//         case typeof(int):
//            if (string.IsNullOrEmpty(val) & attr.DefaultValue == null)
//               return 0;
//            if (string.IsNullOrEmpty(val))
//               return attr.DefaultValue;

//            return int.Parse(val);
//         case typeof(long):
//            if (string.IsNullOrEmpty(val) & attr.DefaultValue == null)
//               return 0;
//            if (string.IsNullOrEmpty(val))
//               return attr.DefaultValue;

//            return long.Parse(val);
//         case typeof(Int64):
//            if (string.IsNullOrEmpty(val) & attr.DefaultValue == null)
//               return 0;
//            if (string.IsNullOrEmpty(val))
//               return attr.DefaultValue;

//            return Int64.Parse(val);
//         case typeof(uint):
//            if (string.IsNullOrEmpty(val) & attr.DefaultValue == null)
//               return 0;
//            if (string.IsNullOrEmpty(val))
//               return attr.DefaultValue;

//            return uint.Parse(val);
//         case typeof(Guid):
//            if (val == null & attr.DefaultValue == null)
//               return Guid.Empty;
//            if (val == null)
//               return attr.DefaultValue;

//            return Guid.Parse(val);
//         case typeof(Text.Encoding):
//            if (string.IsNullOrEmpty(val))
//               return Text.Encoding.Default;
//            switch (val) {
//               case System.Text.Encoding.ASCII.ToString:
//                  return System.Text.Encoding.ASCII;
//               case System.Text.Encoding.BigEndianUnicode.ToString:
//                  return System.Text.Encoding.BigEndianUnicode;
//               case System.Text.Encoding.Unicode.ToString:
//                  return System.Text.Encoding.Unicode;
//               case System.Text.Encoding.ASCII.ToString:
//                  return System.Text.Encoding.ASCII;
//               case System.Text.Encoding.UTF32.ToString:
//                  return System.Text.Encoding.UTF32;
//               case System.Text.Encoding.UTF7.ToString:
//                  return System.Text.Encoding.UTF7;
//               case System.Text.Encoding.UTF8.ToString:
//                  return System.Text.Encoding.UTF8;
//               default:
//                  return System.Text.Encoding.Default;
//            }

//            break;
//         case typeof(List<int>):
//            if (val == null & attr.DefaultValue == null)
//               return null;
//            if (val == null)
//               return attr.DefaultValue;
//            dynamic root = xmlNode.Elements(_itemListElementName);
//            List<int> list = new List<int>();
//            foreach (var item_loopVariable in root.Elements(_itemElementName)) {
//               item = item_loopVariable;
//               list.Add(Int32.Parse(item.Value));
//            }


//            return list;
//         case typeof(List<string>):
//            if (val == null & attr.DefaultValue == null)
//               return null;
//            if (val == null)
//               return attr.DefaultValue;
//            dynamic root = xmlNode.Elements(_itemListElementName);
//            List<string> list = new List<string>();
//            foreach (var item_loopVariable in root.Elements(_itemElementName)) {
//               item = item_loopVariable;
//               list.Add(item.Value);
//            }


//            return list;



//            return list;


//            switch (propertyType.BaseType) {
//               case typeof(Enum):
//                  if (string.IsNullOrEmpty(val) & attr.DefaultValue == null)
//                     return 0;
//                  if (string.IsNullOrEmpty(val))
//                     return attr.DefaultValue;
//                  return Int32.Parse(val);
//            }

//            throw new InvalidCastException("The used property type isn't implemented");
//      }
//   }

//      private string GetPropertyValue(System.Reflection.PropertyInfo p, SettingsAttribute xAttr)
//   {
//      object val = p.GetValue(this);

//      switch (p.PropertyType) {
//         case typeof(string):
//            if (string.IsNullOrEmpty(val) & xAttr.DefaultValue == null)
//               return "";
//            if (val == null)
//               return xAttr.DefaultValue.ToString;

//            return val.ToString;
//         case typeof(bool):
//            if (val == null & xAttr.DefaultValue == null)
//               return false;
//            if (val == null)
//               return xAttr.DefaultValue.ToString;

//            return val.ToString;
//         case typeof(DateTime):
//            if (val == null & xAttr.DefaultValue == null)
//               return (new DateTime()).ToString("s");
//            if (val == null)
//               return xAttr.DefaultValue.ToString("s");
//            DateTime dt = (DateTime)val;

//            return dt.ToString("s");
//         case typeof(TimeSpan):
//            if (val == null & xAttr.DefaultValue == null)
//               return (new TimeSpan()).ToString("s");
//            if (val == null)
//               return xAttr.DefaultValue.ToString("s");
//            TimeSpan dt = (TimeSpan)val;

//            return dt.ToString();
//         case typeof(int):
//            if (val == null & xAttr.DefaultValue == null)
//               return 0;
//            if (val == null)
//               return xAttr.DefaultValue.ToString;

//            return val.ToString;
//         case typeof(long):
//            if (val == null & xAttr.DefaultValue == null)
//               return 0;
//            if (val == null)
//               return xAttr.DefaultValue.ToString;

//            return val.ToString;
//         case typeof(Int64):
//            if (val == null & xAttr.DefaultValue == null)
//               return 0;
//            if (val == null)
//               return xAttr.DefaultValue.ToString;

//            return val.ToString;
//         case typeof(uint):
//            if (val == null & xAttr.DefaultValue == null)
//               return 0;
//            if (val == null)
//               return xAttr.DefaultValue.ToString;

//            return val.ToString;
//         case typeof(Guid):
//            if (val == null & xAttr.DefaultValue == null)
//               return Guid.Empty.ToString;
//            if (val == null)
//               return xAttr.DefaultValue.ToString;

//            return val.ToString;
//         case typeof(List<int>):

//            return null;
//         case typeof(List<string>):

//            return null;
//         case typeof(List<Configuration.Objects.ADPropertyFieldMapping>):

//            return null;
//         case typeof(List<ImportExport.ImportExportColumn>):

//            return null;
//         case typeof(Text.Encoding):

//            return val.ToString;
//         default:
//            switch (p.PropertyType.BaseType) {
//               case typeof(Enum):
//                  if (val == null & xAttr.DefaultValue == null) {
//                     int lowestValue = (from i in Enum.GetValues(p.PropertyType)where i == 0).Count;

//                     if (lowestValue >= 1) {
//                        return 0;
//                     } else {
//                        throw new InvalidOperationException("0 is an invalid value for enum type " + p.PropertyType.ToString);
//                     }
//                  } else {
//                     if (val == null) {
//                        return Convert.ToInt32(xAttr.DefaultValue).ToString;
//                     } else {
//                        return Convert.ToInt32(val).ToString;
//                     }
//                  }

//                  break;
//               default:
//                  throw new InvalidCastException("The used property type isn't implemented");
//            }

//            break;
//      }

//   }

//      private XElement GetPropertyValues(System.Reflection.PropertyInfo p, SettingsAttribute xAttr)
//   {
//      object val = p.GetValue(this);

//      switch (p.PropertyType) {
//         case typeof(List<int>):
//            if (val == null & xAttr.DefaultValue == null)
//               return null;
//            if (val == null)
//               return xAttr.DefaultValue;

//            XElement xSubRoot = new XElement(_itemListElementName);

//            foreach (var item_loopVariable in val) {
//               item = item_loopVariable;
//               XElement xSubNode = new XElement(_itemElementName);
//               xSubNode.Value = item.ToString();
//               xSubRoot.Add(xSubNode);
//            }



//            return xSubRoot;
//         case typeof(List<string>):
//            if (val == null & xAttr.DefaultValue == null)
//               return null;
//            if (val == null)
//               return xAttr.DefaultValue;

//            XElement xSubRoot = new XElement(_itemListElementName);

//            foreach (var item_loopVariable in val) {
//               item = item_loopVariable;
//               XElement xSubNode = new XElement(_itemElementName);
//               xSubNode.Value = item.ToString();
//               xSubRoot.Add(xSubNode);
//            }



//            return xSubRoot;
//         case typeof(List<Configuration.Objects.ADPropertyFieldMapping>):
//            if (val == null & xAttr.DefaultValue == null)
//               return null;
//            if (val == null)
//               return xAttr.DefaultValue;

//            XElement xSubRoot = new XElement(_itemListElementName);

//            foreach (Configuration.Objects.ADPropertyFieldMapping item in val) {
//               XElement xSubNode = new XElement(_itemElementName);
//               XAttribute attr1 = new XAttribute("DirectoryEntryField", item.DirectoryEntryField);
//               //Dim attr2 As New XAttribute("Name", item.Name)
//               XAttribute attr3 = new XAttribute("ADPropertyField", item.ADPropertyField);
//               xSubNode.Add(attr1);
//               //xSubNode.Add(attr2)
//               xSubNode.Add(attr3);
//               xSubRoot.Add(xSubNode);
//            }



//            return xSubRoot;
//         case typeof(List<ImportExport.ImportExportColumn>):
//            if (val == null & xAttr.DefaultValue == null)
//               return null;
//            if (val == null)
//               return xAttr.DefaultValue;

//            XElement xSubRoot = new XElement(_itemListElementName);

//            foreach (ImportExport.ImportExportColumn item in val) {
//               XElement xSubNode = new XElement(_itemElementName);
//               XAttribute attr1 = new XAttribute("ID", item.ID);
//               XAttribute attr2 = new XAttribute("Name", item.Name);
//               XAttribute attr3 = new XAttribute("AssociatedImportfield", item.AssociatedImportfield);
//               XAttribute attr4 = new XAttribute("IsKey", item.IsKey);
//               xSubNode.Add(attr1);
//               xSubNode.Add(attr2);
//               xSubNode.Add(attr3);
//               xSubNode.Add(attr4);
//               xSubRoot.Add(xSubNode);
//            }



//            return xSubRoot;
//         default:
//            throw new InvalidCastException("The used property type isn't implemented");
//      }

//   }

//      private List<System.Reflection.PropertyInfo> GetFlaggedProperties()
//   {
//      //get all flagged properties from class
//      System.Reflection.PropertyInfo[] properties = this.GetType().GetProperties();
//      List<System.Reflection.PropertyInfo> filteredList = new List<System.Reflection.PropertyInfo>();
//      List<string> elementNames = new List<string>();

//      foreach (var p_loopVariable in properties) {
//         var p = p_loopVariable;
//         foreach (var attr_loopVariable in p.GetCustomAttributes(true)) {
//            var attr = attr_loopVariable;
//            if (attr.GetType() != typeof(SettingsAttribute)) {
//               continue;
//            }

//            if (elementNames.Contains(((SettingsAttribute)attr).PropertyName)) {
//               throw new InvalidOperationException("A duplicate xml tag name for property found!");
//            }

//            elementNames.Add(((SettingsAttribute)attr).PropertyName);
//            filteredList.Add(p);

//            break; // TODO: might not be correct. Was : Exit For
//         }
//      }

//      if (properties.Count() == 0) {
//         throw new InvalidOperationException("The current class has no properties.");
//      }

//      return filteredList;
//   }

//      //Public Shared Function FromXml(nd As Xml.XmlNode) As Object
//      //   Using ms As New IO.MemoryStream
//      //      Dim bytes() As Byte = Text.ASCIIEncoding.UTF8.GetBytes(nd.OuterXml)
//      //      ms.Write(bytes, 0, bytes.Length)
//      //      ms.Seek(0, IO.SeekOrigin.Begin)

//      //      Return DoFromXml(ms)
//      //   End Using
//      //End Function

//      //Public Shared Function FromXml(nd As XElement) As Object
//      //   Using ms As New IO.MemoryStream
//      //      nd.Save(ms)
//      //      ms.Seek(0, IO.SeekOrigin.Begin)

//      //      Return DoFromXml(ms)
//      //   End Using
//      //End Function

//      //Private Shared Function DoFromXml(stream As IO.MemoryStream) As Object
//      //   Dim reader As Xml.XmlReader = Xml.XmlReader.Create(stream)
//      //   Dim xmlSerializer As New Xml.Serialization.XmlSerializer(GetType(XmlSettingsBase))
//      //   Return xmlSerializer.Deserialize(reader)
//      //End Function
//   }
//}