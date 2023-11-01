﻿// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Xml;

namespace TestCentric.Engine.Internal
{
    public class TestPackageSerializer
    {
        public TestPackageSerializer() { }

        /// <summary>
        /// Convenience method to serialize a TestPackage to a StringWriter
        /// without an XML declaration.
        /// </summary>
        /// <param name="writer">An StringWriter to use</param>
        /// <param name="package">The package to be serialized</param>
        public void Serialize(StringWriter writer, TestPackage package)
        {
            // Assume we don't want the Xml declaration in our string
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var xmlWriter = XmlWriter.Create(writer, settings);
            Serialize(xmlWriter, package);
            xmlWriter.Flush();
            xmlWriter.Close();
        }

        /// <summary>
        /// Serialize a TestPackage to an XmlWriter
        /// </summary>
        /// <param name="xmlWriter">An XmlWriter to use</param>
        /// <param name="package">The package to be serialized</param>
        public void Serialize(XmlWriter xmlWriter, TestPackage package)
        {
            WriteTestPackage(xmlWriter, package);
        }

        private void WriteTestPackage(XmlWriter xmlWriter, TestPackage package)
        {
            xmlWriter.WriteStartElement("TestPackage");

            WritePackageAttributes();
            WriteSettings();
            WriteSubPackages();

            xmlWriter.WriteEndElement();

            void WritePackageAttributes()
            {
                xmlWriter.WriteAttributeString("id", package.ID);

                if (package.FullName != null)
                    xmlWriter.WriteAttributeString("fullname", package.FullName);
            }

            void WriteSettings()
            {
                if (package.Settings.Count != 0)
                {
                    xmlWriter.WriteStartElement("Settings");

                    foreach (var pair in package.Settings)
                        xmlWriter.WriteAttributeString(pair.Key, pair.Value.ToString());

                    xmlWriter.WriteEndElement();
                }
            }

            void WriteSubPackages()
            {
                if (package.SubPackages != null)
                    foreach (var subPackage in package.SubPackages)
                        WriteTestPackage(xmlWriter, subPackage);
            }
        }

        public TestPackage Deserialize(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var topNode = doc.DocumentElement;
            if (topNode.Name != "TestPackage")
                throw new ArgumentException("Xml provided is not a TestPackage");

            // Top-level package must always be anonymous.
            var id = topNode.GetAttribute("id");
            var package = new TestPackage(new string[0]);
            PopulatePackage(package, topNode);
            return package;
        }

        private void PopulatePackage(TestPackage package, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name)
                {
                    case "Settings":
                        foreach (XmlNode attr in child.Attributes)
                            package.AddSetting(attr.Name, attr.Value);
                        break;
                    case "TestPackage":
                        var fullName = child.GetAttribute("fullname");
                        package.AddSubPackage(fullName);
                        PopulatePackage(package, child);
                        break;
                }
            }
        }
    }
}
