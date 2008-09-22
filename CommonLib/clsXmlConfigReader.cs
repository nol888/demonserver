/*
+===========================================================================+
|	Demon - dAmn Emulator													|
|===========================================================================|
|	Copyright © 2008 Nol888													|
|===========================================================================|
|	This file is part of Demon.												|
|																			|
|	Demon is free software: you can redistribute it and/or modify			|
|	it under the terms of the GNU Affero General Public License as			|
|	published by the Free Software Foundation, either version 3 of the		|
|	License, or (at your option) any later version.							|
|																			|
|	This program is distributed in the hope that it will be useful,			|
|	but WITHOUT ANY WARRANTY; without even the implied warranty of			|
|	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the			|
|	GNU Affero General Public License for more details.						|
|																			|
|	You should have received a copy of the GNU Affero General Public License|
|	along with this program.  If not, see <http://www.gnu.org/licenses/>.	|
|																			|
|===========================================================================|
|	> $Date$
|	> $Revision$
|	> $Author$
+===========================================================================+
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CommonLib
{
	public class XmlConfigReader
	{
		#region Private Properties
		private XmlReader InternalReader;

        private string FileName;
		#endregion

		#region Constuctor and Destructor
		public XmlConfigReader(string Filename)
		{
			if (!File.Exists(Filename))
			{
				throw new FileNotFoundException("Error reading configuration XML file: File not found.", Environment.CurrentDirectory + "\\" + Filename);
			}

			InternalReader = XmlReader.Create(new StreamReader(Filename));
            this.FileName = Filename;
		}
        ~XmlConfigReader()
        {
            this.InternalReader = null;
        }
		#endregion

		#region Public Methods
		/// <summary>
		/// Reads an XML configuration file.
		/// </summary>
		/// <returns>A dictionary of XML elements/attributes to the element.</returns>
		public Dictionary<string, string> ReadConfig()
		{
			Dictionary<string,string> Config = new Dictionary<string,string>();
            string ConfigName = "";

            try
            {
                while (this.InternalReader.Read())
                {
                    switch (this.InternalReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            string Name = this.InternalReader.Name;
                            int AttributeCount = this.InternalReader.AttributeCount;

                            this.InternalReader.MoveToFirstAttribute();
                            for (int i = 0; i < AttributeCount; i++)
                            {
                                ConfigName = Name;
                                if (this.InternalReader.Name != "value")
                                {
                                    ConfigName = Name + "-" + this.InternalReader.Name;
                                }
                                Config.Add(ConfigName, this.InternalReader.Value);
                                this.InternalReader.MoveToNextAttribute();
                            }
                            break;
                    }
                }
            }
            catch (ArgumentException)
            {
                throw new Exception(string.Format("Error parsing config file at directive {0}: Duplicate directive.\nCheck your syntax and try again. File: {1}", ConfigName, this.FileName));
            }
            catch (XmlException Ex)
            {
                throw new Exception(string.Format("Error parsing config file at line {0} col {1}: Invalid XML syntax.\n{2}\nFile: {3}", new object[] { Ex.LineNumber, Ex.LinePosition, Ex.Message, this.FileName }));
            }
            finally
            {
                this.InternalReader.Close();
            }
			return Config;
		}

		/// <summary>
		/// <para>Merges two dictionaries.</para>
		/// <para>Tkey is <typeparamref name="TKey"/></para>
		/// <para>TValue is <typeparamref name="TValue"/></para>
		/// </summary>
		/// <typeparam name="TKey">The type of the keys in the dictionaries.</typeparam>
		/// <typeparam name="TValue">The type of the values in the dictionaries.</typeparam>
		/// <param name="Dic1">The first dictionary to be merged. Any duplicate key in this dictionary will be overwritten
		/// with the key the second one.</param>
		/// <param name="Dic2">The second dictionary to be merged.  Any duplicate key in the first dictionary will be overwritten
		/// with the key in this one.</param>
		/// <returns>The merged dictionaries.</returns>
		public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(Dictionary<TKey, TValue> Dic1, Dictionary<TKey, TValue> Dic2)
		{
			Dictionary<TKey, TValue> ReturnDict = new Dictionary<TKey, TValue>();
			try
			{
				foreach (KeyValuePair<TKey, TValue> Entry in Dic1)
				{
					ReturnDict.Add(Entry.Key, Entry.Value);
				}
				foreach (KeyValuePair<TKey, TValue> Entry in Dic2)
				{
					ReturnDict.Add(Entry.Key, Entry.Value);
				}
			}
			catch (System.Exception Ex)
			{
				Console.ShowError("Error merging databases!\n" + Ex.StackTrace);
			}
			return ReturnDict;
		}
		#endregion
	}
}
